using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    /// <summary>Vendor bank identity. Basic master data only; no payment file or bank integration.</summary>
    public class VendBankAccountDto : EntityDto<long>
    {
        [Required]
        [StringLength(FieldLengths.PaymModeId)]
        public string AccountID { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.VendAccount)]
        public string VendAccount { get; set; } = string.Empty;

        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(48)]
        public string AccountNum { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;
    }
}
