namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples;

/// <summary>Stable document keys for one connected, opt-in trading example in DAT.</summary>
internal static class ErpCycleExampleData
{
    internal const string Company = "dat";
    internal const string Purchase = "ERP-CYCLE-PO-001";
    internal const string Sales = "ERP-CYCLE-SO-001";
    internal const string Payment = "ERP-CYCLE-PAY-001";
    internal const string VendorPayment = "ERP-CYCLE-VPAY-001";
    internal const string VendorPaymentMode = "ERP-BANK";
    internal const string Journal = "ERP-CYCLE-GL-001";
    internal const string InventoryJournal = "ERP-CYCLE-INV-001";
    internal const string Vendor = "VEND-100";
    internal const string Customer = "CUST-100";
    internal const string Item = "ITEM-001";
    internal const string Site = "JED";
    internal const string Warehouse = "JED-1";
    internal const string CurrencyCode = "SAR";
    internal const string Unit = "PCS";
    internal const decimal PurchaseQuantity = 12m;
    internal const decimal SalesQuantity = 4m;
    internal const decimal PurchasePrice = 100m;
    internal const decimal SalesPrice = 150m;
}
