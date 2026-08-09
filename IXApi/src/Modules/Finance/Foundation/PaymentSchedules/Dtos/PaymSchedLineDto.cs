using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class PaymSchedLineDto : EntityDto<long>
    {
        public string Name { get; set; } = string.Empty;
        public decimal LineNum { get; set; }
        public int PercentAmount { get; set; }
        public decimal Value { get; set; }
        public int Qty { get; set; }
        public NoYes CfmPrepayment { get; set; }
        public NoYes McrShipping { get; set; }
    }
}

