using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public enum SalesOrderCancelStatus { NotFound, NotOpen, Cancelled, NothingToCancel, Refused }

/// <summary>
/// The outcome of a cancellation. <see cref="Message"/> carries the named reason for a refusal, so
/// the caller can report why rather than restating a generic restriction.
/// </summary>
public sealed record SalesOrderCancelResult(SalesOrderCancelStatus Status, string? Message = null, object? Data = null);

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

    /// <summary>
    /// Cancels the quantity a sales order has not delivered, which is what D365's sales order
    /// cancellation does: only the undelivered part goes away, and anything already delivered stays
    /// processable.
    /// <para>
    /// A quantity delivered but not yet invoiced therefore stays invoiceable. Example: order 10,
    /// deliver 6, invoice none, then cancel. The physical remainder was 4 and the financial
    /// remainder 10; afterwards they are 0 and 6, so the 6 delivered units can still be invoiced and
    /// nothing already posted is disturbed.
    /// </para>
    /// <para>
    /// This used to refuse any order with a processed line, which left a partly delivered order with
    /// no ending at all: its undelivered remainder stayed open as live demand and reserved supply
    /// indefinitely.
    /// </para>
    /// </summary>
    public async Task<SalesOrderCancelResult> CancelAsync(long recId, CancellationToken cancellationToken)
    {
        return await _dbContext.Context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            var order = await _dbContext.Context.Set<SalesTable>().FirstOrDefaultAsync(
                row => row.RecId == recId && row.DataAreaId == _company.GetDataAreaId(), cancellationToken);
            if (order == null) return new SalesOrderCancelResult(SalesOrderCancelStatus.NotFound);
            if (order.SalesStatus == SalesStatus.Canceled)
                return new SalesOrderCancelResult(SalesOrderCancelStatus.NotOpen,
                    "The sales order is already canceled.");
            var lines = await _dbContext.Context.Set<SalesLine>()
                .Where(row => row.SalesId == order.SalesId && row.DataAreaId == order.DataAreaId
                    && row.SalesStatus != SalesStatus.Canceled)
                .ToListAsync(cancellationToken);
            if (lines.All(line => line.RemainSalesPhysical <= 0))
                return new SalesOrderCancelResult(SalesOrderCancelStatus.NothingToCancel,
                    "The sales order has no undelivered quantity left to cancel.");

            var cancelled = new List<object>();
            foreach (var line in lines)
            {
                if (line.RemainSalesPhysical <= 0) continue;
                var salesQty = line.RemainSalesPhysical;
                decimal inventQty;
                try
                {
                    inventQty = await _inventoryDemand.CancelRemainingAsync(line, cancellationToken);
                }
                catch (InvalidOperationException error)
                {
                    // Nothing is committed, so the whole cancellation is abandoned rather than
                    // leaving some lines released and others not.
                    await transaction.RollbackAsync(cancellationToken);
                    _dbContext.Context.ChangeTracker.Clear();
                    return new SalesOrderCancelResult(SalesOrderCancelStatus.Refused, error.Message);
                }
                cancelled.Add(new
                {
                    lineNumber = line.LineNum, itemId = line.ItemId, salesQty, inventQty,
                    status = line.SalesStatus, remainToInvoice = line.RemainSalesFinancial
                });
            }

            // Derived from the lines that still mean something, matching how the packing slip and
            // invoice flows derive it: a wholly cancelled order is Canceled, otherwise the order
            // follows whatever its surviving lines still owe.
            var surviving = lines.Where(line => line.SalesStatus != SalesStatus.Canceled).ToList();
            order.SalesStatus = surviving.Count == 0 ? SalesStatus.Canceled
                : surviving.All(line => line.RemainSalesFinancial <= 0 && line.RemainSalesPhysical <= 0)
                    ? SalesStatus.Invoiced
                    : SalesStatus.Delivered;

            await _dbContext.CompleteAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new SalesOrderCancelResult(SalesOrderCancelStatus.Cancelled,
                "Sales order remainder canceled.",
                new { salesId = order.SalesId, status = order.SalesStatus, lines = cancelled });
        });
    }
}
