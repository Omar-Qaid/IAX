using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Validation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation;
public static class FoundationCycleRegistration
{
 public static IServiceCollection AddFoundationConfigurationAndMasterDataExamples(this IServiceCollection services)
 {
  services.AddFoundationPhaseOneSeeding();
  services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, FoundationCycleValidator>());
  services.TryAddScoped<FoundationCycleSeeder>();
  return services;
 }
}
