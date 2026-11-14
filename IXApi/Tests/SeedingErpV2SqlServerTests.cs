using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger.Data;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IXApi.Tests;

public sealed class SeedingErpV2SqlServerFactAttribute : FactAttribute
{
    public SeedingErpV2SqlServerFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("IAX_SEED_ERP_V2_SQL_TESTS") != "1")
            Skip = "Set IAX_SEED_ERP_V2_SQL_TESTS=1 to create an isolated database on local SQL Server.";
    }
}

public sealed class SeedingErpV2SqlServerTests(ITestOutputHelper output)
{
    [SeedingErpV2SqlServerFact]
    public async Task All_module_setup_seeds_and_reruns_on_sql_server()
    {
        var database = $"IAX_SeedErpV2_{Guid.NewGuid():N}";
        output.WriteLine($"Disposable SQL Server fixture: {database}");
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer($"Server=.;Database={database};Integrated Security=true;TrustServerCertificate=true",
                sql => sql.CommandTimeout(120))
            .Options;
        var actor = new SeedIdentity();
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<ICompanyExecutionContext>(actor)
            .AddSingleton<ICurrentUserService>(actor)
            .AddScoped(_ => new ApplicationDbContext(options, companyContext: actor))
            .AddGeneralLedgerTransactionExamples();
        using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            Assert.True(await db.Database.EnsureCreatedAsync());

            var runner = scope.ServiceProvider.GetRequiredService<SeedRunner>();
            var first = await runner.RunAsync(2026, "seed-erp-v2-sql-test");
            var before = await ReadTableCountsAsync(db);
            db.ChangeTracker.Clear();
            var second = await runner.RunAsync(2026, "seed-erp-v2-sql-test");
            Assert.Equal(first.Company, second.Company);
            Assert.Equal(first.CompletedSeeders.ToArray(), second.CompletedSeeders.ToArray());
            Assert.Equal(before, await ReadTableCountsAsync(db));
            output.WriteLine($"Verified stable row counts across {before.Length} tables.");

            var examples = scope.ServiceProvider.GetRequiredService<GeneralLedgerTransactionExamples>();
            var journals = await examples.RunAsync(2026, "seed-erp-v2-sql-test");
            Assert.NotEqual(journals.Journal, journals.Reversal);
            var posted = await ReadTableCountsAsync(db);
            Assert.Equal(journals, await examples.RunAsync(2026, "seed-erp-v2-sql-test"));
            Assert.Equal(posted, await ReadTableCountsAsync(db));

            db.ChangeTracker.Clear();
            var journalService = scope.ServiceProvider.GetRequiredService<ILedgerJournalTableService>();
            var lineService = scope.ServiceProvider.GetRequiredService<ILedgerJournalTransService>();
            var header = await journalService.AddAsync(new LedgerJournalTable
                { DataAreaId = "dat", JournalName = "GLV2-GEN", Name = "Concurrent posting", CurrencyCode = "SAR" });
            var debit = await db.Set<DimensionAttributeValueCombination>().Where(x => x.DataAreaId == "dat" && x.MainAccountValue == GeneralLedgerDemoData.DebitAccount).Select(x => x.RecId).SingleAsync();
            var credit = await db.Set<DimensionAttributeValueCombination>().Where(x => x.DataAreaId == "dat" && x.MainAccountValue == GeneralLedgerDemoData.CreditAccount).Select(x => x.RecId).SingleAsync();
            await lineService.AddAsync(new LedgerJournalTrans
            {
                DataAreaId = "dat", JournalNum = header.JournalNum, Voucher = "CONCURRENT",
                TransDate = new DateTime(2026, 1, 15), CurrencyCode = "SAR", AccountType = LedgerJournalACType.Ledger,
                LedgerDimension = debit, OffsetAccountType = LedgerJournalACType.Ledger,
                OffsetLedgerDimension = credit, AmountCurDebit = 125m
            });
            db.ChangeTracker.Clear();
            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            async Task<(LedgerJournalPostResultDto? Result, string? Error)> PostFromNewScope()
            {
                await using var postScope = provider.CreateAsyncScope();
                var service = postScope.ServiceProvider.GetRequiredService<ILedgerJournalTableService>();
                await start.Task;
                return await service.PostAsync(header.RecId, default);
            }
            var firstPost = PostFromNewScope();
            var secondPost = PostFromNewScope();
            start.SetResult();
            var results = await Task.WhenAll(firstPost, secondPost);
            Assert.Single(results, result => result.Result != null && result.Error == null);
            Assert.Single(results, result => result.Result == null && result.Error != null && result.Error.Contains("already posted"));
            Assert.Equal(1, await db.Set<GeneralJournalEntry>().AsNoTracking()
                .CountAsync(x => x.DataAreaId == "dat" && x.JournalNumber == header.JournalNum));

            var otherHeaders = new List<LedgerJournalTable>();
            foreach (var index in Enumerable.Range(1, 2))
            {
                var other = await journalService.AddAsync(new LedgerJournalTable
                    { DataAreaId = "dat", JournalName = "GLV2-GEN", Name = $"Sequence race {index}", CurrencyCode = "SAR" });
                await lineService.AddAsync(new LedgerJournalTrans
                {
                    DataAreaId = "dat", JournalNum = other.JournalNum, Voucher = $"TEMP-{index}",
                    TransDate = new DateTime(2026, 1, 15), CurrencyCode = "SAR", AccountType = LedgerJournalACType.Ledger,
                    LedgerDimension = debit, OffsetAccountType = LedgerJournalACType.Ledger,
                    OffsetLedgerDimension = credit, AmountCurDebit = 125m
                });
                otherHeaders.Add(other);
            }
            db.ChangeTracker.Clear();
            var sequenceStart = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            async Task<(LedgerJournalPostResultDto? Result, string? Error)> PostOther(long id)
            {
                await using var postScope = provider.CreateAsyncScope();
                var service = postScope.ServiceProvider.GetRequiredService<ILedgerJournalTableService>();
                await sequenceStart.Task;
                return await service.PostAsync(id, default);
            }
            var sequencePosts = otherHeaders.Select(x => PostOther(x.RecId)).ToArray();
            sequenceStart.SetResult();
            var sequenceResults = await Task.WhenAll(sequencePosts);
            Assert.All(sequenceResults, result => { Assert.NotNull(result.Result); Assert.Null(result.Error); });
            var journalNumbers = otherHeaders.Select(x => x.JournalNum).ToArray();
            var vouchers = await db.Set<LedgerJournalTrans>().AsNoTracking()
                .Where(x => x.DataAreaId == "dat" && journalNumbers.Contains(x.JournalNum))
                .Select(x => x.Voucher).ToListAsync();
            Assert.Equal(2, vouchers.Count);
            Assert.Equal(2, vouchers.Distinct().Count());
            Assert.DoesNotContain(vouchers, voucher => voucher.StartsWith("TEMP-"));

            var unbalanced = await journalService.AddAsync(new LedgerJournalTable
                { DataAreaId = "dat", JournalName = "GLV2-GEN", Name = "Rollback verification", CurrencyCode = "SAR" });
            await lineService.AddAsync(new LedgerJournalTrans
            {
                DataAreaId = "dat", JournalNum = unbalanced.JournalNum, Voucher = "ROLLBACK-TEMP",
                TransDate = new DateTime(2026, 1, 15), CurrencyCode = "SAR", AccountType = LedgerJournalACType.Ledger,
                LedgerDimension = debit, AmountCurDebit = 125m
            });
            db.ChangeTracker.Clear();
            var nextBefore = await db.Set<SysNumberSequence>().AsNoTracking()
                .Where(x => x.DataAreaId == "dat" && x.NumberSequence == "GLV2-VOUCH")
                .Select(x => x.NextRec).SingleAsync();
            var (failedPosting, postingError) = await journalService.PostAsync(unbalanced.RecId, default);
            Assert.Null(failedPosting);
            Assert.Contains("balance", postingError, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(nextBefore, await db.Set<SysNumberSequence>().AsNoTracking()
                .Where(x => x.DataAreaId == "dat" && x.NumberSequence == "GLV2-VOUCH")
                .Select(x => x.NextRec).SingleAsync());
            Assert.Equal("ROLLBACK-TEMP", await db.Set<LedgerJournalTrans>().AsNoTracking()
                .Where(x => x.DataAreaId == "dat" && x.JournalNum == unbalanced.JournalNum)
                .Select(x => x.Voucher).SingleAsync());
            Assert.False(await db.Set<GeneralJournalEntry>().AsNoTracking()
                .AnyAsync(x => x.DataAreaId == "dat" && x.JournalNumber == unbalanced.JournalNum));
            Assert.Equal(NoYes.No, (await db.Set<LedgerJournalTable>().AsNoTracking()
                .SingleAsync(x => x.RecId == unbalanced.RecId)).Posted);

            var late = await journalService.AddAsync(new LedgerJournalTable
                { DataAreaId = "dat", JournalName = "GLV2-GEN", Name = "Late rollback", CurrencyCode = "SAR" });
            await lineService.AddAsync(new LedgerJournalTrans
            {
                DataAreaId = "dat", JournalNum = late.JournalNum, Voucher = "LATE-TEMP",
                TransDate = new DateTime(2026, 1, 15), CurrencyCode = "SAR", AccountType = LedgerJournalACType.Ledger,
                LedgerDimension = debit, OffsetAccountType = LedgerJournalACType.Ledger,
                OffsetLedgerDimension = credit, AmountCurDebit = 125m
            });
            db.ChangeTracker.Clear();
            var lateNext = await db.Set<SysNumberSequence>().AsNoTracking()
                .Where(x => x.DataAreaId == "dat" && x.NumberSequence == "GLV2-VOUCH")
                .Select(x => x.NextRec).SingleAsync();
            var fault = new FailAfterJournalEntryInsert();
            var faultOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer($"Server=.;Database={database};Integrated Security=true;TrustServerCertificate=true")
                .AddInterceptors(fault).Options;
            var faultServices = new ServiceCollection().AddLogging()
                .AddSingleton<ICompanyExecutionContext>(actor).AddSingleton<ICurrentUserService>(actor)
                .AddGeneralLedgerTransactionExamples()
                .AddScoped(_ => new ApplicationDbContext(faultOptions, companyContext: actor));
            using (var faultProvider = faultServices.BuildServiceProvider())
            await using (var faultScope = faultProvider.CreateAsyncScope())
            {
                var faultJournal = faultScope.ServiceProvider.GetRequiredService<ILedgerJournalTableService>();
                var injected = await Assert.ThrowsAsync<DbUpdateException>(() => faultJournal.PostAsync(late.RecId, default));
                Assert.Equal("Injected failure after journal entry insertion.", injected.InnerException?.Message);
            }
            Assert.True(fault.Triggered);
            Assert.False(await db.Set<GeneralJournalEntry>().AsNoTracking()
                .AnyAsync(x => x.DataAreaId == "dat" && x.JournalNumber == late.JournalNum));
            Assert.Equal(lateNext, await db.Set<SysNumberSequence>().AsNoTracking()
                .Where(x => x.DataAreaId == "dat" && x.NumberSequence == "GLV2-VOUCH")
                .Select(x => x.NextRec).SingleAsync());
            Assert.Equal("LATE-TEMP", await db.Set<LedgerJournalTrans>().AsNoTracking()
                .Where(x => x.DataAreaId == "dat" && x.JournalNum == late.JournalNum)
                .Select(x => x.Voucher).SingleAsync());
            Assert.Equal(NoYes.No, (await db.Set<LedgerJournalTable>().AsNoTracking().SingleAsync(x => x.RecId == late.RecId)).Posted);
        }
        finally
        {
            // This context targets only the unique disposable database generated above.
            // Clean it up even if schema creation or a cycle assertion fails.
            await db.Database.EnsureDeletedAsync();
        }
    }

    private sealed class SeedIdentity : ICompanyExecutionContext, ICurrentUserService
    {
        public void ResetArea() { }
        public string GetDataAreaId() => "dat";
        public bool IsRequestedCompanyAuthorized() => true;
        public string GetCurrentUserId() => "seed-erp-v2-sql-test";
        public string GetOwnerAccountId() => GetCurrentUserId();
    }

    private sealed class FailAfterJournalEntryInsert : DbCommandInterceptor
    {
        public bool Triggered { get; private set; }
        public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData,
            DbDataReader result, CancellationToken cancellationToken = default)
        {
            if (command.CommandText.Contains("INSERT INTO [GeneralJournalEntry]", StringComparison.OrdinalIgnoreCase))
            {
                Triggered = true;
                result.Dispose();
                throw new InvalidOperationException("Injected failure after journal entry insertion.");
            }
            return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }
    }

    private static async Task<(string Table, long Count)[]> ReadTableCountsAsync(ApplicationDbContext db)
    {
        const string query = """
            SELECT s.name + N'.' + t.name, SUM(p.rows)
            FROM sys.tables t
            JOIN sys.schemas s ON s.schema_id = t.schema_id
            JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0, 1)
            GROUP BY s.name, t.name
            ORDER BY s.name, t.name
            """;
        await db.Database.OpenConnectionAsync();
        try
        {
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = query;
            await using var reader = await command.ExecuteReaderAsync();
            var counts = new List<(string Table, long Count)>();
            while (await reader.ReadAsync())
                counts.Add((reader.GetString(0), reader.GetInt64(1)));
            return counts.ToArray();
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}
