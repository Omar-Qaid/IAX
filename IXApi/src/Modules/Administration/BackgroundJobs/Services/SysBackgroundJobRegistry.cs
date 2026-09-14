using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services
{
    public sealed class SysBackgroundJobRegistry : ISysBackgroundJobRegistry
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Lazy<Dictionary<string, Type>> _map;
        private readonly HashSet<string> _selfScopedKeys = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _noRetryKeys = new(StringComparer.OrdinalIgnoreCase);
        private readonly IAX.IXApi.Shared.Application.Batch.IBatchServiceRegistry? _batchServices;

        public SysBackgroundJobRegistry(IServiceScopeFactory scopeFactory, IAX.IXApi.Shared.Application.Batch.IBatchServiceRegistry? batchServices = null)
        {
            _scopeFactory = scopeFactory;
            _batchServices = batchServices;
            _map = new Lazy<Dictionary<string, Type>>(BuildMap, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private Dictionary<string, Type> BuildMap()
        {
            using var scope = _scopeFactory.CreateScope();
            var handlers = scope.ServiceProvider.GetServices<ISysBackgroundJobHandler>();
            var map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            foreach (var handler in handlers)
            {
                if (!map.TryAdd(handler.JobKey, handler.GetType()) && map[handler.JobKey] != handler.GetType())
                    throw new InvalidOperationException($"Duplicate batch handler key '{handler.JobKey}'.");
                if (handler.ManagesCompanyScope) _selfScopedKeys.Add(handler.JobKey);
                if (!handler.SupportsWholeJobRetry) _noRetryKeys.Add(handler.JobKey);
            }
            return map;
        }

        public IReadOnlyCollection<string> RegisteredKeys => _map.Value.Keys
            .Concat(_batchServices?.Services.Select(s => s.ServiceKey) ?? []).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        public IReadOnlyCollection<string> SelfScopedKeys
        {
            get
            {
                _ = _map.Value;
                return _selfScopedKeys.Concat(_batchServices?.Services.Where(s => s.ManagesCompanyScope)
                    .Select(s => s.ServiceKey) ?? []).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            }
        }
        public bool SupportsWholeJobRetry(string jobKey)
        {
            _ = _map.Value;
            var service = _batchServices?.Services.FirstOrDefault(s => string.Equals(s.ServiceKey, jobKey, StringComparison.OrdinalIgnoreCase));
            return service?.SupportsWholeJobRetry ?? !_noRetryKeys.Contains(jobKey);
        }

        public bool IsRegistered(string jobKey) =>
            !string.IsNullOrEmpty(jobKey) && RegisteredKeys.Contains(jobKey, StringComparer.OrdinalIgnoreCase);

        public ISysBackgroundJobHandler? Resolve(string jobKey, IServiceProvider scopedProvider)
        {
            if (_batchServices?.Services.Any(s => string.Equals(s.ServiceKey, jobKey, StringComparison.OrdinalIgnoreCase)) == true)
                return new BatchServiceHandlerAdapter(jobKey, _batchServices.Resolve(jobKey, scopedProvider));
            if (string.IsNullOrEmpty(jobKey) || !_map.Value.TryGetValue(jobKey, out var type))
                return null;

            // Resolve a fresh, scoped instance so handlers get scoped dependencies (DbContext etc.).
            return (ISysBackgroundJobHandler)ActivatorUtilities.GetServiceOrCreateInstance(scopedProvider, type);
        }
    }
}
