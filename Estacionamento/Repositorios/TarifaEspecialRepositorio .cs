using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Estacionamento.Models;

namespace Estacionamento.Repositorios
{
    public class TarifaEspecialRepositorio : IRepositorio<TarifaEspecial>
    {
        private readonly IDbConnection _connection;

        public TarifaEspecialRepositorio(IDbConnection connection)
        {
            _connection = connection;
        }

        public void Atualizar(TarifaEspecial entidade)
        {
            throw new NotImplementedException();
        }

        public void Excluir(int id)
        {
            throw new NotImplementedException();
        }

        public void Inserir(TarifaEspecial entidade)
        {
            throw new NotImplementedException();
        }

        public TarifaEspecial ObterPorId(int id)
        {
            throw new NotImplementedException();
        }

        public List<TarifaEspecial> ObterTodos()
        {
            return _connection.Query<TarifaEspecial>("SELECT * FROM TarifasEspeciais").ToList();
        }

        IEnumerable<TarifaEspecial> IRepositorio<TarifaEspecial>.ObterTodos()
        {
            return ObterTodos();
        }
    }
}