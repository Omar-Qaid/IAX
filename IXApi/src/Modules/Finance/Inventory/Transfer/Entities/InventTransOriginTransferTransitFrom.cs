using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities;

[Table("InventTransOriginTransferTransitFrom")]
public class InventTransOriginTransferTransitFrom : Entity<long>
{
    public long InventTransOrigin { get; set; }
    public string TransferTransitFromDataAreaId { get; set; } = string.Empty;
    public string TransferTransitFromId { get; set; } = string.Empty;
    public decimal TransferTransitFromLineNum { get; set; }
}
