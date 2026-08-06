using System.Collections.Generic;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Shared.Features
{
    public class BulkExchangeRatePairDto : ExchangeRateCurrencyPairDto
    {
        public List<ExchangeRateDto> ExchangeRates { get; set; } = new List<ExchangeRateDto>();
    }
}
