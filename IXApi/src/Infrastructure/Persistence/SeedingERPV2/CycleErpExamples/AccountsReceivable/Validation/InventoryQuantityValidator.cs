namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Validation;

/// <summary>Physically updated quantity on the scenario sales orders, net of anything taken back.</summary>
public sealed class InventoryQuantityValidator : IAccountsReceivableCycleCheck
{
    public void Verify(AccountsReceivableReconciliationSnapshot posted) => AccountsReceivableReconciliationSnapshot.Check(
        posted.PhysicalQuantity == posted.ExpectedPhysicalQuantity,
        $"{posted.ScenarioId}: physical quantity differs ({posted.PhysicalQuantity}).");
}
