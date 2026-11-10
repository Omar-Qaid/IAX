namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Validation;

/// <summary>
/// The on-hand summary agrees with the movements behind it. InventSum is a maintained running total rather than a
/// view, so a posting service that updated the movement but not the summary would leave the two silently apart and
/// every later availability check reading the wrong number.
/// </summary>
public sealed class InventSumConsistencyValidator : IInventoryCycleCheck
{
    public void Verify(InventoryReconciliationSnapshot posted)
    {
        InventoryReconciliationSnapshot.Check(
            posted.Summaries.Sum(x => x.PhysicalInvent) == posted.PhysicalQuantity,
            $"{posted.ScenarioId}: on-hand summary holds {posted.Summaries.Sum(x => x.PhysicalInvent)} but its movements total {posted.PhysicalQuantity}.");
        InventoryReconciliationSnapshot.Check(
            posted.Summaries.All(x => x.AvailPhysical == x.PhysicalInvent - x.ReservPhysical),
            $"{posted.ScenarioId}: available physical is not on hand less reserved.");
        InventoryReconciliationSnapshot.Check(
            posted.Summaries.Sum(x => x.PostedValue) == posted.InventoryValue,
            $"{posted.ScenarioId}: posted value on the summary differs from its movements.");
    }
}
