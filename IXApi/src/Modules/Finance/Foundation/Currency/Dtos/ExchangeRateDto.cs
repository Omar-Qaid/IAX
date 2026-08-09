using System;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class ExchangeRateDto : EntityDto<long>
    {
        public decimal ExchangeRateValue { get; set; }
        public long ExchangeRateCurrencyPair { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }
}

