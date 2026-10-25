using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public class ReturnDispositionCodeDto : EntityDto<long>
{
    public string DispositionCodeId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DispositionAction DispositionAction { get; set; }
}
