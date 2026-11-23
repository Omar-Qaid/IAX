using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests;

public class BatchDatabaseLockTests
{
    private static ApplicationDbContext Context() => new(new DbContextOptionsBuilder<ApplicationDbContext>()
        // These tests only acquire uniquely named application locks; no application schema
        // or data is needed. Use master rather than requiring the hosted ERM database.
        .UseSqlServer(TestEnvironment.SqlServerConnectionString("master")).Options);

    [SqlServerBatchFact]
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

    [SqlServerBatchFact]
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
