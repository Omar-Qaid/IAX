using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.Seeding;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace IXApi.Tests
{
    public class SeedingExecutionTests
    {
        private sealed class TestAppDbContext : ApplicationDbContext
        {
            public TestAppDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

            protected override void OnModelCreating(ModelBuilder b)
            {
                var allowed = new[] { typeof(SysDataSeedLog) };
                foreach (var type in b.Model.GetEntityTypes().Where(x => !allowed.Contains(x.ClrType)).Select(x => x.ClrType).ToList())
                {
                    b.Ignore(type);
                }

                b.Entity<SysDataSeedLog>(e =>
                {
                    e.ToTable("SysDataSeedLogs");
                    e.HasKey(x => x.RecId);
                    e.Property(x => x.TableName).IsRequired();
                });
            }
        }

        private class MockSeeder : IModuleSeeder
        {
            public string Id { get; set; } = string.Empty;
            public int Version { get; set; } = 1;
            public SeederCategory Category { get; set; } = SeederCategory.Required;
            public IReadOnlyCollection<string> DependsOn { get; set; } = Array.Empty<string>();
            public SeederExecutionPolicy Policy { get; set; } = SeederExecutionPolicy.Repeatable;
            public string Scope { get; set; } = "global";

            public int ExecutionCount { get; private set; }
            public Func<Task>? Action { get; set; }

            public async Task SeedAsync(SeedingContext context, CancellationToken cancellationToken)
            {
                ExecutionCount++;
                if (Action != null)
                {
                    await Action();
                }
            }
        }

        private class NullSeederLock : ISeederLock
        {
            public Task<IAsyncDisposable> AcquireLockAsync(int timeoutSeconds, CancellationToken cancellationToken)
            {
                return Task.FromResult<IAsyncDisposable>(new NullDisposable());
            }

            private class NullDisposable : IAsyncDisposable
            {
                public ValueTask DisposeAsync() => ValueTask.CompletedTask;
            }
        }

        private async Task<(SqliteConnection Connection, ApplicationDbContext Db)> CreateSqliteDbAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            var db = new TestAppDbContext(options);
            await db.Database.EnsureCreatedAsync();
            return (connection, db);
        }

        [Fact]
        public async Task RunOnceSeeder_ExecutesOnlyOnce_UnlessVersionIncrements()
        {
            var (connection, db) = await CreateSqliteDbAsync();
            await using var _ = connection;
            await using var __ = db;

            var seeder = new MockSeeder
            {
                Id = "Test.RunOnce",
                Version = 1,
                Policy = SeederExecutionPolicy.RunOnce
            };

            var options = Options.Create(new SeedingOptions { Enabled = true });
            var config = new ConfigurationBuilder().Build();
            var service = new DatabaseSeederService(
                db,
                new[] { seeder },
                new SeederDependencyGraph(),
                new NullSeederLock(),
                options,
                config,
                NullLogger<DatabaseSeederService>.Instance,
                new Microsoft.Extensions.DependencyInjection.ServiceCollection().BuildServiceProvider());

            // Run 1
            await service.SeedAsync();
            Assert.Equal(1, seeder.ExecutionCount);

            var logs = await db.Set<SysDataSeedLog>().ToListAsync();
            Assert.Single(logs);
            Assert.Equal("Test.RunOnce:v1:global", logs[0].TableName);

            // Run 2 (no version change)
            await service.SeedAsync();
            Assert.Equal(1, seeder.ExecutionCount); // Still 1

            // Run 3 (version increment)
            seeder.Version = 2;
            await service.SeedAsync();
            Assert.Equal(2, seeder.ExecutionCount); // Incremented
        }

        [Fact]
        public async Task RepeatableSeeder_ExecutesOnEveryRun()
        {
            var (connection, db) = await CreateSqliteDbAsync();
            await using var _ = connection;
            await using var __ = db;

            var seeder = new MockSeeder
            {
                Id = "Test.Repeatable",
                Version = 1,
                Policy = SeederExecutionPolicy.Repeatable
            };

            var options = Options.Create(new SeedingOptions { Enabled = true });
            var config = new ConfigurationBuilder().Build();
            var service = new DatabaseSeederService(
                db,
                new[] { seeder },
                new SeederDependencyGraph(),
                new NullSeederLock(),
                options,
                config,
                NullLogger<DatabaseSeederService>.Instance,
                new Microsoft.Extensions.DependencyInjection.ServiceCollection().BuildServiceProvider());

            await service.SeedAsync();
            await service.SeedAsync();

            Assert.Equal(2, seeder.ExecutionCount);
        }

        [Fact]
        public async Task SeederFailure_RecordsFailureAndSkipsDependents()
        {
            var (connection, db) = await CreateSqliteDbAsync();
            await using var _ = connection;
            await using var __ = db;

            var failingSeeder = new MockSeeder
            {
                Id = "Parent.Fail",
                Version = 1,
                Category = SeederCategory.Reference,
                Action = () => throw new InvalidOperationException("Simulation failure")
            };
            var dependentSeeder = new MockSeeder
            {
                Id = "Child.Dependent",
                Version = 1,
                Category = SeederCategory.Reference,
                DependsOn = new[] { "Parent.Fail" }
            };

            var options = Options.Create(new SeedingOptions { Enabled = true });
            var config = new ConfigurationBuilder().Build();
            var service = new DatabaseSeederService(
                db,
                new[] { failingSeeder, dependentSeeder },
                new SeederDependencyGraph(),
                new NullSeederLock(),
                options,
                config,
                NullLogger<DatabaseSeederService>.Instance,
                new Microsoft.Extensions.DependencyInjection.ServiceCollection().BuildServiceProvider());

            await service.SeedAsync();

            Assert.Equal(1, failingSeeder.ExecutionCount);
            Assert.Equal(0, dependentSeeder.ExecutionCount); // Skipped because parent failed

            var logs = await db.Set<SysDataSeedLog>().ToListAsync();
            Assert.Contains(logs, l => l.TableName.Contains("Parent.Fail"));
        }

        [Fact]
        public void ExcludedDependency_ThrowsExcludedDependencyException()
        {
            var graph = new SeederDependencyGraph();
            var seeders = new List<IModuleSeeder>
            {
                new MockSeeder { Id = "Parent.Demo", Category = SeederCategory.Demo },
                new MockSeeder { Id = "Child.Required", Category = SeederCategory.Required, DependsOn = new[] { "Parent.Demo" } }
            };

            var options = new SeedingOptions { Enabled = true, EnableDemoData = false };
            var ex = Assert.Throws<ExcludedDependencyException>(() => graph.GetExecutionOrder(seeders, options));
            Assert.Equal("Child.Required", ex.DependentId);
            Assert.Equal("Parent.Demo", ex.ExcludedDependencyId);
            Assert.Equal(SeederCategory.Demo, ex.ExcludedCategory);
        }
    }
}
