using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    public class VendorGroupDto : EntityDto<long>
    {
        public string VendGroup { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NameAR { get; set; }
        public string? ClearingAccount { get; set; }
    }
}