using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;

[Table("EcoResTrackingDimensionGroupFldSetup")]
public sealed class EcoResTrackingDimensionGroupFldSetup : SharedEntity<long>
{
    public long TrackingDimensionGroup { get; set; }
    public int DimensionFieldId { get; set; }
    public decimal LineNumber { get; set; }
    public bool IsAllowBlankReceiptEnabled { get; set; }
    public bool IsAllowBlankIssueEnabled { get; set; }
    public bool IsPhysicalInventoryEnabled { get; set; }
    public bool IsFinancialInventoryEnabled { get; set; }
    public bool IsCoveragePlanByDimensionEnabled { get; set; }
    public bool IsPrimaryStockingEnabled { get; set; }
    public bool IsPurchPriceSearchEnabled { get; set; }
    public bool IsSalesPriceSearchEnabled { get; set; }
    public bool IsSalesProcessActivated { get; set; }
}
