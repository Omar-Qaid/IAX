using System;
using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class TaxExemptCodeDto : EntityDto<long>
    {
        public string ExemptCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}