using Mercury.Engine.Mips.Instructions;

namespace Mercury.Engine.Common;

/// <summary>
/// Implements an event bus to allow decoupled modules to communicate with each other by publishing and subscribing
/// to different event types.
/// </summary>
/// <remarks>
/// This module uses a publish-subscribe system.
/// </remarks>
public sealed class EventBus {
    private readonly Dictionary<Type, List<Delegate>> handlers = new();
    
    /// <summary>
    /// Subscribes a handler to a specific event type. Whenever an event of that type is published,
    /// <paramref name="handler"/> is invoked.
    /// </summary>
    /// <param name="handler"></param>
    /// <typeparam name="T"></typeparam>
    public IDisposable Subscribe<T>(Action<T> handler) {
        Func<T, ValueTask> taskHandler = e => {
            handler(e);
            return ValueTask.CompletedTask;
        };
        if (!handlers.TryGetValue(typeof(T), out List<Delegate>? list)) {
            list = [];
            handlers[typeof(T)] = list;
        }
        list.Add(taskHandler);
        return new Subscription(() => list.Remove(handler));
    }
    
    public IDisposable Subscribe<T>(Func<T, ValueTask> handler) {
        if (!handlers.TryGetValue(typeof(T), out List<Delegate>? list)) {
            list = [];
            handlers[typeof(T)] = list;
        }
        list.Add(handler);
        return new Subscription(() => list.Remove(handler));
    }

    /// <summary>
    /// Publishes an event of type <typeparamref name="T"/>. All handlers subscribed to that event type will be invoked
    /// in no particular order. The event data is passed as an argument to the handlers.
    /// </summary>
    /// <param name="e"></param>
    /// <typeparam name="T"></typeparam>
    public void Publish<T>(in T e) {
        if (handlers.TryGetValue(typeof(T), out List<Delegate>? list)) {
            foreach (Delegate d in list) {
                var handler = (Func<T,ValueTask>)d;
                _ = handler(e);
            }
        }
    }
    
    public async ValueTask PublishAsync<T>(T e) {
        if (handlers.TryGetValue(typeof(T), out List<Delegate>? list)) {
            foreach (Delegate d in list) {
                var handler = (Func<T,ValueTask>)d;
                await handler(e);
            }
        }
    }
    //
    // public TResponse PublishAndWait<TRequest,TResponse>(TRequest request) {
    //     TaskCompletionSource<TResponse> tcs = new();
    //     using IDisposable sub = Subscribe<TResponse>(response => tcs.SetResult(response));
    //     Publish(request);
    //     return tcs.Task.GetAwaiter().GetResult();
    // }
}

internal class Subscription(Action disposeAction) : IDisposable {
    public void Dispose() {
        disposeAction();
    }
}