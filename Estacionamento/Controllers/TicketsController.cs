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

        private readonly IRepositorio<HistoricoVagas> _historicoRepo;

        public TicketsController(IDbConnection cnn)
        {
            _repo = new RepositorioDapper<Ticket>(cnn);
            _repoVaga = new RepositorioDapper<Vaga>(cnn);
            _historicoRepo = new RepositorioDapper<HistoricoVagas>(cnn);
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

            ViewBag.ValorDoMinuto = _cnn.QueryFirstOrDefault<ValorDoMinuto>("Select * From valores order by id desc limit 1");

            return View(tickets);
        }

        [HttpGet("novo")]
        public IActionResult Novo()
        {
            PreencheClientesViewBag();
            preencheVagasViewVag();
            return View();
        }

        [HttpPost("Criar")]
        public async Task<IActionResult> Criar([FromForm] TicketDTO ticketDTO)
        {
            var cliente = _cnn.QueryFirstOrDefault<Cliente>("SELECT * FROM clientes WHERE id = @id", new { id = ticketDTO.Id });
            Veiculo veiculo = BuscaOuCadastrarVeiculoPorDTO(ticketDTO, cliente);


            var ticket = new Ticket();
            ticket.VeiculoId = veiculo.Id;
            ticket.DataEntrada = DateTime.Now;
            ticket.VagaId = ticketDTO.VagaId;

            _repo.Inserir(ticket);
            alteraStatusVaga(ticket.VagaId, true);

            return Redirect("/tickets");
        }

        [HttpPost("{id}/pago")]
        public async Task<IActionResult> Pago([FromRoute] int id)
        {
            var sql = "SELECT t.*, v.*, c.*, vg.* from tickets t inner join Veiculos v on v.id = t.veiculoId INNER JOIN clientes c ON c.id = v.clienteId INNER JOIN vagas vg ON vg.id = t.vagaId where t.id = @id";
            Ticket? ticket = _cnn.Query<Ticket, Veiculo, Cliente, Vaga, Ticket>(sql, (ticket, veiculo, cliente, Vaga) =>
            {
                veiculo.Cliente = cliente;
                ticket.Veiculo = veiculo;
                ticket.Vaga = Vaga;
                return ticket;
            }, new { id = id }, splitOn: "Id, Id, Id").FirstOrDefault();

            if (ticket != null)
            {
                var valorDoMinuto = _cnn.QueryFirstOrDefault<ValorDoMinuto>("Select * From valores order by id desc limit 1");
                ticket.Pago(valorDoMinuto);
                _repo.Atualizar(ticket);
                alteraStatusVaga(ticket.VagaId, false);
            }
            // Atualiza a vaga relacionada
            var vaga = _repoVaga.ObterPorId(ticket.VagaId);
            vaga.Ocupada = false;
            _repoVaga.Atualizar(vaga);
            RegistrarHistorico(ticket);

            TempData["Success"] = "Ticket pago com sucesso!";
            return Redirect("/tickets");
        }

        [HttpPost("{id}/apagar")]
        public async Task<IActionResult> Apagar([FromRoute] int id)
        {
            var ticket = _repo.ObterPorId(id);
            alteraStatusVaga(ticket.VagaId, false);

            var vaga = _repoVaga.ObterPorId(ticket.VagaId);
            if (vaga != null)
            {
                vaga.Ocupada = false;
                _repoVaga.Atualizar(vaga);
            }
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

        private void alteraStatusVaga(int VagaId, bool ocupada)
        {
            const string sql = "UPDATE vagas SET Ocupada = @Ocupada WHERE Id = @Id";
            _cnn.Execute(sql, new { Id = VagaId, Ocupada = ocupada });
        }

        private void PreencheClientesViewBag()
        {
            var sql = "SELECT * FROM clientes";
            var clientes = _cnn.Query<Cliente>(sql);
            ViewBag.Clientes = new SelectList(clientes, "Id", "Nome");
        }

        private void RegistrarHistorico(Ticket ticket)
        {
            var tempoTotal = (ticket.DataSaida.Value - ticket.DataEntrada).TotalMinutes;

            // Cria DTO com os dados simples
            var historicoDTO = new HistoricoVagasDTO
            {
                VagaId = ticket.VagaId,
                VeiculoId = ticket.VeiculoId,
                TicketId = ticket.Id,
                NumeroTicket = ticket.Id, // <-- AQUI!
                DataEntrada = ticket.DataEntrada,
                DataSaida = ticket.DataSaida.Value,
                ValorCobrado = ticket.Valor ?? 0,
                TempoTotalMinutos = (int)tempoTotal
            };

            // Insere no banco via Dapper, evitando erro com propriedades complexas
            const string sql = @"
            INSERT INTO historicovagas
            (VagaId, VeiculoId, TicketId, NumeroTicket, DataEntrada, DataSaida, ValorCobrado, TempoTotalMinutos)
            VALUES
            (@VagaId, @VeiculoId, @TicketId, @NumeroTicket, @DataEntrada, @DataSaida, @ValorCobrado, @TempoTotalMinutos)";


            _cnn.Execute(sql, historicoDTO);
        }

    }
}
