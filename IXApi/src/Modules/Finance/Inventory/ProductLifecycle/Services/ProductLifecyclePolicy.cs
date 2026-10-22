using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle.Entities;
using Microsoft.EntityFrameworkCore;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;

namespace IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle;

public enum ProductLifecycleProcess { Sales, Purchase, Inventory }

public static class ProductLifecyclePolicy
{
    public static async Task<string?> ValidateAsync(DbContext db, string companyId, string itemId,
        ProductLifecycleProcess process, CancellationToken ct)
    {
        if (db.Model.FindEntityType(typeof(ReleasedItem)) is null) return null;
        var stateId = await db.Set<ReleasedItem>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId && x.ItemId == itemId)
            .Select(x => x.ProductLifecycleStateId).FirstOrDefaultAsync(ct);
        if (stateId is null) return null;
        if (stateId.Length == 0) return null;
        var state = await db.Set<EcoResProductLifecycleState>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.StateId == stateId, ct);
        if (state is null) return $"Product status '{stateId}' is not configured.";
        var blocked = process switch
        {
            ProductLifecycleProcess.Sales => state.BlockSales == NoYes.Yes,
            ProductLifecycleProcess.Purchase => state.BlockPurchase == NoYes.Yes,
            _ => state.BlockInventory == NoYes.Yes
        };
        return blocked ? $"Product status '{stateId}' blocks {process.ToString().ToLowerInvariant()} transactions." : null;
    }
}
