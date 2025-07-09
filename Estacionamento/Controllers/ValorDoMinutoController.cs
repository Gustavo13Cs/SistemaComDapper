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

        [HttpPost("{id}/apagar")]
        public async Task<IActionResult> Apagar([FromRoute] int id)
        {
            var sql = @"
                Delete From Valores Where id=@id";

            var rowsInserted = await _connection.ExecuteAsync(sql, new ValorDoMinuto { Id = id });

            if (rowsInserted > 0)
            {
                TempData["Success"] = "Valor excluido com sucesso!";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Não foi possível salvar.");
                return View("Novo", id);
            }
        }

        [HttpGet("{id}/editar")]
        public IActionResult Editar([FromRoute]int id)
        {
            var sql = "SELECT Id, Minutos, Valor AS Valor FROM Valores WHERE Id = @Id";
            var valor = _connection.QueryFirstOrDefault<ValorDoMinuto>(sql, new { Id = id });

            if (valor == null)
                return NotFound();

            return View(valor);
        }
        
        [HttpPost("{id}/alterar")]
        public async Task<IActionResult> Alterar([FromRoute]int id, [FromForm]ValorDoMinuto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var sql = @"
                UPDATE Valores
                SET Minutos = @Minutos,
                    Valor = @Valor
                WHERE Id = @Id
            ";

                await _connection.ExecuteAsync(sql, new {
                    model.Minutos,
                    model.Valor,
                    Id = id
                });

            return RedirectToAction("Index");
        }

    }
}
