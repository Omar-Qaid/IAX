using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Validation;

/// <summary>
/// Every cost settlement the close wrote names the movement it settled and agrees with the adjustment that movement
/// now carries. The cancellation service rebuilds a close from these rows rather than recomputing it, so a settlement
/// that drifted from its movement would reverse a wrong amount.
/// </summary>
public sealed class InventSettlementValidator : IInventoryCycleCheck
{
    public void Verify(InventoryReconciliationSnapshot posted)
    {
        var live = posted.Settlements.Where(x => x.Cancelled == NoYes.No).ToArray();
        InventoryReconciliationSnapshot.Check(live.All(x => x.TransRecId != 0 && x.InventTransId.Length > 0),
            $"{posted.ScenarioId}: a cost settlement does not reference the movement it settled.");
        foreach (var group in live.GroupBy(x => x.TransRecId))
        {
            var movement = posted.Inventory.SingleOrDefault(x => x.RecId == group.Key);
            InventoryReconciliationSnapshot.Check(movement != null,
                $"{posted.ScenarioId}: cost settlement references movement {group.Key}, which is not in this cycle.");
            InventoryReconciliationSnapshot.Check(group.Sum(x => x.CostAmountAdjustment) == movement!.CostAmountAdjustment,
                $"{posted.ScenarioId}: settlements for movement {group.Key} total {group.Sum(x => x.CostAmountAdjustment)} but it carries {movement.CostAmountAdjustment}.");
        }
    }
}
