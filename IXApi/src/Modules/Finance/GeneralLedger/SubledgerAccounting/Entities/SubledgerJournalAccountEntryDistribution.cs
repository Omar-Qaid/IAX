using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
namespace IAX.IXApi.Modules.Finance.Entities;
/// <summary>D365 source-document accounting storage; never seeded as configuration or master data.</summary>
[Table("SubledgerJournalAccountEntryDistribution")]
public sealed class SubledgerJournalAccountEntryDistribution : SharedEntity<long>
{
    public decimal? AccountingCurrencyAmount { get; set; }
    public long? AccountingDistribution { get; set; }
    public long? ParentDistribution { get; set; }
    public decimal? ReportingCurrencyAmount { get; set; }
    public long? SubledgerJournalAccountEntry { get; set; }
}
