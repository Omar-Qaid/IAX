namespace IAX.IXApi.Modules.Finance.Common
{
    public enum ModuleInventCustVend
    {
        Vend = 1,
        Cust = 2,
        Invent = 3
    }

    public enum PriceDiscPartyCodeType
    {
        Table = 0,
        GroupId = 1,
        All = 2
    }

    public enum PriceDiscProductCodeType
    {
        Table = 0,
        GroupId = 1,
        All = 2
    }

    public enum PriceType
    {
        PriceSales = 0,
        LineDiscSales = 1,
        MultilineDiscSales = 2,
        EndDiscSales = 3,
        PricePurch = 4,
        LineDiscPurch = 5,
        MultilineDiscPurch = 6,
        EndDiscPurch = 7
    }
}
