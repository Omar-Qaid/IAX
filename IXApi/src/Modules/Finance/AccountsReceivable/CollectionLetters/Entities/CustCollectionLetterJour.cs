using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.CollectionLetters;

public sealed class CustCollectionLetterJour : Entity<long>
{
    public string CollectionLetterNum { get; set; } = string.Empty;
    public string AccountNum { get; set; } = string.Empty;
    public int CollectionLetterCode { get; set; }
    public DateTime CollectionLetterDate { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string PostingProfile { get; set; } = string.Empty;
    public int? Status { get; set; }
    public DateTime? Updated { get; set; }
    public DateTime? Canceled { get; set; }
    public decimal Fee { get; set; }
    public decimal TaxAmount { get; set; }
    public string? LedgerVoucher { get; set; }
    public DateTime? PrintDate { get; set; }
    public string? PaymTermId { get; set; }
    public DateTime? DueDate { get; set; }
}
