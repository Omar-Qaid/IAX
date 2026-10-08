using System.Collections.Generic;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Foundation.Currency
{
    public class BulkExchangeRatePairDto : ExchangeRateCurrencyPairDto
    {
        public List<ExchangeRateDto> ExchangeRates { get; set; } = new List<ExchangeRateDto>();
    }
}

