using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Text.Json;
using System.Text;
using Estacionamento.Models;

namespace SeuProjeto.Controllers
{
    [Route("chatbot")]
    public class ChatbotController : Controller
    {
        private readonly IDbConnection _cnn;
        private readonly HttpClient _httpClient;

        public ChatbotController(IDbConnection cnn)
        {
            _cnn = cnn;
            _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11434/") };
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] UserMessage input)
        {
            string query = input.Message.ToLower();
            string contexto = "";

            try
            {
                if (query.Contains("vaga"))
                {
                    var vagasLivres = _cnn.ExecuteScalar<int>("SELECT COUNT(*) FROM Vagas WHERE Ocupada = 0");
                    contexto = $"No momento, existem {vagasLivres} vagas livres no estacionamento.";
                }
                else if (query.Contains("ticket"))
                {
                    var ticketsAbertos = _cnn.ExecuteScalar<int>("SELECT COUNT(*) FROM tickets WHERE DataSaida IS NULL;");
                    contexto = $"Existem {ticketsAbertos} tickets ativos no sistema.";
                }
                else if (query.Contains("tarifa"))
                {
                    var tarifa = _cnn.ExecuteScalar<decimal>("SELECT Valor FROM Tarifas WHERE TipoTarifa = 'normal' ORDER BY Id DESC LIMIT 1");
                    contexto = $"A tarifa padrão atual é de {tarifa:C} por minuto.";
                }
                else if (query.Contains("receita"))
                {
                    var receita = _cnn.ExecuteScalar<decimal>("SELECT IFNULL(SUM(Valor), 0) FROM Tickets WHERE Pago = 1 AND DATE(DataSaida) = CURDATE()");
                    contexto = $"A receita do estacionamento hoje é de {receita:C}.";
                }
                else
                {
                    contexto = "Não encontrei informações no banco para essa pergunta. Responda de forma geral e amigável.";
                }

                // 🔹 Monta o payload para o Ollama
                var payload = new
                {
                    model = "mistral", 
                    prompt = $"Contexto: {contexto}\nUsuário: {input.Message}\nBot:",
                    stream = false
                };

                var response = await _httpClient.PostAsync("api/generate",
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                    return Json(new { resposta = "⚠️ Erro ao conectar com a IA." });

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                string respostaIA = doc.RootElement.GetProperty("response").GetString();

                return Json(new { resposta = respostaIA });
            }
            catch (Exception ex)
            {
                return Json(new { resposta = $"❌ Erro ao processar: {ex.Message}" });
            }
        }
    }
}
