using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;

/// <summary>
/// Serializes seeding: in process always, and across API instances with a SQL Server application lock held on its
/// own connection, because a seeder may close the context connection while it writes identity-valued rows.
/// </summary>
public sealed class SeedLock(ApplicationDbContext db)
{
    private const string Resource = "IXApi_DatabaseSeeder";
    private static readonly SemaphoreSlim ProcessLock = new(1, 1);

    public async Task<IAsyncDisposable> AcquireAsync(int timeoutSeconds, CancellationToken ct)
    {
        var seconds = Math.Clamp(timeoutSeconds, 1, int.MaxValue / 1000);
        if (!await ProcessLock.WaitAsync(seconds * 1000, ct))
            throw new TimeoutException($"Seeding lock not acquired within {seconds} seconds.");
        if (!db.Database.IsSqlServer())
            return new Release(() => { ProcessLock.Release(); return ValueTask.CompletedTask; });

        var connection = new SqlConnection(db.Database.GetConnectionString());
        try
        {
            await connection.OpenAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandTimeout = seconds + 5;
            command.CommandText = "DECLARE @r INT; EXEC @r = sp_getapplock @Resource = @res, @LockMode = 'Exclusive', "
                + "@LockOwner = 'Session', @LockTimeout = @timeout; SELECT @r;";
            command.Parameters.AddWithValue("@res", Resource);
            command.Parameters.AddWithValue("@timeout", seconds * 1000);
            var result = Convert.ToInt32(await command.ExecuteScalarAsync(ct));
            if (result == -1)
                throw new TimeoutException($"Timed out after {seconds} seconds waiting for '{Resource}'. Another instance may be seeding.");
            if (result < 0) throw new InvalidOperationException($"sp_getapplock for '{Resource}' returned {result}.");
        }
        catch
        {
            await connection.DisposeAsync();
            ProcessLock.Release();
            throw;
        }
        return new Release(async () =>
        {
            try
            {
                if (connection.State == ConnectionState.Open)
                {
                    await using var release = connection.CreateCommand();
                    release.CommandText = "EXEC sp_releaseapplock @Resource = @res, @LockOwner = 'Session';";
                    release.Parameters.AddWithValue("@res", Resource);
                    await release.ExecuteNonQueryAsync(CancellationToken.None);
                }
            }
            finally
            {
                await connection.DisposeAsync();
                ProcessLock.Release();
            }
        });
    }

    private sealed class Release(Func<ValueTask> action) : IAsyncDisposable
    {
        private int disposed;
        public ValueTask DisposeAsync() => Interlocked.Exchange(ref disposed, 1) == 0 ? action() : ValueTask.CompletedTask;
    }
}
