namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Validation;

/// <summary>Physically updated quantity on the scenario's purchase orders, net of anything returned.</summary>
public sealed class InventoryQuantityValidator : IAccountsPayableCycleCheck
{
    public void Verify(AccountsPayableReconciliationSnapshot posted) => AccountsPayableReconciliationSnapshot.Check(
        posted.PhysicalQuantity == posted.ExpectedPhysicalQuantity,
        $"{posted.ScenarioId}: physical quantity differs ({posted.PhysicalQuantity}).");
}
