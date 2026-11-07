using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public class CustTransService : BaseService<CustTrans>, ICustTransService
{
    private readonly ICurrentUserService _user;

    public CustTransService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        : base(unitOfWork, currentUser)
    {
        _user = currentUser;
    }

    public async Task<CustAgingDto> GetAgingAsync(
        string accountNum, DateTime balanceAsOf, DateTime agingAsOf, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountNum);
        var account = accountNum.Trim();
        var area = _user.GetDataAreaId();
        var cutoff = balanceAsOf.Date;
        var aging = agingAsOf.Date;
        var transactions = _unitOfWork.Context.Set<CustTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.AccountNum == account && x.TransDate.Date <= cutoff);
        // Keep both original and reversing settlement rows. A match cannot reduce an open
        // balance before the offset transaction exists, even if the settlement was backdated.
        var settlements = from settlement in _unitOfWork.Context.Set<CustSettlement>().AsNoTracking()
                          join offset in transactions on settlement.OffsetRecId equals offset.RecId
                          where settlement.DataAreaId == area && settlement.AccountNum == account
                              && settlement.TransDate.Date <= cutoff
                          select settlement;
        var rows = await (from transaction in transactions
                          join settlement in settlements on transaction.RecId equals settlement.TransRecId into matches
                          from match in matches.DefaultIfEmpty()
                          select new
                          {
                              transaction.RecId, transaction.DueDate, transaction.AmountMst,
                              SettledMst = (decimal?)match.SettleAmountMst
                          }).ToListAsync(ct);

        var balances = new decimal[5];
        foreach (var group in rows.GroupBy(x => new { x.RecId, x.DueDate, x.AmountMst }))
        {
            var days = (aging - group.Key.DueDate.Date).Days;
            var bucket = days switch { <= 0 => 0, <= 30 => 1, <= 60 => 2, <= 90 => 3, _ => 4 };
            balances[bucket] += group.Key.AmountMst - group.Sum(x => x.SettledMst ?? 0m);
        }
        return new CustAgingDto(account, cutoff, aging, balances.Sum(),
            [new(null, 0, balances[0]), new(1, 30, balances[1]), new(31, 60, balances[2]),
             new(61, 90, balances[3]), new(91, null, balances[4])]);
    }

    public async Task<CustStatementDto> GetStatementAsync(
        string accountNum, DateTime fromDate, DateTime toDate, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountNum);
        var account = accountNum.Trim();
        var from = fromDate.Date;
        var to = toDate.Date;
        if (to < from)
            throw new ArgumentException("The statement end date must be on or after the start date.", nameof(toDate));

        var area = _user.GetDataAreaId();
        var db = _unitOfWork.Context;
        // A single-line bank receipt links to one customer transaction. Its posted customer
        // entry includes discounts and realized exchange differences in the receivable cleared.
        var receipts = from line in db.Set<LedgerJournalTrans>().AsNoTracking()
                       join journal in db.Set<LedgerJournalTable>().AsNoTracking()
                           on new { line.DataAreaId, line.JournalNum } equals new { journal.DataAreaId, journal.JournalNum }
                       join voucher in db.Set<SubledgerVoucherGeneralJournalEntry>().AsNoTracking()
                           on new { Company = line.DataAreaId, line.Voucher, Date = line.TransDate }
                           equals new { Company = voucher.VoucherDataAreaId, voucher.Voucher, Date = voucher.AccountingDate }
                       join entry in db.Set<GeneralJournalEntry>().AsNoTracking()
                           on voucher.GeneralJournalEntry equals entry.RecId
                       join posting in db.Set<GeneralJournalAccountEntry>().AsNoTracking()
                           on entry.RecId equals posting.GeneralJournalEntry
                       where line.DataAreaId == area && line.LedgerDimensionName == account
                           && line.AccountType == LedgerJournalACType.Cust
                           && line.OffsetAccountType == LedgerJournalACType.Bank
                           && journal.JournalType == LedgerJournalType.CustPayment
                           && journal.Posted == NoYes.Yes && journal.NumOfLines == 1
                           && entry.DataAreaId == area && entry.JournalNumber == line.JournalNum
                           && posting.DataAreaId == area && posting.PostingType == LedgerPostingType.CustBalance
                       select new { line.CustTransId, line.Voucher, line.TransDate, posting.AccountingCurrencyAmount };
        var transactions = from transaction in db.Set<CustTrans>().AsNoTracking()
                           where transaction.DataAreaId == area && transaction.AccountNum == account
                               && transaction.TransDate.Date <= to
                           orderby transaction.TransDate, transaction.RecId
                           select new
                           {
                               transaction.RecId, transaction.TransDate, transaction.Invoice, transaction.Voucher,
                               transaction.TransType, transaction.CurrencyCode, transaction.AmountCur,
                               AmountMst = receipts.Where(x => x.CustTransId == transaction.RecId
                                       && x.Voucher == transaction.Voucher && x.TransDate == transaction.TransDate)
                                   .Select(x => (decimal?)x.AccountingCurrencyAmount).FirstOrDefault() ?? transaction.AmountMst
                           };

        // Settlements alter open balances, while a statement keeps the signed posted movements.
        decimal opening = 0;
        decimal balance = 0;
        var lines = new List<CustStatementLineDto>();
        await foreach (var transaction in transactions.AsAsyncEnumerable().WithCancellation(ct))
        {
            balance += transaction.AmountMst;
            if (transaction.TransDate < from)
            {
                opening = balance;
                continue;
            }
            lines.Add(new CustStatementLineDto(transaction.RecId, transaction.TransDate, transaction.Invoice,
                transaction.Voucher, transaction.TransType, transaction.CurrencyCode, transaction.AmountCur,
                transaction.AmountMst, balance));
        }
        return new CustStatementDto(account, from, to, opening, balance, lines);
    }

    /// <summary>
    /// The balance comes from CustTransOpen, which is the table the payment path settles against,
    /// rather than from a sum recomputed over CustTrans. An unsettled payment remainder is an open
    /// transaction too, and nets against the invoices, so the total is the customer's balance.
    /// </summary>
    public async Task<CustOpenBalanceDto> GetOpenBalanceAsync(string? accountNum, DateTime? asOfDate, CancellationToken ct)
    {
        var area = _user.GetDataAreaId();
        var asOf = (asOfDate ?? DateTime.UtcNow).Date;
        var account = string.IsNullOrWhiteSpace(accountNum) ? null : accountNum.Trim();

        var rows = await (from open in _unitOfWork.Context.Set<CustTransOpen>().AsNoTracking()
                          join trans in _unitOfWork.Context.Set<CustTrans>().AsNoTracking()
                              on open.RefRecId equals trans.RecId
                          where open.DataAreaId == area && trans.DataAreaId == area
                              && (account == null || open.AccountNum == account)
                              && trans.TransDate <= asOf
                          select new
                          {
                              trans.RecId, trans.Invoice, trans.Voucher, trans.TransType, trans.TransDate,
                              open.DueDate, trans.CurrencyCode, trans.AmountCur, trans.SettleAmountCur,
                              trans.AmountMst, trans.SettleAmountMst,
                              OpenAmountCur = open.AmountCur, OpenAmountMst = open.AmountMst
                          }).ToListAsync(ct);

        var transactions = rows
            .OrderBy(x => x.DueDate).ThenBy(x => x.RecId)
            .Select(x => new CustOpenTransDto(
                x.RecId, x.Invoice, x.Voucher, x.TransType, x.TransDate, x.DueDate, x.CurrencyCode,
                x.AmountCur, x.SettleAmountCur, x.OpenAmountCur, x.OpenAmountMst,
                // Arithmetic on the two dates, not an aging policy: no buckets are implied.
                x.DueDate.Date < asOf ? (asOf - x.DueDate.Date).Days : 0))
            .ToList();

        var reconciled = rows.All(x => x.OpenAmountCur == x.AmountCur - x.SettleAmountCur
            && x.OpenAmountMst == x.AmountMst - x.SettleAmountMst);

        return new CustOpenBalanceDto(account, asOf,
            rows.Sum(x => x.OpenAmountCur), rows.Sum(x => x.OpenAmountMst),
            transactions.Count, reconciled, transactions);
    }
}
