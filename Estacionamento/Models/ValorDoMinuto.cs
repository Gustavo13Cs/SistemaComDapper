using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Estacionamento.Models
{
    public class ValorDoMinuto
    {
        public int Id { get; set; }= default!;
        public int Minutos { get; set; }= default!;
        public float ValorCobrado { get; set; }= default!;

        // Relacionamento
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}