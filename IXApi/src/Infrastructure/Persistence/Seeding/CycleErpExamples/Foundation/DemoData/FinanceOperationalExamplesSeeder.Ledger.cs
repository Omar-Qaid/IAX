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
        if (!await db.Set<BankGroup>().IgnoreQueryFilters().AnyAsync(x => x.DataAreaId == "dat" && x.BankGroupId == "DEMO", ct))
            db.Add(new BankGroup { BankGroupId = "DEMO", Name = "Example bank - training only", CurrencyCode = "SAR", DataAreaId = "dat" });
        await db.SaveChangesAsync(ct);
        if (!await db.Set<BankAccountTable>().IgnoreQueryFilters().AnyAsync(x => x.DataAreaId == "dat" && x.AccountId == "DEMO-SAR", ct))
            db.Add(new BankAccountTable { AccountId = "DEMO-SAR", AccountNum = "TRAINING-001", Name = "Example SAR operating account",
                BankGroupId = "DEMO", CurrencyCode = "SAR", LedgerDimension = dimensions["10020000"],
                BankAccountStatus = BankAccountStatus.Active, ActiveFrom = new DateTime(2026,1,1), ActiveTo = new DateTime(2099,12,31),
                CustPaymentJournalName = "DEMO-AR", VendPaymentJournalName = "DEMO-AP", DataAreaId = "dat" });
        if (!await db.Set<CustParameters>().IgnoreQueryFilters().AnyAsync(x => x.DataAreaId == "dat", ct))
            db.Add(new CustParameters { DataAreaId = "dat", CurrencyCode = "SAR", CustPostingProfile = "GEN",
                PaymTermId = "Monthly", PaymMode = "Transfer", DlvMode = "DHL", TaxGroup = "DOM",
                PriceDiscJournalNamePrice = "SALEPRICE", PriceDiscJournalNameLineDisc = "LINEDISC", PriceDiscJournalNameTotalDisc = "TOTALDISC" });
        if (!await db.Set<VendLedgerAccounts>().IgnoreQueryFilters().AnyAsync(x => x.DataAreaId == "dat" && x.PostingProfile == "DEMO" && x.AccountCode == AccountCode.All, ct))
            db.Add(new VendLedgerAccounts { DataAreaId = "dat", PostingProfile = "DEMO", AccountCode = AccountCode.All,
                SummaryLedgerDimension = dimensions["20010000"], Num = "" });
    }
}
