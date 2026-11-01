using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Application.Identity;
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

    public CustTransService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
        : this(unitOfWork, new NullCurrentUserService(company))
    {
    }

    private sealed class NullCurrentUserService(ICompanyExecutionContext company) : ICurrentUserService
    {
        public string GetCurrentUserId() => string.Empty;
        public string GetOwnerAccountId() => string.Empty;
        public string GetDataAreaId() => company.GetDataAreaId();
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
