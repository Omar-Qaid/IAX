using System;
using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class TaxItemGroupDto : EntityDto<long>
    {
        public string TaxItemGroup { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TaxGroupSource Source { get; set; }
        public EuSalesListType EuSalesListType { get; set; }

        public List<TaxOnItemDto> Lines { get; set; } = new List<TaxOnItemDto>();
    }
}