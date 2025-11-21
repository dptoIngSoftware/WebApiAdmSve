using WebApiVotacionElectronica.Models.Tools;

namespace WebApiVotacionElectronica.Tools
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Preparar respuesta HTTP
                var response = context.Response;
                response.ContentType = "application/json";
                response.StatusCode = StatusCodes.Status500InternalServerError;

                // Captura del usuario autenticado (si existe)
                string nombreUsuario = context.User?.Claims
                    ?.FirstOrDefault(claim => claim.Type == "Nombre")
                    ?.Value ?? "Usuario no identificado";

                // Enviar correo con toda la información del request
                _ = CorreoAdministrador.EnviarCorreoExcepcion(context, ex, nombreUsuario);

                // Respuesta al cliente
                await response.WriteAsync("Se produjo un error inesperado al procesar la información.");
            }
        }
    }
}
