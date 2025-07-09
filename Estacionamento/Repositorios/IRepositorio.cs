using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Estacionamento.Repositorios
{
    public interface IRepositorio<T>
    {
        IEnumerable<T> ObterTodos();
        T ObterPorId(int id);
        void Inserir(T entidade);
        void Atualizar(T entidade);
        void Excluir(int id);
    }
}