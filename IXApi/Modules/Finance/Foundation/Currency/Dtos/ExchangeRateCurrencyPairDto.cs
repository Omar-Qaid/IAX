using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class ExchangeRateCurrencyPairDto : EntityDto<long>
    {
        public string FromCurrencyCode { get; set; } = string.Empty;
        public string ToCurrencyCode { get; set; } = string.Empty;
        public long ExchangeRateType { get; set; }
        public ExchangeRateDisplayFactor ExchangeRateDisplayFactor { get; set; }
    }
}

