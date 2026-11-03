using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;

/// <summary>Reserved for phase-two business-service scenarios; none are registered in phase one.</summary>
public interface ISeedScenario
{
    string Id { get; }
    Task ExecuteAsync(SeedContext context, CancellationToken cancellationToken);
}
