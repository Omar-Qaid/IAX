using System;
using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class TaxGroupDataDto : EntityDto<long>
    {
        public string? TaxGroup { get; set; }
        public string? TaxCode { get; set; }
        public string? TaxExemptCode { get; set; }
        public NoYes ExemptTax { get; set; }
        public NoYes UseTax { get; set; }
        public NoYes IntracomVat { get; set; }
        public NoYes ReverseCharge_W { get; set; }

        // Navigation / Joined Display Attributes
        public string? TaxCodeName { get; set; }
        public decimal? TaxValue { get; set; }
    }
}