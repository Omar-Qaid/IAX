using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory.InventoryStatus.Entities;

[Table("WHSInventStatus")]
public sealed class WHSInventStatus : Entity<long>
{
    [Required, StringLength(FieldLengths.InventStatusId)]
    public string InventStatusId { get; set; } = string.Empty;

    [Required, StringLength(FieldLengths.Name)]
    public string Name { get; set; } = string.Empty;

    public NoYes InventStatusBlocking { get; set; }
}
