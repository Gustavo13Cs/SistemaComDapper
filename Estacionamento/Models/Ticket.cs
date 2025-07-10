using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Estacionamento.Repositorios;

namespace Estacionamento.Models
{
    [Table("tickets")]
    public class Ticket
    {
        [IgnoreInDapper]
        public int Id { get; set; } = default;
        public DateTime DataEntrada { get; set; } = default!;
        public DateTime? DataSaida { get; set; }
        public decimal? Valor { get; set; } = default;

        // FKs
        public int VeiculoId { get; set; } = default!;
        [IgnoreInDapper]
        public Veiculo Veiculo { get; set; } =default!;

        public int VagaId { get; set; } = default!;
        [IgnoreInDapper]
        public Vaga Vaga { get; set; } = default!;

        public int? ValoresId { get; set; }
        [IgnoreInDapper]
        public ValorDoMinuto? ValorInfo { get; set; }
    }
}