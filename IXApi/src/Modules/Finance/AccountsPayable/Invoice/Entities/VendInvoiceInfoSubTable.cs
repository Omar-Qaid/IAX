using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
namespace IAX.IXApi.Modules.Finance.AccountsPayable;
/// <summary>
/// D365 VendInvoiceInfoSubTable: one row per purchase order on a pending vendor invoice (VendInvoiceInfoTable),
/// so an invoice can cover several orders. Created by the pending invoice flow, never by seeding.
/// </summary>
[Table("VendInvoiceInfoSubTable")]
public sealed class VendInvoiceInfoSubTable : Entity<long>
{
    [Required, StringLength(FieldLengths.ParmId)]
    public string ParmId { get; set; } = string.Empty;
    [StringLength(FieldLengths.ParmId)]
    public string TableRefId { get; set; } = string.Empty;
    [StringLength(FieldLengths.PurchId)]
    public string OrigPurchId { get; set; } = string.Empty;
    [StringLength(FieldLengths.Name)]
    public string PurchName { get; set; } = string.Empty;
    public long SourceAccountingEvent { get; set; }
}
