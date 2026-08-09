using System;
using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class TaxPeriodHeadDto : EntityDto<long>
    {
        public string TaxPeriod { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string TaxAuthority { get; set; } = string.Empty;
        public string PaymentCode { get; set; } = string.Empty;
        public int QtyUnit { get; set; } = 1;
        public TaxPeriodUnit PeriodUnit { get; set; } = TaxPeriodUnit.Day;
        public NoYes NotGenerateOffsetTaxTrans { get; set; } = NoYes.No;
        public NoYes ReportAdjustment { get; set; } = NoYes.No;
        public NoYes UseBatch { get; set; } = NoYes.No;
        public string ActivePeriodForBatchJobs { get; set; } = string.Empty;
        public List<TaxReportPeriodDto> Intervals { get; set; } = new();
    }
}