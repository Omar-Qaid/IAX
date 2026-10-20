using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities;

[Table("InventParameters")]
public sealed class InventParameters : Entity<long>
{
    [StringLength(FieldLengths.Unit)] public string DefaultUnitId { get; set; } = string.Empty;
    [StringLength(FieldLengths.InventSiteId)] public string MultiSiteDefaultInventSiteId { get; set; } = string.Empty;
    [StringLength(FieldLengths.InventLocationId)] public string MultiSiteDefaultInventLocationId { get; set; } = string.Empty;
    [StringLength(FieldLengths.JournalNameId)] public string MovementJournalNameId { get; set; } = string.Empty;
    [StringLength(FieldLengths.JournalNameId)] public string LossProfitJournalNameId { get; set; } = string.Empty;
    [StringLength(FieldLengths.JournalNameId)] public string TransferJournalNameId { get; set; } = string.Empty;
    [StringLength(FieldLengths.JournalNameId)] public string CountJournalNameId { get; set; } = string.Empty;
    public NoYes ReserveOnOrdered { get; set; }
    public NoYes MandatoryItemTaxGroup { get; set; }
}
