using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services
{
    /// <summary>
    /// Resolves registered <see cref="ISysBackgroundJobHandler"/> implementations by key.
    /// Used by the engine to find the handler for a job and by the API to validate
    /// that a job's <c>JobKey</c> actually maps to runnable code.
    /// </summary>
    public interface ISysBackgroundJobRegistry
    {
        /// <summary>All registered handler keys (for discovery in the UI).</summary>
        IReadOnlyCollection<string> RegisteredKeys { get; }

        /// <summary>Returns true if a handler is registered for the given key.</summary>
        bool IsRegistered(string jobKey);

        /// <summary>
        /// Resolves a handler instance for the key from the supplied (scoped) provider,
        /// or null if none is registered.
        /// </summary>
        ISysBackgroundJobHandler? Resolve(string jobKey, IServiceProvider scopedProvider);
    }

    /// <summary>
    /// Default registry. Discovers handlers automatically from DI — registering a new
    /// <see cref="ISysBackgroundJobHandler"/> is all that's needed to add a job type.
    ///
    /// Registered as a singleton, but handlers are scoped; to avoid a captive dependency the
    /// key→type map is built lazily inside a short-lived scope and only immutable
    /// <see cref="Type"/> metadata is cached (never handler instances).
    /// </summary>
    [SingletonService]
    public sealed class SysBackgroundJobRegistry : ISysBackgroundJobRegistry
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Lazy<Dictionary<string, Type>> _map;

        public SysBackgroundJobRegistry(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _map = new Lazy<Dictionary<string, Type>>(BuildMap, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private Dictionary<string, Type> BuildMap()
        {
            using var scope = _scopeFactory.CreateScope();
            var handlers = scope.ServiceProvider.GetServices<ISysBackgroundJobHandler>();
            var map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            foreach (var handler in handlers)
                map[handler.JobKey] = handler.GetType(); // last one wins for duplicate keys
            return map;
        }

        public IReadOnlyCollection<string> RegisteredKeys => _map.Value.Keys.ToList();

        public bool IsRegistered(string jobKey) =>
            !string.IsNullOrEmpty(jobKey) && _map.Value.ContainsKey(jobKey);

        public ISysBackgroundJobHandler? Resolve(string jobKey, IServiceProvider scopedProvider)
        {
            if (string.IsNullOrEmpty(jobKey) || !_map.Value.TryGetValue(jobKey, out var type))
                return null;

            // Resolve a fresh, scoped instance so handlers get scoped dependencies (DbContext etc.).
            return (ISysBackgroundJobHandler)scopedProvider.GetRequiredService(type);
        }
    }
}
