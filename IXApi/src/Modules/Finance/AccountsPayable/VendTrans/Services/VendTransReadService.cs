using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class VendTransReadService(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
    : IVendTransReadService
{
    public Task<VendTransDto?> GetAsync(long recId, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        return unitOfWork.Context.Set<VendTrans>().AsNoTracking()
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
        var area = company.GetDataAreaId();
        var transactions = unitOfWork.Context.Set<VendTrans>().AsNoTracking()
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
}
