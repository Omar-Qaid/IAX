using System.Data;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;

/// <summary>Explicit phase-one entry point; never invoked by normal application startup.</summary>
public sealed class SeedRunner(ApplicationDbContext db, IEnumerable<IDataSeeder> seeders, IEnumerable<ISeedValidator> validators)
{
    public async Task<SeedResult> RunAsync(int fiscalYear, string actor, CancellationToken ct = default)
    {
        if (fiscalYear is < 1 or > 9999) throw new ArgumentOutOfRangeException(nameof(fiscalYear));
        if (string.IsNullOrWhiteSpace(actor)) throw new ArgumentException("An audit actor is required.", nameof(actor));
        if (db.ChangeTracker.Entries().Any()) throw new InvalidOperationException("Run seeding in a fresh DI scope.");
        var ordered = new SeedDependencyResolver().Resolve(seeders);
        if (ordered.Count == 0) throw new InvalidOperationException("No phase-one seeders registered.");
        await using var seedLock = await new SqlServerSeederLock(db).AcquireLockAsync(120, ct);
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            try
            {
                var context = new SeedContext(db, fiscalYear, actor);
                foreach (var seeder in ordered)
                {
                    await seeder.SeedAsync(context, ct);
                    await new SeedExecutionHistory().RecordAsync(db, seeder.Id, context.Company, ct);
                }
                foreach (var validator in validators)
                    await validator.ValidateAsync(context, ct);
                await transaction.CommitAsync(ct);
                return new SeedResult(context.Company, ordered.Select(x => x.Id).ToArray());
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                db.ChangeTracker.Clear();
                throw;
            }
        });
    }
}
