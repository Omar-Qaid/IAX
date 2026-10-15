using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.Entities;

[Table("InventItemSalesSetup")]
public sealed class InventItemSalesSetup : Entity<long>
{
    [Required, StringLength(FieldLengths.ItemId)]
    public string ItemId { get; set; } = string.Empty;

    [Required, StringLength(FieldLengths.CompanyId)]
    public string ItemDataAreaId { get; set; } = string.Empty;

    [Required, StringLength(FieldLengths.InventDimId)]
    public string InventDimId { get; set; } = string.Empty;

    [StringLength(FieldLengths.InventDimId)]
    public string? InventDimIdDefault { get; set; }

    public int? ATPApplyDemandTimeFence { get; set; }
    public int? ATPApplySupplyTimeFence { get; set; }
    public int? ATPBackwardDemandTimeFence { get; set; }
    public int? ATPBackwardSupplyTimeFence { get; set; }
    public int? ATPInclPlannedOrders { get; set; }
    public int? ATPTimeFence { get; set; }
    public int? CalendarDays { get; set; }
    public int? DeliveryDateControlType { get; set; }
    public decimal? HighestQty { get; set; }
    public int? LeadTime { get; set; }
    public decimal? LowestQty { get; set; }
    public int? MandatoryInventLocation { get; set; }
    public int? MandatoryInventSite { get; set; }
    public decimal? MultipleQty { get; set; }
    public int? Override { get; set; }
    public int? OverrideSalesLeadTime { get; set; }
    public decimal? StandardQty { get; set; }
    public int? Stopped { get; set; }
    public long? Sequence { get; set; }
    public int? OverrideDefaultStorageDimensions { get; set; }
}
