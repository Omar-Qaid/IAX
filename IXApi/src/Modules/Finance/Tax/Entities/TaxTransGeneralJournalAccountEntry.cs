using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.Entities;

/// <summary>D365 association between a tax transaction and its accounting entries.</summary>
/// <remarks>Reference fields retain D365's optional/zero semantics. Posting owns these rows.</remarks>
[Table("TaxTransGeneralJournalAccountEntry")]
public sealed class TaxTransGeneralJournalAccountEntry : Entity<long>
{
    public long? GeneralJournalAccountEntry { get; set; }
    public long? LedgerDimension { get; set; }
    public long? TaxTrans { get; set; }
    public int? TaxTransRelationship { get; set; }
    public long? TaxUncommitted { get; set; }
    public decimal? AccountingCurrencyAmount { get; set; }
    public int? MonetaryAmountType { get; set; }
    public long? SubledgerJournalAccountEntry { get; set; }

    [StringLength(FieldLengths.CurrencyCode)]
    public string? TransactionCurrency { get; set; }

    public decimal? TransactionCurrencyAmount { get; set; }
    public decimal? AllocationFactor { get; set; }
    public long Partition { get; set; }
    public int SysDataStateCode { get; set; }
}
