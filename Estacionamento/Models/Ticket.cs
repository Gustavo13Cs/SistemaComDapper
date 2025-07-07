using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Estacionamento.Models
{
    public class Ticket
    {
        public int Id { get; set; } = default;
        public DateTime DataEntrada { get; set; } = default!;
        public DateTime? DataSaida { get; set; }
        public float? Valor { get; set; } = default;

        // FKs
        public int VeiculoId { get; set; } = default!;
        public Veiculo Veiculo { get; set; } = null!;

        public int VagaId { get; set; }= default!;
        public Vaga Vaga { get; set; } = null!;

        public int? ValoresId { get; set; }
        public ValorDoMinuto? ValorInfo { get; set; }
    }
}