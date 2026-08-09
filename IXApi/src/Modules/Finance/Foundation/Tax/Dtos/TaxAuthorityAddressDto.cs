using System;
using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class TaxAuthorityAddressDto : EntityDto<long>
    {
        public string TaxAuthority { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string TaxAuthorityId { get; set; } = string.Empty;
        public string AccountNum { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty;
        public string Sms { get; set; } = string.Empty;
        public string Telex { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string Pager { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal RoundOff { get; set; }
        public TaxRoundOffType RoundOffType { get; set; }
        public TaxReportLayout TaxReportLayout { get; set; }
        public NoYes UseDefaultLayout { get; set; }
        public NoYes SeparateTaxSummary { get; set; }
        public NoYes PrintBlankPage { get; set; }
    }
}