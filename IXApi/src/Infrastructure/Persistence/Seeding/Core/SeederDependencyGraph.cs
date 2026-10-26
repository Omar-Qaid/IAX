using System;
using System.Collections.Generic;
using System.Linq;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Core
{
    public class SeederDependencyGraph : ISeederDependencyGraph
    {
        public IReadOnlyList<IModuleSeeder> GetExecutionOrder(
            IEnumerable<IModuleSeeder> seeders,
            SeedingOptions options)
        {
            ArgumentNullException.ThrowIfNull(seeders);
            options ??= new SeedingOptions();

            var seederList = seeders.ToList();

            // 1. Detect Duplicate IDs
            var duplicates = seederList
                .GroupBy(s => s.Id, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .ToList();

            if (duplicates.Any())
            {
                var firstDup = duplicates.First();
                throw new DuplicateSeederIdException(
                    firstDup.Key,
                    firstDup.Select(s => s.GetType().FullName ?? s.GetType().Name));
            }

            var allById = seederList.ToDictionary(s => s.Id, StringComparer.OrdinalIgnoreCase);

            // 2. Identify Active / Eligible seeders
            bool IsEligible(IModuleSeeder seeder) => seeder.Category switch
            {
                SeederCategory.Required => true,
                SeederCategory.Reference => true,
                SeederCategory.Demo => options.EnableDemoData,
                SeederCategory.LegacyImport => options.EnableLegacyImports,
                _ => true
            };

            var eligibleSeeders = seederList.Where(IsEligible).ToList();
            var eligibleById = eligibleSeeders.ToDictionary(s => s.Id, StringComparer.OrdinalIgnoreCase);

            // 3. Detect Missing and Excluded Dependencies for eligible seeders
            foreach (var seeder in eligibleSeeders)
            {
                if (seeder.DependsOn == null) continue;

                foreach (var depId in seeder.DependsOn)
                {
                    if (!allById.TryGetValue(depId, out var registeredDependency))
                    {
                        throw new MissingDependencyException(seeder.Id, depId);
                    }

                    if (!eligibleById.ContainsKey(depId))
                    {
                        throw new ExcludedDependencyException(
                            seeder.Id,
                            depId,
                            registeredDependency.Category);
                    }
                }
            }

            // 4. Cycle Detection & Deterministic Topological Sort
            // Adjacency graph: depId -> set of seeders that depend on depId
            var inDegree = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var dependentsMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var s in eligibleSeeders)
            {
                inDegree[s.Id] = 0;
                dependentsMap[s.Id] = new List<string>();
            }

            foreach (var s in eligibleSeeders)
            {
                var deps = s.DependsOn?.Distinct(StringComparer.OrdinalIgnoreCase) ?? Enumerable.Empty<string>();
                foreach (var depId in deps)
                {
                    dependentsMap[depId].Add(s.Id);
                    inDegree[s.Id]++;
                }
            }

            // Priority queue / sorted set for deterministic tie-breaker:
            // Lowest in-degree first (must be 0), then tie-break alphabetically by Id
            var available = new SortedSet<string>(
                Comparer<string>.Create((a, b) => string.Compare(a, b, StringComparison.OrdinalIgnoreCase)));

            foreach (var kvp in inDegree.Where(kvp => kvp.Value == 0))
            {
                available.Add(kvp.Key);
            }

            var result = new List<IModuleSeeder>();

            while (available.Count > 0)
            {
                var currentId = available.Min!;
                available.Remove(currentId);

                result.Add(eligibleById[currentId]);

                foreach (var dependentId in dependentsMap[currentId])
                {
                    inDegree[dependentId]--;
                    if (inDegree[dependentId] == 0)
                    {
                        available.Add(dependentId);
                    }
                }
            }

            if (result.Count < eligibleSeeders.Count)
            {
                // Cycle detected! Reconstruct the exact cycle path using DFS
                var remainingIds = eligibleSeeders.Select(s => s.Id).Except(result.Select(s => s.Id)).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var cyclePath = FindCyclePath(remainingIds, eligibleById);
                throw new CyclicDependencyException(cyclePath);
            }

            return result;
        }

        private static List<string> FindCyclePath(
            HashSet<string> nodeSubset,
            Dictionary<string, IModuleSeeder> eligibleById)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var stack = new List<string>();
            var inStack = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var nodeId in nodeSubset)
            {
                if (!visited.Contains(nodeId))
                {
                    var cycle = DfsCycle(nodeId, nodeSubset, eligibleById, visited, stack, inStack);
                    if (cycle != null) return cycle;
                }
            }

            return nodeSubset.OrderBy(x => x).ToList();
        }

        private static List<string>? DfsCycle(
            string current,
            HashSet<string> nodeSubset,
            Dictionary<string, IModuleSeeder> eligibleById,
            HashSet<string> visited,
            List<string> stack,
            HashSet<string> inStack)
        {
            visited.Add(current);
            stack.Add(current);
            inStack.Add(current);

            var seeder = eligibleById[current];
            var deps = (seeder.DependsOn ?? Array.Empty<string>()).Where(nodeSubset.Contains);

            foreach (var dep in deps)
            {
                if (inStack.Contains(dep))
                {
                    var cycleStartIndex = stack.IndexOf(dep);
                    var cycle = stack.Skip(cycleStartIndex).ToList();
                    cycle.Add(dep);
                    return cycle;
                }

                if (!visited.Contains(dep))
                {
                    var cycle = DfsCycle(dep, nodeSubset, eligibleById, visited, stack, inStack);
                    if (cycle != null) return cycle;
                }
            }

            stack.RemoveAt(stack.Count - 1);
            inStack.Remove(current);
            return null;
        }
    }
}
