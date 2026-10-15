using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.Entities;

/// <summary>D365 F&amp;O shared product category (EcoResCategory).</summary>
[Table("EcoResCategory")]
public sealed class EcoResCategory : SharedEntity<long>
{
    [Required]
    [StringLength(FieldLengths.Name)]
    public string Name { get; set; } = string.Empty;

    public long CategoryHierarchy { get; set; }
    public long ParentCategory { get; set; }
}
