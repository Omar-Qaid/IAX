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
            var completedLogs = await _db.SysDataSeedLogs.AsNoTracking()
                .Where(l => l.Outcome == "Success")
                .ToListAsync(ct);

            var completedTuples = completedLogs
                .Where(l => !string.IsNullOrEmpty(l.SeederId))
                .Select(l => (l.SeederId!, l.Version, l.Scope))
                .ToHashSet();

            var context = new SeedingContext(
                _db,
                _roles,
                _users,
                _configuration,
                _environment,
                _logger,
                _serviceProvider,
                _options.DefaultCompany,
                _identityInsert,
                _options);

            var failedSeederIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var failures = new List<Exception>();

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
                    await RecordLogAsync(seeder.Id, seeder.Version, seeder.Scope, seeder.Policy.ToString(), "Skipped", 0, $"Skipped due to prerequisite failure: {failedDep}", ct);
                    continue;
                }

                // Check RunOnce policy
                if (seeder.Policy == SeederExecutionPolicy.RunOnce)
                {
                    var isCompleted = completedTuples.Contains((seeder.Id, seeder.Version, seeder.Scope))
                        || completedLogs.Any(l => l.SeederId == null &&
                            (string.Equals(l.TableName, seeder.Id, StringComparison.OrdinalIgnoreCase)
                             || string.Equals(l.TableName, $"{seeder.Id}:v{seeder.Version}:{seeder.Scope}", StringComparison.OrdinalIgnoreCase)));

                    if (isCompleted)
                    {
                        _logger.LogDebug("Skipping RunOnce seeder '{SeederId}' (already completed v{Version} for scope '{Scope}').", seeder.Id, seeder.Version, seeder.Scope);
                        continue;
                    }
                }

                _logger.LogInformation("Executing seeder '{SeederId}' (v{Version}, Category: {Category}, Scope: {Scope})...", seeder.Id, seeder.Version, seeder.Category, seeder.Scope);
                var sw = Stopwatch.StartNew();

                try
                {
                    var strategy = _db.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () =>
                    {
                        // The transaction must be created inside the execution strategy.
                        await using var tx = _db.Database.IsRelational()
                            ? await _db.Database.BeginTransactionAsync(ct)
                            : null;
                        try
                        {
                            await seeder.SeedAsync(context, ct);
                            // Commit execution history atomically with seeded data.
                            await RecordLogAsync(seeder.Id, seeder.Version, seeder.Scope,
                                seeder.Policy.ToString(), "Success", sw.ElapsedMilliseconds, null, ct);
                            if (tx != null)
                                await tx.CommitAsync(ct);
                        }
                        catch
                        {
                            if (tx != null)
                                await tx.RollbackAsync(CancellationToken.None);
                            _db.ChangeTracker.Clear();
                            throw;
                        }
                    });

                    if (seeder.Policy == SeederExecutionPolicy.RunOnce)
                        completedTuples.Add((seeder.Id, seeder.Version, seeder.Scope));

                    sw.Stop();
                    _logger.LogInformation("Completed seeder '{SeederId}' in {ElapsedMs} ms.", seeder.Id, sw.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    _logger.LogError(ex, "Seeder '{SeederId}' failed after {ElapsedMs} ms.", seeder.Id, sw.ElapsedMilliseconds);

                    _db.ChangeTracker.Clear();

                    // Record failure after rollback without marking incomplete work as successful
                    try
                    {
                        await RecordLogAsync(seeder.Id, seeder.Version, seeder.Scope, seeder.Policy.ToString(), "Failed", sw.ElapsedMilliseconds, ex.ToString(), CancellationToken.None);
                    }
                    catch (Exception logException)
                    {
                        _logger.LogError(logException, "Could not record failure of seeder {SeederId}", seeder.Id);
                    }

                    failedSeederIds.Add(seeder.Id);
                    failures.Add(new InvalidOperationException($"Seeder '{seeder.Id}' failed.", ex));

                    // If seeder is required, stop startup immediately
                    if (seeder.Category == SeederCategory.Required)
                    {
                        throw new InvalidOperationException($"Required seeder '{seeder.Id}' failed: {ex.Message}", ex);
                    }
                }
            }

            _logger.LogInformation("Database seeding completed. {SuccessCount} succeeded, {FailedCount} failed.",
                orderedSeeders.Count - failedSeederIds.Count,
                failedSeederIds.Count);

            if (failures.Count > 0)
                throw new AggregateException("One or more seeders failed; database initialization is incomplete.", failures);
        }

        private async Task RecordLogAsync(
            string seederId,
            int version,
            string scope,
            string policy,
            string outcome,
            long durationMs,
            string? error,
            CancellationToken ct)
        {
                var log = new SysDataSeedLog
                {
                    SeederId = seederId,
                    Version = version,
                    Scope = scope,
                    Policy = policy,
                    Outcome = outcome,
                    DurationMs = durationMs,
                    Error = error,
                    TableName = $"{seederId}:v{version}:{scope}",
                    CreatedAt = DateTime.UtcNow
                };

                _db.SysDataSeedLogs.Add(log);
                await _db.SaveChangesAsync(ct);
        }

    }
}
