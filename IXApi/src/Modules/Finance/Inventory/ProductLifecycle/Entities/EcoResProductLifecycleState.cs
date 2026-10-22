using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle.Entities;

[Table("EcoResProductLifecycleState")]
public sealed class EcoResProductLifecycleState : SharedEntity<long>
{
    [Required, StringLength(FieldLengths.Name)]
    public string StateId { get; set; } = string.Empty;
    [StringLength(FieldLengths.Description)]
    public string Description { get; set; } = string.Empty;
    public NoYes IsDefaultOnProductRelease { get; set; }
    public NoYes BlockSales { get; set; }
    public NoYes BlockPurchase { get; set; }
    public NoYes BlockInventory { get; set; }
}
