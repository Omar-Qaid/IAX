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
    public const int HcmWorker = 4001;

    public static int For<TEntity>() => typeof(TEntity) switch
    {
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.AccountsReceivable.CustTable) => CustTable,
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.AccountsReceivable.SalesTable) => SalesTable,
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.AccountsReceivable.SalesLine) => SalesLine,
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.Entities.VendTable) => VendTable,
        var type when type == typeof(global::IAX.IXApi.Modules.Finance.Foundation.HcmWorkers.HcmWorker) => HcmWorker,
        _ => throw new ArgumentException($"No Finance table number is registered for {typeof(TEntity).FullName}.")
    };
}
