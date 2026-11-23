using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;

/// <summary>
/// Runs an existing installation-wide V2 seeder (settings, number sequences, jobs, templates, document types,
/// workflow catalog) in the baseline profile under its baseline id and dependencies. The seeder itself is reused
/// as-is, so the two profiles never hold two copies of the same data or logic.
/// </summary>
public sealed class BaselineSeederAdapter<TSeeder>(TSeeder seeder, string id, params string[] dependencies) : IBaselineSeeder
    where TSeeder : IDataSeeder
{
    public string Id => id;
    public IReadOnlyCollection<string> Dependencies => dependencies;
    public Task SeedAsync(SeedContext context, CancellationToken cancellationToken) => seeder.SeedAsync(context, cancellationToken);
}
