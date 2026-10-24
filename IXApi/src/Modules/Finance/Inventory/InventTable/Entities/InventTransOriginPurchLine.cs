using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities;

[Table("InventTransOriginPurchLine")]
public class InventTransOriginPurchLine : Entity<long>
{
    public long InventTransOrigin { get; set; }
    public string PurchLineDataAreaId { get; set; } = string.Empty;
    public string PurchLineInventTransId { get; set; } = string.Empty;
}
