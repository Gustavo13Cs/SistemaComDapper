namespace Estacionamento.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; }
        public string Email { get; set; }
        public string SenhaHash { get; set; }
        public string Perfil { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}
