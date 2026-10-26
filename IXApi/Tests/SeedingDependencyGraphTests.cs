using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using Xunit;

namespace IXApi.Tests
{
    public class SeedingDependencyGraphTests
    {
        private class TestSeeder : IModuleSeeder
        {
            public string Id { get; set; } = string.Empty;
            public int Version { get; set; } = 1;
            public SeederCategory Category { get; set; } = SeederCategory.Required;
            public IReadOnlyCollection<string> DependsOn { get; set; } = Array.Empty<string>();
            public SeederExecutionPolicy Policy { get; set; } = SeederExecutionPolicy.Repeatable;
            public string Scope { get; set; } = "global";

            public Task SeedAsync(SeedingContext context, CancellationToken cancellationToken) => Task.CompletedTask;
        }

        [Fact]
        public void GetExecutionOrder_DeterministicTopologicalSort_Succeeds()
        {
            var seeders = new List<IModuleSeeder>
            {
                new TestSeeder { Id = "Module.C", DependsOn = new[] { "Module.B" } },
                new TestSeeder { Id = "Module.A", DependsOn = Array.Empty<string>() },
                new TestSeeder { Id = "Module.B", DependsOn = new[] { "Module.A" } },
            };

            var graph = new SeederDependencyGraph();
            var options = new SeedingOptions { Enabled = true, EnableDemoData = true, EnableLegacyImports = true };
            var ordered = graph.GetExecutionOrder(seeders, options).Select(s => s.Id).ToList();

            Assert.Equal(new[] { "Module.A", "Module.B", "Module.C" }, ordered);
        }

        [Fact]
        public void GetExecutionOrder_AlphabeticalTieBreaker_Succeeds()
        {
            var seeders = new List<IModuleSeeder>
            {
                new TestSeeder { Id = "Z_Seeder", DependsOn = Array.Empty<string>() },
                new TestSeeder { Id = "A_Seeder", DependsOn = Array.Empty<string>() },
                new TestSeeder { Id = "M_Seeder", DependsOn = Array.Empty<string>() },
            };

            var graph = new SeederDependencyGraph();
            var options = new SeedingOptions { Enabled = true };
            var ordered = graph.GetExecutionOrder(seeders, options).Select(s => s.Id).ToList();

            Assert.Equal(new[] { "A_Seeder", "M_Seeder", "Z_Seeder" }, ordered);
        }

        [Fact]
        public void GetExecutionOrder_DetectsDuplicateIds_ThrowsDuplicateSeederIdException()
        {
            var seeders = new List<IModuleSeeder>
            {
                new TestSeeder { Id = "Module.Duplicate" },
                new TestSeeder { Id = "Module.Duplicate" },
            };

            var graph = new SeederDependencyGraph();
            var options = new SeedingOptions { Enabled = true };
            var ex = Assert.Throws<DuplicateSeederIdException>(() => graph.GetExecutionOrder(seeders, options));
            Assert.Equal("Module.Duplicate", ex.DuplicateId);
        }

        [Fact]
        public void GetExecutionOrder_DetectsMissingDependency_ThrowsMissingDependencyException()
        {
            var seeders = new List<IModuleSeeder>
            {
                new TestSeeder { Id = "Module.A", DependsOn = new[] { "Module.NonExistent" } }
            };

            var graph = new SeederDependencyGraph();
            var options = new SeedingOptions { Enabled = true };
            var ex = Assert.Throws<MissingDependencyException>(() => graph.GetExecutionOrder(seeders, options));
            Assert.Equal("Module.A", ex.SeederId);
            Assert.Equal("Module.NonExistent", ex.MissingDependencyId);
        }

        [Fact]
        public void GetExecutionOrder_DetectsCycles_ThrowsCyclicDependencyExceptionWithCyclePath()
        {
            var seeders = new List<IModuleSeeder>
            {
                new TestSeeder { Id = "Node.A", DependsOn = new[] { "Node.B" } },
                new TestSeeder { Id = "Node.B", DependsOn = new[] { "Node.C" } },
                new TestSeeder { Id = "Node.C", DependsOn = new[] { "Node.A" } },
            };

            var graph = new SeederDependencyGraph();
            var options = new SeedingOptions { Enabled = true };
            var ex = Assert.Throws<CyclicDependencyException>(() => graph.GetExecutionOrder(seeders, options));

            Assert.NotEmpty(ex.CyclePath);
            Assert.Contains("->", ex.Message);
        }
    }
}
