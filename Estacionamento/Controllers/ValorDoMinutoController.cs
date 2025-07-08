using System.Data;
using System.Threading.Tasks;
using Dapper;
using Estacionamento.Models;
using Microsoft.AspNetCore.Mvc;

namespace Estacionamento.Controllers
{
    [Route("/valores")]
    public class ValorDoMinutoController : Controller
    {
        private readonly IDbConnection _connection;

        public ValorDoMinutoController(IDbConnection connection)
        {
            _connection = connection;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var valores = _connection.Query<ValorDoMinuto>("SELECT * FROM Valores");
            return View(valores);
        }

        [HttpGet("novo")]
        public IActionResult Novo()
        {
            return View();
        }

        [HttpPost("Criar")]
        public async Task<IActionResult> Criar([FromForm] ValorDoMinuto valorDoMinuto)
        {
            var sql = @"
                INSERT INTO Valores (Minutos, Valor)
                VALUES (@Minutos, @Valor);
            ";

            var rowsInserted = await _connection.ExecuteAsync(sql, new
            {
                valorDoMinuto.Minutos,
                valorDoMinuto.Valor
            });

            if (rowsInserted > 0)
            {
                TempData["Success"] = "Valor salvo com sucesso!";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Não foi possível salvar.");
                return View("Novo", valorDoMinuto);
            }
        }
    }
}
