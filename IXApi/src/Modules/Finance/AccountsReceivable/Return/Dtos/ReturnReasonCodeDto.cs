using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public class ReturnReasonCodeDto : EntityDto<long>
{
    public string ReasonCodeId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReasonCodeGroupId { get; set; } = string.Empty;
}
