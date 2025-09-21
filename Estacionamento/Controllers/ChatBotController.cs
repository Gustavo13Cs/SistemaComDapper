using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Text.Json;
using System.Text;
using Estacionamento.Models;

using static Estacionamento.Models.IAReques;
using Estacionamento.Servicos;

namespace SeuProjeto.Controllers
{
    [Route("chatbot")]
    public class ChatbotController : Controller
    {
        private readonly IDbConnection _cnn;
        private readonly HttpClient _httpClient;
        private readonly IntentionService _intentionService;

        public ChatbotController(IDbConnection cnn, IntentionService intentionService, IHttpClientFactory httpClientFactory)
        {
            _cnn = cnn;
            _intentionService = intentionService;
            _httpClient = httpClientFactory.CreateClient("OllamaClient");
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest input)
        {
             var lastUserMessage = input.History.LastOrDefault(m => m.Role == "user");
             if (lastUserMessage == null)
                return BadRequest("Nenhuma mensagem de usuário encontrada.");
            
            string query = lastUserMessage.Content.ToLower();
            try
            {
                string contexto = await _intentionService.FindBestIntentionContextAsync(query);
                if (contexto == null)
                {
                    contexto = "Não encontrei informações no banco para essa pergunta. Responda de forma geral e amigável.";
                }

                var promptBuilder = new StringBuilder();
                promptBuilder.AppendLine("Você é um assistente prestativo de um sistema de estacionamento.");
                promptBuilder.AppendLine("Responda de forma concisa e amigável baseado no contexto e no histórico da conversa.");
                if (!string.IsNullOrWhiteSpace(contexto))
                {
                    promptBuilder.AppendLine($"\n[Contexto Relevante para a Pergunta Atual]: {contexto}");
                }

                foreach (var message in input.History)
                {
                    var role = message.Role == "user" ? "Usuário" : "Assistente";
                    promptBuilder.AppendLine($"{role}: {message.Content}");
                }
                promptBuilder.Append("Assistente:");

                var payload = new
                {
                    model = "mistral",
                    prompt = promptBuilder.ToString(),
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
