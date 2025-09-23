namespace ZhooSoft.Tracker.CustomEventBus
{
    public interface IIntegrationEvent { }

    public interface IEventBus
    {
        Task PublishAsync(IIntegrationEvent @event);
        void Subscribe<TEvent, THandler>()
            where TEvent : IIntegrationEvent
            where THandler : IIntegrationEventHandler<TEvent>;
    }

    public interface IIntegrationEventHandler<TEvent> where TEvent : IIntegrationEvent
    {
        Task HandleAsync(TEvent @event);
    }
}
