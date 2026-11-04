using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;

/// <summary>The demo chart of accounts. Seeders insert these codes and the cycle checks reconcile against them.</summary>
public static class GeneralLedgerSeedExample
{
    public const string Bank = "181001";
    public const string Inventory = "181002";
    public const string ReceivedNotInvoicedInterim = "181003";
    public const string RecoverablePurchaseTax = "181004";
    public const string VendorAdvance = "181005";
    public const string VendorControl = "281001";
    public const string ReceivedNotInvoiced = "281002";
    public const string PurchaseCashDiscount = "481001";
    public const string RealizedExchangeGain = "481002";
    public const string MaintenanceExpense = "681001";
    public const string RealizedExchangeLoss = "681002";
    public const string DeliveryExpense = "681003";

    public static IReadOnlyList<(string Code, string Name, MainAccountType Type)> MainAccounts { get; } =
    [
        (Bank, "Treasury bank (demo)", MainAccountType.Asset),
        (Inventory, "Office paper inventory (demo)", MainAccountType.Asset),
        (ReceivedNotInvoicedInterim, "Received office supplies - interim (demo)", MainAccountType.Asset),
        (RecoverablePurchaseTax, "Recoverable purchase tax (demo)", MainAccountType.Asset),
        (VendorAdvance, "Supplier advances (demo)", MainAccountType.Asset),
        (VendorControl, "Trade accounts payable (demo)", MainAccountType.Liability),
        (ReceivedNotInvoiced, "Office supplies received not invoiced (demo)", MainAccountType.Liability),
        (PurchaseCashDiscount, "Purchase cash discounts (demo)", MainAccountType.Revenue),
        (RealizedExchangeGain, "Realized exchange gains (demo)", MainAccountType.Revenue),
        (MaintenanceExpense, "Printer maintenance expense (demo)", MainAccountType.Expense),
        (RealizedExchangeLoss, "Realized exchange losses (demo)", MainAccountType.Expense),
        (DeliveryExpense, "Supplier delivery expense (demo)", MainAccountType.Expense)
    ];
}
