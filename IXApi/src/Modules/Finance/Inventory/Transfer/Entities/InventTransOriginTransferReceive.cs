using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities;

[Table("InventTransOriginTransferReceive")]
public class InventTransOriginTransferReceive : Entity<long>
{
    public long InventTransOrigin { get; set; }
    public string TransferReceiveDataAreaId { get; set; } = string.Empty;
    public string TransferReceiveId { get; set; } = string.Empty;
    public decimal TransferReceiveLineNum { get; set; }
}
