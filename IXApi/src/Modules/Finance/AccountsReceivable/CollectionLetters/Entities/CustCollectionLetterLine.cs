using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.CollectionLetters;

public sealed class CustCollectionLetterLine : Entity<long>
{
    public string CollectionLetterCourse { get; set; } = string.Empty;
    public int CollectionLetterCode { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public int NumOfDays { get; set; }
    public decimal MinAmount { get; set; }
    public decimal Fee { get; set; }
    public long LedgerDimension { get; set; }
    public string? TaxItemGroup { get; set; }
    public int Block { get; set; }
    public string? Txt { get; set; }
    public string? Notes { get; set; }
}
