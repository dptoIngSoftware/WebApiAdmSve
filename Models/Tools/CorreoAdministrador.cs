using System.Net.Mail;

namespace WebApiVotacionElectronica.Models.Tools
{
    public class CorreoAdministrador
    {
        public static void EnviarCorreo(Exception ex, string nombre)
        {
            try
            {
                string enviarCorreo = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("MySettings")["EnviarCorreo"];
                bool Enviar = enviarCorreo.Equals("Si") ? true : false;

                if (Enviar)
                {
                    //string emailOrigen = "anon_error@ucn.cl";
                    string nombreCompleto = "Usuario No Identificado";

                    if (!String.IsNullOrEmpty(nombre))
                    {
                        //var handler = new JwtSecurityTokenHandler();
                        //var jsonToken = handler.ReadToken(token);
                        //var tokenS = jsonToken as JwtSecurityToken;
                        //emailOrigen = tokenS.Claims.First(claim => claim.Type == "Email").Value;
                        //nombreCompleto = tokenS.Claims.First(claim => claim.Type == "Nombre").Value + " " + tokenS.Claims.First(claim => claim.Type == "Paterno").Value + " " + tokenS.Claims.First(claim => claim.Type == "Materno").Value;
                        nombreCompleto = nombre;
                    }

                    string nombreSistema = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("MySettings")["NombreSistema"];
                    string passOrigen = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("MailConfigurationADM")["PassCorreoSistema"];
                    string emailSend = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("MailConfigurationADM")["CorreoSistema"];
                    string smtpSistema = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("MailConfigurationADM")["SmtpCorreoSistema"];
                    string puerto = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("MailConfigurationADM")["PuertoCorreoSistema"];
                    string emailDestino = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("MailConfigurationADM")["CorreoAdministrador"];
                    string subject = "Excepción en " + nombreSistema;
                    string body = "";
                    string bodyHtml = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("MailConfigurationADM")["IsBodyHtml"];
                    #pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
                    bool bodyHtmls = bodyHtml.Equals("Si") ? true : false;
                    #pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.

                    MailMessage correo = new MailMessage();
                    #pragma warning disable CS8604 // Posible argumento de referencia nulo
                    correo.From = new MailAddress(emailSend);
                    #pragma warning restore CS8604 // Posible argumento de referencia nulo
                    #pragma warning disable CS8604 // Posible argumento de referencia nulo
                    correo.To.Add(emailDestino);
                    #pragma warning restore CS8604 // Posible argumento de referencia nulo
                    correo.Subject = subject;

                    //StringBuilder sb = new StringBuilder();
                    body = "Se produjo una excepción en el sistema " + nombreSistema + "<br/><br/><br/>";

                    if (ex != null)
                    {
                        body += "MENSAJE     : " + ex.Message.ToString();
                        body += "<br/>APLICACIÓN  : " + ex.Source?.ToString();
                        body += "<br/>MÉTODO      : " + ex.TargetSite?.ToString();
                        body += "<br/>DESCRIPCIÓN : " + ex.StackTrace?.ToString();
                        body += "<br/>DATA        : " + ex.Data.ToString();
                        body += "<br/>EX. INTERNA : " + ex.InnerException;
                        body += "<br/>FIRMADO POR : " + nombreCompleto;
                    }

                    correo.Body = body + "<br/><br /><br />Nota: este correo es generado de manera automática por favor no responda a este mensaje.";
                    correo.IsBodyHtml = bodyHtmls;
                    correo.Priority = MailPriority.High;
                    SmtpClient smtp = new SmtpClient(smtpSistema);
                    smtp.Port = int.Parse(puerto);

                    smtp.Credentials = new System.Net.NetworkCredential(emailSend, passOrigen);
                    smtp.EnableSsl = true;
                    smtp.Send(correo);
                }

            }
            catch (Exception e)
            {

            }
        }
    }
}
