using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public enum SalesOrderCancelStatus { NotFound, NotOpen, Processed, Cancelled }

public sealed class SalesOrderCancellationService
{
    private readonly IFinanceDataContext _dbContext;
    private readonly ISalesInventoryDemandService _inventoryDemand;

    public SalesOrderCancellationService(IFinanceDataContext dbContext, ISalesInventoryDemandService inventoryDemand)
    {
        _dbContext = dbContext;
        _inventoryDemand = inventoryDemand;
    }

    public async Task<SalesOrderCancelStatus> CancelAsync(long recId, CancellationToken cancellationToken)
    {
        return await _dbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            var order = await _dbContext.Set<SalesTable>().FirstOrDefaultAsync(
                row => row.RecId == recId, cancellationToken);
            if (order == null) return SalesOrderCancelStatus.NotFound;
            if (order.SalesStatus != SalesStatus.Backorder)
                return SalesOrderCancelStatus.NotOpen;
            var lines = await _dbContext.Set<SalesLine>()
                .Where(row => row.SalesId == order.SalesId && row.DataAreaId == order.DataAreaId)
                .ToListAsync(cancellationToken);
            if (lines.Any(line => line.SalesStatus != SalesStatus.Backorder
                || line.RemainSalesPhysical != line.SalesQty
                || line.RemainSalesFinancial != line.SalesQty))
                return SalesOrderCancelStatus.Processed;
            foreach (var line in lines)
                await _inventoryDemand.CancelRemainingAsync(line, cancellationToken);
            order.SalesStatus = SalesStatus.Canceled;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return SalesOrderCancelStatus.Cancelled;
        });
    }
}
