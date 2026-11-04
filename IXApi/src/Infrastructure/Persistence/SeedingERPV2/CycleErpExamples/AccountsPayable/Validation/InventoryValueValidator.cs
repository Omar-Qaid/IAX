using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Validation;

/// <summary>Financially updated inventory value ties to the inventory GL account.</summary>
public sealed class InventoryValueValidator : IAccountsPayableCycleCheck
{
    public void Verify(AccountsPayableReconciliationSnapshot posted) => AccountsPayableReconciliationSnapshot.Check(
        posted.LedgerAmount(GeneralLedgerSeedExample.Inventory)
            == posted.Inventory.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment),
        $"{posted.ScenarioId}: financial inventory does not match GL.");
}
