using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class CurrencyDto : EntityDto<long>
    {
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencyCodeIso { get; set; } = string.Empty;
        public string Txt { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public NoYes IsEuro { get; set; }
        public decimal RoundOffSales { get; set; }
        public RoundOffType RoundOffTypeSales { get; set; }
        public decimal RoundOffPurch { get; set; }
        public RoundOffType RoundOffTypePurch { get; set; }
        public decimal RoundOffPrice { get; set; }
        public RoundOffType RoundOffTypePrice { get; set; }
        public decimal RoundingPrecision { get; set; }
        public decimal LtmRoundOffLineAmount { get; set; }
        public RoundOffType LtmRoundOffTypeLineAmount { get; set; }
    }
}

