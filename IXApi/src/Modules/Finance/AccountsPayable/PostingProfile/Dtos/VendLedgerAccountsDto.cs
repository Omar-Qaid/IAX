using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendLedgerAccountsDto : EntityDto<long>
{
    public string PostingProfile { get; set; } = string.Empty;
    public AccountCode AccountCode { get; set; }
    public string Num { get; set; } = string.Empty;
    public long SummaryLedgerDimension { get; set; }
    public long ClearingLedgerDimension { get; set; }
}
