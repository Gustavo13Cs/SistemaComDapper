using System.Data;
using System.Threading.Tasks;
using Dapper;
using Estacionamento.Models;
using Estacionamento.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Estacionamento.Controllers
{
    [Route("/clientes")]
    public class clientesController : Controller
    {
        private readonly IRepositorio<Cliente> _repo;

        public clientesController(IRepositorio<Cliente> repo)
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
        public async Task<IActionResult> Criar([FromForm] Cliente cliente)
        {
            _repo.Inserir(cliente);
            return Redirect("/clientes");
        }

        [HttpPost("{id}/apagar")]
        public async Task<IActionResult> Apagar([FromRoute] int id)
        {
            _repo.Excluir(id);
            return Redirect("/clientes");
        }

        [HttpGet("{id}/editar")]
        public IActionResult Editar([FromRoute]int id)
        {
           var valor = _repo.ObterPorId(id);
            return View(valor);
        }

        [HttpPost("{id}/alterar")]
        public async Task<IActionResult> Alterar([FromRoute] int id, [FromForm] Cliente cliente)
        {
            cliente.Id = id;
            _repo.Atualizar(cliente);
            return Redirect("/clientes");
        }

    }
}
