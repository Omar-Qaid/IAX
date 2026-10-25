using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class VendTransDto
{
    public long RecId { get; set; }
    public string AccountNum { get; set; } = string.Empty;
    public DateTime TransDate { get; set; }
    public string Voucher { get; set; } = string.Empty;
    public string Invoice { get; set; } = string.Empty;
    public string Txt { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal AmountCur { get; set; }
    public decimal AmountMst { get; set; }
    public decimal SettleAmountCur { get; set; }
    public decimal SettleAmountMst { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime Closed { get; set; }
    public LedgerTransType TransType { get; set; }
}

public sealed class VendTransQueryDto
{
    public string? AccountNum { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed record VendTransPageDto(int Page, int PageSize, int TotalCount, List<VendTransDto> Rows);
