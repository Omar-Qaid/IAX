using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Inventory;

public class SiteInputDto
{
    [Required, StringLength(FieldLengths.InventSiteId)]
    public string SiteId { get; set; } = string.Empty;

    [Required, StringLength(FieldLengths.Name)]
    public string Name { get; set; } = string.Empty;

    [StringLength(FieldLengths.DefaultInventStatusID)]
    public string DefaultInventStatusId { get; set; } = string.Empty;

    public int TimeZone { get; set; }
    public bool IsReceivingWarehouseOverrideAllowed { get; set; }
    public long DefaultDimension { get; set; }
}
