using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;

/// <summary>
/// The facts of the Inventory cycle fixtures, owned here beside the seeder that writes them. The example package
/// describes these records in its manifests and checks the description against these constants, so a fixture can
/// only change in one place.
/// </summary>
public static class InventoryCycleSeedExample
{
    public const string StockSuffix = "STOCK";
    public const string ServiceSuffix = "SVC";
    public const string AdjustmentSuffix = "ADJ";
    public const string CountSuffix = "COUNT";
    public const string CountOverageSuffix = "CPLUS";
    public const string CountMatchSuffix = "CMATCH";
    public const string TransferSuffix = "TRANS";
    public const string TransferJournalSuffix = "TJOUR";
    public const string MovementSuffix = "MOVE";
    public static IReadOnlyList<string> ItemSuffixes { get; } = [StockSuffix, ServiceSuffix, AdjustmentSuffix, CountSuffix, CountOverageSuffix, CountMatchSuffix, TransferSuffix, TransferJournalSuffix, MovementSuffix];
    public const string ItemGroup = "APV2";
    public const string StockModelGroup = "APV2";
    public const string ServiceModelGroup = "APV2-SVC";
    public const string ItemTaxGroup = "IV2-ZERO";
    public const string Unit = "EA";
    public const decimal PriceUnit = 1m;

    /// <summary>The reference cycle buys at 10 and sells at 25; the service is billed at 300.</summary>
    public const decimal StockPurchasePrice = 10m;
    public const decimal StockSalesPrice = 25m;
    public const decimal ServicePrice = 300m;

    public static string Name(string suffix) => suffix switch
    {
        StockSuffix => "Inventory cycle stocked item",
        ServiceSuffix => "Inventory cycle service",
        AdjustmentSuffix => "Inventory adjustment cycle item",
        CountSuffix => "Inventory counting cycle item",
        CountOverageSuffix => "Inventory counting overage item",
        CountMatchSuffix => "Inventory matching count item",
        TransferSuffix => "Inventory transfer cycle item",
        TransferJournalSuffix => "Inventory transfer journal item",
        MovementSuffix => "Inventory movement journal item",
        _ => throw new ArgumentOutOfRangeException(nameof(suffix))
    };

    public static ItemType ProductType(string suffix) =>
        suffix == ServiceSuffix ? ItemType.Service : ItemType.Item;

    public static string ModelGroup(string suffix) =>
        suffix == ServiceSuffix ? ServiceModelGroup : StockModelGroup;

    /// <summary>The module price the seeder writes. Only the stocked item is priced differently to sell.</summary>
    public static decimal Price(string suffix, ModuleInventPurchSales module) => suffix == ServiceSuffix
        ? ServicePrice
        : module == ModuleInventPurchSales.Sales ? StockSalesPrice : StockPurchasePrice;

    /// <summary>The inventory posting profiles the item group carries, as <see cref="InventPostingSeeder"/> writes them.</summary>
    public static IReadOnlyList<(InventAccountType AccountType, string MainAccount)> PostingProfiles { get; } =
    [
        // The purchase side, as the reference cycle posts it.
        (InventAccountType.PurchPackingSlip, GeneralLedgerSeedExample.ReceivedNotInvoicedInterim),
        (InventAccountType.PurchPackingSlipOffsetAccount, GeneralLedgerSeedExample.ReceivedNotInvoiced),
        (InventAccountType.PurchReceipt, GeneralLedgerSeedExample.Inventory),
        (InventAccountType.PurchConsump, GeneralLedgerSeedExample.MaintenanceExpense),
        (InventAccountType.PurchDisc, GeneralLedgerSeedExample.PurchaseCashDiscount),

        // The journal side. InventJournalPostingService resolves the inventory half of an adjustment
        // from InventReceipt/InventIssue and its offset from InventProfit/InventLoss, and
        // InventParametersService.GetReadiness requires all four before it reports a company ready.
        (InventAccountType.InventReceipt, GeneralLedgerSeedExample.Inventory),
        (InventAccountType.InventIssue, GeneralLedgerSeedExample.Inventory),
        (InventAccountType.InventProfit, GeneralLedgerSeedExample.InventoryProfit),
        (InventAccountType.InventLoss, GeneralLedgerSeedExample.InventoryLoss)
    ];
}
