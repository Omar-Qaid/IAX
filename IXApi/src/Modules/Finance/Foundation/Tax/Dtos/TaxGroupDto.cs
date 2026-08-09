using System;
using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class TaxGroupDto : EntityDto<long>
    {
        public string TaxGroup { get; set; } = string.Empty;
        public string TaxGroupName { get; set; } = string.Empty;
        public TaxGroupSetup TaxGroupSetup { get; set; }
        public TaxGroupSource Source { get; set; }
        public TaxGroupRounding TaxGroupRounding { get; set; }
        public NoYes TaxReverseOnCashDisc { get; set; }
        public NoYes EuTrade_W { get; set; }
        public NoYes MandatorySalesDate_W { get; set; }
        public NoYes FillSalesDate_W { get; set; }
        public int FillVatDueDatePeriodNumber { get; set; }
        public NoYes FillVatDueDate_W { get; set; }
        public TaxPointBase FillVatDueDateBasedOn { get; set; }
        public TaxPeriodUnit FillVatDueDatePeriod { get; set; }
        public TaxPrintDetail TaxPrintDetail { get; set; }

        public List<TaxGroupDataDto> Lines { get; set; } = new List<TaxGroupDataDto>();
    }
}