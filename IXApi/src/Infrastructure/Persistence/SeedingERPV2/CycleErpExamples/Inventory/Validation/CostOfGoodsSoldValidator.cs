using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Validation;

/// <summary>
/// The issue side of the movements ties to the cost-of-goods-sold account. The ledger holds the expense as a debit,
/// so it is compared against the positive cost figure rather than the signed movement sum.
/// </summary>
public sealed class CostOfGoodsSoldValidator : IInventoryCycleCheck
{
    public void Verify(InventoryReconciliationSnapshot posted)
    {
        InventoryReconciliationSnapshot.Check(posted.CostOfGoodsSold == posted.ExpectedCostOfGoodsSold,
            $"{posted.ScenarioId}: cost of goods sold differs ({posted.CostOfGoodsSold} instead of {posted.ExpectedCostOfGoodsSold}).");
        InventoryReconciliationSnapshot.Check(
            posted.LedgerAmount(GeneralLedgerSeedExample.CostOfGoodsSold) == posted.CostOfGoodsSold,
            $"{posted.ScenarioId}: cost of goods sold does not match GL.");
    }
}
