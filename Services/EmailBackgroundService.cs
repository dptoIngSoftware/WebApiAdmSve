using WebApiVotacionElectronica.Helper;
using WebApiVotacionElectronica.Repository.Interfaces;

namespace WebApiVotacionElectronica.Services
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IBackgroundEmailQueue _queue;
        private readonly ILogger<EmailBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMailHelper _mailHelper;
        private readonly int _throttlingDelay;

        public EmailBackgroundService(
            IBackgroundEmailQueue queue,
            ILogger<EmailBackgroundService> logger,
            IServiceScopeFactory scopeFactory,
            IMailHelper mailHelper,
            IConfiguration config)
        {
            _queue = queue;
            _logger = logger;
            _scopeFactory = scopeFactory;
            _mailHelper = mailHelper;

            _throttlingDelay = int.Parse(config["MailConfiguration:DelayMsEntreCorreos"] ?? "0");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de correo iniciado con 2 workers.");

            // Ejecutamos 2 workers en paralelo
            var workers = new List<Task>
            {
                WorkerAsync(stoppingToken),
                WorkerAsync(stoppingToken)
            };

            await Task.WhenAll(workers);
        }

        private async Task WorkerAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await _queue.DequeueAsync(stoppingToken);

                try
                {
                    int intentos = 0;
                    bool enviado = false;

                    while (intentos < 3 && !enviado)
                    {
                        intentos++;

                        if (_throttlingDelay > 0)
                            await Task.Delay(_throttlingDelay, stoppingToken); // anti-throttling

                        enviado = await _mailHelper.EnviarCorreoAsync(
                            workItem.Destinatario,
                            workItem.Nombre,
                            workItem.Asunto,
                            workItem.Link
                        );

                        if (!enviado)
                        {
                            int wait = intentos switch
                            {
                                1 => 1000,
                                2 => 3000,
                                _ => 5000
                            };

                            _logger.LogWarning(
                                "Error enviando correo a {destinatario}. Reintentando {intento}/3 en {wait}ms...",
                                workItem.Destinatario, intentos, wait
                            );

                            await Task.Delay(wait, stoppingToken);
                        }
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IVotante_Repository>();

                    await repo.ActualizarEnvioCorreoAsync(workItem.VotanteId, enviado);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error crítico enviando correo.");
                }
            }
        }
    }
}
