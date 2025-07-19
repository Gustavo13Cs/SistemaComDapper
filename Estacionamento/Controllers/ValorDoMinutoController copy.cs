using System.Data;
using System.Threading.Tasks;
using Dapper;
using Estacionamento.Models;
using Estacionamento.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Estacionamento.Controllers
{
    [Route("/valores")]
    public class ValorDoMinutoController : Controller
    {
        private readonly IRepositorio<ValorDoMinuto> _repo;
        private readonly IDbConnection _cnn;

        public ValorDoMinutoController(IDbConnection cnn, IRepositorio<ValorDoMinuto> repo)
        {
            _repo = repo;
            _cnn = cnn;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var tarifaPadrao = _cnn.QueryFirstOrDefault<ValorDoMinuto>("SELECT * FROM Valores ORDER BY Id DESC LIMIT 1");
            var tarifasEspeciais = _cnn.Query<TarifaEspecial>("SELECT * FROM TarifasEspeciais ORDER BY HoraInicio").ToList();

            ViewBag.TarifaPadrao = tarifaPadrao;
            ViewBag.TarifasEspeciais = tarifasEspeciais;

            return View();
        }

        [HttpGet("novo")]
        public IActionResult Novo()
        {
            return View();
        }

        [HttpPost("Criar")]
        public async Task<IActionResult> Criar([FromForm] ValorDoMinuto valorDoMinuto)
        {
            _repo.Inserir(valorDoMinuto);
            return Redirect("/valores");
        }

        [HttpPost("{id}/apagar")]
        public async Task<IActionResult> Apagar([FromRoute] int id)
        {
            _repo.Excluir(id);
            return Redirect("/valores");
        }

        [HttpGet("{id}/editar")]
        public IActionResult Editar([FromRoute]int id)
        {
           var valor = _repo.ObterPorId(id);
            return View(valor);
        }

        [HttpPost("{id}/alterar")]
        public async Task<IActionResult> Alterar([FromRoute] int id, [FromForm] ValorDoMinuto valorDoMinuto)
        {
            valorDoMinuto.Id = id;
            _repo.Atualizar(valorDoMinuto);
            return Redirect("/valores");
        }

    }
}
