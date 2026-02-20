namespace ChainReactionConveyor.Services
{
    /// <summary>
    /// Interface for components that subscribe to EventBus events
    /// Provides a consistent pattern for subscribing/unsubscribing
    /// </summary>
    public interface IEventSubscriber
    {
        /// <summary>
        /// Subscribe to events - called in OnEnable
        /// </summary>
        void Subscribe();

        /// <summary>
        /// Unsubscribe from events - called in OnDisable
        /// </summary>
        void Unsubscribe();
    }
}
