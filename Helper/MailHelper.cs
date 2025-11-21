using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace WebApiVotacionElectronica.Helper
{
    public interface IMailHelper
    {
        Task<bool> EnviarCorreoAsync(string destinatario, string nombre, string asunto, string link);
    }

    public class MailHelper : IMailHelper
    {
        private readonly IConfiguration _config;

        public MailHelper(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> EnviarCorreoAsync(
            string destinatario,
            string nombre,
            string asunto,
            string link)
        {
            try
            {
                string pass = _config["MailConfiguration:PassCorreoSistema"];
                string email = _config["MailConfiguration:CorreoSistema"];
                string smtp = _config["MailConfiguration:SmtpCorreoSistema"];
                int puerto = int.Parse(_config["MailConfiguration:PuertoCorreoSistema"]);
                bool isHtml = _config["MailConfiguration:IsBodyHtml"]?.Equals("Si", StringComparison.OrdinalIgnoreCase) == true;

                // Plantilla HTML
                string ruta = Path.Combine(AppContext.BaseDirectory, "Templates", "Correo.html");
                string html = await File.ReadAllTextAsync(ruta);

                html = html.Replace("{{Nombre completo}}", nombre)
                           .Replace("{{Nombre de la votación}}", asunto)
                           .Replace("{{LINK}}", link);

                var msg = new MimeMessage();
                msg.From.Add(new MailboxAddress("Sistema de Votación", email));
                msg.To.Add(new MailboxAddress(destinatario, destinatario));
                msg.Subject = "Invitación a participar en la votación: " + asunto;

                var body = new BodyBuilder
                {
                    HtmlBody = isHtml ? html : null,
                    TextBody = !isHtml ? html : null
                };

                msg.Body = body.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(smtp, puerto, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(email, pass);

                await client.SendAsync(msg);
                await client.DisconnectAsync(true);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}