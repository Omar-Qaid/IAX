using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Workflow.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Workflow;

public static class WorkflowCycleRegistration
{
    public static IServiceCollection AddWorkflowConfigurationAndMasterDataExamples(this IServiceCollection services)
    {
        services.AddWorkflowPhaseOneSeeding();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, WorkflowCycleValidator>());
        services.TryAddScoped<WorkflowCycleSeeder>();
        return services;
    }
}
