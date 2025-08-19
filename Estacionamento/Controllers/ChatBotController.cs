using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using System.Text.Json;

namespace Estacionamento.Controllers
{
    [Route("chatbot")]
    public class ChatBotController : Controller
    {
        private readonly IDbConnection _cnn;
        private readonly IWebHostEnvironment _env;

        public ChatBotController(IDbConnection cnn, IWebHostEnvironment env)
        {
            _cnn = cnn;
            _env = env;
        }

        [HttpGet("responder")]
        public IActionResult Responder([FromQuery] string pergunta)
        {
            pergunta = pergunta?.ToLower().Trim() ?? "";

            string resposta;

            if (pergunta.Contains("vaga") && pergunta.Contains("disponível"))
            {
                int total = _cnn.ExecuteScalar<int>("SELECT COUNT(*) FROM Vagas WHERE Ocupada = false");
                resposta = $"Temos {total} vaga(s) disponível(is) no momento.";
            }
            else if (pergunta.Contains("vaga") && pergunta.Contains("ocupada"))
            {
                int ocupadas = _cnn.ExecuteScalar<int>("SELECT COUNT(*) FROM Vagas WHERE Ocupada = true");
                resposta = $"Atualmente, {ocupadas} vaga(s) estão ocupadas.";
            }
            else if (pergunta.Contains("tarifa") || pergunta.Contains("valor do minuto"))
            {
                var valor = _cnn.QueryFirstOrDefault<float>("SELECT ValorCobrado FROM Valores ORDER BY id DESC LIMIT 1");
                resposta = $"A tarifa padrão atual é de R$ {valor:F2} por minuto.";
            }
            else if (pergunta.Contains("ticket") && (pergunta.Contains("ativo") || pergunta.Contains("aberto")))
            {
                int ativos = _cnn.ExecuteScalar<int>("SELECT COUNT(*) FROM Tickets WHERE DataSaida IS NULL");
                resposta = $"Existem {ativos} ticket(s) ativos no sistema.";
            }
            else if (pergunta.Contains("receita") || pergunta.Contains("faturamento"))
            {
                var hoje = DateTime.Today;
                var receita = _cnn.ExecuteScalar<float>(
                    "SELECT COALESCE(SUM(Valor), 0) FROM Tickets WHERE DataSaida BETWEEN @inicio AND @fim",
                    new { inicio = hoje, fim = hoje.AddDays(1).AddSeconds(-1) });

                resposta = $"A receita de hoje até agora é de R$ {receita:F2}.";
            }
            else
            {
                resposta = "Desculpe, não entendi sua pergunta. Tente perguntar sobre vagas, tickets, receita ou tarifa.";
            }

            var sugestoesPath = Path.Combine(_env.WebRootPath, "data", "sugestoes.json");
            var sugestoesJson = System.IO.File.ReadAllText(sugestoesPath);
            var sugestoes = JsonSerializer.Deserialize<Dictionary<string, string[]>>(sugestoesJson);

            return Json(new
            {
                resposta,
                sugestoes = sugestoes?["sugestoes"] ?? new string[] { }
            });
        }
    }
}
