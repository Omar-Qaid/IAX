using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;

internal static class LedgerJournalTransSeeder
{
    internal static async Task SeedAsync(ErpCycleExampleContext context, LedgerJournalTable journal, BankAccountTable bank, DimensionAttributeValueCombination cashDimension, CancellationToken ct)
    {
        var db = context.Db;
        var line = await db.Set<LedgerJournalTrans>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.DataAreaId == Company && x.JournalNum == journal.JournalNum, ct);
        if (line == null)
        {
            if (journal.Posted == NoYes.Yes)
                throw new InvalidOperationException("The posted ERP ledger example has no matching line.");
            await new LedgerJournalTransService(context.Work, context.Actor).AddAsync(LedgerJournalTransDemoData.Create(journal.JournalNum, bank.AccountId, bank.LedgerDimension, cashDimension.RecId,
                (await context.Numbers.NextAsync("Voucher", Company, ct)).Code, context.Date), ct);
        }
        else if (line.IsDeleted || line.AccountType != LedgerJournalACType.Bank || line.LedgerDimensionName != bank.AccountId
            || line.LedgerDimension != bank.LedgerDimension || line.OffsetLedgerDimension != cashDimension.RecId
            || line.OffsetAccountType != LedgerJournalACType.Ledger || line.CurrencyCode != CurrencyCode
            || line.Company != Company || line.OffsetCompany != Company || line.AmountCurDebit != 250m || line.AmountCurCredit != 0m)
            throw new InvalidOperationException("ERP ledger example line was customized; no posting was attempted.");
    }
}
