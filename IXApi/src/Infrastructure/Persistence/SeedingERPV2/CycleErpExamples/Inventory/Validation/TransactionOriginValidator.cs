namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Inventory.Validation;

/// <summary>
/// Every stock movement is traceable to the document that caused it. An orphaned movement cannot be reversed,
/// marked or settled by the services that reach stock through its origin.
/// </summary>
public sealed class TransactionOriginValidator : IInventoryCycleCheck
{
    public void Verify(InventoryReconciliationSnapshot posted)
    {
        var origins = posted.Origins.Select(x => x.RecId).ToHashSet();
        var orphans = posted.Inventory.Where(x => !origins.Contains(x.InventTransOrigin)).ToArray();
        InventoryReconciliationSnapshot.Check(orphans.Length == 0,
            $"{posted.ScenarioId}: {orphans.Length} inventory movement(s) have no transaction origin.");
        InventoryReconciliationSnapshot.Check(posted.Origins.All(x => x.ReferenceId.Length > 0),
            $"{posted.ScenarioId}: a transaction origin does not name its source document.");
    }
}
