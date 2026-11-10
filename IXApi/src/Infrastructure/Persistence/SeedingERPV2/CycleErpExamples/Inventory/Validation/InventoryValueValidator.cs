using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Validation;

/// <summary>Financially updated inventory ties to the inventory GL account, and to the figure the cycle expects.</summary>
public sealed class InventoryValueValidator : IInventoryCycleCheck
{
    public void Verify(InventoryReconciliationSnapshot posted)
    {
        InventoryReconciliationSnapshot.Check(posted.InventoryValue == posted.ExpectedInventoryValue,
            $"{posted.ScenarioId}: inventory value differs ({posted.InventoryValue} instead of {posted.ExpectedInventoryValue}).");
        InventoryReconciliationSnapshot.Check(
            posted.LedgerAmount(GeneralLedgerSeedExample.Inventory) == posted.InventoryValue,
            $"{posted.ScenarioId}: financial inventory does not match GL.");
    }
}
