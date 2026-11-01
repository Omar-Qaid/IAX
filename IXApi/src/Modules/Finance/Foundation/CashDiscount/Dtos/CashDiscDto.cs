using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.Foundation.CashDiscount;

public class CashDiscDto : EntityDto<long>
{
    public string CashDiscCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int NumOfMonths { get; set; }
    public int NumOfDays { get; set; }
    public decimal DiscPct { get; set; }
    public long CashDiscLedgerDimension { get; set; }
}
