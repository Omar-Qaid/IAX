using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;

[Table("EcoResStorageDimensionGroupItem")]
public sealed class EcoResStorageDimensionGroupItem : Entity<long>
{
    [Required]
    [StringLength(FieldLengths.ItemId)]
    public string ItemId { get; set; } = string.Empty;

    [Required]
    [StringLength(FieldLengths.CompanyId)]
    public string ItemDataAreaId { get; set; } = string.Empty;

    [Required]
    [StringLength(FieldLengths.StorageDimensionGroupId)]
    public string StorageDimensionGroupId { get; set; } = string.Empty;
}
