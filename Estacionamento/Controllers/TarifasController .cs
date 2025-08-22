using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using Dapper;
using Estacionamento.Models;
using Estacionamento.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Estacionamento.Controllers
{
    [Route("/tarifas")]
    public class TarifasController : Controller
    {
        private readonly IRepositorio<Tarifas> _repo;
        private readonly IDbConnection _cnn;

        public TarifasController(IDbConnection cnn, IRepositorio<Tarifas> repo)
        {
            _repo = repo;
            _cnn = cnn;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var tarifas = _cnn.Query<Tarifas>("SELECT * FROM Tarifas ORDER BY TipoTarifa DESC, HoraInicio").ToList();
            var tarifasNormais = tarifas.Where(t => t.TipoTarifa == "Normal").ToList();
            var tarifasEspeciais = tarifas.Where(t => t.TipoTarifa == "Especial").ToList();

            ViewBag.TarifasNormais = tarifasNormais;
            ViewBag.TarifasEspeciais = tarifasEspeciais;

            return View();
        }

        [HttpGet("novo")]
        public IActionResult Novo()
        {
            return View();
        }

        [HttpPost("criar")]
        public async Task<IActionResult> Criar([FromForm] Tarifas tarifa)
        {
            tarifa.Valor = decimal.Parse(
                tarifa.Valor.ToString(),
                new CultureInfo("pt-BR")
            );

            tarifa.DataCriacao = DateTime.Now;
            _repo.Inserir(tarifa);
            return Redirect("/tarifas");
        }

        [HttpPost("{id}/apagar")]
        public async Task<IActionResult> Apagar([FromRoute] int id)
        {
            _repo.Excluir(id);
            return Redirect("/tarifas");
        }

        [HttpGet("{id}/editar")]
        public IActionResult Editar([FromRoute] int id)
        {
            var tarifa = _repo.ObterPorId(id);

            if (tarifa == null)
            {
                TempData["Erro"] = "Registro não encontrado.";
                return RedirectToAction("Index");
            }
            return View(tarifa);
        }

        [HttpPost("{id}/alterar")]
        public async Task<IActionResult> Alterar([FromRoute] int id, [FromForm] Tarifas tarifa)
        {
            tarifa.Id = id;

            tarifa.Valor = decimal.Parse(
                tarifa.Valor.ToString(),
                new CultureInfo("pt-BR")
            );

            tarifa.DataCriacao = DateTime.Now;
            _repo.Atualizar(tarifa);
            return Redirect("/tarifas");
        }

        // Método para pegar a tarifa ativa
        public Tarifas ObterTarifaAtual()
        {
            var tarifas = _cnn.Query<Tarifas>("SELECT * FROM Tarifas").ToList();
            var agora = DateTime.Now.TimeOfDay;

            var tarifaEspecial = tarifas
                .Where(t => t.TipoTarifa == "Especial" && agora >= t.HoraInicio && agora <= t.HoraFim)
                .FirstOrDefault();

            var tarifaPadrao = tarifas.FirstOrDefault(t => t.TipoTarifa == "Normal");

            return tarifaEspecial ?? tarifaPadrao;
        }
    }
}
