using System.Data;
using Dapper;
using Estacionamento.DTO;
using Estacionamento.Models;
using Estacionamento.Repositorios;
using Estacionamento.Servicos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Estacionamento.Controllers
{
    [Route("/tickets")]
    public class TicketsController : Controller
    {
        private readonly IDbConnection _cnn;
        private readonly IRepositorio<Ticket> _repo;
        private readonly IRepositorio<Vaga> _repoVaga;
        private readonly IRepositorio<HistoricoVagas> _historicoRepo;
        private readonly EmailService _emailService;
        private readonly TarifaService _tarifaService;

        public TicketsController(
            IDbConnection cnn,
            EmailService emailService,
            TarifaService tarifaService)
        {
            _repo = new RepositorioDapper<Ticket>(cnn);
            _repoVaga = new RepositorioDapper<Vaga>(cnn);
            _historicoRepo = new RepositorioDapper<HistoricoVagas>(cnn);
            _emailService = emailService;
            _tarifaService = tarifaService;
            _cnn = cnn;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var sql = @"SELECT t.*, v.*, c.*, vg.* 
                        FROM tickets t
                        INNER JOIN Veiculos v ON v.id = t.veiculoId
                        INNER JOIN clientes c ON c.id = v.clienteId
                        INNER JOIN vagas vg ON vg.id = t.vagaId";

            var tickets = _cnn.Query<Ticket, Veiculo, Cliente, Vaga, Ticket>(sql, (ticket, veiculo, cliente, vaga) =>
            {
                veiculo.Cliente = cliente;
                ticket.Veiculo = veiculo;
                ticket.Vaga = vaga;
                return ticket;
            }, splitOn: "Id, Id, Id");

            ViewBag.TarifaAtual = _tarifaService.ObterTarifaAtual();

            return View(tickets);
        }

        [HttpGet("novo")]
        public IActionResult Novo()
        {
            PreencherClientes();
            PreencherVagasDisponiveis();
            return View();
        }

        [HttpPost("criar")]
        public IActionResult Criar([FromForm] TicketDTO ticketDTO)
        {
            var cliente = _cnn.QueryFirstOrDefault<Cliente>(
                "SELECT * FROM clientes WHERE id = @id", new { id = ticketDTO.Id });

            var veiculo = BuscaOuCadastrarVeiculoPorDTO(ticketDTO, cliente);

            var ticket = new Ticket
            {
                VeiculoId = veiculo.Id,
                DataEntrada = DateTime.Now,
                VagaId = ticketDTO.VagaId
            };

            _repo.Inserir(ticket);
            AlterarStatusVaga(ticket.VagaId, true);

            return Redirect("/tickets");
        }

        [HttpPost("{id}/pago")]
        public IActionResult Pago([FromRoute] int id)
        {
            var ticket = ObterTicketCompleto(id);
            if (ticket == null) return Redirect("/tickets");

            var tarifas = _tarifaService.ObterTodas();

            ticket.DataSaida = DateTime.Now;
            ticket.Valor = ticket.CalcularValor(tarifas);

            _repo.Atualizar(ticket);
            AlterarStatusVaga(ticket.VagaId, false);

            var vaga = _repoVaga.ObterPorId(ticket.VagaId);
            vaga.Ocupada = false;
            _repoVaga.Atualizar(vaga);

            RegistrarHistorico(ticket);
            EnviarComprovantePorEmail(ticket);

            TempData["Success"] = "Ticket pago com sucesso!";
            return Redirect("/tickets");
        }

        [HttpPost("{id}/apagar")]
        public IActionResult Apagar([FromRoute] int id)
        {
            var ticket = _repo.ObterPorId(id);
            AlterarStatusVaga(ticket.VagaId, false);

            var vaga = _repoVaga.ObterPorId(ticket.VagaId);
            if (vaga != null)
            {
                vaga.Ocupada = false;
                _repoVaga.Atualizar(vaga);
            }

            _repo.Excluir(id);
            return Redirect("/tickets");
        }

        #region Métodos auxiliares

        private Ticket? ObterTicketCompleto(int id)
        {
            var sql = @"SELECT t.*, v.*, c.*, vg.* 
                        FROM tickets t
                        INNER JOIN Veiculos v ON v.id = t.veiculoId
                        INNER JOIN clientes c ON c.id = v.clienteId
                        INNER JOIN vagas vg ON vg.id = t.vagaId
                        WHERE t.id = @id";

            return _cnn.Query<Ticket, Veiculo, Cliente, Vaga, Ticket>(sql, (ticket, veiculo, cliente, vaga) =>
            {
                veiculo.Cliente = cliente;
                ticket.Veiculo = veiculo;
                ticket.Vaga = vaga;
                return ticket;
            }, new { id }, splitOn: "Id, Id, Id").FirstOrDefault();
        }

        private void PreencherVagasDisponiveis()
        {
            var vagas = _cnn.Query<Vaga>("SELECT * FROM vagas WHERE Ocupada = false");
            ViewBag.Vaga = new SelectList(vagas, "Id", "CodigoLocalizacao");
        }

        private void PreencherClientes()
        {
            var clientes = _cnn.Query<Cliente>("SELECT * FROM clientes");
            ViewBag.Clientes = new SelectList(clientes, "Id", "Nome");
        }

        private Veiculo BuscaOuCadastrarVeiculoPorDTO(TicketDTO ticketDTO, Cliente cliente)
        {
            var veiculo = _cnn.QueryFirstOrDefault<Veiculo>(
                "SELECT * FROM veiculos WHERE placa = @Placa AND ClienteId = @ClienteId",
                new { ticketDTO.Placa, ClienteId = cliente.Id });

            if (veiculo != null) return veiculo;

            veiculo = new Veiculo
            {
                Placa = ticketDTO.Placa,
                Marca = ticketDTO.Marca,
                Modelo = ticketDTO.Modelo,
                ClienteId = cliente.Id
            };

            veiculo.Id = _cnn.QuerySingle<int>(
                "INSERT INTO veiculos (Placa,Marca,Modelo,ClienteId) VALUES (@Placa, @Marca, @Modelo, @ClienteId); SELECT LAST_INSERT_ID()",
                veiculo
            );

            return veiculo;
        }

        private void AlterarStatusVaga(int vagaId, bool ocupada)
        {
            _cnn.Execute("UPDATE vagas SET Ocupada = @Ocupada WHERE Id = @Id", new { Id = vagaId, Ocupada = ocupada });
        }

        private void RegistrarHistorico(Ticket ticket)
        {
            var tempoTotal = (ticket.DataSaida.Value - ticket.DataEntrada).TotalMinutes;

            var historicoDTO = new HistoricoVagasDTO
            {
                VagaId = ticket.VagaId,
                VeiculoId = ticket.VeiculoId,
                TicketId = ticket.Id,
                NumeroTicket = ticket.Id,
                DataEntrada = ticket.DataEntrada,
                DataSaida = ticket.DataSaida.Value,
                ValorCobrado = ticket.Valor ?? 0,
                TempoTotalMinutos = (int)tempoTotal
            };

            _cnn.Execute(@"
                INSERT INTO historicovagas
                (VagaId, VeiculoId, TicketId, NumeroTicket, DataEntrada, DataSaida, ValorCobrado, TempoTotalMinutos)
                VALUES
                (@VagaId, @VeiculoId, @TicketId, @NumeroTicket, @DataEntrada, @DataSaida, @ValorCobrado, @TempoTotalMinutos)", historicoDTO);
        }

        private void EnviarComprovantePorEmail(Ticket ticket)
        {
            var cliente = ticket.Veiculo.Cliente;
            string assunto = "Seu Comprovante de Pagamento do Estacionamento";

            // ----- INÍCIO DO NOVO TEMPLATE PROFISSIONAL -----
            string corpo = $@"
        <html lang=""pt-br"">
        <head>
            <meta charset=""UTF-8"">
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
            <title>Comprovante de Pagamento</title>
        </head>
        <body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
            
            <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""border-collapse: collapse; margin-top: 20px; margin-bottom: 20px; background-color: #ffffff; box-shadow: 0 4px 10px rgba(0,0,0,0.05);"">
                
                <tr>
                    <td align=""center"" style=""background-color: #2c3e50; padding: 30px;"">
                        <h1 style=""color: #ffffff; margin: 0; font-size: 28px;"">Estacionamento Digital</h1>
                    </td>
                </tr>

                <tr>
                    <td style=""padding: 40px 30px 30px 30px;"">
                        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                            <tr>
                                <td align=""center"">
                                    <span style=""font-size: 50px;"">✅</span>
                                    <h2 style=""color: #2ecc71; margin-top: 10px; margin-bottom: 20px; font-size: 24px;"">Pagamento Confirmado!</h2>
                                    <p style=""color: #555555; font-size: 16px; line-height: 1.5;"">
                                        Olá, <strong>{cliente.Nome}</strong>,
                                    </p>
                                    <p style=""color: #555555; font-size: 16px; line-height: 1.5; margin-top: 0;"">
                                        Recebemos seu pagamento com sucesso. Obrigado por utilizar nossos serviços!
                                    </p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>

                <tr>
                    <td style=""padding: 0 30px 30px 30px;"">
                        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""border-top: 2px solid #eeeeee; padding-top: 20px;"">
                            <tr>
                                <td style=""padding: 10px 0; color: #777777; font-size: 15px;"">Ticket ID:</td>
                                <td align=""right"" style=""padding: 10px 0; color: #333333; font-size: 16px; font-weight: bold;"">#{ticket.Id}</td>
                            </tr>
                            <tr>
                                <td style=""padding: 10px 0; color: #777777; font-size: 15px;"">Veículo (Placa):</td>
                                <td align=""right"" style=""padding: 10px 0; color: #333333; font-size: 16px; font-weight: bold;"">
                                    {ticket.Veiculo.Placa} </td>
                            </tr>
                            <tr>
                                <td style=""padding: 10px 0; color: #777777; font-size: 15px;"">Data de Entrada:</td>
                                <td align=""right"" style=""padding: 10px 0; color: #333333; font-size: 16px; font-weight: bold;"">
                                    {ticket.DataEntrada.ToString("G")} </td>
                            </tr>
                            <tr>
                                <td style=""padding: 10px 0; color: #777777; font-size: 15px;"">Data de Saída:</td>
                                <td align=""right"" style=""padding: 10px 0; color: #333333; font-size: 16px; font-weight: bold;"">
                                    {ticket.DataSaida.Value.ToString("G")} </td>
                            </tr>
                        </table>
                    </td>
                </tr>

                <tr>
                    <td style=""padding: 0 30px 40px 30px;"">
                        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""border-top: 2px solid #eeeeee; padding-top: 20px;"">
                            <tr>
                                <td style=""padding: 10px 0; color: #333333; font-size: 20px; font-weight: bold;"">VALOR TOTAL:</td>
                                <td align=""right"" style=""padding: 10px 0; color: #2ecc71; font-size: 24px; font-weight: bold;"">
                                    R$ {ticket.Valor.Value.ToString("F2")}
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>

                <tr>
                    <td align=""center"" style=""background-color: #f9f9f9; padding: 30px; border-top: 1px solid #eeeeee;"">
                        <p style=""color: #999999; font-size: 12px; margin: 0;"">
                            © 2025 [NOME DO SEU ESTACIONAMENTO]. Todos os direitos reservados.<br>
                            Este é um e-mail transacional automático. Por favor, não responda.
                        </p>
                    </td>
                </tr>

            </table>

        </body>
        </html>
        ";
            // ----- FIM DO NOVO TEMPLATE -----

            _emailService.Enviar(cliente.Email, assunto, corpo);
        }

        #endregion
    }
}
