using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>The Tax scenarios (CycleErpExamples/Tax/Scenarios/01-02) checked against their README: direction, code and account.</summary>
[Collection(SeedingCollection.Name)]
public sealed class TaxScenarioTests
{
    private const string Company = TestIdentity.DefaultCompany;

    [Fact]
    public async Task Input_tax_from_the_vendor_journal_and_output_tax_from_the_free_text_invoice_match_their_scenarios()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        var actor = new TestIdentity("tax-scenario-test");
        using var provider = new ServiceCollection().AddSingleton<ApplicationDbContext>(db)
            .AddSingleton<ICompanyExecutionContext>(actor).AddSingleton<ICurrentUserService>(actor)
            .AddTaxTransactionExamples().BuildServiceProvider();
        using var scope = provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<TaxTransactionExamples>().RunAsync(new(2026, "tax-scenario-test"));

        // 01: the journal's tax is Incoming APV2-15, reconciled to its detail and linked to a 181004 ledger line.
        var detail = Assert.Single(await db.Set<TaxJournalTrans>().ToListAsync());
        var input = await db.Set<TaxTrans>().SingleAsync(x => x.SourceTableId == detail.TransTableId && x.SourceRecId == detail.TransRecId);
        Assert.Equal((TaxDirection.Incoming, "APV2-15"), (input.TaxDirection, input.TaxCode));
        Assert.Equal((detail.TaxAmount, detail.TaxBaseAmount), (input.TaxAmount, input.TaxBaseAmount));
        Assert.Equal(52.50m, input.TaxAmount); // 15% of 350
        var link = await db.Set<TaxTransGeneralJournalAccountEntry>().SingleAsync(x => x.TaxTrans == input.RecId);
        Assert.Equal(GeneralLedgerSeedExample.RecoverablePurchaseTax, await MainAccountAsync(db, link.LedgerDimension!.Value));

        // 02: the free text invoice's tax is Outgoing ARV2-15, negative, credited to 281004, and moves no stock.
        var output = await db.Set<TaxTrans>().SingleAsync(x => x.TaxCode == "ARV2-15");
        Assert.Equal(TaxDirection.Outgoing, output.TaxDirection);
        Assert.Equal((-350m, -52.50m), (output.TaxBaseAmount, output.TaxAmount)); // 15% of a 350 service line
        Assert.Equal(-52.50m, await LedgerBalances.OfMainAccountAsync(db, Company, GeneralLedgerSeedExample.PayableSalesTax));
        var invoiceVoucher = output.Voucher;
        Assert.False(await db.Set<InventTrans>().AnyAsync(x => x.Voucher == invoiceVoucher));
        await LedgerBalances.AssertEveryVoucherBalancesAsync(db, Company);
    }

    private static async Task<string> MainAccountAsync(ApplicationDbContext db, long ledgerDimension) =>
        (await db.Set<DimensionAttributeValueCombination>().SingleAsync(x => x.RecId == ledgerDimension)).MainAccountValue;
}
