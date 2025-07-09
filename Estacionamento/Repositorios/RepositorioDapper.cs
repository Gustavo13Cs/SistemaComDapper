using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;

namespace Estacionamento.Repositorios
{
    public class RepositorioDapper<T> : IRepositorio<T>
    {
        private readonly IDbConnection _conexao;
        private readonly string _nomeTabela;

        public RepositorioDapper(IDbConnection conexao)
        {
            _conexao = conexao;
            _nomeTabela = ObterNomeTabela();
        }

        private string ObterNomeTabela()
        {
            var tipo = typeof(T);
            var atributoTabela = tipo.GetCustomAttribute<TableAttribute>();
            if (atributoTabela != null)
            {
                return atributoTabela.Name;
            }
            return tipo.Name;
        }

        public void Atualizar(T entidade)
        {
            var campos = ObterCamposUpdate(entidade);
            var sql = $"UPDATE {_nomeTabela} SET {campos} WHERE Id = @id";
            _conexao.Execute(sql, entidade);
        }

        private string ObterCamposUpdate(T entidade)
        {
            var tipo = typeof(T);
            var propriedades = tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name != "Id" && !Attribute.IsDefined(p, typeof(IgnoreInDapperAttribute)));

            var nomesCampos = propriedades.Select(p => $"{p.Name} = @{p.Name}");
            return string.Join(", ", nomesCampos);
        }


        public void Excluir(int id)
        {
            var sql = $"DELETE FROM {_nomeTabela} WHERE Id = @Id";
            _conexao.Execute(sql, new { Id = id });
        }

        public void Inserir(T entidade)
        {
            var campos = ObterCamposInsert(entidade);
            var valores = ObterValoresInsert(entidade);
            var sql = $"INSERT INTO {_nomeTabela} ({campos}) VALUES ({valores})";
            _conexao.Execute(sql, entidade);
        }

        private object ObterValoresInsert(T? entidade)
        {
            var tipo = typeof(T);
            var propriedades = tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !Attribute.IsDefined(p, typeof(IgnoreInDapperAttribute))); // <- ISSO AQUI

            var nomesCampos = propriedades.Select(p => "@" + p.Name);
            return string.Join(", ", nomesCampos);
        }


        private object ObterCamposInsert(T? entidade)
        {
            var tipo = typeof(T);
            var propriedades = tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => !Attribute.IsDefined(p, typeof(IgnoreInDapperAttribute)));
            var nomesCampos = propriedades.Select(p =>
            {
                var colunaName = p.GetCustomAttribute<ColumnAttribute>()?.Name;
                return colunaName ?? p.Name;
            });
            
            return string.Join(", ", nomesCampos);
        }

        public T ObterPorId(int id)
        {
            var sql = $"SELECT * FROM {_nomeTabela} WHERE Id = @id";
            return _conexao.QueryFirstOrDefault<T>(sql, new { Id = id });
        }

        public IEnumerable<T> ObterTodos()
        {
            var sql = $"Select * From {_nomeTabela}";
            return _conexao.Query<T>(sql);
        }
    }
}