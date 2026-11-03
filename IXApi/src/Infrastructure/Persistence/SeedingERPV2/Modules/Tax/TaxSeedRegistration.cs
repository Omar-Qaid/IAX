using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;
public static class TaxSeedRegistration
{
    public static IServiceCollection AddTaxSeeders(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, TaxPostingSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, TaxTableSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, TaxGroupHeadingSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, TaxItemGroupHeadingSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, TaxGroupDataSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, TaxOnItemSeeder>());
        return services;
    }
}
