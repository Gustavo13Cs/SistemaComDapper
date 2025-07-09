using System.Data;
using System.Threading.Tasks;
using Dapper;
using Estacionamento.Models;
using Estacionamento.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Estacionamento.Controllers
{
    [Route("/veiculos")]
    public class VeiculosController : Controller
    {
        private readonly IRepositorio<Veiculo> _repo;

        public VeiculosController(IRepositorio<Veiculo> repo)
        {
            _repo = repo;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var valores = _repo.ObterTodos();
            return View(valores);
        }

        [HttpGet("novo")]
        public IActionResult Novo()
        {
            return View();
        }

        [HttpPost("Criar")]
        public async Task<IActionResult> Criar([FromForm] Veiculo veiculo)
        {
            _repo.Inserir(veiculo);
            return Redirect("/veiculos");
        }

        [HttpPost("{id}/apagar")]
        public async Task<IActionResult> Apagar([FromRoute] int id)
        {
            _repo.Excluir(id);
            return Redirect("/veiculos");
        }

        [HttpGet("{id}/editar")]
        public IActionResult Editar([FromRoute]int id)
        {
           var valor = _repo.ObterPorId(id);
            return View(valor);
        }

        [HttpPost("{id}/alterar")]
        public async Task<IActionResult> Alterar([FromRoute] int id, [FromForm] Veiculo veiculo)
        {
            veiculo.Id = id;
            _repo.Atualizar(veiculo);
            return Redirect("/veiculos");
        }

    }
}
