namespace IAX.IXApi.Shared.Domain.Events
{
    /// <summary>
    /// Marker for an in-process domain event. Any module can define events that implement
    /// this interface and publish them through <see cref="ISysEventBus"/> without referencing
    /// the Notification / Realtime / Background-Job systems directly. Handlers (registered in
    /// the infrastructure layer) decide what side effects an event triggers — this is the
    /// decoupling seam between business modules and cross-cutting infrastructure.
    /// </summary>
    public interface ISysEvent
    {
    }
}
