using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
namespace IAX.IXApi.Modules.Finance.Entities;
/// <summary>Basic purchase defaults; purchase-order consumption belongs to the transaction phase.</summary>
[Table("PurchParameters")]
public sealed class PurchParameters : Entity<long>
{
    public PurchaseType PurchaseType { get; set; } = PurchaseType.Purch;

    /// <summary>
    /// D365 <c>PRICEDATETYPE</c>: which date a purchase trade agreement is resolved as of.
    /// <see cref="PriceDateType.Today"/> is D365's default and means the date the price is looked
    /// up; <see cref="PriceDateType.DeliveryDate"/> uses the line's delivery date. Read by
    /// <c>PurchOrderService.FindPurchasePriceAsync</c>.
    /// </summary>
    public PriceDateType PriceDateType { get; set; } = PriceDateType.Today;
}
