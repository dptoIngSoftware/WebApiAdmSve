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
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";


                //var token = context.Response.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
                var nombre = context.User.Claims.FirstOrDefault(claim => claim.Type == "Nombre")?.Value;

                CorreoAdministrador.EnviarCorreo(error, nombre);

                response.StatusCode = 400;

                await response.WriteAsync(error.Message);

            }
        }
    }
}
