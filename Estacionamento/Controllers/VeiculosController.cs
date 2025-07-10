using System.Data;
using System.Threading.Tasks;
using Dapper;
using Estacionamento.Models;
using Estacionamento.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Estacionamento.Controllers
{
    [Route("/Veiculos")]
    public class VeiculosController : Controller
    {
        private readonly IDbConnection _cnn;
        private readonly IRepositorio<Veiculo> _repo;

        public VeiculosController(IDbConnection cnn)
        {
            _repo = new RepositorioDapper<Veiculo>(cnn);
            _cnn = cnn;
        }

        public IActionResult Index()
        {
            var sql = "SELECT v.*, c.* from veiculos v inner join clientes c on c.id = v.clienteId";
            var veiculos = _cnn.Query<Veiculo, Cliente, Veiculo>(sql , (veiculo, cliente) =>
            {
                veiculo.Cliente = cliente;
                return veiculo;
            }, splitOn:"Id");


            return View(veiculos);
        }

        [HttpGet("novo")]
        public IActionResult Novo()
        {
            var sql = "SELECT * FROM clientes";
            var clientes = _cnn.Query<Cliente>(sql);
            ViewBag.Clientes = new SelectList(clientes, "Id", "Nome");
            return View();
        }

        [HttpPost("Criar")]
        public async Task<IActionResult> Criar([FromForm] Veiculo veiculo)
        {
            if (veiculo.ClienteId == 0)
            {
                ModelState.AddModelError("ClienteId", "Selecione um cliente.");
                var sql = "SELECT * FROM clientes";
                var clientes = _cnn.Query<Cliente>(sql);
                ViewBag.Clientes = new SelectList(clientes, "Id", "Nome");
                return View(veiculo);
            }
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
