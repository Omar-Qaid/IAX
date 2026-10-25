using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.GeneralLedger
{
    public class MainAccountDto : EntityDto<long>
    {
        public string MainAccountId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
