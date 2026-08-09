namespace IAX.IXApi.Shared.Domain.Events
{
    /// <summary>
    /// Handles a published <typeparamref name="TEvent"/>. Implement this in the infrastructure
    /// (or any) layer and register it in DI — the event bus discovers every handler for an
    /// event type and invokes them. Multiple handlers per event are supported (fan-out).
    ///
    /// Handlers should be side-effecting and self-contained: a failing handler is logged and
    /// isolated by the bus so it cannot break the publisher or sibling handlers.
    /// </summary>
    public interface ISysEventHandler<in TEvent> where TEvent : ISysEvent
    {
        Task HandleAsync(TEvent @event, CancellationToken ct = default);
    }
}
