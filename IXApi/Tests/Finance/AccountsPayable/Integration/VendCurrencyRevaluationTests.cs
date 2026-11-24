using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsPayable.CurrencyRevaluation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// D365 foreign-currency revaluation of an open vendor invoice: 805 USD posted at 3.75 (3,018.75 SAR) and revalued at
/// 3.80 (3,059 SAR) posts an unrealized loss of 40.25, a rerun posts nothing, reversal restores zero, and a payment
/// reverses an outstanding revaluation before it realizes the difference against the original value.
/// </summary>
[Collection(SeedingCollection.Name)]
public sealed class VendCurrencyRevaluationTests
{
    private const string Company = TestIdentity.DefaultCompany;
    private const string Vendor = "APV2-dat-USD";
    private static readonly DateTime InvoiceDate = new(2026, 4, 15);
    private static readonly DateOnly RevaluationDate = new(2026, 5, 20); // the seeded 3.80 rate starts here

    [Fact]
    public async Task Open_usd_invoice_revalues_at_the_closing_rate_reverses_and_settles_against_its_original_value()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        var actor = new TestIdentity("vend-revaluation-test");
        using var provider = new ServiceCollection().AddSingleton<ApplicationDbContext>(db)
            .AddSingleton<ICompanyExecutionContext>(actor).AddSingleton<ICurrentUserService>(actor)
            .AddAccountsPayableTransactionExamples().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;
        await services.GetRequiredService<AccountsPayableCycleSeeder>().PrepareAsync(new(2026, "vend-revaluation-test"));
        db.ChangeTracker.Clear();
        await AddUnrealizedAccountsAsync(db);
        var invoice = await PostUsdInvoiceAsync(services, db);
        var open = await db.Set<VendTransOpen>().AsNoTracking().SingleAsync(x => x.RefRecId == invoice.RecId);
        Assert.Equal((-805m, -3018.75m), (open.AmountCur, open.AmountMst)); // 700 + 15% tax, at 3.75

        var revaluation = new VendCurrencyRevaluationService(services.GetRequiredService<IUnitOfWork>(), actor,
            services.GetRequiredService<ISysNumberSequenceService>());
        var preview = await revaluation.PreviewAsync(invoice.RecId, RevaluationDate, default);
        Assert.NotNull(preview);
        Assert.Equal((-3018.75m, -3059m, -40.25m), (preview!.PreviousAccountingValue, preview.RevaluedAccountingValue, preview.AccountingDifference));

        // Post: the liability grows by 40.25 (credit), the unrealized loss is debited.
        var voucher = await revaluation.PostAsync(invoice.RecId, RevaluationDate, RevaluationDate, default);
        Assert.NotNull(voucher);
        Assert.Equal(-40.25m, await VoucherAmountAsync(db, voucher!, GeneralLedgerSeedExample.VendorControl));
        Assert.Equal(40.25m, await VoucherAmountAsync(db, voucher!, GeneralLedgerSeedExample.RealizedExchangeLoss));
        var revalued = await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.RecId == invoice.RecId);
        Assert.Equal((-40.25m, voucher), (revalued.VendExchAdjustmentUnrealized, revalued.LastExchAdjVoucher));
        Assert.Equal(1, await db.Set<VendExchRateAdjustment>().CountAsync());

        // Same rate again: nothing to post.
        db.ChangeTracker.Clear();
        Assert.Null(await revaluation.PostAsync(invoice.RecId, RevaluationDate, RevaluationDate, default));

        // Reverse: the previous voucher is offset and the transaction carries no revaluation.
        db.ChangeTracker.Clear();
        var reversed = await revaluation.ReverseAsync(invoice.RecId, RevaluationDate, default);
        Assert.NotNull(reversed);
        Assert.Equal(40.25m, await VoucherAmountAsync(db, reversed!, GeneralLedgerSeedExample.VendorControl));
        Assert.Equal(0m, (await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.RecId == invoice.RecId)).VendExchAdjustmentUnrealized);

        // Revalue again, then pay at 3.80: the payment first reverses the revaluation, then realizes 40.25 against
        // the original 3,018.75, so the payable and the loss accounts end exactly as with no revaluation at all.
        db.ChangeTracker.Clear();
        Assert.NotNull(await revaluation.PostAsync(invoice.RecId, RevaluationDate, RevaluationDate, default));
        db.ChangeTracker.Clear();
        var paid = await services.GetRequiredService<IVendPaymentService>().PostAsync(
            new(Vendor, "APV2-USD", 805m, "VREV-PAY", RevaluationDate.ToDateTime(TimeOnly.MinValue), SettleInvoiceId: invoice.Invoice), default);
        Assert.True(paid.StatusCode == 200, paid.Message);
        var settled = await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.RecId == invoice.RecId);
        Assert.Equal((0m, 40.25m), (settled.VendExchAdjustmentUnrealized, -settled.VendExchAdjustmentRealized));
        Assert.False(await db.Set<VendTransOpen>().AnyAsync(x => x.RefRecId == invoice.RecId));
        Assert.Equal(0m, await LedgerBalances.OfMainAccountAsync(db, Company, GeneralLedgerSeedExample.VendorControl));
        Assert.Equal(40.25m, await LedgerBalances.OfMainAccountAsync(db, Company, GeneralLedgerSeedExample.RealizedExchangeLoss));
        await LedgerBalances.AssertEveryVoucherBalancesAsync(db, Company);
    }

    private static async Task<VendTrans> PostUsdInvoiceAsync(IServiceProvider services, ApplicationDbContext db)
    {
        var orders = services.GetRequiredService<IPurchOrderService>();
        var created = await orders.CreateAsync(new(Vendor, null, "USD", InvoiceDate, "APV2-SITE", "APV2-WH", "VREV-ORDER", InvoiceDate.AddDays(-14)), default);
        Assert.True(created.StatusCode == 200, created.Message);
        var order = (PurchTable)created.Data!;
        Assert.Equal(200, (await orders.AddLineAsync(order.RecId, new("APV2-STOCK", 100m, "EA", 7m, 1m, "APV2-SITE", "APV2-WH", InvoiceDate, null), default)).StatusCode);
        Assert.Equal(200, (await services.GetRequiredService<IPurchConfirmationService>().ConfirmAsync(order.RecId, default, InvoiceDate.AddDays(-12))).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync(x => x.PurchId == order.PurchId);
        Assert.Equal(200, (await services.GetRequiredService<IProductReceiptService>().PostAsync(order.RecId, new("VREV-R1", InvoiceDate.AddDays(-5), [new(line.RecId, 100m)]), default)).StatusCode);
        var invoiced = await services.GetRequiredService<IVendorInvoiceService>().PostAsync(order.RecId, new("VREV-I1", InvoiceDate, [new(line.RecId, 100m, 7m)]), default);
        Assert.True(invoiced.StatusCode == 200, invoiced.Message);
        db.ChangeTracker.Clear();
        return await db.Set<VendTrans>().AsNoTracking().SingleAsync(x => x.AccountNum == Vendor && x.Invoice == "VREV-I1");
    }

    /// <summary>Unrealized gain and loss accounts for the ledger (D365 LedgerGainLossAccount), on the demo exchange accounts.</summary>
    private static async Task AddUnrealizedAccountsAsync(ApplicationDbContext db)
    {
        var ledger = await db.Set<Ledger>().SingleAsync(x => x.DataAreaId == Company);
        foreach (var (type, code) in new[] { (CurrencyGainLossAccountType.UnrealizedGain, GeneralLedgerSeedExample.RealizedExchangeGain),
                     (CurrencyGainLossAccountType.UnrealizedLoss, GeneralLedgerSeedExample.RealizedExchangeLoss) })
        {
            var dimension = await db.Set<DimensionAttributeValueCombination>().FirstAsync(x => x.DataAreaId == Company && x.MainAccountValue == code);
            db.Add(new LedgerGainLossAccount { DataAreaId = Company, Ledger = ledger.RecId, AccountType = type, LedgerDimension = dimension.RecId });
        }
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    private static async Task<decimal> VoucherAmountAsync(ApplicationDbContext db, string voucher, string mainAccount)
    {
        var entry = await db.Set<GeneralJournalEntry>().AsNoTracking().SingleAsync(x => x.SubledgerVoucher == voucher);
        var dimensions = await db.Set<DimensionAttributeValueCombination>().AsNoTracking()
            .Where(x => x.MainAccountValue == mainAccount).Select(x => x.RecId).ToListAsync();
        return (await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => x.GeneralJournalEntry == entry.RecId && dimensions.Contains(x.LedgerDimension)).ToListAsync())
            .Sum(x => x.AccountingCurrencyAmount);
    }
}
