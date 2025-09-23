using System.Text.Json;

namespace ZhooSoft.Tracker.CustomEventBus
{
    //public class AzureServiceBusEventBus : IEventBus
    //{
    //    private readonly ServiceBusClient _client;
    //    private readonly string _topicName = "integration-events";

    //    public AzureServiceBusEventBus(string connectionString)
    //    {
    //        _client = new ServiceBusClient(connectionString);
    //    }

    //    public async Task PublishAsync(IIntegrationEvent @event)
    //    {
    //        var sender = _client.CreateSender(_topicName);

    //        var messageBody = JsonSerializer.Serialize(@event);
    //        var message = new ServiceBusMessage(messageBody)
    //        {
    //            Subject = @event.GetType().Name // event type = routing key
    //        };

    //        await sender.SendMessageAsync(message);
    //    }

    //    public void Subscribe<TEvent, THandler>()
    //        where TEvent : IIntegrationEvent
    //        where THandler : IIntegrationEventHandler<TEvent>
    //    {
    //        // In Azure SB, this means creating a subscription & processor
    //        // Example: subscription per event type
    //        throw new NotImplementedException("Subscription setup goes here");
    //    }
    //}
}
