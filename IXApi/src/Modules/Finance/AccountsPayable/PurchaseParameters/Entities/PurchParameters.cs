using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
namespace IAX.IXApi.Modules.Finance.Entities;
/// <summary>Basic purchase defaults; purchase-order consumption belongs to the transaction phase.</summary>
[Table("PurchParameters")]
public sealed class PurchParameters : Entity<long>
{
    public PurchaseType PurchaseType { get; set; } = PurchaseType.Purch;
}
