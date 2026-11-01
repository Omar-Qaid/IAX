using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry;

/// <summary>
/// Trial balance over posted accounting. Posted general ledger is scoped by its ledger and
/// accounting date, with the posting layer, rather than by company on the posted tables: company
/// context follows the ledger.
///
/// Amounts are read as signed accounting amounts. IsCredit is not applied a second time and the
/// amounts are not made absolute, so correction and reversal entries keep their own presentation.
/// </summary>
public sealed class LedgerTrialBalanceService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    : ILedgerTrialBalanceService
{
    private DbContext Db => unitOfWork.Context;

    public async Task<(LedgerTrialBalanceDto? Result, string? Error)> GetAsync(DateTime fromDate, DateTime toDate,
        CurrentToOperationsPostingLayer postingLayer, CancellationToken ct)
    {
        var area = currentUser.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return (null, "A company must be selected.");
        var start = fromDate.Date;
        var end = toDate.Date;
        if (end < start) return (null, "The end date cannot precede the start date.");

        var ledger = await Db.Set<Entities.Ledger>().AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == area, ct);
        if (ledger == null) return (null, "A ledger is required for the selected company.");
        var fiscalYear = await Db.Set<FiscalCalendarYear>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.FiscalCalendar == ledger.FiscalCalendar && x.StartDate <= start && x.EndDate >= start, ct);
        var hasOpeningTransfer = fiscalYear != null && await Db.Set<GeneralJournalEntry>().AsNoTracking().AnyAsync(x =>
            x.Ledger == ledger.RecId && x.FiscalCalendarYear == fiscalYear.RecId &&
            x.JournalCategory == LedgerJournalCategory.YearEndOpening && x.PostingLayer == postingLayer, ct);

        var rows = await (from line in Db.Set<GeneralJournalAccountEntry>().AsNoTracking()
                          join entry in Db.Set<GeneralJournalEntry>().AsNoTracking() on line.GeneralJournalEntry equals entry.RecId
                          where entry.Ledger == ledger.RecId && entry.PostingLayer == postingLayer
                              && entry.AccountingDate <= end
                          select new { line.MainAccount, line.LedgerAccount, line.AccountingCurrencyAmount,
                              entry.AccountingDate, entry.JournalCategory })
            .ToListAsync(ct);

        if (hasOpeningTransfer)
            rows = rows.Where(x => x.AccountingDate >= fiscalYear!.StartDate.Date).ToList();
        var accountIds = rows.Select(x => x.MainAccount).Distinct().ToList();
        var accounts = await Db.Set<MainAccount>().AsNoTracking()
            .Where(x => accountIds.Contains(x.RecId))
            .Select(x => new { x.RecId, x.MainAccountId, x.Name })
            .ToListAsync(ct);

        var lines = rows.GroupBy(x => x.MainAccount)
            .Select(group =>
            {
                var account = accounts.FirstOrDefault(x => x.RecId == group.Key);
                var opening = group.Where(x => x.AccountingDate < start ||
                    (x.AccountingDate == start && x.JournalCategory == LedgerJournalCategory.YearEndOpening))
                    .Sum(x => x.AccountingCurrencyAmount);
                var period = group.Where(x => x.AccountingDate >= start &&
                    !(x.AccountingDate == start && x.JournalCategory == LedgerJournalCategory.YearEndOpening)).ToList();
                var debit = period.Where(x => x.AccountingCurrencyAmount > 0).Sum(x => x.AccountingCurrencyAmount);
                var credit = -period.Where(x => x.AccountingCurrencyAmount < 0).Sum(x => x.AccountingCurrencyAmount);
                var net = opening + debit - credit;
                return new LedgerTrialBalanceLineDto(
                    account?.MainAccountId ?? group.First().LedgerAccount,
                    account?.Name ?? string.Empty,
                    debit, credit,
                    net > 0 ? net : 0m,
                    net < 0 ? -net : 0m)
                {
                    OpeningDebit = opening > 0 ? opening : 0m,
                    OpeningCredit = opening < 0 ? -opening : 0m
                };
            })
            .OrderBy(x => x.MainAccountId, StringComparer.Ordinal)
            .ToList();

        return (new LedgerTrialBalanceDto(start, end, postingLayer, ledger.AccountingCurrency, lines,
            lines.Sum(x => x.Debit), lines.Sum(x => x.Credit),
            lines.Sum(x => x.DebitBalance), lines.Sum(x => x.CreditBalance)), null);
    }
}
