using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Estacionamento.Repositorios;

namespace Estacionamento.Models
{
    [Table("tickets")]
    public class Ticket
    {
        [IgnoreInDapper]
        public int Id { get; set; }
        public DateTime DataEntrada { get; set; }
        public DateTime? DataSaida { get; set; }
        public float? Valor { get; set; }

        // FKs
        public int VeiculoId { get; set; }
        [IgnoreInDapper]
        public Veiculo Veiculo { get; set; }

        public int VagaId { get; set; }
        [IgnoreInDapper]
        public Vaga Vaga { get; set; }

        /// <summary>
        /// Calcula o valor total usando lista de tarifas unificada.
        /// "Normal" é o valor base e "Especial" substitui se o minuto cair no intervalo.
        /// </summary>
        public float CalcularValor(List<Tarifas> tarifas)
        {
            if (DataSaida == null) return 0;

            int minutosTotais = (int)(DataSaida.Value - DataEntrada).TotalMinutes;
            float valorTotal = 0;

            // Tarifa normal como valor padrão
            var tarifaNormal = tarifas.FirstOrDefault(t => t.TipoTarifa == "Normal");
            var valorPadrao = tarifaNormal != null ? (float)tarifaNormal.Valor : 0;

            // Lista de tarifas especiais
            var tarifasEspeciais = tarifas
                .Where(t => t.TipoTarifa == "Especial")
                .ToList();

            for (int i = 0; i < minutosTotais; i++)
            {
                var minutoAtual = DataEntrada.AddMinutes(i);
                var horaDoMinuto = minutoAtual.TimeOfDay;

                var tarifaEspecial = tarifasEspeciais
                    .FirstOrDefault(t =>
                        t.HoraInicio.HasValue &&
                        t.HoraFim.HasValue &&
                        horaDoMinuto >= t.HoraInicio.Value &&
                        horaDoMinuto <= t.HoraFim.Value
                    );

                valorTotal += tarifaEspecial != null ? (float)tarifaEspecial.Valor : valorPadrao;
            }

            return valorTotal;
        }
    }
}
