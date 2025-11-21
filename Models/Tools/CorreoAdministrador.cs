using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using System;
using System.Threading.Tasks;

namespace WebApiVotacionElectronica.Models.Tools
{
    public class CorreoAdministrador
    {
        private static IConfiguration GetConfig()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }

        public static string PlantillaErrorUCN(string titulo, string contenido)
        {
            string html = @"
<!DOCTYPE html>
<html lang='es'>
<head>
<meta charset='UTF-8' />
<meta name='viewport' content='width=device-width, initial-scale=1.0' />
<title>Error en el Sistema</title>
</head>

<body style='margin:0; padding:0; background:#f4f4f4; font-family:Arial, sans-serif;'>

<table width='100%' cellpadding='0' cellspacing='0' border='0' style='margin:0; padding:0; background:#f4f4f4;'>
<tr><td align='center'>

<table width='100%' cellpadding='0' cellspacing='0' border='0' 
       style='max-width:650px; background:#ffffff; border-radius:8px;'>


    <tr>
        <td style='padding:24px 32px 8px 32px; text-align:left;'>
            <h2 style='color:#c82333; margin:0; font-size:22px;'>
                ⚠️ " + titulo + @"
            </h2>
        </td>
    </tr>

    <tr>
        <td style='padding:0 32px 20px 32px;'>
            <table width='100%' cellpadding='0' cellspacing='0' border='0'
                   style='background:#f8d7da; border-left:6px solid #c82333; border-radius:6px;'>

                <tr>
                    <td style='padding:18px;
                               font-family:Consolas, monospace;
                               font-size:14px;
                               color:#721c24;
                               white-space:pre-wrap;
                               word-break:break-word;
                               overflow-wrap:anywhere;
                               text-align:left !important;'>
                        {{CONTENIDO_ERROR}}
                    </td>
                </tr>
            </table>
        </td>
    </tr>

    <tr>
        <td style='padding:20px 32px 32px 32px; text-align:center; color:#666; font-size:12px;'>
            Mensaje generado automáticamente por los sistemas de la<br/>
            <b>Universidad Católica del Norte</b>. No responda este correo.
        </td>
    </tr>

</table>

</td></tr>
</table>

</body>
</html>";

            return html.Replace("{{CONTENIDO_ERROR}}", contenido);
        }


        // MÉTODO CENTRAL DE ENVÍO
        private static async Task SendEmailAsync(string destino, string asunto, string html, string sender)
        {
            var config = GetConfig();

            string emailSend = config["MailConfiguration:CorreoSistema"];
            string passOrigen = config["MailConfiguration:PassCorreoSistema"];
            string smtpSistema = config["MailConfiguration:SmtpCorreoSistema"];
            int puerto = int.Parse(config["MailConfiguration:PuertoCorreoSistema"]);

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(sender, emailSend));
            email.To.Add(MailboxAddress.Parse(destino));
            email.Subject = asunto;
            email.Body = new TextPart(TextFormat.Html) { Text = html };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(smtpSistema, puerto, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(emailSend, passOrigen);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public static string ConstruirDetalleError(HttpContext http, Exception ex, string usuario)
        {
            string ambiente = GetConfig()["MySettings:Ambiente"] ?? "No especificado";
            string sistema = GetConfig()["MySettings:NombreSistema"] ?? "Sistema UCN";

            string ip = http?.Connection?.RemoteIpAddress?.ToString() ?? "IP no disponible";
            string endpoint = http?.Request?.Path.Value ?? "No disponible";
            string metodo = http?.Request?.Method ?? "No disponible";
            string query = http?.Request?.QueryString.Value ?? "";
            string servidor = Environment.MachineName;

            string fecha = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
            string errorId = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();

            // Sanitizamos el usuario
            if (string.IsNullOrWhiteSpace(usuario))
                usuario = "Usuario no identificado";

            return $@"
<b>FECHA:</b> {fecha}<br/>
<b>AMBIENTE:</b> {ambiente}<br/>
<b>SISTEMA:</b> {sistema}<br/>
<b>SERVIDOR:</b> {servidor}<br/><br/>

<b>ENDPOINT:</b> {endpoint}<br/>
<b>MÉTODO HTTP:</b> {metodo}<br/>
<b>QUERY:</b> {query}<br/>
<b>IP CLIENTE:</b> {ip}<br/>
<b>USUARIO:</b> {usuario}<br/><br/>

<b>MENSAJE:</b> {ex.Message}<br/><br/>

<b>APLICACIÓN:</b> {ex.Source}<br/>
<b>MÉTODO CONFLICTIVO:</b> {ex.TargetSite}<br/><br/>

<b>STACK TRACE:</b><br/>{ex.StackTrace}<br/><br/>

<b>INNER EXCEPTION:</b><br/>{ex.InnerException?.ToString() ?? "No disponible"}<br/>";
        }

        public static async Task EnviarCorreoExcepcion(HttpContext http, Exception ex, string nombreUsuario)
        {
            try
            {
                var config = GetConfig();
                if (config["MySettings:EnviarCorreo"]?.ToLower() != "si")
                    return;

                string emailAdmin = config["MailConfiguration:CorreoAdministrador"];
                string sistema = config["MySettings:NombreSistema"];

                // Generar contenido automáticamente
                string contenido = ConstruirDetalleError(http, ex, nombreUsuario);

                // Insertar en plantilla
                string html = PlantillaErrorUCN($"Error en {sistema}", contenido);

                await SendEmailAsync(emailAdmin, $"Error en {sistema}", html, "Excepción del Sistema");
            }
            catch
            {
                // Evitar loops
            }
        }
    }
}
