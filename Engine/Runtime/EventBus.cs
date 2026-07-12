namespace Breakout;

interface ISceneEvent
{
}

sealed class EventBus
{
    private readonly Dictionary<Type, List<Subscription>> _subscriptions = [];

    public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
        where TEvent : struct, ISceneEvent
    {
        Type eventType = typeof(TEvent);

        if (!_subscriptions.TryGetValue(eventType, out List<Subscription>? subscriptions))
        {
            subscriptions = [];
            _subscriptions.Add(eventType, subscriptions);
        }

        Subscription subscription = new(this, eventType, handler);
        subscriptions.Add(subscription);
        return subscription;
    }

    public void Publish<TEvent>(TEvent evt)
        where TEvent : struct, ISceneEvent
    {
        if (!_subscriptions.TryGetValue(typeof(TEvent), out List<Subscription>? subscriptions))
        {
            return;
        }

        Subscription[] snapshot = [.. subscriptions];

        foreach (Subscription subscription in snapshot)
        {
            if (!subscription.IsDisposed)
            {
                ((Action<TEvent>)subscription.Handler)(evt);
            }
        }
    }

    private void Unsubscribe(Subscription subscription)
    {
        subscription.IsDisposed = true;

        if (!_subscriptions.TryGetValue(subscription.EventType, out List<Subscription>? subscriptions))
        {
            return;
        }

        subscriptions.Remove(subscription);

        if (subscriptions.Count == 0)
        {
            _subscriptions.Remove(subscription.EventType);
        }
    }

    private class Subscription(EventBus eventBus, Type eventType, Delegate handler) : IDisposable
    {
        public Type EventType { get; } = eventType;

        public Delegate Handler { get; } = handler;

        public bool IsDisposed { get; set; }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                eventBus.Unsubscribe(this);
            }
        }
    }
}
