using IAX.IXApi.Shared.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Shared.Domain.Events
{
    /// <summary>
    /// Default in-process implementation of <see cref="ISysEventBus"/>.
    /// Resolves every <see cref="ISysEventHandler{TEvent}"/> for the concrete event type and
    /// invokes them sequentially with per-handler error isolation.
    /// </summary>
    [ScopedService]
    public class SysEventBus : ISysEventBus
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<SysEventBus> _logger;

        public SysEventBus(IServiceProvider services, ILogger<SysEventBus> logger)
        {
            _services = services;
            _logger = logger;
        }

        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
            where TEvent : ISysEvent
        {
            var handlers = _services.GetServices<ISysEventHandler<TEvent>>().ToList();
            if (handlers.Count == 0)
            {
                _logger.LogDebug("[EventBus] No handlers for {EventType}.", typeof(TEvent).Name);
                return;
            }

            foreach (var handler in handlers)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    await handler.HandleAsync(@event, ct);
                }
                catch (Exception ex)
                {
                    // Isolate: a failing subscriber must not break the publisher or siblings.
                    _logger.LogError(ex,
                        "[EventBus] Handler {Handler} failed for {EventType}.",
                        handler.GetType().Name, typeof(TEvent).Name);
                }
            }
        }
    }
}
