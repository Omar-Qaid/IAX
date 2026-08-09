using System;
using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class TaxOnItemDto : EntityDto<long>
    {
        public string TaxItemGroup { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string TaxExemptCode { get; set; } = string.Empty;

        // Navigation / Joined Display Attributes
        public string? TaxCodeName { get; set; }
        public decimal? TaxValue { get; set; }
    }
}