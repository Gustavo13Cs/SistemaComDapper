using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estacionamento.Models
{
    public class Tarifas
    {
        public int Id { get; set; }
        /// <summary>
        /// Tipo da tarifa: "Normal" ou "Especial".
        /// </summary>
        public string TipoTarifa { get; set; } = string.Empty;
        /// <summary>
        /// Valor por minuto da tarifa.
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Valor { get; set; }
        /// <summary>
        /// Hora de início da tarifa (apenas para "Especial").
        /// </summary>
        public TimeSpan? HoraInicio { get; set; }
        /// <summary>
        /// Hora de fim da tarifa (apenas para "Especial").
        /// </summary>
        public TimeSpan? HoraFim { get; set; }
        /// <summary>
        /// Dia da semana aplicável (opcional).
        /// </summary>
        public int? DiaSemana { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
