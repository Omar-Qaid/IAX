using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
namespace IAX.IXApi.Modules.Finance.AccountsPayable;
/// <summary>D365 transaction storage; created by the owning business flow, never by phase-one seeding.</summary>
[Table("VendInvoiceInfoSubLine")]
public sealed class VendInvoiceInfoSubLine : Entity<long>
{
    [Required, StringLength(FieldLengths.PackingSlipId)]
    public string DocumentId { get; set; } = string.Empty;
    [StringLength(FieldLengths.ParmId)]
    public string? ParmId { get; set; }
    public long? LineRefRecId { get; set; }
    // Polymorphic journal reference: do not bind it to one receipt table.
    public long? JournalRefRecId { get; set; }
    public int? JournalRefTableId { get; set; }
    public decimal? ReceiveNow { get; set; }
    public decimal? InventNow { get; set; }
}
