using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;

public sealed class LedgerCycleExampleService
{
    internal async Task SeedAsync(ErpCycleExampleContext context, CancellationToken ct)
    {
        var db = context.Db;
        var bank = await db.Set<BankAccountTable>().SingleAsync(x => x.AccountId == "DEMO-SAR", ct);
        var cash = await db.Set<MainAccount>().SingleAsync(x => x.MainAccountId == "10010000", ct);
        // Reuse the main-account-only setup shape of FinanceOperationalExamplesSeeder.
        var cashDimension = await db.Set<DimensionAttributeValueCombination>().IgnoreQueryFilters().SingleOrDefaultAsync(x =>
            x.DataAreaId == Company && x.DisplayValue == cash.MainAccountId, ct);
        if (cashDimension == null)
        {
            cashDimension = new DimensionAttributeValueCombination { DataAreaId = Company,
                MainAccount = cash.RecId, MainAccountValue = cash.MainAccountId, DisplayValue = cash.MainAccountId,
                CreatedBy = context.Actor.GetCurrentUserId(), OwnerAccountId = context.Actor.GetOwnerAccountId() };
            db.Add(cashDimension);
            await db.SaveChangesAsync(ct);
        }
        if (cashDimension.IsDeleted || cashDimension.MainAccount != cash.RecId || cashDimension.MainAccountValue != cash.MainAccountId)
            throw new InvalidOperationException("The cash ledger combination conflicts with the ERP example; it was preserved.");
        var journals = new LedgerJournalTableService(context.Work, context.Actor, context.Numbers);
        var journal = await LedgerJournalTableSeeder.SeedAsync(context, journals, ct);
        await LedgerJournalTransSeeder.SeedAsync(context, journal, bank, cashDimension, ct);
        // The Bank account type makes the posting service generate the matching bank subledger movement.
        if (journal.Posted != NoYes.Yes)
        {
            var result = await journals.PostAsync(journal.RecId, ct);
            if (result.Error != null || result.Result == null)
                throw new InvalidOperationException(result.Error ?? "ERP general journal posting returned no result.");
        }
    }
}
