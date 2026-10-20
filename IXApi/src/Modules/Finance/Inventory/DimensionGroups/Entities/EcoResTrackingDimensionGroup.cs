using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;

[Table("EcoResTrackingDimensionGroup")]
public sealed class EcoResTrackingDimensionGroup : SharedEntity<long>
{
    [Required]
    [StringLength(FieldLengths.Name)]
    public string Name { get; set; } = string.Empty;

    [StringLength(FieldLengths.Description)]
    public string Description { get; set; } = string.Empty;
}
