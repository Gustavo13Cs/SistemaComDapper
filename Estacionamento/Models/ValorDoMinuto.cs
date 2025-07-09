using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Estacionamento.Models
{
    [Table("valores")]
    public class ValorDoMinuto
    {
        public int Id { get; set; } = default!;
        public int Minutos { get; set; } = default!;
        public decimal Valor { get; set; } = default!;

        // Relacionamento
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}