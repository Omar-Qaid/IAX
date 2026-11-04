using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
namespace IAX.IXApi.Modules.Finance.Entities;
/// <summary>D365 transaction storage; created by the owning business flow, never by phase-one seeding.</summary>
[Table("AccountingDistribution")]
public sealed class AccountingDistribution : SharedEntity<long>
{
    public DateTime? AccountingDate { get; set; }
    // Linked to the owning source document and its accounting events.
    public long? AccountingEvent { get; set; }
    public long? AccountingLegalEntity { get; set; }
    public decimal? AllocationFactor { get; set; }
    public int? AmountSource { get; set; }
    public long? FinalizeAccountingEvent { get; set; }
    public long? LedgerDimension { get; set; }
    public int? MonetaryAmount { get; set; }
    public long? Number { get; set; }
    public long? ParentDistribution { get; set; }
    public long? ReferenceDistribution { get; set; }
    public int? ReferenceRole { get; set; }
    public long? SourceDocumentHeader { get; set; }
    public long? SourceDocumentLine { get; set; }
    [StringLength(FieldLengths.CurrencyCode)]
    public string? TransactionCurrency { get; set; }
    public decimal? TransactionCurrencyAmount { get; set; }
}
