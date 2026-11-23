using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Workflow;

public static class WorkflowSeedRegistration
{
    public static IServiceCollection AddWorkflowSeeders(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, WfCategorySeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, WfPrioritySeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, WfProcessTypeSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, WfActivityTypeSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, WfOperatorSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, WfDataTypeSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, WfControlSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, WfPerformerTypeSeeder>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, WfPerformerSeeder>());
        return services;
    }
}
