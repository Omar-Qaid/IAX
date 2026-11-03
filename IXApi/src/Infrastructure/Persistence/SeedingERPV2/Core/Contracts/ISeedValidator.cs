using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;

public interface ISeedValidator
{
    Task ValidateAsync(SeedContext context, CancellationToken cancellationToken);
}
