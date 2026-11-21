using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry;

/// <summary>
/// Voucher transactions over posted accounting. Posted general ledger is scoped by its ledger,
/// so the selected company is resolved to its ledger rather than filtered on the posted tables.
/// Amounts are the signed posted amounts; IsCredit is not applied a second time.
/// </summary>
public sealed class LedgerTransVoucherService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    : ILedgerTransVoucherService
{
    private DbContext Db => unitOfWork.Context;

    public async Task<(IReadOnlyList<LedgerTransVoucherRow>? Rows, string? Error)> GetAsync(
        string? voucher, DateTime? fromDate, DateTime? toDate, CancellationToken ct)
    {
        var area = currentUser.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return (null, "A company must be selected.");
        var start = fromDate?.Date;
        // The whole accounting day belongs to the range, including imported timestamps.
        var end = toDate?.Date.AddTicks(TimeSpan.TicksPerDay - 1);
        if (start != null && end != null && end < start)
            return (null, "The end date cannot precede the start date.");

        var ledgers = await Db.Set<Entities.Ledger>().AsNoTracking()
            .Where(x => x.DataAreaId == area).Take(2).ToListAsync(ct);
        if (ledgers.Count == 0) return (null, "A ledger is required for the selected company.");
        if (ledgers.Count > 1) return (null, "Exactly one ledger is required for the selected company.");
        var ledger = ledgers[0];
        var trimmedVoucher = voucher?.Trim();

        var rows = await (from line in Db.Set<GeneralJournalAccountEntry>().AsNoTracking()
                          join entry in Db.Set<GeneralJournalEntry>().AsNoTracking()
                              on line.GeneralJournalEntry equals entry.RecId
                          join account in Db.Set<MainAccount>().AsNoTracking()
                              on line.MainAccount equals account.RecId into accounts
                          from account in accounts.DefaultIfEmpty()
                          where entry.Ledger == ledger.RecId
                              && (string.IsNullOrEmpty(trimmedVoucher) || entry.SubledgerVoucher == trimmedVoucher)
                              && (start == null || entry.AccountingDate >= start)
                              && (end == null || entry.AccountingDate <= end)
                          orderby entry.AccountingDate, entry.SubledgerVoucher, line.RecId
                          select new
                          {
                              line.RecId,
                              entry.JournalNumber,
                              entry.SubledgerVoucher,
                              entry.AccountingDate,
                              entry.FiscalCalendarYear,
                              line.LedgerAccount,
                              AccountName = account == null ? string.Empty : account.Name,
                              line.Text,
                              line.TransactionCurrencyCode,
                              line.TransactionCurrencyAmount,
                              line.AccountingCurrencyAmount,
                              line.PostingType
                          }).ToListAsync(ct);

        var closedYears = await ClosedFiscalYearsAsync(ledger.RecId,
            rows.Select(x => x.FiscalCalendarYear).Distinct().ToList(), ct);

        return (rows.Select(x => new LedgerTransVoucherRow(
            x.RecId, x.JournalNumber, x.SubledgerVoucher, x.AccountingDate,
            closedYears.Contains(x.FiscalCalendarYear), x.LedgerAccount, x.AccountName, x.Text,
            x.TransactionCurrencyCode, x.TransactionCurrencyAmount, x.AccountingCurrencyAmount,
            x.PostingType.ToString())).ToList(), null);
    }

    /// <summary>A fiscal year reads as closed once the ledger holds no period of it still open.</summary>
    private async Task<HashSet<long>> ClosedFiscalYearsAsync(long ledger, List<long> years, CancellationToken ct)
    {
        if (years.Count == 0) return [];
        var periods = await (from ledgerPeriod in Db.Set<LedgerFiscalCalendarPeriod>().AsNoTracking()
                             join period in Db.Set<FiscalCalendarPeriod>().AsNoTracking()
                                 on ledgerPeriod.FiscalCalendarPeriod equals period.RecId
                             where ledgerPeriod.Ledger == ledger && years.Contains(period.FiscalCalendarYear)
                             select new { period.FiscalCalendarYear, ledgerPeriod.Status }).ToListAsync(ct);
        return periods.GroupBy(x => x.FiscalCalendarYear)
            .Where(group => group.All(x => x.Status is FiscalPeriodStatus.Closed or FiscalPeriodStatus.YearClosed))
            .Select(group => group.Key).ToHashSet();
    }
}
