using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;

public sealed class SeedDependencyResolver
{
    public IReadOnlyList<IDataSeeder> Resolve(IEnumerable<IDataSeeder> seeders)
    {
        var map = seeders.ToDictionary(x => x.Id, StringComparer.Ordinal);
        var visiting = new HashSet<string>();
        var visited = new HashSet<string>();
        var result = new List<IDataSeeder>();
        void Visit(string id)
        {
            if (visited.Contains(id)) return;
            if (!map.TryGetValue(id, out var seeder)) throw new InvalidOperationException($"Missing seed dependency {id}.");
            if (!visiting.Add(id)) throw new InvalidOperationException($"Cyclic seed dependency {id}.");
            foreach (var dependency in seeder.Dependencies) Visit(dependency);
            visiting.Remove(id);
            visited.Add(id);
            result.Add(seeder);
        }
        foreach (var id in map.Keys.Order(StringComparer.Ordinal)) Visit(id);
        return result;
    }
}
