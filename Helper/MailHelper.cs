using System.Net.Mail;

namespace WebApiVotacionElectronica.Helper
{
    public static class MailHelper
    {
        public static async Task<bool> EnviarCorreoPersonalizadoAsync(
            IConfiguration configuration,
            string destinatario,
            string nombre,
            string asunto,
            string link)
        {
            try
            {
                string passOrigen = configuration["MailConfiguration:PassCorreoSistema"];
                string emailSend = configuration["MailConfiguration:CorreoSistema"];
                string smtpSistema = configuration["MailConfiguration:SmtpCorreoSistema"];
                string puerto = configuration["MailConfiguration:PuertoCorreoSistema"];
                string bodyHtml = configuration["MailConfiguration:IsBodyHtml"];

                bool bodyHtmls = bodyHtml != null && bodyHtml.Equals("Si", StringComparison.OrdinalIgnoreCase);

                string rutaPlantilla = Path.Combine(AppContext.BaseDirectory, "Templates", "Correo.html");
                string html = File.ReadAllText(rutaPlantilla);

                html = html.Replace("{{Nombre completo}}", nombre)
                           .Replace("{{Nombre de la votación}}", asunto)
                           .Replace("{{LINK}}", link);

                // Este será el cuerpo final que enviarás
                string cuerpo = html;

                using (MailMessage mensaje = new MailMessage())
                {
                    mensaje.From = new MailAddress(emailSend);
                    mensaje.Subject = "Invitación a participar en la votación:" + asunto;
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

                return true;

            }
            catch
            {

                return false;
            }

        }
    }
}
