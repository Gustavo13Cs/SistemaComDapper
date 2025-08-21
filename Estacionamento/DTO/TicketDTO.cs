using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Estacionamento.DTO
{
    public record TicketDTO
    {
        public int Id { get; set; } = default!;
        [Required(ErrorMessage = "O campo Nome é obrigatório")]
        public string Nome { get; set; } = default!;
        [Required(ErrorMessage = "O campo Cpf é obrigatório")]
        public string? Cpf { get; set; }
        [Required(ErrorMessage = "O campo Placa é obrigatório")]
        public string Placa { get; set; } = default!;
        [Required(ErrorMessage = "O campo Modelo é obrigatório")]
        public string Modelo { get; set; } = default!;
        [Required(ErrorMessage = "O campo Marca é obrigatório")]
        public string Marca { get; set; } = default!;
        [Required(ErrorMessage = "O campo Vaga é obrigatório")]
        public int VagaId { get; set; } = default!;
    }
}