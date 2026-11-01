using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendTransService : BaseService<VendTrans>, IVendTransService
{
    private readonly ICompanyExecutionContext _company;

    public VendTransService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICompanyExecutionContext company)
        : base(unitOfWork, currentUser)
    {
        _company = company;
    }

    public VendTransService(
        IUnitOfWork unitOfWork,
        ICompanyExecutionContext company)
        : this(unitOfWork, new NullCurrentUserService(company), company)
    {
    }

    private sealed class NullCurrentUserService(ICompanyExecutionContext company) : ICurrentUserService
    {
        public string GetCurrentUserId() => string.Empty;
        public string GetOwnerAccountId() => string.Empty;
        public string GetDataAreaId() => company.GetDataAreaId();
    }

    public Task<VendTransDto?> GetAsync(long recId, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        return _unitOfWork.Context.Set<VendTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.RecId == recId)
            .Select(x => new VendTransDto
            {
                RecId = x.RecId, AccountNum = x.AccountNum, TransDate = x.TransDate,
                Voucher = x.Voucher, Invoice = x.Invoice, Txt = x.Txt,
                CurrencyCode = x.CurrencyCode, AmountCur = x.AmountCur, AmountMst = x.AmountMst,
                SettleAmountCur = x.SettleAmountCur, SettleAmountMst = x.SettleAmountMst,
                DueDate = x.DueDate, Closed = x.Closed, TransType = x.TransType
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<VendTransPageDto> SearchAsync(VendTransQueryDto query, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        var transactions = _unitOfWork.Context.Set<VendTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == area);
        if (!string.IsNullOrWhiteSpace(query.AccountNum))
        {
            var accountNum = query.AccountNum.Trim();
            transactions = transactions.Where(x => x.AccountNum == accountNum);
        }

        var totalCount = await transactions.CountAsync(ct);
        var rows = await transactions.OrderByDescending(x => x.TransDate).ThenByDescending(x => x.RecId)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new VendTransDto
            {
                RecId = x.RecId, AccountNum = x.AccountNum, TransDate = x.TransDate,
                Voucher = x.Voucher, Invoice = x.Invoice, Txt = x.Txt,
                CurrencyCode = x.CurrencyCode, AmountCur = x.AmountCur, AmountMst = x.AmountMst,
                SettleAmountCur = x.SettleAmountCur, SettleAmountMst = x.SettleAmountMst,
                DueDate = x.DueDate, Closed = x.Closed, TransType = x.TransType
            }).ToListAsync(ct);
        return new VendTransPageDto(query.Page, query.PageSize, totalCount, rows);
    }

    /// <summary>
    /// The balance comes from VendTransOpen, which is the table the payment path settles against,
    /// rather than from a sum recomputed over VendTrans. An unsettled payment remainder is an open
    /// transaction too, and nets against the invoices, so the total is the vendor's balance.
    /// </summary>
    public async Task<VendOpenBalanceDto> GetOpenBalanceAsync(string? accountNum, DateTime? asOfDate, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        var asOf = (asOfDate ?? DateTime.UtcNow).Date;
        var account = string.IsNullOrWhiteSpace(accountNum) ? null : accountNum.Trim();

        var rows = await (from open in _unitOfWork.Context.Set<VendTransOpen>().AsNoTracking()
                          join trans in _unitOfWork.Context.Set<VendTrans>().AsNoTracking()
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
            .Select(x => new VendOpenTransDto(
                x.RecId, x.Invoice, x.Voucher, x.TransType, x.TransDate, x.DueDate, x.CurrencyCode,
                x.AmountCur, x.SettleAmountCur, x.OpenAmountCur, x.OpenAmountMst,
                // Arithmetic on the two dates, not an aging policy: no buckets are implied.
                x.DueDate.Date < asOf ? (asOf - x.DueDate.Date).Days : 0))
            .ToList();

        var reconciled = rows.All(x => x.OpenAmountCur == x.AmountCur - x.SettleAmountCur
            && x.OpenAmountMst == x.AmountMst - x.SettleAmountMst);

        return new VendOpenBalanceDto(account, asOf,
            rows.Sum(x => x.OpenAmountCur), rows.Sum(x => x.OpenAmountMst),
            transactions.Count, reconciled, transactions);
    }
}
