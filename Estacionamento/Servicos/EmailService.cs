using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Estacionamento.Servicos
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void Enviar(string para, string assunto, string corpo)
        {
            var smtp = _config.GetSection("Smtp");
            var client = new SmtpClient(smtp["Servidor"], int.Parse(smtp["Porta"]))
            {
                Credentials = new NetworkCredential(smtp["EmailRemetente"], smtp["Senha"]),
                EnableSsl = true
            };

            var remetente = smtp["EmailRemetente"];
            var copia = smtp["EmailReceberCopias"];

            var mail = new MailMessage(remetente, para, assunto, corpo);
            mail.IsBodyHtml = true;

            if (!string.IsNullOrWhiteSpace(copia))
                mail.CC.Add(copia);

            client.Send(mail);
        }
    }
}
