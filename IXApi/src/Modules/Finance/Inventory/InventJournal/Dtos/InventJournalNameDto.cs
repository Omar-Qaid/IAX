using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventJournalNameDto
{
    public string Id { get; set; } = string.Empty;
    public long RecId { get; set; }
    public string JournalNameId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InventJournalType JournalType { get; set; }
    public long VoucherNumberSequenceTable { get; set; }
    public string VoucherNumberSequenceCode { get; set; } = string.Empty;
    public DetailSummary DetailSummary { get; set; }
    public bool DeletePostedLines { get; set; }
    public ItemReservation Reservation { get; set; }
}

public class InventJournalNameInputDto
{
    [Required, StringLength(FieldLengths.JournalNameId)]
    public string JournalNameId { get; set; } = string.Empty;
    [Required, StringLength(FieldLengths.Description)]
    public string Description { get; set; } = string.Empty;
    public InventJournalType JournalType { get; set; }
    public long VoucherNumberSequenceTable { get; set; }
    public DetailSummary DetailSummary { get; set; }
    public bool DeletePostedLines { get; set; }
    public ItemReservation Reservation { get; set; }
}

public sealed record InventJournalNumberSequenceLookupDto(long RecId, string NumberSequence, string Description);

public sealed class InventJournalNameLookupsDto
{
    public List<InventJournalNumberSequenceLookupDto> NumberSequences { get; set; } = [];
}
