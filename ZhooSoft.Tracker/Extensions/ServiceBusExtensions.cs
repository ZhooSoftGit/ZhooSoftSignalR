using Microsoft.Extensions.Options;
using Zhoosoft.EventBus;
using ZhooSoft.Tracker.AzureServiceBus;

namespace ZhooSoft.Tracker
{
    public static class ServiceBusExtensions
    {
        #region Methods

        public static IServiceCollection AddTrackerApiServiceBus(
         this IServiceCollection services,
         IConfiguration configuration)
        {
            // 1. Bind options
            services.Configure<ServiceBusOptions>(
                configuration.GetSection("ServiceBus"));

            // 2. Register Publisher (singleton – safe)
            services.AddSingleton<IEventPublisher>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<ServiceBusOptions>>().Value;
                return new EventPublisher(options.ConnectionString, options.QueueName);
            });

            // 3. Register Consumer (singleton – ServicioBusClient is thread-safe)
            services.AddSingleton<IEventConsumer>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<ServiceBusOptions>>().Value;
                return new EventConsumer(options.ConnectionString, options.ConsumerQueueName);
            });

            // 5. Event handler for Main API
            services.AddScoped<TrackerApiRideEventHandler>(); // implements IRideEventHandler

            // 6. Background service that connects consumer → handler
            services.AddHostedService<RideEventConsumerBackgroundService<TrackerApiRideEventHandler>>();

            // 7. Publisher wrapper for Main API usage
            services.AddScoped<ITrackerApiRideEventPublisher, TrackerApiRideEventPublisher>();

            return services;
        }

        #endregion
    }
}
