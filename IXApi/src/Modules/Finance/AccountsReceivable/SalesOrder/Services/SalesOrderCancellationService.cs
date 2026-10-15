using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public enum SalesOrderCancelStatus { NotFound, NotOpen, Processed, Cancelled }

public sealed class SalesOrderCancellationService : ISalesOrderCancellationService
{
    private readonly IUnitOfWork _dbContext;
    private readonly ISalesInventoryDemandService _inventoryDemand;
    private readonly ICompanyExecutionContext _company;

    public SalesOrderCancellationService(IUnitOfWork dbContext, ISalesInventoryDemandService inventoryDemand,
        ICompanyExecutionContext company)
    {
        _dbContext = dbContext;
        _inventoryDemand = inventoryDemand;
        _company = company;
    }

    public async Task<SalesOrderCancelStatus> CancelAsync(long recId, CancellationToken cancellationToken)
    {
        return await _dbContext.Context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            var order = await _dbContext.Context.Set<SalesTable>().FirstOrDefaultAsync(
                row => row.RecId == recId && row.DataAreaId == _company.GetDataAreaId(), cancellationToken);
            if (order == null) return SalesOrderCancelStatus.NotFound;
            if (order.SalesStatus != SalesStatus.Backorder)
                return SalesOrderCancelStatus.NotOpen;
            var lines = await _dbContext.Context.Set<SalesLine>()
                .Where(row => row.SalesId == order.SalesId && row.DataAreaId == order.DataAreaId)
                .ToListAsync(cancellationToken);
            if (lines.Any(line => line.SalesStatus != SalesStatus.Backorder
                || line.RemainSalesPhysical != line.SalesQty
                || line.RemainSalesFinancial != line.SalesQty))
                return SalesOrderCancelStatus.Processed;
            foreach (var line in lines)
                await _inventoryDemand.CancelRemainingAsync(line, cancellationToken);
            order.SalesStatus = SalesStatus.Canceled;
            await _dbContext.CompleteAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return SalesOrderCancelStatus.Cancelled;
        });
    }
}
