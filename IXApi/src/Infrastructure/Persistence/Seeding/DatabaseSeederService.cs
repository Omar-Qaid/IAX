using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding
{
    public class DatabaseSeederService : IDatabaseSeederService
    {
        private readonly ApplicationDbContext _db;
        private readonly IEnumerable<IModuleSeeder> _seeders;
        private readonly ISeederDependencyGraph _graph;
        private readonly ISeederLock _seederLock;
        private readonly SeedingOptions _options;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment? _environment;
        private readonly ILogger<DatabaseSeederService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly RoleManager<AspNetRole>? _roles;
        private readonly UserManager<AspNetUser>? _users;
        private readonly IIdentityInsertService _identityInsert;

        public DatabaseSeederService(
            ApplicationDbContext db,
            IEnumerable<IModuleSeeder> seeders,
            ISeederDependencyGraph graph,
            ISeederLock seederLock,
            IOptions<SeedingOptions> options,
            IConfiguration configuration,
            ILogger<DatabaseSeederService> logger,
            IServiceProvider serviceProvider,
            IHostEnvironment? environment = null,
            RoleManager<AspNetRole>? roles = null,
            UserManager<AspNetUser>? users = null,
            IIdentityInsertService? identityInsert = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _seeders = seeders ?? Enumerable.Empty<IModuleSeeder>();
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _seederLock = seederLock ?? throw new ArgumentNullException(nameof(seederLock));
            _options = options?.Value ?? new SeedingOptions();
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _environment = environment;
            _roles = roles;
            _users = users;
            _identityInsert = identityInsert ?? new IdentityInsertHelper();
        }

        public async Task SeedAsync(CancellationToken ct = default)
        {
            // Production environment guard
            if (_environment?.IsProduction() == true)
            {
                throw new InvalidOperationException("Database seeding cannot be executed in the Production environment.");
            }

            _logger.LogInformation("Beginning database seeding process. Distributed lock acquisition requested...");

            // Acquire cross-instance synchronization lock
            await using var lockHandle = await _seederLock.AcquireLockAsync(_options.LockTimeoutSeconds, ct);
            _logger.LogInformation("Startup lock acquired. Validating seeder dependency graph...");

            // 1. Validate complete dependency graph and determine deterministic topological execution order
            var orderedSeeders = _graph.GetExecutionOrder(_seeders, _options);
            _logger.LogInformation("Resolved {Count} eligible seeders in deterministic dependency order.", orderedSeeders.Count);

            // 2. Fetch existing successful seed executions
            var completedLogs = await _db.SysDataSeedLogs
                .AsNoTracking()
                .ToListAsync(ct);

            var completedTuples = completedLogs
                .Where(l => !string.IsNullOrEmpty(l.SeederId))
                .Select(l => (l.SeederId!, l.Version, l.Scope ?? "global"))
                .ToHashSet();

            var completedTableNames = completedLogs
                .Select(l => l.TableName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var context = new SeedingContext(
                _db,
                _roles,
                _users,
                _configuration,
                _environment,
                _logger,
                _serviceProvider,
                _options.DefaultCompany,
                _identityInsert);

            var failedSeederIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 3. Execute seeders sequentially in validated topological order
            foreach (var seeder in orderedSeeders)
            {
                ct.ThrowIfCancellationRequested();

                // Check if any prerequisite dependency failed during this run
                var failedDep = seeder.DependsOn?.FirstOrDefault(d => failedSeederIds.Contains(d));
                if (failedDep != null)
                {
                    _logger.LogWarning("Skipping seeder '{SeederId}' because prerequisite dependency '{FailedDep}' failed.", seeder.Id, failedDep);
                    failedSeederIds.Add(seeder.Id);
                    await RecordLogAsync(seeder.Id, seeder.Version, seeder.Scope, "Skipped", 0, $"Skipped due to prerequisite failure: {failedDep}", ct);
                    continue;
                }

                // Check RunOnce policy
                if (seeder.Policy == SeederExecutionPolicy.RunOnce)
                {
                    var isCompleted = completedTuples.Contains((seeder.Id, seeder.Version, seeder.Scope))
                        || completedTableNames.Contains(seeder.Id)
                        || completedTableNames.Contains($"{seeder.Id}:v{seeder.Version}:{seeder.Scope}");

                    if (isCompleted)
                    {
                        _logger.LogDebug("Skipping RunOnce seeder '{SeederId}' (already completed v{Version} for scope '{Scope}').", seeder.Id, seeder.Version, seeder.Scope);
                        continue;
                    }
                }

                _logger.LogInformation("Executing seeder '{SeederId}' (v{Version}, Category: {Category}, Scope: {Scope})...", seeder.Id, seeder.Version, seeder.Category, seeder.Scope);
                var sw = Stopwatch.StartNew();

                // Transaction management: use relational transaction if supported
                var canUseTransaction = _db.Database.IsRelational() && !_db.Database.ProviderName!.Contains("InMemory", StringComparison.OrdinalIgnoreCase);
                Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? tx = null;

                if (canUseTransaction)
                {
                    tx = await _db.Database.BeginTransactionAsync(ct);
                }

                try
                {
                    await seeder.SeedAsync(context, ct);

                    // Commit seeder execution history atomically with seeded data
                    await RecordLogAsync(seeder.Id, seeder.Version, seeder.Scope, "Success", sw.ElapsedMilliseconds, null, ct);

                    if (tx != null)
                    {
                        await tx.CommitAsync(ct);
                    }

                    sw.Stop();
                    _logger.LogInformation("Completed seeder '{SeederId}' in {ElapsedMs} ms.", seeder.Id, sw.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    _logger.LogError(ex, "Seeder '{SeederId}' failed after {ElapsedMs} ms.", seeder.Id, sw.ElapsedMilliseconds);

                    if (tx != null)
                    {
                        await tx.RollbackAsync(ct);
                    }

                    _db.ChangeTracker.Clear();

                    // Record failure after rollback without marking incomplete work as successful
                    await RecordLogAsync(seeder.Id, seeder.Version, seeder.Scope, "Failed", sw.ElapsedMilliseconds, ex.ToString(), CancellationToken.None);

                    failedSeederIds.Add(seeder.Id);

                    // If seeder is required, stop startup immediately
                    if (seeder.Category == SeederCategory.Required)
                    {
                        throw new InvalidOperationException($"Required seeder '{seeder.Id}' failed: {ex.Message}", ex);
                    }
                }
                finally
                {
                    if (tx != null)
                    {
                        await tx.DisposeAsync();
                    }
                }
            }

            _logger.LogInformation("Database seeding completed. {SuccessCount} succeeded, {FailedCount} failed.",
                orderedSeeders.Count - failedSeederIds.Count,
                failedSeederIds.Count);
        }

        private async Task RecordLogAsync(
            string seederId,
            int version,
            string scope,
            string outcome,
            long durationMs,
            string? error,
            CancellationToken ct)
        {
            try
            {
                var log = new SysDataSeedLog
                {
                    SeederId = seederId,
                    Version = version,
                    Scope = scope,
                    Outcome = outcome,
                    DurationMs = durationMs,
                    Error = error,
                    TableName = $"{seederId}:v{version}:{scope}",
                    CreatedAt = DateTime.UtcNow
                };

                _db.SysDataSeedLogs.Add(log);
                await _db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to persist execution log for seeder {SeederId}", seederId);
            }
        }

        /// <summary>
        /// Utility to run a block of code only once based on a persistent log. Preserved for backward compatibility.
        /// </summary>
        public async Task RunOnceAsync(string seedName, Func<Task> action, CancellationToken ct = default)
        {
            var already = await _db.SysDataSeedLogs.AnyAsync(s => s.TableName == seedName, ct);
            if (already) return;

            await action();

            _db.SysDataSeedLogs.Add(new SysDataSeedLog
            {
                TableName = seedName,
                SeederId = seedName,
                Version = 1,
                Scope = "global",
                Outcome = "Success",
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync(ct);
        }
    }
}
