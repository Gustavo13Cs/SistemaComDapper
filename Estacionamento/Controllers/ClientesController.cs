using System.Data;
using System.Threading.Tasks;
using Dapper;
using Estacionamento.Models;
using Estacionamento.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Estacionamento.Controllers
{
    [Route("/Clientes")]
    public class clientesController : Controller
    {
        private readonly IRepositorio<Cliente> _repo;

        public clientesController(IRepositorio<Cliente> repo)
        {
            _repo = repo;
        }

        [HttpGet("/clientes/index")]
        public IActionResult Index([FromQuery] string? busca)
        {
            var valores = _repo.ObterTodos();

            if (!string.IsNullOrEmpty(busca))
            {
                busca = busca.ToLower().Trim();
                valores = valores.Where(c =>
                    c.Nome.ToLower().Contains(busca) ||
                    c.Cpf.ToLower().Contains(busca)).ToList();
            }

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
            return RedirectToAction("Index");

        }

        [HttpPost("{id}/apagar")]
        public async Task<IActionResult> Apagar([FromRoute] int id)
        {
            _repo.Excluir(id);
            return RedirectToAction("Index");
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
            return RedirectToAction("Index");

        }

    }
}
