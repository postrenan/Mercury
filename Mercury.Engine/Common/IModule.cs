namespace Mercury.Engine.Common;

/// <summary>
/// Shared interface for all modules that are used in a machine.
/// </summary>
public interface IModule {
    
    public void SubscribeToEvents(EventBus eventBus);
    public void UnsubscribeFromEvents();
}