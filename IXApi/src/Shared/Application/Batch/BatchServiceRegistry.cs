using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Shared.Application.Batch;

public sealed record BatchServiceDescriptor(string ServiceKey, string Name,
    bool ManagesCompanyScope = false, bool SupportsWholeJobRetry = true);
internal sealed record BatchServiceRegistration(BatchServiceDescriptor Descriptor, Type ServiceType);

public interface IBatchServiceRegistry
{
    IReadOnlyCollection<BatchServiceDescriptor> Services { get; }
    IBatchService Resolve(string serviceKey, IServiceProvider scopedProvider);
}

internal sealed class BatchServiceRegistry(IEnumerable<BatchServiceRegistration> registrations) : IBatchServiceRegistry
{
    private readonly Dictionary<string, BatchServiceRegistration> entries = registrations
        .ToDictionary(r => r.Descriptor.ServiceKey, StringComparer.OrdinalIgnoreCase);
    public IReadOnlyCollection<BatchServiceDescriptor> Services => entries.Values.Select(r => r.Descriptor).ToArray();
    public IBatchService Resolve(string serviceKey, IServiceProvider scopedProvider) =>
        entries.TryGetValue(serviceKey, out var registration)
            ? (IBatchService)scopedProvider.GetRequiredService(registration.ServiceType)
            : throw new KeyNotFoundException($"Batch service '{serviceKey}' is not registered.");
}

public static class BatchServiceRegistrationExtensions
{
    /// <summary>Registers the shared catalog even when no module has contributed services yet.</summary>
    public static IServiceCollection AddBatchFramework(this IServiceCollection services)
    {
        services.TryAddSingleton<IBatchServiceRegistry, BatchServiceRegistry>();
        return services;
    }

    public static IServiceCollection AddBatchService<T>(this IServiceCollection services, string serviceKey, string name,
        bool managesCompanyScope = false, bool supportsWholeJobRetry = true)
        where T : class, IBatchService
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (serviceKey.Length > 200 || serviceKey != serviceKey.Trim())
            throw new ArgumentException("Use a service key of at most 200 characters without surrounding spaces.", nameof(serviceKey));
        if (services.Any(d => d.ServiceType == typeof(BatchServiceRegistration) &&
            d.ImplementationInstance is BatchServiceRegistration r &&
            string.Equals(r.Descriptor.ServiceKey, serviceKey, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Batch service '{serviceKey}' is already registered.");
        services.TryAddScoped<T>();
        services.AddSingleton(new BatchServiceRegistration(new(serviceKey, name, managesCompanyScope, supportsWholeJobRetry), typeof(T)));
        services.AddBatchFramework();
        return services;
    }
}
