using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using Microsoft.EntityFrameworkCore;
using static IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.ErpCycleExampleData;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;

internal static class LedgerJournalTransDemoData
{
    internal static LedgerJournalTrans Create(string journalNum, string bankAccount, long bankDimension, long cashDimension, string voucher, DateTime date) =>
        new LedgerJournalTrans {
                DataAreaId = Company, JournalNum = journalNum, LineNum = 1,
                Voucher = voucher,
                TransDate = date, AccountType = LedgerJournalACType.Bank,
                LedgerDimensionName = bankAccount, LedgerDimension = bankDimension,
                OffsetAccountType = LedgerJournalACType.Ledger,
                OffsetLedgerDimension = cashDimension, AmountCurDebit = 250m, CurrencyCode = CurrencyCode,
                Company = Company, OffsetCompany = Company, Txt = "ERP cycle: cash deposited to bank ledger account" };
}
