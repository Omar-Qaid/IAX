using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
namespace IAX.IXApi.Modules.Finance.Entities;
/// <summary>D365 source-document accounting storage; never seeded as configuration or master data.</summary>
[Table("AccountingEvent")]
public sealed class AccountingEvent : SharedEntity<long>
{
    public DateTime AccountingDate { get; set; }
    public DateTime? EventDateTime { get; set; }
    public long? SourceDocumentHeader { get; set; }
    // D365 enum storage only; lifecycle values are supplied by the future owning flow.
    public int? State { get; set; }
    public int? Type { get; set; }
    public NoYes? IsAccountingRequired { get; set; }
}
