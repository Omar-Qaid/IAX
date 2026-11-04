namespace IAX.IXApi.Modules.Finance.Common;

/// <summary>Table numbers persisted in polymorphic Finance references such as MarkupTrans.TransTableId.</summary>
public static class FinanceTableNumbers
{
    /// <summary>Legacy or unspecified polymorphic reference. This is not a physical table number.</summary>
    public const int Unspecified = 0;

    // Keep these application reference numbers aligned with IXApp's documentTableIds.
    public const int CustTable = 2001;
    public const int SalesTable = 2002;
    public const int SalesLine = 2003;
    public const int VendTable = 3001;
    public const int VendInvoiceTrans = 1326239103;
    public const int HcmWorker = 4001;
    // Existing IXApp recordTableId fallback values (FNV-1a over normalized table names).
    public const int CustTransOpen = 472585700;
    public const int LedgerJournalTrans = 1864676057;

    public const int PurchTable = 3002;

    public const int PurchLine = 821905317;

    public const int VendInvoiceInfoTable = 1180248369;

    public const int VendInvoiceInfoLine = 852736745;

    public const int VendInvoiceJour = 737742047;

    public const int VendPackingSlipJour = 488159705;

    public const int VendPackingSlipTrans = 1103935757;

    public static int For<TEntity>() => typeof(TEntity) switch
    {
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.AccountsReceivable.CustTable) => CustTable,
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.AccountsReceivable.SalesTable) => SalesTable,
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.AccountsReceivable.SalesLine) => SalesLine,
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.AccountsReceivable.CustTransOpen) => CustTransOpen,
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans) => LedgerJournalTrans,
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.Entities.VendTable) => VendTable,
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.AccountsPayable.VendInvoiceTrans) => VendInvoiceTrans,
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.Foundation.HcmWorkers.HcmWorker) => HcmWorker,
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.AccountsPayable.PurchTable) => PurchTable,
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.AccountsPayable.PurchLine) => PurchLine,
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.AccountsPayable.VendInvoiceInfoTable) => VendInvoiceInfoTable,
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.AccountsPayable.VendInvoiceInfoLine) => VendInvoiceInfoLine,
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.AccountsPayable.VendInvoiceJour) => VendInvoiceJour,
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.AccountsPayable.VendPackingSlipJour) => VendPackingSlipJour,
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.AccountsPayable.VendPackingSlipTrans) => VendPackingSlipTrans,
        _ => throw new ArgumentException($"No Finance table number is registered for {typeof(TEntity).FullName}.")
    };
}
