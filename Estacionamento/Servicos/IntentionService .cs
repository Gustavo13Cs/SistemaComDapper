using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace Estacionamento.Servicos
{

    public class Intention
    {
        public string Description { get; set; } // A frase que descreve a intenção
        public Func<string> Action { get; set; } // O código que busca a info no BD
        public float[] Vector { get; set; } // O "cérebro" da intenção
    }
    
    public class IntentionService
    {
        private readonly List<Intention> _intentions;
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _serviceProvider;

        public IntentionService(IServiceProvider serviceProvider,HttpClient httpClient)
        {
            _serviceProvider = serviceProvider;
            _httpClient = httpClient;
            
            _intentions = new List<Intention>
            {
                new Intention {
                    Description = "Verificar o número de vagas disponíveis no estacionamento.",
                    Action = () => {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var cnn = scope.ServiceProvider.GetRequiredService<IDbConnection>();
                            var vagas = cnn.ExecuteScalar<int>("SELECT COUNT(*) FROM Vagas WHERE Ocupada = 0");
                            return $"No momento, existem {vagas} vagas livres.";
                        }
                    }
                },
                new Intention {
                    Description = "Consultar a tarifa ou preço por minuto.",
                    Action = () => {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var cnn = scope.ServiceProvider.GetRequiredService<IDbConnection>();
                            var tarifa = cnn.ExecuteScalar<decimal>("SELECT Valor FROM Tarifas WHERE TipoTarifa = 'normal' ORDER BY Id DESC LIMIT 1");
                            return $"A tarifa padrão atual é de {tarifa:C} por minuto.";
                        }
                    }
                },
                new Intention {
                    Description = "Verificar a receita ou faturamento do dia.",
                    Action = () => {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var cnn = scope.ServiceProvider.GetRequiredService<IDbConnection>();
                            var receita = cnn.ExecuteScalar<decimal>("SELECT IFNULL(SUM(Valor), 0) FROM Tickets WHERE Pago = 1 AND DATE(DataSaida) = CURDATE()");
                            return $"A receita do estacionamento hoje é de {receita:C}.";
                        }
                    }
                }
            };
        }

        // Este método deve ser chamado na inicialização do seu app (em Program.cs)
        public async Task InitializeAsync()
        {
            foreach (var intention in _intentions)
            {
                intention.Vector = await GetEmbeddingAsync(intention.Description);
            }
        }

        // Novo método para encontrar a melhor intenção
        public async Task<string> FindBestIntentionContextAsync(string userQuery)
        {
            var queryVector = await GetEmbeddingAsync(userQuery);
            if (queryVector == null) return null;

            Intention bestIntention = null;
            double highestSimilarity = -1;

            foreach (var intention in _intentions)
            {
                double similarity = CosineSimilarity(queryVector, intention.Vector);
                if (similarity > highestSimilarity)
                {
                    highestSimilarity = similarity;
                    bestIntention = intention;
                }
            }

            // Um "threshold" para ter certeza. Se a melhor intenção ainda for muito diferente, ignore.
            if (highestSimilarity > 0.6) // Ajuste este valor conforme os testes
            {
                return bestIntention.Action(); // Executa a busca no BD!
            }

            return null; // Nenhuma intenção relevante encontrada
        }

        // Método para chamar a API de embeddings do Ollama
        private async Task<float[]> GetEmbeddingAsync(string text)
        {
            var payload = new { model = "nomic-embed-text", prompt = text };
            var response = await _httpClient.PostAsJsonAsync("/api/embeddings", payload);
            if(!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>();
            return result?.Embedding.Select(d => (float)d).ToArray();
        }

        // A matemática da similaridade
        private double CosineSimilarity(float[] vecA, float[] vecB)
        {
            double dotProduct = 0.0;
            double normA = 0.0;
            double normB = 0.0;
            for (int i = 0; i < vecA.Length; i++)
            {
                dotProduct += vecA[i] * vecB[i];
                normA += Math.Pow(vecA[i], 2);
                normB += Math.Pow(vecB[i], 2);
            }
            return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }

        private class OllamaEmbeddingResponse { public double[] Embedding { get; set; } }
    }
}