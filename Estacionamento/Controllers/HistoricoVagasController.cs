using System.Data;
using Dapper;
using Estacionamento.Models;
using Estacionamento.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Estacionamento.Controllers
{
    [Route("HistoricoVagas")]
    public class HistoricoVagasController : Controller
    {
        private readonly IDbConnection _cnn;
        private readonly IRepositorio<HistoricoVagas> _repo;

        public HistoricoVagasController(IRepositorio<HistoricoVagas> repo, IDbConnection cnn)
        {
            _repo = repo;
            _cnn = cnn;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            const string sql = @"
                SELECT h.*, v.*, c.* 
                FROM historicovagas h
                JOIN veiculos v ON v.id = h.VeiculoId
                JOIN clientes c ON c.id = v.ClienteId";

            var registros = _cnn.Query<HistoricoVagas, Veiculo, Cliente, HistoricoVagas>(
                sql,
                (historico, veiculo, cliente) =>
                {
                    veiculo.Cliente = cliente;
                    historico.Veiculo = veiculo;
                    return historico;
                },
                splitOn: "Id,Id"
            );

            return View(registros);
        }

    }
}
