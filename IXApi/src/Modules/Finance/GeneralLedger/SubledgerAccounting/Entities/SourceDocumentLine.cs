using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
namespace IAX.IXApi.Modules.Finance.Entities;
/// <summary>D365 source-document accounting storage; never seeded as configuration or master data.</summary>
[Table("SourceDocumentLine")]
public sealed class SourceDocumentLine : SharedEntity<long>
{
    public int? AccountingStatus { get; set; }
    public DateTime? ExchangeRateDate { get; set; }
    public NoYes? IsFinal { get; set; }
    public long? ParentSourceDocumentLine { get; set; }
    public long SourceDocumentHeader { get; set; }
    public int SourceRelationType { get; set; }
    public byte[]? StateContainer { get; set; }
    [Required]
    public string TypeEnumName { get; set; } = string.Empty;
    public int TypeEnumValue { get; set; }
    public decimal? TermQuantity { get; set; }
    public int? DraftVersion { get; set; }
}
