namespace ZhooSoft.Tracker.CustomEventBus
{
    #region Interfaces

    public interface IEventBus
    {
        #region Methods

        Task PublishAsync(IIntegrationEvent @event);

        void Subscribe<TEvent, THandler>()
            where TEvent : IIntegrationEvent
            where THandler : IIntegrationEventHandler<TEvent>;

        #endregion
    }

    public interface IIntegrationEvent
    {
    }

    public interface IIntegrationEventHandler<TEvent> where TEvent : IIntegrationEvent
    {
        #region Methods

        Task HandleAsync(TEvent @event);

        #endregion
    }

    #endregion
}
