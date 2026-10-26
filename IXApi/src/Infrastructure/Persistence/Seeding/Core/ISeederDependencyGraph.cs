using System.Collections.Generic;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Core
{
    public interface ISeederDependencyGraph
    {
        /// <summary>
        /// Validates the dependency graph and returns eligible seeders in deterministic topological execution order.
        /// Throws detailed validation exceptions upon detecting duplicates, missing dependencies, cycles, or excluded dependencies.
        /// </summary>
        IReadOnlyList<IModuleSeeder> GetExecutionOrder(
            IEnumerable<IModuleSeeder> seeders,
            SeedingOptions options);
    }
}
