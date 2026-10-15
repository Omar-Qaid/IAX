using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;

[Table("EcoResStorageDimensionGroup")]
public sealed class EcoResStorageDimensionGroup : SharedEntity<long>
{
    [Required]
    [StringLength(FieldLengths.StorageDimensionGroupId)]
    public string StorageDimensionGroupId { get; set; } = string.Empty;

    [Required]
    [StringLength(FieldLengths.Name)]
    public string Name { get; set; } = string.Empty;

    public bool UseSite { get; set; }
    public bool UseWarehouse { get; set; }
    public bool UseLocation { get; set; }
}
