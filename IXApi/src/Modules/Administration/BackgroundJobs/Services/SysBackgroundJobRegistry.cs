using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services
{
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