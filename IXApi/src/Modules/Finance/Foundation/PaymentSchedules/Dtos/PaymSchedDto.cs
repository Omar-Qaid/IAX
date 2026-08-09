using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class PaymSchedDto : EntityDto<long>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int NumOfPayment { get; set; }
        public decimal AmountCur { get; set; }
        public decimal LowestAmount { get; set; }
        public PaymSchedAllocateMethod PayBy { get; set; }
        public PeriodUnit PeriodUnit { get; set; }
        public int QtyUnit { get; set; }
        public PaymSchedTaxDist TaxDistribution { get; set; }
        public PaymSchedMiscChargeDist McrMiscChargeDist { get; set; }
        public decimal McrMinOrderValue { get; set; }
        public decimal McrMaxOrderValue { get; set; }
        public int McrMinNumInstallments { get; set; }
        public int McrMaxNumInstallments { get; set; }
        public NoYes McrFlexiblePlan { get; set; }
    }
}

