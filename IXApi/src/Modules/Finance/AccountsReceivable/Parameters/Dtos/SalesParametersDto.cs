using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    /// <summary>Basic sales order defaults, one row per company.</summary>
    public class SalesParametersDto : EntityDto<long>
    {
        public SalesType SalesType { get; set; } = SalesType.Sales;

        /// <summary>Which date a sales trade agreement is resolved as of (D365 PRICEDATETYPE).</summary>
        public PriceDateType PriceDateType { get; set; } = PriceDateType.Today;
    }
}
