namespace IAX.IXApi.Modules.Finance.Common;

/// <summary>Table numbers persisted in polymorphic Finance references such as MarkupTrans.TransTableId.</summary>
public static class FinanceTableNumbers
{
    public const int SalesTable = 2002;
    public const int SalesLine = 2003;

    public static int For<TEntity>() => typeof(TEntity) == typeof(global::IAX.IXApi.Modules.Finance.AccountsReceivable.SalesTable)
        ? SalesTable
        : typeof(TEntity) == typeof(global::IAX.IXApi.Modules.Finance.AccountsReceivable.SalesLine)
            ? SalesLine
            : throw new ArgumentException($"No Finance table number is registered for {typeof(TEntity).FullName}.");
}
