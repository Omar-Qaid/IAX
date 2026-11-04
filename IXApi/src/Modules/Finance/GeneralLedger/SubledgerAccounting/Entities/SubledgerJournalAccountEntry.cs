using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
namespace IAX.IXApi.Modules.Finance.Entities;
/// <summary>D365 transaction storage; created by the owning business flow, never by phase-one seeding.</summary>
[Table("SubledgerJournalAccountEntry")]
public sealed class SubledgerJournalAccountEntry : SharedEntity<long>
{
    public long? SubledgerJournalEntry { get; set; }
    public long LedgerDimension { get; set; }
    public LedgerPostingType PostingType { get; set; }
    public int? Side { get; set; }
    public NoYes? IsCorrection { get; set; }
    public NoYes? IsTransferredInDetail { get; set; }
    public int? OriginalTransferPolicy { get; set; }
    public int? AccountEntryEventRole { get; set; }
    [StringLength(FieldLengths.CurrencyCode)]
    public string? TransactionCurrency { get; set; }
    public decimal? TransactionCurrencyAmount { get; set; }
    public decimal? AccountingCurrencyAmount { get; set; }
    public decimal? ReportingCurrencyAmount { get; set; }
    public decimal? ExchangeRate1 { get; set; }
    public decimal? ExchangeRate2 { get; set; }
    public decimal? ReportingExchangeRate1 { get; set; }
    public decimal? ReportingExchangeRate2 { get; set; }
    public DateTime? ExchangeRateDate { get; set; }
    public DateTime? HistoricalExchangeRateDate { get; set; }
    public long? GeneralJournalAccountEntry { get; set; }
}
