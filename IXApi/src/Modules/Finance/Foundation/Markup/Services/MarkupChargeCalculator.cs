using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Markup;

public sealed class MarkupChargeCalculator : IMarkupChargeCalculator
{
    private readonly IUnitOfWork _unitOfWork;
    private DbContext _db => _unitOfWork.Context;
    private readonly ICompanyExecutionContext _company;

    public MarkupChargeCalculator(IUnitOfWork unitOfWork, ICompanyExecutionContext company)
    {
        _unitOfWork = unitOfWork;
        _company = company;
    }

    public async Task<decimal> CalculateAmount(MarkupTrans charge, CancellationToken ct)
    {
        if (charge.MarkupCategory == MarkupCategory.Fixed)
            return charge.Value;

        var dataAreaId = _company.GetDataAreaId() ?? "dat";
        if (charge.TransTableId == FinanceTableNumbers.SalesLine)
        {
            var line = await _db.Set<SalesLine>().AsNoTracking()
                .Where(x => x.RecId == charge.TransRecId && x.DataAreaId == dataAreaId)
                .Select(x => new { x.SalesQty, x.LineAmount, x.LineDisc, x.MultiLnDisc })
                .SingleAsync(ct);
            var netAmount = Math.Max(0m, line.LineAmount - line.LineDisc - line.MultiLnDisc);
            return CalculateForBase(charge, line.SalesQty, netAmount);
        }

        if (charge.TransTableId == FinanceTableNumbers.SalesTable || charge.TransTableId == 0)
        {
            var order = await _db.Set<SalesTable>().AsNoTracking()
                .Where(x => x.RecId == charge.TransRecId && x.DataAreaId == dataAreaId)
                .Select(x => x.SalesId)
                .SingleAsync(ct);
            var lines = await _db.Set<SalesLine>().AsNoTracking()
                .Where(x => x.SalesId == order && x.DataAreaId == dataAreaId)
                .Select(x => new { x.SalesQty, x.LineAmount, x.LineDisc, x.MultiLnDisc })
                .ToListAsync(ct);
            var netAmount = lines.Sum(x => Math.Max(0m, x.LineAmount - x.LineDisc - x.MultiLnDisc));
            return CalculateForBase(charge, lines.Sum(x => x.SalesQty), netAmount);
        }

        return charge.Value;
    }

    internal static decimal CalculateForBase(MarkupTrans charge, decimal quantity, decimal netAmount) =>
        charge.MarkupCategory switch
        {
            MarkupCategory.Fixed => charge.Value,
            MarkupCategory.Pcs => decimal.Round(charge.Value * quantity, 2, MidpointRounding.AwayFromZero),
            _ => decimal.Round(netAmount * charge.Value / 100m, 2, MidpointRounding.AwayFromZero)
        };

}
