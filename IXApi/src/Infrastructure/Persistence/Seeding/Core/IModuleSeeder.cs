using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Core
{
    /// <summary>
    /// Shared contract for all modular, scalable seeders.
    /// </summary>
    public interface IModuleSeeder
    {
        /// <summary>
        /// Stable, unique identifier across all seeders in the system.
        /// Suggested format: [Module].[Subsystem].[Name] (e.g., "Finance.Foundation.CompanyInfo").
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Schema / data version for this seeder. Incrementing triggers re-execution for RunOnce policies.
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Category defining operational requirement (Required, Reference, Demo, LegacyImport).
        /// </summary>
        SeederCategory Category { get; }

        /// <summary>
        /// IDs of seeders that must successfully complete before this seeder runs.
        /// </summary>
        IReadOnlyCollection<string> DependsOn { get; }

        /// <summary>
        /// Repeatable or RunOnce policy.
        /// </summary>
        SeederExecutionPolicy Policy { get; }

        /// <summary>
        /// Applicable tenant or legal entity scope (e.g. "global", "dat", "all-companies").
        /// </summary>
        string Scope { get; }

        /// <summary>
        /// Executes the seeding logic within the provided execution context.
        /// </summary>
        Task SeedAsync(SeedingContext context, CancellationToken cancellationToken);
    }
}
