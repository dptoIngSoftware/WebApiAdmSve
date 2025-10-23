using System.Net.Mail;

namespace WebApiVotacionElectronica.Helper
{
    public static class MailHelper
    {
        public static async Task EnviarCorreoPersonalizadoAsync(
            IConfiguration configuration,
            string destinatario,
            string nombre,
            string asunto,
            string contenidoPersonal)
        {
            string nombreSistema = configuration["MySettings:NombreSistema"];
            string passOrigen = configuration["MailConfiguration:PassCorreoSistema"];
            string emailSend = configuration["MailConfiguration:CorreoSistema"];
            string smtpSistema = configuration["MailConfiguration:SmtpCorreoSistema"];
            string puerto = configuration["MailConfiguration:PuertoCorreoSistema"];
            string bodyHtml = configuration["MailConfiguration:IsBodyHtml"];

            bool bodyHtmls = bodyHtml != null && bodyHtml.Equals("Si", StringComparison.OrdinalIgnoreCase);

            string cuerpo = $"<b>Estimado/a {nombre},</b><br/><br/>{contenidoPersonal}<br/><br/>" +
                            $"Saludos,<br/><b>{nombreSistema}</b><br/><br/>" +
                            $"<i>Este correo fue generado automáticamente, por favor no responda.</i>";

            using (MailMessage mensaje = new MailMessage())
            {
                mensaje.From = new MailAddress(emailSend);
                mensaje.Subject = asunto;
                mensaje.Body = cuerpo;
                mensaje.IsBodyHtml = bodyHtmls;
                mensaje.Priority = MailPriority.Normal;
                mensaje.To.Add(destinatario);

                using (SmtpClient smtp = new SmtpClient(smtpSistema))
                {
                    smtp.Port = int.Parse(puerto);
                    smtp.Credentials = new System.Net.NetworkCredential(emailSend, passOrigen);
                    smtp.EnableSsl = true;

                    await smtp.SendMailAsync(mensaje);
                }
            }
        }
    }
}
