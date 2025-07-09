using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Estacionamento.Models
{
    [Table("veiculos")]
    public class Veiculo
    {
        public int Id { get; set; }
        public string Placa { get; set; } = default!;
        public string Modelo { get; set; } = default!;
        public string Marca { get; set; } = default!;

        // FK
        public int ClienteId { get; set; } = default!;
        public Cliente Cliente { get; set; } = null!;

        // Relacionamento
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}