using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Estacionamento.Repositorios;

namespace Estacionamento.Models
{
    [Table("clientes")]
    public class Cliente
    {
        [IgnoreInDapper]
        public int Id { get; set; }
        [Required(ErrorMessage = "O campo Nome é obrigatório")]
        public string Nome { get; set; } = string.Empty;
        [Required(ErrorMessage = "O campo CPF é obrigatório")]
        public string Cpf { get; set; } = string.Empty;
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        [Required(ErrorMessage = "O campo Email é obrigatório")]
        public string Email { get; set; } = string.Empty;
        [IgnoreInDapper]
        // Relacionamento
        public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    }
}