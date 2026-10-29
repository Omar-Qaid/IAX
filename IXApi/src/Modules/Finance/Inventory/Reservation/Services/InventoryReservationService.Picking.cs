using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle;
using IAX.IXApi.Modules.Finance.Inventory.InventoryStatus.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed partial class InventoryReservationService
{
    /// <summary>Basic non-WHS picking in inventory units. Lowering the target returns picked stock to reservation.</summary>
    public Task<(PickingResultDto? Result, string? Error)> SetPickedAsync(string inventTransId, decimal targetQuantity, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => SetPickedCoreAsync(inventTransId, targetQuantity, ct));

    private async Task<(PickingResultDto? Result, string? Error)> SetPickedCoreAsync(string inventTransId, decimal target, CancellationToken ct)
    {
        var area = _company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area) || string.IsNullOrWhiteSpace(inventTransId) || target < 0 || decimal.Round(target, 6) != target)
            return (null, "Select a company and sales inventory source and a nonnegative picked quantity with at most six decimals.");
        await _unitOfWork.BeginTransactionAsync(ct);
        async Task<(PickingResultDto?, string?)> Reject(string message)
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            return (null, message);
        }
        try
        {
            await LockPostingKeyAsync($"InventReservation:{area}:{inventTransId}", ct);
            var origin = await Db.Set<InventTransOrigin>().SingleOrDefaultAsync(x => x.DataAreaId == area
                && x.InventTransId == inventTransId && x.ReferenceCategory == InventRefType.SalesTable, ct);
            if (origin == null) return await Reject("The sales inventory source was not found in this company.");
            var line = await Db.Set<SalesLine>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.InventTransId == inventTransId
                && x.SalesId == origin.ReferenceId, ct);
            if (line == null || line.SalesType == SalesType.ReturnItem || line.Blocked == SalesLineBlocked.Yes
                || line.SalesStatus != SalesStatus.Backorder || line.ItemId != origin.ItemId)
                return await Reject("Only an open, unblocked sales issue can be picked.");
            await LockPostingKeyAsync($"InventStock:{area}:{line.ItemId}:{line.InventDimId}", ct);
            var rows = await Db.Set<InventTrans>().Where(x => x.DataAreaId == area && x.InventTransOrigin == origin.RecId
                && (x.StatusIssue == StatusIssue.ReservPhysical || x.StatusIssue == StatusIssue.Picked)).OrderBy(x => x.RecId).ToListAsync(ct);
            if (rows.Any(x => x.ItemId != line.ItemId || x.InventDimId != line.InventDimId || x.Qty > 0 || x.PdscwQty != 0))
                return await Reject("Basic picking requires matching dimensions and ordinary inventory units.");
            var picked = -rows.Where(x => x.StatusIssue == StatusIssue.Picked).Sum(x => x.Qty);
            var reserved = -rows.Where(x => x.StatusIssue == StatusIssue.ReservPhysical).Sum(x => x.Qty);
            if (target > picked + reserved || target > line.RemainInventPhysical)
                return await Reject("Picked quantity cannot exceed this line's reserved, undelivered quantity.");
            var sum = await Db.Set<InventSum>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.ItemId == line.ItemId
                && x.InventDimId == line.InventDimId, ct);
            if (sum == null || sum.Picked < picked || sum.ReservPhysical < reserved)
                return await Reject("Inventory reservation and picked balances do not reconcile.");
            var delta = target - picked;
            if (delta > 0)
            {
                var lifecycleError = await ProductLifecyclePolicy.ValidateAsync(Db, area, line.ItemId, ProductLifecycleProcess.Sales, ct);
                if (lifecycleError != null) return await Reject(lifecycleError);
                var dimension = await Db.Set<InventDim>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.InventDimId == line.InventDimId, ct);
                if (dimension == null) return await Reject("The inventory dimension was not found.");
                if (!string.IsNullOrWhiteSpace(dimension.InventStatusId))
                {
                    var status = await Db.Set<WHSInventStatus>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.InventStatusId == dimension.InventStatusId, ct);
                    if (status == null || status.InventStatusBlocking == NoYes.Yes)
                        return await Reject("The inventory status does not allow picking.");
                }
            }
            var from = delta > 0 ? StatusIssue.ReservPhysical : StatusIssue.Picked;
            var to = delta > 0 ? StatusIssue.Picked : StatusIssue.ReservPhysical;
            var left = Math.Abs(delta);
            foreach (var row in rows.Where(x => x.StatusIssue == from).ToList())
            {
                if (left == 0) break;
                var take = Math.Min(left, -row.Qty);
                if (take == -row.Qty) row.StatusIssue = to;
                else
                {
                    var split = (InventTrans)Db.Entry(row).CurrentValues.ToObject();
                    split.RecId = 0;
                    split.Qty = -take;
                    split.StatusIssue = to;
                    split.DateStatus = DateTime.UtcNow;
                    Db.Add(split);
                    row.Qty += take;
                }
                row.DateStatus = DateTime.UtcNow;
                left -= take;
            }
            sum.ReservPhysical -= delta;
            sum.Picked += delta;
            sum.AvailPhysical = sum.PhysicalInvent - sum.ReservPhysical - sum.Picked;
            sum.AvailOrdered = sum.AvailPhysical + sum.Ordered - sum.OnOrder - sum.ReservOrdered;
            await _unitOfWork.CompleteAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return (new PickingResultDto(inventTransId, target, reserved - delta), null);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }
}
