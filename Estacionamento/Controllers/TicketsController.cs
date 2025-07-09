using System.Data;
using System.Threading.Tasks;
using Dapper;
using Estacionamento.Models;
using Estacionamento.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Estacionamento.Controllers
{
    [Route("/Tickets")]
    public class TicketsController : Controller
    {
        private readonly IRepositorio<Ticket> _repo;

        public TicketsController(IRepositorio<Ticket> repo)
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
        public async Task<IActionResult> Criar([FromForm] Ticket ticket)
        {
            _repo.Inserir(ticket);
            return Redirect("/ticket");
        }

        [HttpPost("{id}/apagar")]
        public async Task<IActionResult> Apagar([FromRoute] int id)
        {
            _repo.Excluir(id);
            return Redirect("/ticket");
        }

        [HttpGet("{id}/editar")]
        public IActionResult Editar([FromRoute]int id)
        {
           var valor = _repo.ObterPorId(id);
            return View(valor);
        }

        [HttpPost("{id}/alterar")]
        public async Task<IActionResult> Alterar([FromRoute] int id, [FromForm] Ticket ticket)
        {
            ticket.Id = id;
            _repo.Atualizar(ticket);
            return Redirect("/ticket");
        }

    }
}
