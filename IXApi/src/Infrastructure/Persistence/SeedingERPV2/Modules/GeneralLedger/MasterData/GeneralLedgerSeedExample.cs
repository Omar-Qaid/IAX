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

    // Accounts receivable side of the same demo chart. Kept in disjoint code ranges from the
    // payable accounts so the sales account structure never overlaps the purchase one.
    public const string CustomerControl = "181006";
    public const string DeliveredNotInvoicedInterim = "181007";
    public const string CustomerAdvance = "281003";
    public const string PayableSalesTax = "281004";
    public const string DeliveredNotInvoiced = "281005";
    public const string SalesRevenue = "481003";
    public const string CostOfGoodsSold = "681004";
    public const string SalesCashDiscount = "681005";

    // Inventory journal side of the demo chart. Adjustment and counting journals post their
    // difference to these two accounts, which no purchase or sales document ever reaches.
    public const string InventoryProfit = "481004";
    public const string InventoryLoss = "681006";

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
        (DeliveryExpense, "Supplier delivery expense (demo)", MainAccountType.Expense),
        (CustomerControl, "Trade accounts receivable (demo)", MainAccountType.Asset),
        (DeliveredNotInvoicedInterim, "Cost of office paper delivered - interim (demo)", MainAccountType.Asset),
        (CustomerAdvance, "Customer prepayments (demo)", MainAccountType.Liability),
        (PayableSalesTax, "Payable sales tax (demo)", MainAccountType.Liability),
        (DeliveredNotInvoiced, "Office paper delivered not invoiced (demo)", MainAccountType.Liability),
        (SalesRevenue, "Office paper sales revenue (demo)", MainAccountType.Revenue),
        (CostOfGoodsSold, "Cost of office paper sold (demo)", MainAccountType.Expense),
        (SalesCashDiscount, "Sales cash discounts (demo)", MainAccountType.Expense),
        (InventoryProfit, "Inventory profit (demo)", MainAccountType.Revenue),
        (InventoryLoss, "Inventory loss (demo)", MainAccountType.Expense)
    ];

    /// <summary>The receivable codes, in the order the sales account structure allows them.</summary>
    public static IReadOnlyList<string> AccountsReceivableAccounts { get; } =
        [CustomerControl, DeliveredNotInvoicedInterim, CustomerAdvance, PayableSalesTax,
         DeliveredNotInvoiced, SalesRevenue, CostOfGoodsSold, SalesCashDiscount];

    /// <summary>The inventory journal codes, in the order the inventory account structure allows them.</summary>
    public static IReadOnlyList<string> InventoryJournalAccounts { get; } = [InventoryProfit, InventoryLoss];
}
