namespace IAX.IXApi.Shared.Domain.Events
{
    /// <summary>
    /// In-process publish/subscribe bus. Modules publish <see cref="ISysEvent"/>s; the bus
    /// resolves and invokes every registered <see cref="ISysEventHandler{TEvent}"/>.
    ///
    /// This is the integration seam tying the three infrastructure systems together without
    /// coupling business code to them: a workflow/HR/finance module publishes a domain event,
    /// and infrastructure handlers translate it into notifications, realtime broadcasts, or
    /// background jobs.
    /// </summary>
    public interface ISysEventBus
    {
        /// <summary>
        /// Publishes an event to all registered handlers. Handler exceptions are isolated and
        /// logged; one failing handler never prevents the others from running.
        /// </summary>
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : ISysEvent;
    }
}
