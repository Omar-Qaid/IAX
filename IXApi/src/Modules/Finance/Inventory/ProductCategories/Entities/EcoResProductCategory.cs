using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;

[Table("EcoResProductCategory")]
public sealed class EcoResProductCategory : SharedEntity<long>
{
    public long Product { get; set; }
    public long Category { get; set; }
    public long CategoryHierarchy { get; set; }
    public int DisplayOrder { get; set; }
}
