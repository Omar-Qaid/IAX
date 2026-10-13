using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Markup;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("IXApi.Tests")]

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

internal static class SalesOrderChargeRefresh
{

    public static async Task RefreshAsync(DbContext db, SalesTable order, CancellationToken cancellationToken)
    {
        var lines = await db.Set<SalesLine>().AsNoTracking()
            .Where(line => line.DataAreaId == order.DataAreaId && line.SalesId == order.SalesId)
            .Select(line => new { line.RecId, line.SalesQty, line.LineAmount, line.LineDisc, line.MultiLnDisc })
            .ToListAsync(cancellationToken);
        var lineIds = lines.Select(line => line.RecId).ToList();
        var charges = await db.Set<MarkupTrans>()
            .Where(charge => charge.DataAreaId == order.DataAreaId
                && charge.IsDeleted != NoYes.Yes
                && (charge.ModuleType == MarkupModuleType.Customer || charge.ModuleType == MarkupModuleType.Sales)
                && ((charge.TransRecId == order.RecId
                        && (charge.TransTableId == FinanceTableNumbers.SalesTable || charge.TransTableId == FinanceTableNumbers.Unspecified))
                    || (lineIds.Contains(charge.TransRecId) && charge.TransTableId == FinanceTableNumbers.SalesLine)))
            .ToListAsync(cancellationToken);
        var lineAmounts = lines.ToDictionary(line => line.RecId,
            line => (line.SalesQty, Net: Math.Max(0m, line.LineAmount - line.LineDisc - line.MultiLnDisc)));
        var orderQuantity = lines.Sum(line => line.SalesQty);
        var orderNet = lineAmounts.Values.Sum(line => line.Net);
        foreach (var charge in charges)
        {
            var basis = charge.TransTableId == FinanceTableNumbers.SalesLine
                ? lineAmounts[charge.TransRecId]
                : (orderQuantity, orderNet);
            charge.CalculatedAmount = MarkupChargeCalculator.CalculateForBase(
                charge, basis.Item1, basis.Item2);
        }
    }
}
