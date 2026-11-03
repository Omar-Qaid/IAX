using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Administration.NumberSequences;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

public sealed partial class FinanceOperationalExamplesSeeder
{
    private static async Task<Dictionary<string, long>> SeedLedgerDimensionsAsync(DbContext db, CancellationToken ct)
    {
        var accounts = await db.Set<MainAccount>().IgnoreQueryFilters().Where(x => x.DataAreaId == "dat")
            .ToDictionaryAsync(x => x.MainAccountId, ct);
        var dimensions = new Dictionary<string, long>();
        foreach (var code in new[] { "10020000", "20010000", "22010005" })
        {
            var account = accounts[code];
            var dimension = await db.Set<DimensionAttributeValueCombination>().IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.DataAreaId == "dat" && x.MainAccount == account.RecId && x.DisplayValue == code, ct);
            if (dimension == null)
            {
                dimension = new DimensionAttributeValueCombination { MainAccount = account.RecId, MainAccountValue = code,
                    DisplayValue = code, DataAreaId = "dat", CreatedBy = "sys", OwnerAccountId = "sys" };
                db.Add(dimension);
                await db.SaveChangesAsync(ct);
            }
            dimensions[code] = dimension.RecId;
        }
        return dimensions;
    }

    private static async Task SeedBankAndSubledgerSetupAsync(DbContext db, Dictionary<string, long> dimensions, CancellationToken ct)
    {
        var taxPosting = await db.Set<TaxLedgerAccountGroup>().IgnoreQueryFilters()
            .SingleAsync(x => x.DataAreaId == "dat" && x.TaxAccountGroup == "VAT-STD", ct);
        if (taxPosting.TaxOutgoingLedgerDimension is null or 0)
            taxPosting.TaxOutgoingLedgerDimension = dimensions["22010005"];
        if (taxPosting.TaxIncomingLedgerDimension is null or 0)
            taxPosting.TaxIncomingLedgerDimension = dimensions["22010005"];
        foreach (var row in new[] {
            new LedgerJournalName { JournalName = "DEMO-GL", Name = "Example general journal", JournalType = LedgerJournalType.Daily },
            new LedgerJournalName { JournalName = "DEMO-AR", Name = "Example customer payments", JournalType = LedgerJournalType.CustPayment },
            new LedgerJournalName { JournalName = "DEMO-AP", Name = "Example vendor payments", JournalType = LedgerJournalType.VendDisbursement } })
        {
            if (await db.Set<LedgerJournalName>().IgnoreQueryFilters().AnyAsync(x => x.DataAreaId == "dat" && x.JournalName == row.JournalName, ct)) continue;
            row.DataAreaId = "dat"; row.CurrencyCode = "SAR"; row.CreatedBy = "sys"; row.OwnerAccountId = "sys";
            db.Add(row);
        }
        await BankGroupSeeder.SeedAsync(db, ct);
        await db.SaveChangesAsync(ct);
        await BankAccountTableSeeder.SeedAsync(db, dimensions["10020000"], ct);
        await new CustomerConfigurationExampleService().SeedAsync(db, ct);
        await VendLedgerAccountsSeeder.SeedAsync(db, dimensions["20010000"], ct);
    }
}
