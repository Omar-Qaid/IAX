using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory.InventTable;

[Table("EcoResProductTranslation")]
public sealed class EcoResProductTranslation : SharedEntity<long>
{
    [Required]
    public long Product { get; set; }

    [Required]
    [StringLength(FieldLengths.LanguageId)]
    public string LanguageId { get; set; } = string.Empty;

    [Required]
    [StringLength(FieldLengths.Name)]
    public string Name { get; set; } = string.Empty;

    [StringLength(FieldLengths.Memo)]
    public string Description { get; set; } = string.Empty;
}
