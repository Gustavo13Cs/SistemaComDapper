using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Estacionamento.Controllers
{
    [Route("chatbot")]
    public class ChatbotController : Controller
    {
        private readonly HttpClient _http;

        public ChatbotController(HttpClient httpClient)
        {
            _http = httpClient;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] UserMessage request)
        {
            try
            {
                var payload = new
                {
                    model = "mistral", 
                    prompt = request.Message,
                    stream = false
                };

                var response = await _http.PostAsJsonAsync("http://localhost:11434/api/generate", payload);

                if (!response.IsSuccessStatusCode)
                    return BadRequest(new { resposta = "⚠️ Erro ao conectar com o Ollama. Verifique se o servidor está rodando." });

                var content = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(content);
                var text = doc.RootElement.GetProperty("response").GetString();

                return Ok(new { resposta = text });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { resposta = $"❌ Erro interno: {ex.Message}" });
            }
        }
    }
    public class UserMessage
    {
        public string Message { get; set; }
    }
}
