using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.GeneralLedger
{
    public class MainAccountCategoryDto : EntityDto<long>
    {
        public string AccountCategory { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
