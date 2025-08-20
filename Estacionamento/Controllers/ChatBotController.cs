using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Data;
using System.Text.Json;
using System.Net.Http.Headers;

[ApiController]
[Route("api/[controller]")]
public class ChatbotController : ControllerBase
{
    private readonly IDbConnection _cnn;
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public ChatbotController(IDbConnection cnn, IConfiguration config)
    {
        _cnn = cnn;
        _config = config;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _config["OpenAI:ApiKey"]);
    }

    [HttpPost("perguntar")]
    public async Task<IActionResult> Perguntar([FromBody] JsonElement data)
    {
        string query = data.GetProperty("pergunta").GetString()?.ToLower() ?? "";
        string resposta;

        try
        {
            if (PerguntaEhComando(query))
            {
                resposta = ExecutaNoBanco(query);
            }
            else
            {
                var contexto = MontaContextoDoSistema();
                resposta = await ChamarIA(query, contexto);
            }
        }
        catch (Exception ex)
        {
            resposta = $"❌ Erro ao processar sua pergunta: {ex.Message}";
        }

        return Ok(new { resposta });
    }

    private bool PerguntaEhComando(string query)
    {
        return query.Contains("ticket") ||
               query.Contains("vaga") ||
               query.Contains("tarifa") ||
               query.Contains("receita") ||
               query.Contains("cliente");
    }

    private string ExecutaNoBanco(string query)
    {
        if (query.Contains("ticket"))
        {
            var count = _cnn.ExecuteScalar<int>("SELECT COUNT(*) FROM tickets WHERE Pago = 0");
            return $"🎫 Existem {count} ticket(s) ativos no sistema.";
        }
        else if (query.Contains("vaga"))
        {
            var livres = _cnn.ExecuteScalar<int>("SELECT COUNT(*) FROM vagas WHERE Ocupada = 0");
            return $"🅿️ Temos {livres} vaga(s) livres disponíveis.";
        }
        else if (query.Contains("tarifa"))
        {
            var tarifa = _cnn.ExecuteScalar<decimal?>(
                "SELECT Valor FROM tarifas WHERE TipoTarifa = 'Padrão' ORDER BY Id DESC LIMIT 1"
            ) ?? 0m;
            return $"💰 A tarifa padrão atual é {tarifa:C} por minuto.";
        }
        else if (query.Contains("receita"))
        {
            var receita = _cnn.ExecuteScalar<decimal?>(
                "SELECT SUM(Valor) FROM tickets WHERE Pago = 1 AND DATE(DataSaida) = CURDATE()"
            ) ?? 0m;
            return $"📊 A receita de hoje é {receita:C}.";
        }
        else if (query.Contains("cliente"))
        {
            var clientes = _cnn.Query<string>("SELECT Nome FROM clientes LIMIT 5").ToList();
            return $"👥 Alguns clientes cadastrados: {string.Join(", ", clientes)}...";
        }

        return "🤔 Não entendi sua pergunta, tente ser mais específico.";
    }

    private string MontaContextoDoSistema()
    {
        var vagasTotais = _cnn.ExecuteScalar<int>("SELECT COUNT(*) FROM vagas");
        var vagasLivres = _cnn.ExecuteScalar<int>("SELECT COUNT(*) FROM vagas WHERE Ocupada = 0");
        var receitaHoje = _cnn.ExecuteScalar<decimal?>(
            "SELECT SUM(Valor) FROM tickets WHERE Pago = 1 AND DATE(DataSaida) = CURDATE()"
        ) ?? 0m;
        var tarifaPadrao = _cnn.ExecuteScalar<decimal?>(
            "SELECT Valor FROM tarifas WHERE TipoTarifa = 'Padrão' ORDER BY Id DESC LIMIT 1"
        ) ?? 0m;

        return $@"
        Situação atual do estacionamento:
        - Vagas totais: {vagasTotais}
        - Vagas livres: {vagasLivres}
        - Receita de hoje: {receitaHoje:C}
        - Tarifa padrão: {tarifaPadrao:C}/minuto
        ";
    }

    private async Task<string> ChamarIA(string pergunta, string contexto)
    {
        var payload = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = "Você é um assistente de estacionamento. Responda sempre em português, de forma clara e útil." },
                new { role = "user", content = $"Contexto do sistema:\n{contexto}\n\nPergunta do usuário:\n{pergunta}" }
            }
        };

        var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", payload);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();
            return $"❌ Erro na API OpenAI: {erro}";
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        try
        {
            return json.GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
        }
        catch
        {
            return $"❌ Erro ao interpretar resposta da IA: {json}";
        }
    }

}
