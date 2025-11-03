using WebApiVotacionElectronica.Helper;
using WebApiVotacionElectronica.Repository.Interfaces;

namespace WebApiVotacionElectronica.Services
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IBackgroundEmailQueue _queue;
        private readonly ILogger<EmailBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;

        public EmailBackgroundService(
            IBackgroundEmailQueue queue,
            ILogger<EmailBackgroundService> logger,
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration)
        {
            _queue = queue;
            _logger = logger;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de envío de correos iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await _queue.DequeueAsync(stoppingToken);

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var votanteRepo = scope.ServiceProvider.GetRequiredService<IVotante_Repository>();

                    bool envioExitoso = await MailHelper.EnviarCorreoPersonalizadoAsync(
                        _configuration,
                        workItem.Destinatario,
                        workItem.Nombre,
                        workItem.Asunto,
                        workItem.Link
                    );

                    await votanteRepo.ActualizarEnvioCorreoAsync(workItem.VotanteId, envioExitoso);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error enviando correo al votante {workItem.VotanteId}");
                }
            }

            _logger.LogInformation("Servicio de envío de correos detenido.");
        }
    }
}
