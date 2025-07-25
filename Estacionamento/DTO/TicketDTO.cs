using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Estacionamento.DTO
{
    public record TicketDTO
    {
        public int Id { get; set; } = default!;
        public string Nome { get; set; }= default!;
        public string? Cpf { get; set; }
        public string Placa { get; set; }= default!;
        public string Modelo { get; set; }= default!;
        public string Marca { get; set; }= default!;
        public int VagaId { get; set; } = default!;
    }
}