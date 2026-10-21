using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;

[Table("EcoResCategoryHierarchy")]
public sealed class EcoResCategoryHierarchy : SharedEntity<long>
{
    [Required]
    [StringLength(FieldLengths.Name)]
    public string Name { get; set; } = string.Empty;
}
