using Xunit;

namespace IAX.IXApi.Tests.Support;

/// <summary>
/// Every test that runs seeders shares this collection. Seeding is serialized by the process-wide
/// <c>SeedLock</c> (as production requires), so seeding tests running in parallel only queue on that lock and can
/// exceed its timeout; running them one after another keeps them deterministic.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class SeedingCollection
{
    public const string Name = "Seeding";
}
