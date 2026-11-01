using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry;

[ApiController]
[Route("api/v1/LedgerFinancialStatements")]
[DomainPermission("GeneralLedger", "JournalEntries")]
public sealed class LedgerFinancialStatementsController(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate,
        [FromQuery] CurrentToOperationsPostingLayer postingLayer = CurrentToOperationsPostingLayer.Current,
        CancellationToken ct = default)
    {
        var area = currentUser.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return UnprocessableEntity(new { error = "A company must be selected." });
        if (toDate.Date < fromDate.Date) return BadRequest(new { error = "The end date cannot precede the start date." });
        var db = unitOfWork.Context;
        var ledger = await db.Set<Entities.Ledger>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area, ct);
        if (ledger == null) return UnprocessableEntity(new { error = "A ledger is required for the selected company." });
        var fiscalYear = await db.Set<FiscalCalendarYear>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.FiscalCalendar == ledger.FiscalCalendar &&
                x.StartDate <= toDate.Date && x.EndDate >= toDate.Date, ct);
        if (fiscalYear == null) return UnprocessableEntity(new { error = "No fiscal year contains the report end date." });
        var hasOpeningTransfer = await db.Set<GeneralJournalEntry>().AsNoTracking().AnyAsync(x =>
            x.Ledger == ledger.RecId && x.FiscalCalendarYear == fiscalYear.RecId &&
            x.JournalCategory == LedgerJournalCategory.YearEndOpening && x.PostingLayer == postingLayer, ct);

        var posted = await (from line in db.Set<GeneralJournalAccountEntry>().AsNoTracking()
                            join entry in db.Set<GeneralJournalEntry>().AsNoTracking()
                                on line.GeneralJournalEntry equals entry.RecId
                            where entry.Ledger == ledger.RecId && entry.PostingLayer == postingLayer
                                && entry.AccountingDate <= toDate.Date
                            select new { line.MainAccount, line.AccountingCurrencyAmount, entry.AccountingDate, entry.JournalCategory })
            .ToListAsync(ct);
        var ids = posted.Select(x => x.MainAccount).Distinct().ToList();
        var accounts = await db.Set<MainAccount>().AsNoTracking()
            .Where(x => ids.Contains(x.RecId) && x.LedgerChartOfAccounts == ledger.ChartOfAccounts)
            .Select(x => new { x.RecId, x.MainAccountId, x.Name, x.Type }).ToListAsync(ct);
        if (accounts.Count != ids.Count) return UnprocessableEntity(new { error = "A posted main account is missing from the ledger chart of accounts." });

        var lines = accounts.Select(account =>
        {
            var amounts = posted.Where(x => x.MainAccount == account.RecId);
            var balance = account.Type is MainAccountType.Asset or MainAccountType.Liability or MainAccountType.Equity
                ? amounts.Where(x => x.JournalCategory != LedgerJournalCategory.YearEndClosing &&
                    (!hasOpeningTransfer || x.AccountingDate >= fiscalYear.StartDate.Date)).Sum(x => x.AccountingCurrencyAmount)
                : amounts.Where(x => x.AccountingDate >= fromDate.Date &&
                    x.JournalCategory != LedgerJournalCategory.YearEndClosing).Sum(x => x.AccountingCurrencyAmount);
            return new { account.MainAccountId, account.Name, account.Type, Balance = balance };
        }).Where(x => x.Balance != 0m).OrderBy(x => x.MainAccountId).ToList();

        var assets = lines.Where(x => x.Type == MainAccountType.Asset).ToList();
        var liabilities = lines.Where(x => x.Type == MainAccountType.Liability).ToList();
        var equity = lines.Where(x => x.Type == MainAccountType.Equity).ToList();
        var revenue = lines.Where(x => x.Type == MainAccountType.Revenue).ToList();
        var expenses = lines.Where(x => x.Type == MainAccountType.Expense).ToList();
        var profit = -revenue.Sum(x => x.Balance) - expenses.Sum(x => x.Balance);
        var yearProfit = posted.Where(x => x.AccountingDate >= fiscalYear.StartDate &&
                x.JournalCategory != LedgerJournalCategory.YearEndClosing &&
                accounts.Any(a => a.RecId == x.MainAccount &&
                    (a.Type == MainAccountType.Revenue || a.Type == MainAccountType.Expense)))
            .Sum(x => x.AccountingCurrencyAmount);
        var balanceSheetDifference = assets.Sum(x => x.Balance) + liabilities.Sum(x => x.Balance) +
            equity.Sum(x => x.Balance) + yearProfit;
        return Ok(new
        {
            fromDate = fromDate.Date, toDate = toDate.Date, postingLayer, ledger.AccountingCurrency,
            assets, liabilities, equity, revenue, expenses,
            netProfit = profit,
            balanceSheetDifference
        });
    }
}
