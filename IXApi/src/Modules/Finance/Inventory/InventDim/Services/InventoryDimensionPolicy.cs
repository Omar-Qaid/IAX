using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventoryStatus.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

/// <summary>
/// The dimension rules every stock movement must satisfy, in one place, so the inventory journal
/// and the transfer order cannot disagree about what a valid dimension is. Each check returns the
/// reason the movement is not allowed, or null when it is; each caller wraps that reason in the
/// exception type its own posting flow uses.
/// </summary>
internal static class InventoryDimensionPolicy
{
    /// <summary>The item's tracking dimension group, or null when no group is assigned.</summary>
    public static async Task<long?> TrackingGroupAsync(DbContext db, string companyId, string itemId, CancellationToken ct)
    {
        var assignment = await db.Set<EcoResTrackingDimensionGroupItem>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.ItemDataAreaId == companyId && x.ItemId == itemId, ct);
        return assignment?.TrackingDimensionGroup;
    }

    /// <summary>
    /// Whether the group requires a batch or serial number on physical inventory. A flow that
    /// cannot carry those dimensions uses this to refuse the item instead of posting a blank one.
    /// </summary>
    public static async Task<bool> RequiresBatchOrSerialAsync(DbContext db, long trackingDimensionGroup, CancellationToken ct) =>
        await db.Set<EcoResTrackingDimensionGroupFldSetup>().AsNoTracking()
            .AnyAsync(x => x.TrackingDimensionGroup == trackingDimensionGroup &&
                (x.DimensionFieldId == InventDimensionFields.BatchNumber || x.DimensionFieldId == InventDimensionFields.SerialNumber) &&
                x.IsPhysicalInventoryEnabled, ct);

    /// <summary>
    /// The batch and serial requirements of one movement: a required dimension must be present
    /// unless the group allows it blank on this side, a stated batch or serial must exist for the
    /// item, and a serial-tracked line must identify exactly one unit.
    /// </summary>
    public static async Task<string?> CheckTrackingAsync(DbContext db, string itemId, long trackingDimensionGroup,
        InventDim dim, decimal signedQty, CancellationToken ct)
    {
        var setup = await db.Set<EcoResTrackingDimensionGroupFldSetup>().AsNoTracking()
            .Where(x => x.TrackingDimensionGroup == trackingDimensionGroup &&
                (x.DimensionFieldId == InventDimensionFields.BatchNumber || x.DimensionFieldId == InventDimensionFields.SerialNumber))
            .ToListAsync(ct);
        var batch = setup.FirstOrDefault(x => x.DimensionFieldId == InventDimensionFields.BatchNumber);
        var serial = setup.FirstOrDefault(x => x.DimensionFieldId == InventDimensionFields.SerialNumber);
        var batchId = dim.InventBatchId.Trim();
        var serialId = dim.InventSerialId.Trim();
        var isIssue = signedQty < 0;
        if (batch?.IsPhysicalInventoryEnabled == true && batchId.Length == 0 &&
            !(isIssue ? batch.IsAllowBlankIssueEnabled : batch.IsAllowBlankReceiptEnabled))
            return $"A batch number is required for item '{itemId}'.";
        if (serial?.IsPhysicalInventoryEnabled == true && serialId.Length == 0 &&
            !(isIssue ? serial.IsAllowBlankIssueEnabled : serial.IsAllowBlankReceiptEnabled))
            return $"A serial number is required for item '{itemId}'.";
        if (batchId.Length > 0 && !await db.Set<InventBatch>().AnyAsync(x => x.ItemId == itemId && x.InventBatchId == batchId, ct))
            return $"Batch '{batchId}' is not defined for item '{itemId}'.";
        if (serialId.Length > 0 && !await db.Set<InventSerial>().AnyAsync(x => x.ItemId == itemId && x.InventSerialId == serialId, ct))
            return $"Serial number '{serialId}' is not defined for item '{itemId}'.";
        if (serial?.IsPhysicalInventoryEnabled == true && signedQty != 0 &&
            (serialId.Length == 0 || Math.Abs(signedQty) != 1m))
            return $"Each line for serial-tracked item '{itemId}' must identify one serial-numbered unit.";
        return null;
    }

    /// <summary>
    /// The inventory status of one movement: the status must still exist, and stock held under a
    /// blocking status cannot be issued.
    /// </summary>
    public static async Task<string?> CheckStatusAsync(DbContext db, string companyId, InventDim dim,
        decimal signedQty, CancellationToken ct)
    {
        var statusId = dim.InventStatusId.Trim();
        if (statusId.Length == 0) return null;
        var status = await db.Set<WHSInventStatus>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.InventStatusId == statusId, ct);
        if (status is null) return $"Inventory status '{statusId}' no longer exists.";
        return status.InventStatusBlocking == NoYes.Yes && signedQty < 0
            ? $"Inventory status '{statusId}' is blocked for issue."
            : null;
    }
}
