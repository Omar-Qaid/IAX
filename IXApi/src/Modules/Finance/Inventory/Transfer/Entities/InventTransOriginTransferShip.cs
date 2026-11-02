using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Finance.Entities;

[Table("InventTransOriginTransferShip")]
public class InventTransOriginTransferShip : Entity<long>
{
    public long InventTransOrigin { get; set; }
    public string TransferShipDataAreaId { get; set; } = string.Empty;
    public string TransferShipId { get; set; } = string.Empty;
    public decimal TransferShipLineNum { get; set; }
}
