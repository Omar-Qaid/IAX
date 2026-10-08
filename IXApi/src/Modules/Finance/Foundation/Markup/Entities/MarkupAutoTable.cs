using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities;

[Table("MarkupAutoTable")]
public class MarkupAutoTable : Entity<long>
{
    public int AccountCode { get; set; }
    public string AccountRelation { get; set; } = string.Empty;
    public int DlvModeCode { get; set; }
    public string DlvModeRelation { get; set; } = string.Empty;
    public int ItemCode { get; set; }
    public string ItemRelation { get; set; } = string.Empty;
    public int MarkupReturn { get; set; }
    public MarkupModuleCategory ModuleCategory { get; set; }
    public MarkupModuleType ModuleType { get; set; }
    public string ReturnRelation { get; set; } = string.Empty;
    public int RetailConcessionFeeLegacy { get; set; }
    public int RetailConcessionFee { get; set; }
    public string SHA256Hash { get; set; } = string.Empty;
    public int RetailAdvancedChargesDeliveryProrate { get; set; }
    public int RetailChannelCode { get; set; }
    public string RetailChannelRelation { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
