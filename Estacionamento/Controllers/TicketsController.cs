using System.Data;
using System.Threading.Tasks;
using Dapper;
using Estacionamento.DTO;
using Estacionamento.Models;
using Estacionamento.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Estacionamento.Controllers
{
    [Route("/Tickets")]
    public class TicketsController : Controller
    {
        private readonly IDbConnection _cnn;
        private readonly IRepositorio<Ticket> _repo;
        private readonly IRepositorio<Vaga> _repoVaga;

        public TicketsController(IDbConnection cnn)
        {
            _repo = new RepositorioDapper<Ticket>(cnn);
            _repoVaga = new RepositorioDapper<Vaga>(cnn);
            _cnn = cnn;
        }
        [HttpGet("")]
        public IActionResult Index()
        {
            var sql = "SELECT t.*, v.*, c.*, vg.* from tickets t inner join Veiculos v on v.id = t.veiculoId INNER JOIN clientes c ON c.id = v.clienteId INNER JOIN vagas vg ON vg.id = t.vagaId";
            var tickets = _cnn.Query<Ticket, Veiculo, Cliente, Vaga, Ticket>(sql, (ticket, veiculo, cliente, Vaga) =>
            {
                veiculo.Cliente = cliente;
                ticket.Veiculo = veiculo;
                ticket.Vaga = Vaga;
                return ticket;
            }, splitOn: "Id, Id, Id");

            return View(tickets);
        }

        [HttpGet("novo")]
        public IActionResult Novo()
        {
            preencheVagasViewVag();
            return View();
        }

        [HttpPost("Criar")]
        public async Task<IActionResult> Criar([FromForm] TicketDTO ticketDTO)
        {
            Cliente cliente = BuscaOuCadastrarClientePorDTO(ticketDTO);
            Veiculo veiculo = BuscaOuCadastrarVeiculoPorDTO(ticketDTO, cliente);


            var ticket = new Ticket();
            ticket.VeiculoId = veiculo.Id;
            ticket.DataEntrada = DateTime.Now;
            ticket.VagaId = ticketDTO.VagaId;

            _repo.Inserir(ticket);
            alteraStatusVaga(ticket.VagaId, true);

            return Redirect("/tickets");
        }

        [HttpPost("{id}/apagar")]
        public async Task<IActionResult> Apagar([FromRoute] int id)
        {
            var ticket = _repo.ObterPorId(id);
            alteraStatusVaga(ticket.VagaId, false);
            _repo.Excluir(id);
            return Redirect("/tickets");
        }

        private void preencheVagasViewVag()
        {
            var sql = "SELECT * FROM vagas where Ocupada = false";
            var vagas = _cnn.Query<Vaga>(sql);
            ViewBag.Vaga = new SelectList(vagas, "Id", "CodigoLocalizacao");
        }

        private Cliente BuscaOuCadastrarClientePorDTO(TicketDTO ticketDTO)
        {
            Cliente? cliente = null;
            if (!string.IsNullOrEmpty(ticketDTO.Cpf))
            {
                var query = "SELECT * FROM clientes where Cpf = @Cpf";
                cliente = _cnn.QueryFirstOrDefault<Cliente>(query, new Cliente { Cpf = ticketDTO.Cpf });
            }

            if (cliente != null) return cliente;
            cliente = new Cliente();
            cliente.Nome = ticketDTO.Nome;
            cliente.Cpf = ticketDTO.Cpf;

            string sql = $"INSERT INTO clientes (Nome,CPF) VALUES (@Nome, @Cpf); SELECT LAST_INSERT_ID();";
            cliente.Id = _cnn.QuerySingle<int>(sql, cliente);

            return cliente;
        }

        private Veiculo BuscaOuCadastrarVeiculoPorDTO(TicketDTO ticketDTO, Cliente cliente)
        {
            Veiculo? veiculo = null;
            if (!string.IsNullOrEmpty(ticketDTO.Placa))
            {
                var query = "SELECT * FROM veiculos where placa = @Placa and ClienteId = @ClienteId";
                veiculo = _cnn.QueryFirstOrDefault<Veiculo>(query, new Veiculo { Placa = ticketDTO.Placa, ClienteId = cliente.Id });
            }

            if (veiculo != null) return veiculo;
            veiculo = new Veiculo();
            veiculo.Placa = ticketDTO.Placa;
            veiculo.Marca = ticketDTO.Marca;
            veiculo.Modelo = ticketDTO.Modelo;
            veiculo.ClienteId = cliente.Id;

            var sql = $"INSERT INTO veiculos (Placa,Marca,Modelo,ClienteId) VALUES (@Placa, @Marca, @Modelo, @ClienteId); SELECT LAST_INSERT_ID()";
            veiculo.Id = _cnn.QuerySingle<int>(sql, veiculo);

            return veiculo;
        }

        private void alteraStatusVaga(int VagaId , bool ocupada)
        {
            var sql = $"UPDATE vagas SET ocupada = true where id = @Id";
            _cnn.Execute(sql, new Vaga { Id = VagaId , Ocupada = ocupada});
        }


    }
}
