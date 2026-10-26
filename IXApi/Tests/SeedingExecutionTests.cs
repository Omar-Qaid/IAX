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

        [Fact]
        public async Task ScopeIsolation_RunOnceExecutesSeparatelyPerScope()
        {
            var (connection, db) = await CreateSqliteDbAsync();
            await using var _ = connection;
            await using var __ = db;

            var seederDat = new MockSeeder
            {
                Id = "Test.ScopedSeeder",
                Version = 1,
                Scope = "DAT",
                Policy = SeederExecutionPolicy.RunOnce
            };
            var seederUsmf = new MockSeeder
            {
                Id = "Test.ScopedSeeder",
                Version = 1,
                Scope = "USMF",
                Policy = SeederExecutionPolicy.RunOnce
            };

            var options = Options.Create(new SeedingOptions { Enabled = true });
            var config = new ConfigurationBuilder().Build();

            // Run for DAT
            var service1 = new DatabaseSeederService(
                db,
                new[] { seederDat },
                new SeederDependencyGraph(),
                new NullSeederLock(),
                options,
                config,
                NullLogger<DatabaseSeederService>.Instance,
                new Microsoft.Extensions.DependencyInjection.ServiceCollection().BuildServiceProvider());

            await service1.SeedAsync();
            Assert.Equal(1, seederDat.ExecutionCount);

            // Run for USMF (same ID and version, different scope)
            var service2 = new DatabaseSeederService(
                db,
                new[] { seederUsmf },
                new SeederDependencyGraph(),
                new NullSeederLock(),
                options,
                config,
                NullLogger<DatabaseSeederService>.Instance,
                new Microsoft.Extensions.DependencyInjection.ServiceCollection().BuildServiceProvider());

            await service2.SeedAsync();
            Assert.Equal(1, seederUsmf.ExecutionCount);

            // Run again for DAT - should be skipped
            await service1.SeedAsync();
            Assert.Equal(1, seederDat.ExecutionCount);

            var logs = await db.Set<SysDataSeedLog>().ToListAsync();
            Assert.Equal(2, logs.Count);
            Assert.Contains(logs, l => l.TableName == "Test.ScopedSeeder:v1:DAT");
            Assert.Contains(logs, l => l.TableName == "Test.ScopedSeeder:v1:USMF");
        }

        [Fact]
        public async Task RetryAfterFailure_SucceedsAndRunsDownstreamSeeders()
        {
            var (connection, db) = await CreateSqliteDbAsync();
            await using var _ = connection;
            await using var __ = db;

            bool shouldFail = true;
            var parent = new MockSeeder
            {
                Id = "Retry.Parent",
                Version = 1,
                Category = SeederCategory.Reference,
                Action = () =>
                {
                    if (shouldFail) throw new InvalidOperationException("First attempt transient failure");
                    return Task.CompletedTask;
                }
            };

            var child = new MockSeeder
            {
                Id = "Retry.Child",
                Version = 1,
                Category = SeederCategory.Reference,
                DependsOn = new[] { "Retry.Parent" }
            };

            var options = Options.Create(new SeedingOptions { Enabled = true });
            var config = new ConfigurationBuilder().Build();
            var service = new DatabaseSeederService(
                db,
                new[] { parent, child },
                new SeederDependencyGraph(),
                new NullSeederLock(),
                options,
                config,
                NullLogger<DatabaseSeederService>.Instance,
                new Microsoft.Extensions.DependencyInjection.ServiceCollection().BuildServiceProvider());

            // Run 1: Failure occurs
            await service.SeedAsync();
            Assert.Equal(1, parent.ExecutionCount);
            Assert.Equal(0, child.ExecutionCount); // Child skipped

            // Run 2: Transient issue resolved
            shouldFail = false;
            await service.SeedAsync();
            Assert.Equal(2, parent.ExecutionCount); // Retried
            Assert.Equal(1, child.ExecutionCount); // Child now successfully executes!
        }

        [Fact]
        public async Task ConcurrentLockCoordination_SerializesExecution()
        {
            var (connection, db) = await CreateSqliteDbAsync();
            await using var _ = connection;
            await using var __ = db;

            var lockProvider1 = new SqlServerSeederLock(db, NullLogger<SqlServerSeederLock>.Instance);
            var lockProvider2 = new SqlServerSeederLock(db, NullLogger<SqlServerSeederLock>.Instance);

            int concurrentCount = 0;
            int maxConcurrentObserved = 0;

            async Task Worker(ISeederLock locker)
            {
                await using var handle = await locker.AcquireLockAsync(10, CancellationToken.None);
                var current = Interlocked.Increment(ref concurrentCount);
                lock (locker)
                {
                    if (current > maxConcurrentObserved) maxConcurrentObserved = current;
                }
                await Task.Delay(50);
                Interlocked.Decrement(ref concurrentCount);
            }

            var task1 = Task.Run(() => Worker(lockProvider1));
            var task2 = Task.Run(() => Worker(lockProvider2));

            await Task.WhenAll(task1, task2);

            Assert.Equal(1, maxConcurrentObserved);
        }

        [Fact]
        public async Task IdentityInsertHelper_PropagatesActionException_OnNonSqlServer()
        {
            var (connection, db) = await CreateSqliteDbAsync();
            await using var _ = connection;
            await using var __ = db;

            var helper = new IdentityInsertHelper();

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await helper.ExecuteWithIdentityInsertAsync(db, "CustTable", () =>
                {
                    throw new InvalidOperationException("Insert error");
                });
            });

            Assert.Equal("Insert error", ex.Message);
        }
    }
}
