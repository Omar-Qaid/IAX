using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities;

[Table("InventTransOriginTransferTransitTo")]
public class InventTransOriginTransferTransitTo : Entity<long>
{
    public long InventTransOrigin { get; set; }
    public string TransferTransitToDataAreaId { get; set; } = string.Empty;
    public string TransferTransitToId { get; set; } = string.Empty;
    public decimal TransferTransitToLineNum { get; set; }
}
