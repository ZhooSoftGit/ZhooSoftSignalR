using Zhoosoft.EventBus;

namespace ZhooSoft.Tracker.AzureServiceBus
{
    public class RideEventConsumerBackgroundService<THandler> : BackgroundService
    where THandler : ITrackerApiRideEventHandler
    {
        #region Fields

        private readonly IEventConsumer _eventConsumer;

        private readonly ILogger<RideEventConsumerBackgroundService<THandler>> _logger;

        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Constructors

        public RideEventConsumerBackgroundService(
            IEventConsumer eventConsumer,
            IServiceProvider serviceProvider,
            ILogger<RideEventConsumerBackgroundService<THandler>> logger)
        {
            _eventConsumer = eventConsumer;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        #endregion

        #region Methods

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting RideEventConsumerBackgroundService<{Handler}>", typeof(THandler).Name);

            // Start Azure Service Bus processor
            await _eventConsumer.StartAsync(async message =>
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<THandler>();

                try
                {
                    await handler.HandleAsync(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling message {@Message}", message);
                    // Optionally: log and let SB retry / dead-letter
                    throw;
                }
            });

            // Keep the background service alive until cancellation
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        #endregion
    }
}
