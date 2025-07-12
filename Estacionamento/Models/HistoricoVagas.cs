using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Estacionamento.Models
{
    public class HistoricoVagas
    {
        public int Id { get; set; }
        public int VagaId { get; set; }
        public int VeiculoId { get; set; }
        public int? TicketId { get; set; }
        public DateTime DataEntrada { get; set; }
        public DateTime DataSaida { get; set; }
        public float ValorCobrado { get; set; }
        public int TempoTotalMinutos { get; set; }
        public int NumeroTicket { get; set; }

        // Navegação (opcional, se quiser exibir nomes na view futuramente)
        public Vaga? Vaga { get; set; }
        public Veiculo? Veiculo { get; set; }
        public Ticket? Ticket { get; set; }
    }
}