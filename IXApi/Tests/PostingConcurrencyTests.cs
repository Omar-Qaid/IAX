using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IXApi.Tests;

/// <summary>
/// <see cref="SubledgerPostingSupport.LockAsync"/> is a no-op on every provider except SQL Server,
/// so the SQLite suite cannot show that two posters of the same key are actually serialized. These
/// run against a disposable SQL Server database, opted into with the same
/// IXAPI_AP_PHASE_ONE_SQLSERVER variable as the other SQL Server tests.
/// <para>
/// The lock needs a connection and an ambient transaction, not a schema, so the database is created
/// empty rather than through EnsureCreated - which keeps these fast.
/// </para>
/// </summary>
[Collection("AccountsPayable phase one")]
public sealed class PostingConcurrencyTests(Xunit.Abstractions.ITestOutputHelper output)
{
    private const string Key = "VendInvoice:dat:V-0001";

    [SqlServerPhaseOneFact]
    public async Task The_posting_lock_excludes_a_second_poster_of_the_same_key_until_the_first_commits()
    {
        await using var database = await ScratchDatabase.CreateAsync();
        await using var firstDb = database.Context();
        await using var secondDb = database.Context();

        await using var firstTransaction = await firstDb.Database.BeginTransactionAsync();
        await new SubledgerPostingSupport(firstDb).LockAsync(Key, default);

        // The second poster must not get the lock while the first transaction is open.
        var second = Task.Run(async () =>
        {
            await using var transaction = await secondDb.Database.BeginTransactionAsync();
            await new SubledgerPostingSupport(secondDb).LockAsync(Key, default);
            await transaction.CommitAsync();
        });
        var raced = await Task.WhenAny(second, Task.Delay(TimeSpan.FromSeconds(2)));
        Assert.NotSame(second, raced);
        Assert.False(second.IsCompleted);

        // Releasing the first transaction hands the lock over, so the second poster proceeds.
        await firstTransaction.CommitAsync();
        await second.WaitAsync(TimeSpan.FromSeconds(20));
        Assert.True(second.IsCompletedSuccessfully);
    }

    [SqlServerPhaseOneFact]
    public async Task The_posting_lock_is_keyed_so_a_different_key_is_not_blocked()
    {
        await using var database = await ScratchDatabase.CreateAsync();
        await using var firstDb = database.Context();
        await using var secondDb = database.Context();

        await using var firstTransaction = await firstDb.Database.BeginTransactionAsync();
        await new SubledgerPostingSupport(firstDb).LockAsync(Key, default);

        // A different account is a different resource, so posting it must not wait.
        await using var secondTransaction = await secondDb.Database.BeginTransactionAsync();
        var other = new SubledgerPostingSupport(secondDb).LockAsync("VendInvoice:dat:V-0002", default);
        await other.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(other.IsCompletedSuccessfully);
    }

    [SqlServerPhaseOneFact]
    public async Task A_rolled_back_poster_releases_the_lock_rather_than_holding_it()
    {
        await using var database = await ScratchDatabase.CreateAsync();
        await using var firstDb = database.Context();
        await using var secondDb = database.Context();

        var firstTransaction = await firstDb.Database.BeginTransactionAsync();
        await new SubledgerPostingSupport(firstDb).LockAsync(Key, default);
        // A refused posting rolls back; the lock is owned by the transaction, so it must go with it.
        await firstTransaction.RollbackAsync();
        await firstTransaction.DisposeAsync();

        await using var secondTransaction = await secondDb.Database.BeginTransactionAsync();
        var retry = new SubledgerPostingSupport(secondDb).LockAsync(Key, default);
        await retry.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(retry.IsCompletedSuccessfully);
    }

    /// <summary>
    /// The contrast that gives the test above its meaning: on SQLite the lock returns without doing
    /// anything, so two posters of the same key are <b>not</b> serialized. This pins the limitation
    /// rather than leaving it as a comment, and shows the SQL Server assertion is not passing for
    /// some incidental reason.
    /// </summary>
    [Fact]
    public async Task On_SQLite_the_posting_lock_does_not_exclude_anything()
    {
        await using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection).Options);

        await using var transaction = await db.Database.BeginTransactionAsync();
        await new SubledgerPostingSupport(db).LockAsync(Key, default);
        // Same key, same moment, and it still returns: nothing is excluded off SQL Server.
        var again = new SubledgerPostingSupport(db).LockAsync(Key, default);
        await again.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(again.IsCompletedSuccessfully);
    }

    /// <summary>
    /// Number sequence allocation reads <c>NextRec</c> and writes it back inside a Serializable
    /// transaction with no update-lock hint, and nothing configures <c>EnableRetryOnFailure</c>, so
    /// two concurrent allocations of the same sequence can collide. The property that must hold is
    /// that a number is <b>never handed out twice</b>, and additionally that no caller is refused:
    /// allocation used to deadlock here, killing five of eight callers, and nothing retries because
    /// EnableRetryOnFailure is configured nowhere. This is the regression test for that fix.
    /// </summary>
    [SqlServerPhaseOneFact]
    public async Task Concurrent_number_sequence_allocation_never_hands_out_the_same_number_twice()
    {
        await using var database = await ScratchDatabase.CreateAsync();
        await using (var schema = database.Context())
        {
            await schema.Database.EnsureCreatedAsync();
            schema.Add(new SysNumberSequence
            {
                DataAreaId = "dat", NumberSequence = "CONC", Txt = "Concurrency test",
                Format = "CONC-{SEQ}", AnnotatedFormat = "CONC-{SEQ}",
                Lowest = 1, NextRec = 1, Highest = 9999, IsActive = true, InUse = 1,
                Manual = 0, NoIncrement = 0, Blocked = 0
            });
            await schema.SaveChangesAsync();
        }

        // Sanity: one caller on its own must succeed, so a failure below is about contention.
        await using (var single = database.Context())
        {
            var first = await new SysNumberSequenceService(new UnitOfWork(single), new LockTestIdentity())
                .NextAsync("CONC", "dat");
            Assert.Equal(1, (int)first.Value);
        }

        const int callers = 8;
        var values = new System.Collections.Concurrent.ConcurrentBag<int>();
        var failures = new System.Collections.Concurrent.ConcurrentBag<string>();
        await Task.WhenAll(Enumerable.Range(0, callers).Select(_ => Task.Run(async () =>
        {
            await using var db = database.Context();
            var service = new SysNumberSequenceService(new UnitOfWork(db), new LockTestIdentity());
            try { values.Add((int)(await service.NextAsync("CONC", "dat")).Value); }
            catch (Exception ex) { failures.Add(Describe(ex)); }
        })));

        // The invariant: whatever succeeded must be unique.
        Assert.Equal(values.Count, values.Distinct().Count());
        // Before TakeUpdateLockAsync existed this was 3 of 8, the other five killed as deadlock
        // victims. The uniqueness assertion held even then; this one is what the fix added.
        Assert.True(values.Count == callers,
            $"{failures.Count} of {callers} concurrent allocations were refused: " + string.Join(" | ", failures.Distinct()));
        Assert.Equal(Enumerable.Range(2, callers), values.Order());
        // Recorded, not asserted as zero: a refused caller is acceptable, a duplicate is not.
        Assert.Equal(callers, values.Count + failures.Count);
        // Printed rather than asserted: the split between serialized and refused callers is a
        // property of the server's locking, and the invariant above is what must never break.
        output.WriteLine($"{callers} concurrent allocations: {values.Count} succeeded "
            + $"({string.Join(",", values.Order())}), {failures.Count} refused");
        foreach (var failure in failures.Distinct()) output.WriteLine("  refused: " + failure);
    }

    private static string Describe(Exception ex)
    {
        var root = ex;
        while (root.InnerException != null) root = root.InnerException;
        var message = root.Message.ReplaceLineEndings(" ");
        return root.GetType().Name + ": " + message[..Math.Min(140, message.Length)];
    }

    private sealed class LockTestIdentity : IAX.IXApi.Infrastructure.Identity.ICurrentUserService
    {
        public string GetDataAreaId() => "dat";
        public string GetCurrentUserId() => "concurrency-test";
        public string GetOwnerAccountId() => "concurrency-test";
    }

    /// <summary>A disposable, empty SQL Server database, removed on dispose.</summary>
    private sealed class ScratchDatabase : IAsyncDisposable
    {
        private readonly string _name;
        private readonly string _connectionString;

        private ScratchDatabase(string name, string connectionString)
        {
            _name = name;
            _connectionString = connectionString;
        }

        public static async Task<ScratchDatabase> CreateAsync()
        {
            var server = Environment.GetEnvironmentVariable("IXAPI_AP_PHASE_ONE_SQLSERVER")!;
            var name = "IXApi_APV2_Lock_" + Guid.NewGuid().ToString("N");
            await using (var master = new SqlConnection(Master(server)))
            {
                await master.OpenAsync();
                await using var create = master.CreateCommand();
                create.CommandText = $"CREATE DATABASE [{name}]";
                await create.ExecuteNonQueryAsync();
            }
            return new ScratchDatabase(name, new SqlConnectionStringBuilder
            {
                DataSource = server, InitialCatalog = name, IntegratedSecurity = true,
                TrustServerCertificate = true, ConnectTimeout = 30
            }.ConnectionString);
        }

        public ApplicationDbContext Context() => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connectionString).Options);

        public async ValueTask DisposeAsync()
        {
            SqlConnection.ClearAllPools();
            var server = Environment.GetEnvironmentVariable("IXAPI_AP_PHASE_ONE_SQLSERVER")!;
            await using var master = new SqlConnection(Master(server));
            await master.OpenAsync();
            await using var drop = master.CreateCommand();
            // The name is this class's own generated name, never caller input.
            Assert.StartsWith("IXApi_APV2_Lock_", _name);
            drop.CommandText = $"ALTER DATABASE [{_name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_name}];";
            await drop.ExecuteNonQueryAsync();
        }

        private static string Master(string server) => new SqlConnectionStringBuilder
        {
            DataSource = server, InitialCatalog = "master", IntegratedSecurity = true,
            TrustServerCertificate = true, ConnectTimeout = 30
        }.ConnectionString;
    }
}
