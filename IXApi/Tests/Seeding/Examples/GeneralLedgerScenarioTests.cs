using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Modules.Finance;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// The General ledger scenarios (CycleErpExamples/GeneralLedger/Scenarios), each run alone on an empty company so
/// the whole ledger is that scenario's postings, and checked account by account against its README. Expected
/// amounts are computed here from the scenario inputs, not read from the flow.
/// </summary>
public sealed class GeneralLedgerScenarioTests
{
    private const string Company = TestIdentity.DefaultCompany;

    [Fact]
    public async Task GL01_reclassification_posts_125_and_the_reversal_restores_both_accounts()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        var examples = Examples(db, out var provider);
        using var _ = provider;

        await examples.RunAsync(new GeneralLedgerTransactionOptions(2026, "gl-scenario-test", ["01"]));

        var journal = await db.Set<LedgerJournalTable>().SingleAsync(x => x.Name == "GLV2-2026-01");
        Assert.Equal(NoYes.Yes, journal.Posted);
        var reversal = await db.Set<LedgerJournalTable>().SingleAsync(x => x.OriginalJournalNum == journal.JournalNum);
        Assert.Equal(NoYes.Yes, reversal.Posted);

        // Original (January 15): maintenance +125, delivery -125. Reversal (January 16): the opposite.
        var original = await VoucherAsync(db, journal.JournalNum);
        Assert.Equal(new DateTime(2026, 1, 15), original.Date);
        Assert.Equal((125m, -125m), (original.Amount(GeneralLedgerSeedExample.MaintenanceExpense), original.Amount(GeneralLedgerSeedExample.DeliveryExpense)));
        var reversed = await VoucherAsync(db, reversal.JournalNum);
        Assert.Equal(new DateTime(2026, 1, 16), reversed.Date);
        Assert.Equal((-125m, 125m), (reversed.Amount(GeneralLedgerSeedExample.MaintenanceExpense), reversed.Amount(GeneralLedgerSeedExample.DeliveryExpense)));

        Assert.Equal(0m, await LedgerBalances.OfMainAccountAsync(db, Company, GeneralLedgerSeedExample.MaintenanceExpense));
        Assert.Equal(0m, await LedgerBalances.OfMainAccountAsync(db, Company, GeneralLedgerSeedExample.DeliveryExpense));
        Assert.Equal(2, await db.Set<GeneralJournalEntry>().CountAsync());
        await LedgerBalances.AssertEveryVoucherBalancesAsync(db, Company);

        // Replay finds both journals and posts nothing.
        var counts = (await db.Set<LedgerJournalTable>().CountAsync(), await db.Set<GeneralJournalAccountEntry>().CountAsync());
        db.ChangeTracker.Clear();
        await examples.RunAsync(new GeneralLedgerTransactionOptions(2026, "gl-scenario-test", ["01"]));
        Assert.Equal(counts, (await db.Set<LedgerJournalTable>().CountAsync(), await db.Set<GeneralJournalAccountEntry>().CountAsync()));
    }

    [Fact]
    public async Task GL02_reference_cycle_reaches_the_documented_trial_balance_account_by_account()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        var examples = Examples(db, out var provider);
        using var _ = provider;

        await examples.RunAsync(new GeneralLedgerTransactionOptions(2026, "gl-scenario-test", ["02"]));

        // Inputs: buy 100 at 50; sell 30 at 90 plus a 300 service; collect 3,000; pay the vendor 2,000; accrue 300.
        const decimal purchase = 100 * 50m, cost = 30 * 50m, revenue = 30 * 90m + 300m, paid = 2000m, accrual = 300m;
        var expected = new Dictionary<string, decimal>
        {
            [GeneralLedgerSeedExample.Bank] = revenue - paid,                 // 1,000 debit
            [GeneralLedgerSeedExample.Inventory] = purchase - cost,           // 3,500 debit
            [GeneralLedgerSeedExample.CostOfGoodsSold] = cost,                // 1,500 debit
            [GeneralLedgerSeedExample.MaintenanceExpense] = accrual,          // 300 debit
            [GeneralLedgerSeedExample.VendorControl] = -(purchase - paid),    // 3,000 credit
            [GeneralLedgerSeedExample.SalesRevenue] = -revenue,               // 3,000 credit
            [GeneralLedgerSeedExample.ReceivedNotInvoiced] = -accrual,        // 300 credit
            [GeneralLedgerSeedExample.CustomerControl] = 0m,
        };
        foreach (var (account, amount) in expected)
            Assert.True(amount == await LedgerBalances.OfMainAccountAsync(db, Company, account),
                $"GL-02 account {account}: expected {amount}, actual {await LedgerBalances.OfMainAccountAsync(db, Company, account)}.");
        Assert.Equal(6300m, expected.Values.Where(x => x > 0).Sum());
        await LedgerBalances.AssertEveryVoucherBalancesAsync(db, Company);

        var stock = await db.Set<InventSum>().Where(x => x.ItemId == "IV2-2026-GLREF").ToListAsync();
        Assert.Equal((70m, 3500m), (stock.Sum(x => x.PhysicalInvent), stock.Sum(x => x.PostedValue)));
    }

    private static GeneralLedgerTransactionExamples Examples(ApplicationDbContext db, out ServiceProvider provider)
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(FinanceModule).Assembly);
        var actor = new TestIdentity("gl-scenario-test");
        provider = new ServiceCollection().AddSingleton(db)
            .AddSingleton<ICompanyExecutionContext>(actor).AddSingleton<ICurrentUserService>(actor)
            .AddLogging().AddGeneralLedgerTransactionExamples().BuildServiceProvider();
        return provider.CreateScope().ServiceProvider.GetRequiredService<GeneralLedgerTransactionExamples>();
    }

    private static async Task<(DateTime Date, Func<string, decimal> Amount)> VoucherAsync(ApplicationDbContext db, string journalNum)
    {
        var voucher = await db.Set<LedgerJournalTrans>().Where(x => x.JournalNum == journalNum).Select(x => x.Voucher).Distinct().SingleAsync();
        var entry = await db.Set<GeneralJournalEntry>().SingleAsync(x => x.SubledgerVoucher == voucher);
        var lines = await db.Set<GeneralJournalAccountEntry>().Where(x => x.GeneralJournalEntry == entry.RecId).ToListAsync();
        var accounts = await db.Set<DimensionAttributeValueCombination>()
            .Where(x => lines.Select(l => l.LedgerDimension).Contains(x.RecId)).ToDictionaryAsync(x => x.RecId, x => x.MainAccountValue);
        Assert.Equal(0m, lines.Sum(x => x.AccountingCurrencyAmount));
        return (entry.AccountingDate, account => lines.Where(x => accounts[x.LedgerDimension] == account).Sum(x => x.AccountingCurrencyAmount));
    }
}
