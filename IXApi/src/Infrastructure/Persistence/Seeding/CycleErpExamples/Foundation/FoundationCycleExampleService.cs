using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Foundation;

public sealed class FoundationCycleExampleService
{
    internal Task SeedAsync(ErpCycleExampleContext context, CancellationToken ct) =>
        SeedDataAsync(context.Db, context.Actor.GetCurrentUserId(), ct);

    public async Task SeedDataAsync(DbContext db, string owner, CancellationToken ct = default)
    {
        // Reuse the reference/master seeders. Never duplicate customers, vendors, products or ledger setup here.
        _ = await db.Set<CompanyInfo>().SingleAsync(x => x.DataArea == ErpCycleExampleData.Company, ct);
        _ = await db.Set<CustTable>().SingleAsync(x => x.DataAreaId == ErpCycleExampleData.Company && x.AccountNum == ErpCycleExampleData.Customer, ct);
        _ = await db.Set<VendTable>().SingleAsync(x => x.DataAreaId == ErpCycleExampleData.Company && x.AccountNum == ErpCycleExampleData.Vendor, ct);
        _ = await db.Set<InventTable>().SingleAsync(x => x.DataAreaId == ErpCycleExampleData.Company && x.ItemId == ErpCycleExampleData.Item, ct);
        var ledger = await db.Set<Ledger>().SingleAsync(x => x.DataAreaId == ErpCycleExampleData.Company, ct);
        if (ledger.AccountingCurrency != ErpCycleExampleData.CurrencyCode || ledger.ReportingCurrency != ErpCycleExampleData.CurrencyCode)
            throw new InvalidOperationException("The ERP cycle requires the existing SAR example ledger; customized currencies were preserved.");
        var mode = await db.Set<VendPaymModeTable>().IgnoreQueryFilters().SingleOrDefaultAsync(x =>
            x.DataAreaId == ErpCycleExampleData.Company && x.PaymMode == ErpCycleExampleData.VendorPaymentMode, ct);
        if (mode == null)
        {
            db.Add(new VendPaymModeTable { DataAreaId = ErpCycleExampleData.Company,
                PaymMode = ErpCycleExampleData.VendorPaymentMode, Name = "ERP cycle bank payment",
                AccountType = LedgerJournalACType.Bank, CreatedBy = owner, OwnerAccountId = owner });
            await db.SaveChangesAsync(ct);
        }
        else if (mode.IsDeleted || mode.AccountType != LedgerJournalACType.Bank)
            throw new InvalidOperationException("The ERP example payment method was customized; it was preserved.");
    }
}
