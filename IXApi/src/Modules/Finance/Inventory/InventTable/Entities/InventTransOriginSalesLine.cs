using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities;

[Table("InventTransOriginSalesLine")]
public class InventTransOriginSalesLine : Entity<long>
{
    public long InventTransOrigin { get; set; }
    public string SalesLineDataAreaId { get; set; } = string.Empty;
    public string SalesLineInventTransId { get; set; } = string.Empty;
}
