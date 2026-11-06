using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Validation;

/// <summary>Financially updated inventory value ties to the inventory GL account.</summary>
public sealed class InventoryValueValidator : IAccountsReceivableCycleCheck
{
    public void Verify(AccountsReceivableReconciliationSnapshot posted) => AccountsReceivableReconciliationSnapshot.Check(
        posted.LedgerAmount(GeneralLedgerSeedExample.Inventory)
            == posted.Inventory.Sum(x => x.CostAmountPosted + x.CostAmountAdjustment),
        $"{posted.ScenarioId}: financial inventory does not match GL.");
}
