using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable;
/// <summary>Unposted journal entry through its owning services, then the shared journal posting engine.</summary>
public sealed class AccountsPayableJournalExamples(ApplicationDbContext db, ILedgerJournalTableService journals, ILedgerJournalTransService lines)
{
    public async Task InvoiceAsync(string key, string account, string invoice, decimal gross, bool creditNote, DateTime date, CancellationToken ct)
    {
        var area = db.GetDataAreaId();
        var header = await db.Set<LedgerJournalTable>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.Name == key, ct);
        if (header == null) header = await journals.AddAsync(new LedgerJournalTable { DataAreaId = area, JournalName = "APV2-INV",
            Name = key, CurrencyCode = "SAR", LedgerJournalInclTax = NoYes.Yes }, ct);
        if (header.Posted == NoYes.Yes) return;
        if (!await db.Set<LedgerJournalTrans>().AnyAsync(x => x.DataAreaId == area && x.JournalNum == header.JournalNum, ct))
        {
            var expense = await db.Set<DimensionAttributeValueCombination>().SingleAsync(x => x.DataAreaId == area && x.MainAccountValue == "681001", ct);
            await lines.AddAsync(new LedgerJournalTrans { DataAreaId = area, JournalNum = header.JournalNum, TransDate = date,
                AccountType = LedgerJournalACType.Vend, LedgerDimensionName = account, OffsetAccountType = LedgerJournalACType.Ledger,
                OffsetLedgerDimension = expense.RecId, CurrencyCode = "SAR", Invoice = invoice, DocumentNum = invoice,
                AmountCurCredit = creditNote ? 0m : gross, AmountCurDebit = creditNote ? gross : 0m,
                Payment = "APV2-N30", TaxGroup = "APV2", TaxItemGroup = "APV2", PaymReference = key, Txt = "APV2 direct expense / financial credit example" }, ct);
        }
        var result = await journals.PostAsync(header.RecId, ct);
        if (result.Error != null || result.Result == null) throw new InvalidOperationException("AP journal posting: " + result.Error);
    }
    public async Task PrepaymentAsync(string key, string account, decimal amount, DateTime date, CancellationToken ct)
    {
        var area = db.GetDataAreaId();
        var header = await db.Set<LedgerJournalTable>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.Name == key, ct);
        if (header == null) header = await journals.AddAsync(new LedgerJournalTable { DataAreaId = area, JournalName = "APV2-PAY", Name = key, CurrencyCode = "SAR" }, ct);
        if (header.Posted == NoYes.Yes) return;
        var entered = await db.Set<LedgerJournalTrans>().Where(x => x.DataAreaId == area && x.JournalNum == header.JournalNum).ToListAsync(ct);
        if (!entered.Any(x => x.AccountType == LedgerJournalACType.Vend))
            await lines.AddAsync(new LedgerJournalTrans { DataAreaId = area, JournalNum = header.JournalNum, TransDate = date,
                AccountType = LedgerJournalACType.Vend, LedgerDimensionName = account, CurrencyCode = "SAR", Prepayment = NoYes.Yes,
                AmountCurDebit = amount, PaymReference = key, Txt = "APV2 vendor payment-journal prepayment" }, ct);
        if (!entered.Any(x => x.AccountType == LedgerJournalACType.Bank))
            await lines.AddAsync(new LedgerJournalTrans { DataAreaId = area, JournalNum = header.JournalNum, TransDate = date,
                AccountType = LedgerJournalACType.Bank, LedgerDimensionName = "APV2-BANK", CurrencyCode = "SAR",
                AmountCurCredit = amount, PaymReference = key + "-BANK", Txt = "APV2 prepayment bank movement" }, ct);
        var result = await journals.PostAsync(header.RecId, ct);
        if (result.Error != null || result.Result == null) throw new InvalidOperationException("AP prepayment journal posting: " + result.Error);
    }
}
