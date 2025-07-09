using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Estacionamento.Models
{
    public class TicketComCliente
    {
        public Ticket ticket { get; set; } = default!;
        public string NomeCLiente { get; set; }= default!;
    }
}