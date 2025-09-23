namespace ZhooSoft.Tracker.CustomEventBus
{
    public class InMemoryEventBus : IEventBus
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, List<Type>> _handlers = new();

        public InMemoryEventBus(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task PublishAsync(IIntegrationEvent @event)
        {
            var eventType = @event.GetType();

            if (_handlers.ContainsKey(eventType))
            {
                foreach (var handlerType in _handlers[eventType])
                {
                    using var scope = _serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService(handlerType);
                    var method = handlerType.GetMethod("HandleAsync");
                    await (Task)method.Invoke(handler, new object[] { @event });
                }
            }
        }

        public void Subscribe<TEvent, THandler>()
            where TEvent : IIntegrationEvent
            where THandler : IIntegrationEventHandler<TEvent>
        {
            var eventType = typeof(TEvent);
            var handlerType = typeof(THandler);

            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Type>();
            }

            _handlers[eventType].Add(handlerType);
        }
    }
}
