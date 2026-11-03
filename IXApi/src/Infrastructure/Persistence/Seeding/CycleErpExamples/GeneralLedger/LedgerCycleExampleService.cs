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
        var cashDimension = await db.Set<DimensionAttributeValueCombination>().SingleOrDefaultAsync(x =>
            x.DataAreaId == Company && x.DisplayValue == cash.MainAccountId, ct);
        if (cashDimension == null)
        {
            cashDimension = new DimensionAttributeValueCombination { DataAreaId = Company,
                MainAccount = cash.RecId, MainAccountValue = cash.MainAccountId, DisplayValue = cash.MainAccountId,
                CreatedBy = context.Actor.GetCurrentUserId(), OwnerAccountId = context.Actor.GetOwnerAccountId() };
            db.Add(cashDimension);
            await db.SaveChangesAsync(ct);
        }
        if (cashDimension.MainAccount != cash.RecId)
            throw new InvalidOperationException("The cash ledger combination conflicts with the ERP example; it was preserved.");
        var journals = new LedgerJournalTableService(context.Work, context.Actor, context.Numbers);
        var journal = await db.Set<LedgerJournalTable>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.DataAreaId == Company && x.Name == Journal, ct);
        if (journal == null)
            journal = await journals.AddAsync(new LedgerJournalTable { DataAreaId = Company,
                JournalName = "DEMO-GL", Name = Journal, CurrencyCode = CurrencyCode }, ct);
        if (journal.IsDeleted || journal.JournalName != "DEMO-GL" || journal.CurrencyCode != CurrencyCode)
            throw new InvalidOperationException("ERP ledger example conflicts with an existing journal; it was preserved.");
        var line = await db.Set<LedgerJournalTrans>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.DataAreaId == Company && x.JournalNum == journal.JournalNum, ct);
        if (line == null)
        {
            if (journal.Posted == NoYes.Yes)
                throw new InvalidOperationException("The posted ERP ledger example has no matching line.");
            await new LedgerJournalTransService(context.Work, context.Actor).AddAsync(new LedgerJournalTrans {
                DataAreaId = Company, JournalNum = journal.JournalNum, LineNum = 1,
                Voucher = (await context.Numbers.NextAsync("Voucher", Company, ct)).Code,
                TransDate = context.Date, AccountType = LedgerJournalACType.Bank,
                LedgerDimensionName = bank.AccountId, LedgerDimension = bank.LedgerDimension,
                OffsetAccountType = LedgerJournalACType.Ledger,
                OffsetLedgerDimension = cashDimension.RecId, AmountCurDebit = 250m, CurrencyCode = CurrencyCode,
                Company = Company, OffsetCompany = Company, Txt = "ERP cycle: cash deposited to bank ledger account" }, ct);
        }
        else if (line.IsDeleted || line.AccountType != LedgerJournalACType.Bank || line.LedgerDimensionName != bank.AccountId
            || line.LedgerDimension != bank.LedgerDimension || line.OffsetLedgerDimension != cashDimension.RecId
            || line.AmountCurDebit != 250m || line.AmountCurCredit != 0m)
            throw new InvalidOperationException("ERP ledger example line was customized; no posting was attempted.");
        // The Bank account type makes the posting service generate the matching bank subledger movement.
        if (journal.Posted != NoYes.Yes)
        {
            var result = await journals.PostAsync(journal.RecId, ct);
            if (result.Error != null || result.Result == null)
                throw new InvalidOperationException(result.Error ?? "ERP general journal posting returned no result.");
        }
    }
}
