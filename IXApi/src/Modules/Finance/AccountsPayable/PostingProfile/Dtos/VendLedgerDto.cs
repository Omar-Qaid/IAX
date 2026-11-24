using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public class VendLedgerDto : EntityDto<long>
{
    public string PostingProfile { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public NoYes Settlement { get; set; }
    public NoYes Cancel { get; set; }
    public string PostingProfileClose { get; set; } = string.Empty;
}
