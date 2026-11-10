namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Validation;

/// <summary>
/// Reserved quantity is what the scenario expects, and it never exceeds what is physically on hand. A reservation
/// above on hand would let two documents promise the same stock.
/// </summary>
public sealed class ReservationValidator : IInventoryCycleCheck
{
    public void Verify(InventoryReconciliationSnapshot posted)
    {
        InventoryReconciliationSnapshot.Check(posted.ReservedQuantity == posted.ExpectedReservedQuantity,
            $"{posted.ScenarioId}: reserved quantity differs ({posted.ReservedQuantity} instead of {posted.ExpectedReservedQuantity}).");
        InventoryReconciliationSnapshot.Check(posted.Summaries.All(x => x.ReservPhysical <= x.PhysicalInvent),
            $"{posted.ScenarioId}: more stock is reserved than is on hand.");
    }
}
