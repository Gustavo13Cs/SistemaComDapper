using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Estacionamento.Repositorios;

namespace Estacionamento.Models
{
    [Table("tickets")]
    public class Ticket
    {
        [IgnoreInDapper]
        public int Id { get; set; } = default;
        public DateTime DataEntrada { get; set; } = default!;
        public DateTime? DataSaida { get; set; }
        public float? Valor { get; set; } = default;

        // FKs
        public int VeiculoId { get; set; } = default!;
        [IgnoreInDapper]
        public Veiculo Veiculo { get; set; } = default!;

        public int VagaId { get; set; } = default!;
        [IgnoreInDapper]
        public Vaga Vaga { get; set; } = default!;

        public int? ValoresId { get; set; }
        [IgnoreInDapper]
        public ValorDoMinuto? ValorInfo { get; set; }


        public float ValorTotal(ValorDoMinuto valorDoMinuto, DateTime dataSaida)
        {
            var valorMinuto = valorDoMinuto.Valor / valorDoMinuto.Minutos;
            TimeSpan diferenca = dataSaida - this.DataEntrada;
            int minutos = (int)diferenca.TotalMinutes;

            return minutos * valorMinuto;
        }

        public void Pago(ValorDoMinuto valorDoMinuto)
        {
            var agora = DateTime.Now;
            this.Valor = this.ValorTotal(valorDoMinuto, agora);
            this.DataSaida = agora;
        }

        private float ObterTarifaPorMinuto(DateTime minuto, List<TarifaEspecial> tarifas, float valorPadrao)
        {
            var horaDoMinuto = minuto.TimeOfDay;

            var tarifa = tarifas.FirstOrDefault(t =>
                horaDoMinuto >= t.HoraInicio && horaDoMinuto <= t.HoraFim);

            return tarifa?.ValorPorMinuto ?? valorPadrao;
        }

        public float CalcularValor(ValorDoMinuto valorPadrao, List<TarifaEspecial> tarifasEspeciais)
        {
            if (DataSaida == null) return 0;

            int minutosTotais = (int)(DataSaida.Value - DataEntrada).TotalMinutes;
            float valorTotal = 0;

            for (int i = 0; i < minutosTotais; i++)
            {
                var minutoAtual = DataEntrada.AddMinutes(i);
                valorTotal += ObterTarifaPorMinuto(minutoAtual, tarifasEspeciais, valorPadrao.Valor);
            }

            return valorTotal;
        }


    }
}