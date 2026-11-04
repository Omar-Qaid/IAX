using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
namespace IAX.IXApi.Modules.Finance.Entities;
/// <summary>D365 transaction storage; created by the owning business flow, never by phase-one seeding.</summary>
[Table("SubledgerJournalEntry")]
public sealed class SubledgerJournalEntry : SharedEntity<long>
{
    public long? AccountingEvent { get; set; }
    public long? FiscalCalendarPeriod { get; set; }
    [StringLength(FieldLengths.JournalNumber)]
    public string? JournalNumber { get; set; }
    public long? Ledger { get; set; }
    public CurrentToOperationsPostingLayer PostingLayer { get; set; }
    // Preserve D365 enum storage without inventing statuses or processing behavior.
    public int? ReferenceIdentityTransferStatus { get; set; }
    public int? Status { get; set; }
    public long? TransferId { get; set; }
    public int Type { get; set; }
    [StringLength(FieldLengths.Voucher)]
    public string? Voucher { get; set; }
    [StringLength(FieldLengths.CompanyId)]
    public string? VoucherDataAreaId { get; set; }
    [StringLength(FieldLengths.Txt)]
    public string? TransTxt { get; set; }
    public DateTime? DocumentDate { get; set; }
    [StringLength(FieldLengths.DocumentNumber)]
    public string? DocumentNumber { get; set; }
}
