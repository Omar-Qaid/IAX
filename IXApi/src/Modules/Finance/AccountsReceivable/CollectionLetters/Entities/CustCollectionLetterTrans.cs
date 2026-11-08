using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.CollectionLetters;

public sealed class CustCollectionLetterTrans : Entity<long>
{
    public string CollectionLetterNum { get; set; } = string.Empty;
    public string AccountNum { get; set; } = string.Empty;
    public int CollectionLetterCode { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public long CustTransId { get; set; }
    public long CustTransOpenRecId { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime TransDate { get; set; }
    public string? Invoice { get; set; }
    public string? Voucher { get; set; }
    public decimal RemainAmount { get; set; }
    public decimal CalculationAmount { get; set; }
    public decimal Fee { get; set; }
    public int CollectionLetterIssued { get; set; }
}
