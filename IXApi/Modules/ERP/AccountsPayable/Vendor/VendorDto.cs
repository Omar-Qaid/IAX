using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.ERP.AccountsPayable
{
    public class VendorDto : EntityDto<long>
    {
        public string AccountNum { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NameAR { get; set; }
        public string VendGroup { get; set; } = string.Empty;
        public string? Currency { get; set; }
        public string? PaymTermId { get; set; }
        public string? TaxGroup { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool Blocked { get; set; } = false;
        public string DataAreaId { get; set; } = "dat";
    }

    public class VendorGroupDto : EntityDto<long>
    {
        public string VendGroup { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NameAR { get; set; }
        public string? ClearingAccount { get; set; }
        public string DataAreaId { get; set; } = "dat";
    }
}
