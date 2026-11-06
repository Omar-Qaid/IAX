using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    /// <summary>Basic purchasing defaults, one row per company.</summary>
    public class PurchParametersDto : EntityDto<long>
    {
        public PurchaseType PurchaseType { get; set; } = PurchaseType.Purch;

        /// <summary>Which date a purchase trade agreement is resolved as of (D365 PRICEDATETYPE).</summary>
        public PriceDateType PriceDateType { get; set; } = PriceDateType.Today;
    }
}
