using System.Diagnostics;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;

/// <summary>
/// The startup entry point. Runs the baseline profile in dependency order, each seeder in its own transaction
/// together with its history row, so a failed seeder never leaves half its rows behind. Imports run once per
/// version and scope; everything else only adds what is missing, so a rerun adds nothing.
/// </summary>
public sealed class BaselineSeedRunner(ApplicationDbContext db, IEnumerable<IBaselineSeeder> seeders,
    IOptions<SeedingOptions> options, ILogger<BaselineSeedRunner> logger, IHostEnvironment? environment = null)
{
    public const string Actor = "sys";

    public async Task<SeedResult> RunAsync(CancellationToken ct = default)
    {
        if (environment?.IsProduction() == true)
            throw new InvalidOperationException("Database seeding cannot be executed in the Production environment.");
        var settings = options.Value;
        var eligible = seeders.Where(x => x.Kind != BaselineSeederKind.LegacyImport || settings.EnableLegacyImports).ToArray();
        var ordered = new SeedDependencyResolver().Resolve(eligible).Cast<IBaselineSeeder>().ToArray();
        var history = new SeedExecutionHistory();
        await using var seedLock = await new SeedLock(db).AcquireAsync(settings.LockTimeoutSeconds, ct);
        var context = new SeedContext(db, DateTime.UtcNow.Year, Actor, settings.DefaultCompany);
        var completed = new List<string>();
        foreach (var seeder in ordered)
        {
            var policy = seeder.Kind == BaselineSeederKind.LegacyImport ? "RunOnce" : "Repeatable";
            if (policy == "RunOnce" && await history.CompletedAsync(db, seeder.Id, seeder.Version, seeder.Scope, ct)) continue;
            var watch = Stopwatch.StartNew();
            try
            {
                await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
                {
                    db.ChangeTracker.Clear();
                    await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
                    await seeder.SeedAsync(context, ct);
                    await history.RecordAsync(db, seeder.Id, seeder.Version, seeder.Scope, policy, ct, watch.ElapsedMilliseconds);
                    if (transaction != null) await transaction.CommitAsync(ct);
                });
                completed.Add(seeder.Id);
                logger.LogInformation("Seeder {SeederId} completed in {Elapsed} ms.", seeder.Id, watch.ElapsedMilliseconds);
            }
            catch (Exception error) when (error is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                logger.LogError(error, "Seeder {SeederId} failed.", seeder.Id);
                await history.RecordFailureAsync(db, seeder.Id, seeder.Version, seeder.Scope, policy, error, watch.ElapsedMilliseconds);
                throw new InvalidOperationException($"Baseline seeder '{seeder.Id}' failed: {error.Message}", error);
            }
        }
        return new SeedResult(context.Company, completed.ToArray());
    }
}
