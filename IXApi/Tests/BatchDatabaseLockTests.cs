using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IXApi.Tests;

public sealed class LocalBatchSqlFactAttribute : FactAttribute
{
    public LocalBatchSqlFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("IAX_BATCH_SQL_TESTS") != "1")
            Skip = "Opt in to session-lock tests against local ERM with IAX_BATCH_SQL_TESTS=1.";
    }
}

public class BatchDatabaseLockTests
{
    private static ApplicationDbContext Context() => new(new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlServer("Server=.;Database=ERM;Integrated Security=true;TrustServerCertificate=true").Options);

    [LocalBatchSqlFact]
    public async Task SeparateConnectionsExcludeSameResourceAndReleaseOnDispose()
    {
        using var firstDb = Context();
        using var secondDb = Context();
        var resource = $"Batch:Verification:{Guid.NewGuid()}";
        var first = await BatchDatabaseLock.TryAcquireAsync(firstDb, resource, CancellationToken.None);
        Assert.NotNull(first);
        try
        {
            await first.CheckAsync(CancellationToken.None);
            await using var blocked = await BatchDatabaseLock.TryAcquireAsync(secondDb, resource, CancellationToken.None);
            Assert.Null(blocked);
            await using var other = await BatchDatabaseLock.TryAcquireAsync(secondDb, resource + ":other", CancellationToken.None);
            Assert.NotNull(other);
        }
        finally { await first.DisposeAsync(); }
        await using var acquired = await BatchDatabaseLock.TryAcquireAsync(secondDb, resource, CancellationToken.None);
        Assert.NotNull(acquired);
        await acquired.CheckAsync(CancellationToken.None);
    }

    [LocalBatchSqlFact]
    public async Task CancelledAcquisitionDoesNotRetainLock()
    {
        using var db = Context();
        var resource = $"Batch:Verification:{Guid.NewGuid()}";
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            BatchDatabaseLock.TryAcquireAsync(db, resource, new CancellationToken(true)));
        await using var acquired = await BatchDatabaseLock.TryAcquireAsync(db, resource, CancellationToken.None);
        Assert.NotNull(acquired);
    }
}
