using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;

namespace Estacionamento.Controllers
{
    [Route("chatbot")]
    public class ChatBotController : Controller
    {
        private readonly IDbConnection _cnn;

        public ChatBotController(IDbConnection cnn)
        {
            _cnn = cnn;
        }

        [HttpGet("responder")]
        public IActionResult Responder([FromQuery] string pergunta)
        {
            pergunta = pergunta.ToLower().Trim();

            if (pergunta.Contains("vaga") && pergunta.Contains("disponível"))
            {
                int total = _cnn.ExecuteScalar<int>("SELECT COUNT(*) FROM Vagas WHERE Ocupada = false");
                return Content($"Temos {total} vaga(s) disponível(is) no momento.");
            }

            if (pergunta.Contains("vaga") && pergunta.Contains("ocupada"))
            {
                int ocupadas = _cnn.ExecuteScalar<int>("SELECT COUNT(*) FROM Vagas WHERE Ocupada = true");
                return Content($"Atualmente, {ocupadas} vaga(s) estão ocupadas.");
            }

            if (pergunta.Contains("tarifa") || pergunta.Contains("valor do minuto"))
            {
                var valor = _cnn.QueryFirstOrDefault<float>("SELECT ValorCobrado FROM Valores ORDER BY id DESC LIMIT 1");
                return Content($"A tarifa padrão atual é de R$ {valor:F2} por minuto.");
            }

            if (pergunta.Contains("ticket") && (pergunta.Contains("ativo") || pergunta.Contains("aberto")))
            {
                int ativos = _cnn.ExecuteScalar<int>("SELECT COUNT(*) FROM Tickets WHERE DataSaida IS NULL");
                return Content($"Existem {ativos} ticket(s) ativos no sistema.");
            }

            if (pergunta.Contains("receita") || pergunta.Contains("faturamento"))
            {
                var hoje = DateTime.Today;
                var receita = _cnn.ExecuteScalar<float>(
                    "SELECT COALESCE(SUM(Valor), 0) FROM Tickets WHERE DataSaida BETWEEN @inicio AND @fim",
                    new { inicio = hoje, fim = hoje.AddDays(1).AddSeconds(-1) });

                return Content($"A receita de hoje até agora é de R$ {receita:F2}.");
            }

            return Content("Desculpe, não entendi sua pergunta. Tente perguntar sobre vagas, tickets, receita ou tarifa.");
        }
    }
}
