using System.Text.Json.Serialization; 
using System.Data;
using Dapper;


namespace Estacionamento.Servicos
{
    public class Intention
    {
        public string Description { get; set; }
        public Func<string> Action { get; set; }
        public float[] Vector { get; set; }
    }
    
    public class IntentionService
    {
        private readonly List<Intention> _intentions;
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _serviceProvider;

        public IntentionService(IServiceProvider serviceProvider, IHttpClientFactory  httpClientFactory)
        {
            _serviceProvider = serviceProvider;
            _httpClient = httpClientFactory.CreateClient("OllamaClient");
            
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
                            var receita = cnn.ExecuteScalar<decimal?>("SELECT SUM(ValorCobrado) FROM historicovagas WHERE DATE(DataSaida) = CURDATE()");
                            return $"A receita do estacionamento hoje é de {receita:C}.";
                        }
                    }
                }
            };
        }

        public async Task InitializeAsync()
        {
            foreach (var intention in _intentions)
            {
                intention.Vector = await GetEmbeddingAsync(intention.Description);
            }
        }

        public async Task<string> FindBestIntentionContextAsync(string userQuery)
        {
            var queryVector = await GetEmbeddingAsync(userQuery);
            if (queryVector == null)
            {
                Console.WriteLine($"Não foi possível gerar o vetor para a query: {userQuery}");
                return null;
            }

            Intention bestIntention = null;
            double highestSimilarity = -1;

            foreach (var intention in _intentions)
            {
                if (intention.Vector == null)
                {
                    Console.WriteLine($"Vetor nulo para a intenção: {intention.Description}");
                    continue; 
                }
                double similarity = CosineSimilarity(queryVector, intention.Vector);
                if (similarity > highestSimilarity)
                {
                    highestSimilarity = similarity;
                    bestIntention = intention;
                }
            }

            if (highestSimilarity > 0.6)
            {
                return bestIntention.Action();
            }

            return null;
        }

        private async Task<float[]> GetEmbeddingAsync(string text)
        {
            var payload = new 
            { 
                model = "nomic-embed-text", 
                prompt = text,
                options = new {
                    embedding_only = true 
                }
            };
            
            var response = await _httpClient.PostAsJsonAsync("/api/embeddings", payload);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erro ao chamar a API de embedding. Status: {response.StatusCode}, Resposta: {errorContent}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>();
            
            if (result?.Embedding == null)
            {
                Console.WriteLine("A resposta do Ollama foi bem-sucedida, mas a propriedade 'embedding' veio nula ou vazia no JSON.");
                return null;
            }

            return result.Embedding.Select(d => (float)d).ToArray();
        }

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

        private class OllamaEmbeddingResponse
        {
            [JsonPropertyName("embedding")]             public double[] Embedding { get; set; }
        }
        
    }
}
