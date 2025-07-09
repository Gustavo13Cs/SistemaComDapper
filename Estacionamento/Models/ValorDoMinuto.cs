using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Estacionamento.Repositorios;

namespace Estacionamento.Models
{
    [Table("valores")]
    public class ValorDoMinuto
    {
        [IgnoreInDapper]
        public int Id { get; set; } = default!;

        [Column("minutos")]
        public int Minutos { get; set; } = default!;
        public decimal Valor { get; set; } = default!;

        // Relacionamento
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}