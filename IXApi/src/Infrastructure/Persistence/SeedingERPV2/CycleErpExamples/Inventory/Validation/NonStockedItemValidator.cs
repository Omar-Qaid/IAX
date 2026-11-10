namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Validation;

/// <summary>
/// A non-stocked service leaves no stock movement behind. It is checked explicitly because the service line is
/// ordered, delivered and invoiced alongside the stocked line, and a movement raised for it would be absorbed into
/// the item's cost and quietly change the closing value.
/// </summary>
public sealed class NonStockedItemValidator : IInventoryCycleCheck
{
    public void Verify(InventoryReconciliationSnapshot posted) => InventoryReconciliationSnapshot.Check(
        posted.ServiceInventory.Count == 0,
        $"{posted.ScenarioId}: non-stocked item {posted.ServiceItemId} raised {posted.ServiceInventory.Count} inventory movement(s).");
}
