using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger.Data;

public static class GeneralLedgerDemoData
{
    public const decimal ReclassificationAmount = 125m;
    public const string DebitAccount = GeneralLedgerSeedExample.MaintenanceExpense;
    public const string CreditAccount = GeneralLedgerSeedExample.DeliveryExpense;
}
