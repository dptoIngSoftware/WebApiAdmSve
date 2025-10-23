namespace WebApiVotacionElectronica.Services
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IBackgroundEmailQueue _queue;
        private readonly ILogger<EmailBackgroundService> _logger;

        public EmailBackgroundService(IBackgroundEmailQueue queue, ILogger<EmailBackgroundService> logger)
        {
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de envío de correos iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await _queue.DequeueAsync(stoppingToken);

                try
                {
                    await workItem();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar envío de correo.");
                }
            }

            _logger.LogInformation("Servicio de envío de correos detenido.");
        }
    }
}
