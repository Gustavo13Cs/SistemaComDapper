using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Estacionamento.Models;

namespace Estacionamento.Servicos
{
    public class TarifaService
    {
        private readonly IDbConnection _cnn;

        public TarifaService(IDbConnection cnn)
        {
            _cnn = cnn;
        }

        /// <summary>
        /// Retorna todas as tarifas ordenadas (Normal e depois Especiais por hora de início).
        /// </summary>
        public List<Tarifa> ObterTodas()
        {
            return _cnn.Query<Tarifa>(
                "SELECT * FROM Tarifas ORDER BY TipoTarifa DESC, HoraInicio"
            ).ToList();
        }

        /// <summary>
        /// Retorna a tarifa ativa no momento (Especial se aplicável, senão Normal).
        /// </summary>
        public Tarifa ObterTarifaAtual()
        {
            var tarifas = ObterTodas();
            var agora = DateTime.Now.TimeOfDay;

            var tarifaEspecial = tarifas
                .FirstOrDefault(t =>
                    t.TipoTarifa == "Especial" &&
                    t.HoraInicio.HasValue &&
                    t.HoraFim.HasValue &&
                    agora >= t.HoraInicio.Value &&
                    agora <= t.HoraFim.Value
                );

            var tarifaNormal = tarifas.FirstOrDefault(t => t.TipoTarifa == "Normal");

            return tarifaEspecial ?? tarifaNormal ?? new Tarifa
            {
                TipoTarifa = "Normal",
                Valor = 0,
                DataCriacao = DateTime.Now
            };
        }
    }
}
