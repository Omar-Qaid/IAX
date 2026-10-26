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
                    e.HasIndex(x => new { x.SeederId, x.Version, x.Scope }).IsUnique()
                        .HasFilter("[SeederId] IS NOT NULL AND [Policy] = 'RunOnce' AND [Outcome] = 'Success'");
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

        private sealed class LegacyBridgeSeeder : BaseModuleSeeder
        {
            public override string Id => "Test.LegacyBridge";
            public override SeederCategory Category => SeederCategory.Required;
            public int Calls { get; private set; }

            public override Task SeedAsync(
                ApplicationDbContext db,
                Microsoft.AspNetCore.Identity.RoleManager<IAX.IXApi.Modules.Identity.Roles.AspNetRole> roles,
                Microsoft.AspNetCore.Identity.UserManager<IAX.IXApi.Modules.Identity.Users.AspNetUser> users,
                CancellationToken ct)
            {
                Calls++;
                return Task.CompletedTask;
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
        public async Task LegacyBridge_InvokesTheConcreteSeeder()
        {
            var (connection, db) = await CreateSqliteDbAsync();
            await using var _ = connection;
            await using var __ = db;
            var seeder = new LegacyBridgeSeeder();

            await seeder.SeedAsync(SeedingContext.CreateDefault(db), CancellationToken.None);

            Assert.Equal(1, seeder.Calls);
        }

        [Fact]
        public void LegacySeedMethods_DoNotHideTheBaseImplementation()
        {
            var methods = typeof(BaseModuleSeeder).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(BaseModuleSeeder)))
                .Select(type => type.GetMethod("SeedAsync", new[]
                {
                    typeof(ApplicationDbContext),
                    typeof(Microsoft.AspNetCore.Identity.RoleManager<IAX.IXApi.Modules.Identity.Roles.AspNetRole>),
                    typeof(Microsoft.AspNetCore.Identity.UserManager<IAX.IXApi.Modules.Identity.Users.AspNetUser>),
                    typeof(CancellationToken)
                }))
                .Where(method => method is not null && method.DeclaringType != typeof(BaseModuleSeeder))
                .ToList();

            Assert.All(methods, method =>
                Assert.Equal(typeof(BaseModuleSeeder), method!.GetBaseDefinition().DeclaringType));
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
            Assert.Equal("Test.RunOnce", logs[0].SeederId);
            Assert.Equal(1, logs[0].Version);
            Assert.Equal("global", logs[0].Scope);
            Assert.Equal("RunOnce", logs[0].Policy);
            Assert.Equal("Success", logs[0].Outcome);

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

            var error = await Record.ExceptionAsync(() => service.SeedAsync());
            Assert.Equal(1, failingSeeder.ExecutionCount);
            Assert.True(error is AggregateException,
                $"Unexpected error: {error}; logs: {string.Join(",", (await db.SysDataSeedLogs.ToListAsync()).Select(x => x.Outcome))}");
            Assert.Equal(0, dependentSeeder.ExecutionCount); // Skipped because parent failed

            var logs = await db.Set<SysDataSeedLog>().ToListAsync();
            Assert.Contains(logs, l => l.TableName.Contains("Parent.Fail"));
        }

        [Fact]
        public async Task FailedSeeder_RollsBackItsRowsAndCanRetry()
        {
            var (connection, db) = await CreateSqliteDbAsync();
            await using var _ = connection;
            await using var __ = db;
            var shouldFail = true;
            var seeder = new MockSeeder
            {
                Id = "Test.Retry",
                Category = SeederCategory.Reference,
                Policy = SeederExecutionPolicy.RunOnce,
                Action = async () =>
                {
                    db.SysDataSeedLogs.Add(new SysDataSeedLog { TableName = "partial-data" });
                    await db.SaveChangesAsync();
                    if (shouldFail) throw new InvalidOperationException("failed after saving");
                }
            };
            var service = new DatabaseSeederService(db, new[] { seeder }, new SeederDependencyGraph(),
                new NullSeederLock(), Options.Create(new SeedingOptions { Enabled = true }),
                new ConfigurationBuilder().Build(), NullLogger<DatabaseSeederService>.Instance,
                new ServiceCollection().BuildServiceProvider());

            var error = await Record.ExceptionAsync(() => service.SeedAsync());
            Assert.Equal(1, seeder.ExecutionCount);
            Assert.True(error is AggregateException,
                $"Unexpected error: {error}; logs: {string.Join(",", (await db.SysDataSeedLogs.ToListAsync()).Select(x => x.Outcome))}");
            Assert.False(await db.SysDataSeedLogs.AnyAsync(x => x.TableName == "partial-data"));
            Assert.Equal(1, await db.SysDataSeedLogs.CountAsync(x => x.Outcome == "Failed"));

            shouldFail = false;
            await service.SeedAsync();
            Assert.Equal(2, seeder.ExecutionCount);
            Assert.True(await db.SysDataSeedLogs.AnyAsync(x => x.TableName == "partial-data"));
            Assert.Equal(1, await db.SysDataSeedLogs.CountAsync(x => x.SeederId == "Test.Retry" && x.Outcome == "Success"));
        }

        [Fact]
        public async Task RunOnceSuccess_HasDatabaseUniquenessProtection()
        {
            var (connection, db) = await CreateSqliteDbAsync();
            await using var _ = connection;
            await using var __ = db;
            Assert.Contains(db.Model.FindEntityType(typeof(SysDataSeedLog))!.GetIndexes(),
                index => index.IsUnique && index.Properties.Count == 3);

            db.SysDataSeedLogs.Add(new SysDataSeedLog
            {
                TableName = "first", SeederId = "Test.Unique", Version = 1,
                Scope = "dat", Policy = "RunOnce", Outcome = "Success"
            });
            await db.SaveChangesAsync();
            db.SysDataSeedLogs.Add(new SysDataSeedLog
            {
                TableName = "second", SeederId = "Test.Unique", Version = 1,
                Scope = "dat", Policy = "RunOnce", Outcome = "Success"
            });

            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
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
