using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;

public interface IDataSeeder
{
    string Id { get; }
    IReadOnlyCollection<string> Dependencies { get; }
    Task SeedAsync(SeedContext context, CancellationToken cancellationToken);
}
