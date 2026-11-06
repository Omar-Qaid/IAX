using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
namespace IAX.IXApi.Modules.Finance.Entities;

/// <summary>
/// Basic sales order defaults, one row per company. The sales-side counterpart of
/// <see cref="PurchParameters"/>; the receivable defaults - posting profile, credit limit, payment
/// terms, delivery mode and the trade-agreement search flags - belong to <c>CustParameters</c> and
/// are deliberately not repeated here.
/// </summary>
[Table("SalesParameters")]
public sealed class SalesParameters : Entity<long>
{
    /// <summary>The order type a new sales order defaults to.</summary>
    public SalesType SalesType { get; set; } = SalesType.Sales;

    /// <summary>
    /// D365 <c>PRICEDATETYPE</c>: which date a sales trade agreement is resolved as of.
    /// <see cref="PriceDateType.Today"/> is D365's default; <see cref="PriceDateType.DeliveryDate"/>
    /// uses the line's delivery date. Read by <c>SalesOrderLineService</c>; when no parameters row
    /// exists the order date is kept, which is what that resolution used before this field existed.
    /// </summary>
    public PriceDateType PriceDateType { get; set; } = PriceDateType.Today;
}
