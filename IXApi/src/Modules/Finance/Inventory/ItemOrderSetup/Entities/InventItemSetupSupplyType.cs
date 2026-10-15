using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.Entities;

[Table("InventItemSetupSupplyType")]
public sealed class InventItemSetupSupplyType : Entity<long>
{
    [Required, StringLength(FieldLengths.ItemId)]
    public string ItemId { get; set; } = string.Empty;

    [Required, StringLength(FieldLengths.CompanyId)]
    public string ItemDataAreaId { get; set; } = string.Empty;

    public int DefaultOrderType { get; set; }
}
