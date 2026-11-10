namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Validation;

/// <summary>The scenario leaves exactly the on-hand quantity it set out to leave.</summary>
public sealed class OnHandQuantityValidator : IInventoryCycleCheck
{
    public void Verify(InventoryReconciliationSnapshot posted) => InventoryReconciliationSnapshot.Check(
        posted.PhysicalQuantity == posted.ExpectedPhysicalQuantity,
        $"{posted.ScenarioId}: on-hand quantity differs ({posted.PhysicalQuantity} instead of {posted.ExpectedPhysicalQuantity}).");
}
