using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
namespace IAX.IXApi.Modules.Finance.Common;
/// <summary>Source accounting audit/transfer records created atomically with the existing GL posting engine.
/// Numeric lifecycle/type projections without a repository enum are left unset, never guessed.</summary>
public sealed class PurchaseSourceAccounting(DbContext db)
{
    public async Task<long> HeaderAsync(string area, int sourceTable, CancellationToken ct)
    {
        var row = new SourceDocumentHeader { DataAreaId = area, SourceRelationType = sourceTable };
        db.Add(row);
        await db.SaveChangesAsync(ct);
        return row.RecId;
    }
    public async Task<long> LineAsync(string area, long header, int sourceTable, decimal quantity, DateTime date, CancellationToken ct)
    {
        var row = new SourceDocumentLine { DataAreaId = area, SourceDocumentHeader = header,
            SourceRelationType = sourceTable, TermQuantity = quantity, ExchangeRateDate = date };
        db.Add(row);
        await db.SaveChangesAsync(ct);
        return row.RecId;
    }
    public async Task CaptureAsync(string area, GeneralJournalEntry entry, long header, long sourceLine, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction == null) throw new SubledgerPostingException("Source accounting requires the owning posting transaction.");
        await db.SaveChangesAsync(ct);
        var ledger = await db.Set<Ledger>().AsNoTracking().SingleAsync(x => x.RecId == entry.Ledger && x.DataAreaId == area, ct);
        var journal = await db.Set<SubledgerJournalEntry>().SingleOrDefaultAsync(x => x.Ledger == entry.Ledger
            && x.VoucherDataAreaId == area && x.Voucher == entry.SubledgerVoucher, ct);
        if (journal == null)
        {
            var accountingEvent = new AccountingEvent { DataAreaId = area, SourceDocumentHeader = header,
                AccountingDate = entry.AccountingDate, EventDateTime = DateTime.UtcNow, IsAccountingRequired = NoYes.Yes };
            db.Add(accountingEvent);
            await db.SaveChangesAsync(ct);
            journal = new SubledgerJournalEntry { DataAreaId = area, AccountingEvent = accountingEvent.RecId,
                Ledger = entry.Ledger, FiscalCalendarPeriod = entry.FiscalCalendarPeriod, PostingLayer = entry.PostingLayer,
                JournalNumber = entry.JournalNumber, Voucher = entry.SubledgerVoucher, VoucherDataAreaId = area,
                DocumentDate = entry.DocumentDate, DocumentNumber = entry.DocumentNumber };
            db.Add(journal);
            await db.SaveChangesAsync(ct);
        }
        var accounts = await db.Set<GeneralJournalAccountEntry>().Where(x => x.DataAreaId == area && x.GeneralJournalEntry == entry.RecId).ToListAsync(ct);
        var ids = accounts.Select(x => x.RecId).ToArray();
        var existing = await db.Set<SubledgerJournalAccountEntry>().Where(x => x.GeneralJournalAccountEntry != null
            && ids.Contains(x.GeneralJournalAccountEntry.Value)).Select(x => x.GeneralJournalAccountEntry!.Value).ToListAsync(ct);
        foreach (var account in accounts.Where(x => !existing.Contains(x.RecId)))
        {
            var distribution = new AccountingDistribution { DataAreaId = area, AccountingDate = entry.AccountingDate,
                AccountingEvent = journal.AccountingEvent, AccountingLegalEntity = ledger.PrimaryForLegalEntity > 0 ? ledger.PrimaryForLegalEntity : null,
                SourceDocumentHeader = header, SourceDocumentLine = sourceLine, LedgerDimension = account.LedgerDimension,
                AllocationFactor = 1m, TransactionCurrency = account.TransactionCurrencyCode, TransactionCurrencyAmount = account.TransactionCurrencyAmount };
            var subledger = new SubledgerJournalAccountEntry { DataAreaId = area, SubledgerJournalEntry = journal.RecId,
                GeneralJournalAccountEntry = account.RecId, LedgerDimension = account.LedgerDimension,
                PostingType = account.PostingType, IsCorrection = account.IsCorrection, IsTransferredInDetail = NoYes.Yes,
                TransactionCurrency = account.TransactionCurrencyCode, TransactionCurrencyAmount = account.TransactionCurrencyAmount,
                AccountingCurrencyAmount = account.AccountingCurrencyAmount, ReportingCurrencyAmount = account.ReportingCurrencyAmount };
            db.AddRange(distribution, subledger);
            await db.SaveChangesAsync(ct);
            db.Add(new SubledgerJournalAccountEntryDistribution { DataAreaId = area, AccountingDistribution = distribution.RecId,
                SubledgerJournalAccountEntry = subledger.RecId, AccountingCurrencyAmount = account.AccountingCurrencyAmount,
                ReportingCurrencyAmount = account.ReportingCurrencyAmount });
        }
        await db.SaveChangesAsync(ct);
    }
    public async Task VerifyBalancedAsync(GeneralJournalEntry entry, CancellationToken ct)
    {
        var accounts = await db.Set<GeneralJournalAccountEntry>().Where(x => x.DataAreaId == entry.DataAreaId && x.GeneralJournalEntry == entry.RecId).ToListAsync(ct);
        if (accounts.Sum(x => x.AccountingCurrencyAmount) != 0m || accounts.Sum(x => x.ReportingCurrencyAmount) != 0m
            || accounts.GroupBy(x => x.TransactionCurrencyCode).Any(g => g.Sum(x => x.TransactionCurrencyAmount) != 0m))
            throw new SubledgerPostingException("The posted voucher does not balance.");
    }
}
