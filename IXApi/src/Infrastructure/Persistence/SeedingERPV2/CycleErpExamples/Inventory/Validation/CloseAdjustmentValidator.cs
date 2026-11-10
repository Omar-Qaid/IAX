namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Validation;

/// <summary>
/// The close moved exactly the adjustment the cycle expects onto the already-invoiced issues. This is the crux of
/// periodic weighted average: a receipt that arrives after an issue still changes the period average, so an issue
/// invoiced at a running cost must be adjusted at close. An adjustment of zero would mean the close did nothing.
/// </summary>
public sealed class CloseAdjustmentValidator : IInventoryCycleCheck
{
    public void Verify(InventoryReconciliationSnapshot posted)
    {
        InventoryReconciliationSnapshot.Check(posted.CloseAdjustment == posted.ExpectedCloseAdjustment,
            $"{posted.ScenarioId}: close adjustment differs ({posted.CloseAdjustment} instead of {posted.ExpectedCloseAdjustment}).");
        InventoryReconciliationSnapshot.Check(
            posted.ExpectedCloseAdjustment == 0m || posted.Closings.Count > 0,
            $"{posted.ScenarioId}: an adjustment is expected but no inventory close was recorded.");
    }
}
