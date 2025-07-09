using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Estacionamento.Repositorios;

namespace Estacionamento.Models
{
    [Table("vagas")]
    public class Vaga
    {
        [IgnoreInDapper]
        public int Id { get; set; } = default!;
        public string CodigoLocalizacao { get; set; } = default!;
        public bool Ocupada { get; set; } = default!;

        // Relacionamento
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}