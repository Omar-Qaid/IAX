using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar;

[ApiController]
[Route("api/v1/LedgerYearEndClose")]
[DomainPermission("GeneralLedger", "JournalEntries")]
public sealed class LedgerYearEndCloseController(IUnitOfWork work, ICurrentUserService user) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Close([FromBody] LedgerYearEndCloseRequest request, CancellationToken ct)
    {
        var area = user.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return UnprocessableEntity(new { error = "A company must be selected." });
        var db = work.Context;
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await work.BeginTransactionAsync(ct);
            try
            {
                var support = new SubledgerPostingSupport(db);
                await support.LockAsync($"YearEnd:{area}:{request.FiscalCalendarYearId}", ct);
                var ledger = await db.Set<Entities.Ledger>()
                    .SingleOrDefaultAsync(x => x.DataAreaId == area, ct);
                if (ledger == null) return await Reject("A ledger is required for the selected company.");
                if (!string.IsNullOrWhiteSpace(ledger.ReportingCurrency) &&
                    !string.Equals(ledger.ReportingCurrency, ledger.AccountingCurrency, StringComparison.OrdinalIgnoreCase))
                    return await Reject("Year-end transfer requires reporting-currency balance conversion, which is not supported.");
                var year = await db.Set<FiscalCalendarYear>().AsNoTracking()
                    .SingleOrDefaultAsync(x => x.RecId == request.FiscalCalendarYearId && x.FiscalCalendar == ledger.FiscalCalendar, ct);
                if (year == null) return await Reject("Fiscal year was not found on the selected ledger calendar.");
                if (ledger.MostRecentYearEndClose > year.EndDate.Date)
                    return await Reject("A later fiscal year has already been closed.");
                var ledgerYear = await db.Set<LedgerFiscalCalendarYear>()
                    .SingleOrDefaultAsync(x => x.Ledger == ledger.RecId && x.FiscalCalendarYear == year.RecId, ct);
                if (ledgerYear == null || ledgerYear.Status == FiscalPeriodStatus.YearClosed)
                    return await Reject("The fiscal year is missing or permanently closed.");
                var nextYear = await db.Set<FiscalCalendarYear>().AsNoTracking()
                    .Where(x => x.FiscalCalendar == ledger.FiscalCalendar && x.StartDate > year.EndDate)
                    .OrderBy(x => x.StartDate).FirstOrDefaultAsync(ct);
                if (nextYear == null) return await Reject("The next fiscal year must be configured before year-end transfer.");
                var nextPeriod = await db.Set<FiscalCalendarPeriod>().AsNoTracking()
                    .Where(x => x.FiscalCalendarYear == nextYear.RecId && x.StartDate <= nextYear.StartDate && x.EndDate >= nextYear.StartDate)
                    .OrderBy(x => x.Type == FiscalPeriodType.Opening ? 0 : 1).FirstOrDefaultAsync(ct);
                if (nextPeriod == null) return await Reject("The next fiscal year needs a period containing its opening date.");
                var periods = await (from lp in db.Set<LedgerFiscalCalendarPeriod>()
                                     join p in db.Set<FiscalCalendarPeriod>() on lp.FiscalCalendarPeriod equals p.RecId
                                     where lp.Ledger == ledger.RecId && p.FiscalCalendarYear == year.RecId
                                     select new { LedgerPeriod = lp, Period = p }).ToListAsync(ct);
                if (periods.Count == 0) return await Reject("The fiscal year has no ledger periods.");
                var retainedDimension = await db.Set<DimensionAttributeValueCombination>().AsNoTracking()
                    .SingleOrDefaultAsync(x => x.DataAreaId == area && x.RecId == request.RetainedEarningsLedgerDimension, ct);
                if (retainedDimension == null) return await Reject("Retained earnings ledger dimension was not found.");
                var retainedAccount = await db.Set<MainAccount>().AsNoTracking()
                    .SingleOrDefaultAsync(x => x.RecId == retainedDimension.MainAccount &&
                        x.LedgerChartOfAccounts == ledger.ChartOfAccounts && x.Type == MainAccountType.Equity, ct);
                if (retainedAccount == null) return await Reject("Retained earnings must be an equity account on the ledger chart.");

                var posted = await (from line in db.Set<GeneralJournalAccountEntry>().AsNoTracking()
                                    join journal in db.Set<GeneralJournalEntry>().AsNoTracking()
                                        on line.GeneralJournalEntry equals journal.RecId
                                    where journal.Ledger == ledger.RecId &&
                                        journal.PostingLayer == CurrentToOperationsPostingLayer.Current &&
                                        journal.AccountingDate >= year.StartDate.Date && journal.AccountingDate <= year.EndDate.Date &&
                                        journal.JournalCategory != LedgerJournalCategory.YearEndClosing
                                    select new { line.LedgerDimension, line.MainAccount, line.AccountingCurrencyAmount })
                    .ToListAsync(ct);
                var accountIds = posted.Select(x => x.MainAccount).Distinct().ToList();
                var allAccounts = await db.Set<MainAccount>().AsNoTracking()
                    .Where(x => accountIds.Contains(x.RecId) && x.LedgerChartOfAccounts == ledger.ChartOfAccounts)
                    .Select(x => new { x.RecId, x.Type }).ToListAsync(ct);
                if (allAccounts.Count != accountIds.Count)
                    return await Reject("A posted main account is missing from the ledger chart of accounts.");
                var balances = posted
                    .GroupBy(x => x.LedgerDimension)
                    .Select(x => new { Dimension = x.Key, MainAccount = x.First().MainAccount,
                        Amount = x.Sum(y => y.AccountingCurrencyAmount) })
                    .Where(x => x.Amount != 0m).ToList();
                if (balances.Sum(x => x.Amount) != 0m)
                    return await Reject("The fiscal-year ledger balances are not balanced.");
                var dimensionIds = balances.Select(x => x.Dimension).ToList();
                var dimensions = await db.Set<DimensionAttributeValueCombination>().AsNoTracking()
                    .Where(x => x.DataAreaId == area && dimensionIds.Contains(x.RecId)).ToDictionaryAsync(x => x.RecId, ct);
                if (dimensions.Count != balances.Count) return await Reject("A posted ledger dimension is missing.");
                var openingPrefix = $"YEAR-OPEN-{year.RecId}-";
                var priorOpening = await (from line in db.Set<GeneralJournalAccountEntry>().AsNoTracking()
                                          join journal in db.Set<GeneralJournalEntry>().AsNoTracking()
                                              on line.GeneralJournalEntry equals journal.RecId
                                          where journal.Ledger == ledger.RecId &&
                                              journal.JournalCategory == LedgerJournalCategory.YearEndOpening &&
                                              journal.JournalNumber.StartsWith(openingPrefix)
                                          select new { line.LedgerDimension, line.AccountingCurrencyAmount }).ToListAsync(ct);
                var expected = balances.Where(x => allAccounts.Any(a => a.RecId == x.MainAccount &&
                        a.Type is MainAccountType.Asset or MainAccountType.Liability or MainAccountType.Equity))
                    .ToDictionary(x => x.Dimension, x => x.Amount);
                var profit = balances.Where(x => allAccounts.Any(a => a.RecId == x.MainAccount &&
                        a.Type is MainAccountType.Revenue or MainAccountType.Expense)).Sum(x => x.Amount);
                expected[retainedDimension.RecId] = expected.GetValueOrDefault(retainedDimension.RecId) + profit;
                var previouslyTransferred = priorOpening.GroupBy(x => x.LedgerDimension)
                    .ToDictionary(x => x.Key, x => x.Sum(y => y.AccountingCurrencyAmount));
                var delta = expected.Keys.Union(previouslyTransferred.Keys)
                    .Select(id => (Dimension: id, Amount: expected.GetValueOrDefault(id) - previouslyTransferred.GetValueOrDefault(id)))
                    .Where(x => x.Amount != 0m).ToList();
                if (delta.Count == 0)
                {
                    if (request.CreateClosingTransaction)
                    {
                        var closingPrefix = $"YEAR-CLOSE-{year.RecId}-";
                        var alreadyClosed = await db.Set<GeneralJournalEntry>().AnyAsync(x => x.Ledger == ledger.RecId &&
                            x.JournalCategory == LedgerJournalCategory.YearEndClosing &&
                            x.JournalNumber.StartsWith(closingPrefix), ct);
                        if (!alreadyClosed && balances.Count > 0)
                        {
                            var closingPeriod = periods.OrderByDescending(x => x.Period.EndDate).First().Period;
                            var closingNumber = $"{closingPrefix}1";
                            var closing = new GeneralJournalEntry
                            {
                                DataAreaId = area, JournalNumber = closingNumber, Ledger = ledger.RecId,
                                JournalCategory = LedgerJournalCategory.YearEndClosing,
                                PostingLayer = CurrentToOperationsPostingLayer.Current,
                                SubledgerVoucher = closingNumber, SubledgerVoucherDataAreaId = area,
                                DocumentNumber = closingNumber, AccountingDate = year.EndDate.Date,
                                DocumentDate = year.EndDate.Date, FiscalCalendarYear = year.RecId,
                                FiscalCalendarPeriod = closingPeriod.RecId
                            };
                            db.Add(closing);
                            await db.SaveChangesAsync(ct);
                            db.Add(new SubledgerVoucherGeneralJournalEntry
                            {
                                DataAreaId = area, GeneralJournalEntry = closing.RecId, Voucher = closingNumber,
                                VoucherDataAreaId = area, AccountingDate = year.EndDate.Date
                            });
                            foreach (var balance in balances)
                                db.Add(SubledgerPostingSupport.AccountEntry(area, closing.RecId, dimensions[balance.Dimension],
                                    LedgerPostingType.LedgerJournal, -balance.Amount, "Year-end closing", 0m, ledger.AccountingCurrency));
                        }
                    }
                    if (request.PermanentlyCloseYear)
                    {
                        foreach (var period in periods) period.LedgerPeriod.Status = FiscalPeriodStatus.YearClosed;
                        ledgerYear.Status = FiscalPeriodStatus.YearClosed;
                    }
                    await work.CompleteAsync(ct);
                    await work.CommitTransactionAsync(ct);
                    return Ok(new { message = "Opening balances are current; no adjustment transfer was needed." });
                }
                if (delta.Sum(x => x.Amount) != 0m) return await Reject("The opening transfer is not balanced.");
                var run = await db.Set<GeneralJournalEntry>().CountAsync(x => x.Ledger == ledger.RecId &&
                    x.JournalCategory == LedgerJournalCategory.YearEndOpening && x.JournalNumber.StartsWith(openingPrefix), ct) + 1;
                var number = $"{openingPrefix}{run}";
                var entry = new GeneralJournalEntry
                {
                    DataAreaId = area, JournalNumber = number, Ledger = ledger.RecId,
                    JournalCategory = LedgerJournalCategory.YearEndOpening,
                    PostingLayer = CurrentToOperationsPostingLayer.Current,
                    SubledgerVoucher = number, SubledgerVoucherDataAreaId = area,
                    DocumentNumber = number, AccountingDate = nextYear.StartDate.Date,
                    DocumentDate = nextYear.StartDate.Date, FiscalCalendarYear = nextYear.RecId,
                    FiscalCalendarPeriod = nextPeriod.RecId
                };
                db.Add(entry);
                await db.SaveChangesAsync(ct);
                db.Add(new SubledgerVoucherGeneralJournalEntry
                {
                    DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = number,
                    VoucherDataAreaId = area, AccountingDate = nextYear.StartDate.Date
                });
                foreach (var balance in delta)
                {
                    var dimension = balance.Dimension == retainedDimension.RecId ? retainedDimension :
                        dimensions.GetValueOrDefault(balance.Dimension);
                    if (dimension == null) return await Reject("An opening ledger dimension is missing.");
                    db.Add(SubledgerPostingSupport.AccountEntry(area, entry.RecId, dimension,
                        LedgerPostingType.LedgerJournal, balance.Amount, "Year-end opening transfer", 0m, ledger.AccountingCurrency));
                }
                if (request.CreateClosingTransaction)
                {
                    var closingNumber = $"YEAR-CLOSE-{year.RecId}-{run}";
                    var closingPeriod = periods.OrderByDescending(x => x.Period.EndDate).First().Period;
                    var closing = new GeneralJournalEntry
                    {
                        DataAreaId = area, JournalNumber = closingNumber, Ledger = ledger.RecId,
                        JournalCategory = LedgerJournalCategory.YearEndClosing,
                        PostingLayer = CurrentToOperationsPostingLayer.Current,
                        SubledgerVoucher = closingNumber, SubledgerVoucherDataAreaId = area,
                        DocumentNumber = closingNumber, AccountingDate = year.EndDate.Date,
                        DocumentDate = year.EndDate.Date, FiscalCalendarYear = year.RecId,
                        FiscalCalendarPeriod = closingPeriod.RecId
                    };
                    db.Add(closing);
                    await db.SaveChangesAsync(ct);
                    db.Add(new SubledgerVoucherGeneralJournalEntry
                    {
                        DataAreaId = area, GeneralJournalEntry = closing.RecId, Voucher = closingNumber,
                        VoucherDataAreaId = area, AccountingDate = year.EndDate.Date
                    });
                    var priorClosing = await (from line in db.Set<GeneralJournalAccountEntry>().AsNoTracking()
                                              join journal in db.Set<GeneralJournalEntry>().AsNoTracking()
                                                  on line.GeneralJournalEntry equals journal.RecId
                                              where journal.Ledger == ledger.RecId &&
                                                  journal.JournalCategory == LedgerJournalCategory.YearEndClosing &&
                                                  journal.JournalNumber.StartsWith($"YEAR-CLOSE-{year.RecId}-") &&
                                                  journal.RecId != closing.RecId
                                              select new { line.LedgerDimension, line.AccountingCurrencyAmount }).ToListAsync(ct);
                    var closedByDimension = priorClosing.GroupBy(x => x.LedgerDimension)
                        .ToDictionary(x => x.Key, x => x.Sum(y => y.AccountingCurrencyAmount));
                    foreach (var balance in balances)
                    {
                        var closeDelta = -balance.Amount - closedByDimension.GetValueOrDefault(balance.Dimension);
                        if (closeDelta != 0m)
                            db.Add(SubledgerPostingSupport.AccountEntry(area, closing.RecId, dimensions[balance.Dimension],
                                LedgerPostingType.LedgerJournal, closeDelta, "Year-end closing", 0m, ledger.AccountingCurrency));
                    }
                }
                if (request.PermanentlyCloseYear)
                {
                    foreach (var period in periods) period.LedgerPeriod.Status = FiscalPeriodStatus.YearClosed;
                    ledgerYear.Status = FiscalPeriodStatus.YearClosed;
                }
                ledger.MostRecentYearEndClose = year.EndDate.Date;
                await work.CompleteAsync(ct);
                await work.CommitTransactionAsync(ct);
                return Ok(new { entry.RecId, entry.JournalNumber, nextYear.StartDate, TransferredAccounts = delta.Count });
            }
            catch
            {
                await work.RollbackTransactionAsync(ct);
                db.ChangeTracker.Clear();
                throw;
            }

            async Task<IActionResult> Reject(string error)
            {
                await work.RollbackTransactionAsync(ct);
                db.ChangeTracker.Clear();
                return UnprocessableEntity(new { error });
            }
        });
    }
}

public sealed record LedgerYearEndCloseRequest(long FiscalCalendarYearId, long RetainedEarningsLedgerDimension,
    bool CreateClosingTransaction = false, bool PermanentlyCloseYear = false);
