using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Common;
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
        var (account, asOf, rows) = await LoadOpenRowsAsync(accountNum, asOfDate, ct);

        var transactions = rows
            .OrderBy(x => x.DueDate).ThenBy(x => x.RecId)
            .Select(x => new VendOpenTransDto(
                x.RecId, x.Invoice, x.Voucher, x.TransType, x.TransDate, x.DueDate, x.CurrencyCode,
                x.AmountCur, x.SettleAmountCur, x.OpenAmountCur, x.OpenAmountMst,
                // Arithmetic on the two dates, not an aging policy: no buckets are implied.
                DaysOverdue(x.DueDate, asOf)))
            .ToList();

        return new VendOpenBalanceDto(account, asOf,
            rows.Sum(x => x.OpenAmountCur), rows.Sum(x => x.OpenAmountMst),
            transactions.Count, Reconciled(rows), transactions);
    }

    public async Task<VendAgingDto> GetAgingAsync(string? accountNum, DateTime? asOfDate, CancellationToken ct)
    {
        var (account, asOf, rows) = await LoadOpenRowsAsync(accountNum, asOfDate, ct);

        return new VendAgingDto(account, asOf,
            rows.Sum(x => x.OpenAmountCur), rows.Sum(x => x.OpenAmountMst),
            rows.Count, Reconciled(rows), Bucket(rows, asOf),
            // One row per vendor, so a company-wide report is readable without a second call.
            // A single-vendor request still lists that vendor, which keeps the shape stable.
            rows.GroupBy(x => x.AccountNum).OrderBy(x => x.Key)
                .Select(vendor => new VendAgingRowDto(vendor.Key,
                    vendor.Sum(x => x.OpenAmountCur), vendor.Sum(x => x.OpenAmountMst),
                    vendor.Count(), Bucket(vendor.ToList(), asOf)))
                .ToList());
    }

    /// <summary>
    /// The open vendor rows as of a date, which both the open balance and the aging report read.
    /// Kept in one place so the two can never disagree about what is open.
    /// </summary>
    private async Task<(string? Account, DateTime AsOf, List<OpenRow> Rows)> LoadOpenRowsAsync(
        string? accountNum, DateTime? asOfDate, CancellationToken ct)
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
                          select new OpenRow(
                              trans.RecId, open.AccountNum, trans.Invoice, trans.Voucher, trans.TransType,
                              trans.TransDate, open.DueDate, trans.CurrencyCode, trans.AmountCur,
                              trans.SettleAmountCur, trans.AmountMst, trans.SettleAmountMst,
                              open.AmountCur, open.AmountMst)).ToListAsync(ct);

        return (account, asOf, rows);
    }

    private sealed record OpenRow(long RecId, string AccountNum, string Invoice, string Voucher,
        LedgerTransType TransType, DateTime TransDate, DateTime DueDate, string CurrencyCode,
        decimal AmountCur, decimal SettleAmountCur, decimal AmountMst, decimal SettleAmountMst,
        decimal OpenAmountCur, decimal OpenAmountMst);

    /// <summary>Days past the due date, which is zero for anything not yet due.</summary>
    private static int DaysOverdue(DateTime dueDate, DateTime asOf) =>
        dueDate.Date < asOf ? (asOf - dueDate.Date).Days : 0;

    private static bool Reconciled(List<OpenRow> rows) =>
        rows.All(x => x.OpenAmountCur == x.AmountCur - x.SettleAmountCur
            && x.OpenAmountMst == x.AmountMst - x.SettleAmountMst);

    /// <summary>
    /// The aging periods. These are the same fixed boundaries the receivable side ages by, so a
    /// payable and a receivable report are read against the same periods. D365 holds configurable
    /// period definitions instead; that table does not exist here and is a recorded gap, so the
    /// boundaries are not presented as configuration.
    /// </summary>
    private static readonly (int? Min, int? Max)[] AgingPeriods =
        [(null, 0), (1, 30), (31, 60), (61, 90), (91, null)];

    private static List<VendAgingBucketDto> Bucket(List<OpenRow> rows, DateTime asOf)
    {
        var amountCur = new decimal[AgingPeriods.Length];
        var amountMst = new decimal[AgingPeriods.Length];
        var counts = new int[AgingPeriods.Length];
        foreach (var row in rows)
        {
            var days = DaysOverdue(row.DueDate, asOf);
            var index = days switch { <= 0 => 0, <= 30 => 1, <= 60 => 2, <= 90 => 3, _ => 4 };
            amountCur[index] += row.OpenAmountCur;
            amountMst[index] += row.OpenAmountMst;
            counts[index]++;
        }
        return [.. AgingPeriods.Select((period, index) =>
            new VendAgingBucketDto(period.Min, period.Max, amountCur[index], amountMst[index], counts[index]))];
    }
}
