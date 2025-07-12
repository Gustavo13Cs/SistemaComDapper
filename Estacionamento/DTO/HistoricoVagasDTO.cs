using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Estacionamento.DTO
{
    public class HistoricoVagasDTO
    {
        public int VagaId { get; set; }
        public int VeiculoId { get; set; }
        public int? TicketId { get; set; }
        public DateTime DataEntrada { get; set; }
        public DateTime DataSaida { get; set; }
        public float ValorCobrado { get; set; }
        public int TempoTotalMinutos { get; set; }
        public int NumeroTicket { get; set; }
    }
}