using System.Data;
using Dapper;
using BCrypt.Net;
using Estacionamento.Models;

namespace Estacionamento.Services
{
    public class AuthService
    {
        private readonly IDbConnection _connection;

        public AuthService(IDbConnection connection)
        {
            _connection = connection;
        }

        // Registrar novo usuário
        public bool Registrar(Usuario usuario)
        {
            try
            {
                // 🔐 Gera o hash seguro da senha
                string senhaHash = BCrypt.Net.BCrypt.HashPassword(usuario.SenhaHash);

                string sql = @"INSERT INTO Usuarios (NomeCompleto, Email, SenhaHash, Perfil)
                               VALUES (@NomeCompleto, @Email, @SenhaHash, @Perfil)";

                int linhas = _connection.Execute(sql, new
                {
                    usuario.NomeCompleto,
                    usuario.Email,
                    SenhaHash = senhaHash,
                    usuario.Perfil
                });

                return linhas > 0;
            }
            catch
            {
                return false;
            }
        }

        // Verifica login
        public Usuario? Login(string email, string senha)
        {
            string sql = "SELECT * FROM Usuarios WHERE Email = @Email";
            var usuario = _connection.QueryFirstOrDefault<Usuario>(sql, new { Email = email });

            // Se achou o usuário, compara a senha criptografada
            if (usuario != null && BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash))
                return usuario;

            return null;
        }
    }
}
