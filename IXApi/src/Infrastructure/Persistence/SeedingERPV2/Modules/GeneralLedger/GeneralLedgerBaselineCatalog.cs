using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;

/// <summary>The startup data of the General ledger module, read from <c>Data/baseline-generalledger.json</c>.</summary>
public sealed record GeneralLedgerBaselineCatalog(
    GeneralLedgerBaselineCatalog.LedgerRow Ledger,
    GeneralLedgerBaselineCatalog.StructureRow[] AccountStructures,
    GeneralLedgerBaselineCatalog.MainAccountRow[] MainAccounts)
{
    public static GeneralLedgerBaselineCatalog Load() => BaselineManifest.Read<GeneralLedgerBaselineCatalog>("baseline-generalledger.json");

    public sealed record LedgerRow(string CalendarId, string Description, string AccountingCurrency, string DefaultExchangeRateType);
    public sealed record SegmentRow(string Dimension, string Criteria);
    public sealed record StructureRow(string Name, string Description, SegmentRow[] Segments);
    public sealed record MainAccountRow(string MainAccountId, string Name, MainAccountType Type, string CurrencyCode,
        string ConsolidationMainAccount, string GroupLevel01, string GroupLevel02, string GroupLevel03);
}
