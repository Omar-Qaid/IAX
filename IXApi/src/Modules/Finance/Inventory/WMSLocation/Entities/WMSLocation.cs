using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Entities;

[Table("WMSLocation")]
[Index(nameof(DataAreaId), nameof(InventLocationId), nameof(WMSLocationId), IsUnique = true)]
public sealed class WMSLocation : Entity<long>
{
    [Required, StringLength(FieldLengths.InventLocationId)]
    public string InventLocationId { get; set; } = string.Empty;

    [Required, StringLength(FieldLengths.WmsLocationId)]
    public string WMSLocationId { get; set; } = string.Empty;

    [StringLength(FieldLengths.WmsLocationId)]
    public string InputLocation { get; set; } = string.Empty;

    [StringLength(FieldLengths.Text)]
    public string CheckText { get; set; } = string.Empty;

    public int LocationType { get; set; }
    public int SortCode { get; set; }
    public NoYes ManualName { get; set; }
}
