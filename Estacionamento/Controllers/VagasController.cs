using System.Data;
using System.Threading.Tasks;
using Dapper;
using Estacionamento.Models;
using Estacionamento.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Estacionamento.Controllers
{
    [Route("/Vagas")]
    public class VagasController : Controller
    {
        private readonly IRepositorio<Vaga> _repo;

        public VagasController(IRepositorio<Vaga> repo)
        {
            _repo = repo;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var vagas = _repo.ObterTodos();
            return View(vagas);
        }

        [HttpGet("novo")]
        public IActionResult Novo()
        {
            return View();
        }

        [HttpPost("Criar")]
        public async Task<IActionResult> Criar([FromForm] Vaga vaga)
        {
            _repo.Inserir(vaga);
            return Redirect("/vagas");
        }

        [HttpPost("{id}/apagar")]
        public async Task<IActionResult> Apagar([FromRoute] int id)
        {
            _repo.Excluir(id);
            return Redirect("/vagas");
        }

        [HttpGet("{id}/editar")]
        public IActionResult Editar([FromRoute]int id)
        {
           var vagas = _repo.ObterPorId(id);
            return View(vagas);
        }

        [HttpPost("{id}/alterar")]
        public async Task<IActionResult> Alterar([FromRoute] int id, [FromForm] Vaga vaga)
        {
            vaga.Id = id;
            _repo.Atualizar(vaga);
            return Redirect("/Vagas");
        }

    }
}
