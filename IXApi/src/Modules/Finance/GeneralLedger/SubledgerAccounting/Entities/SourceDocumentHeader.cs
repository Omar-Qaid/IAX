using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
namespace IAX.IXApi.Modules.Finance.Entities;
/// <summary>D365 source-document accounting storage; never seeded as configuration or master data.</summary>
[Table("SourceDocumentHeader")]
public sealed class SourceDocumentHeader : SharedEntity<long>
{
    public int? AccountingStatus { get; set; }
    public int SourceRelationType { get; set; }
    [Required]
    public string TypeEnumName { get; set; } = string.Empty;
    public int TypeEnumValue { get; set; }
}
