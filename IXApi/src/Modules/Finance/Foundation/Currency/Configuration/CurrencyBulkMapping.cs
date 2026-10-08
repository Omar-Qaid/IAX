using Mapster;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.Currency;

public sealed record ExchangeRateBulkMappingSource(ExchangeRateDto Rate);

public sealed class CurrencyBulkMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<BulkExchangeRatePairDto, ExchangeRateCurrencyPair>()
            .IgnoreNonMapped(true)
            .Map(d => d.FromCurrencyCode, s => s.FromCurrencyCode)
            .Map(d => d.ToCurrencyCode, s => s.ToCurrencyCode)
            .Map(d => d.ExchangeRateType, s => s.ExchangeRateType)
            .Map(d => d.ExchangeRateDisplayFactor, s => s.ExchangeRateDisplayFactor);
        config.NewConfig<ExchangeRateBulkMappingSource, ExchangeRate>()
            .IgnoreNonMapped(true)
            .Map(d => d.ValidFrom, s => s.Rate.ValidFrom)
            .Map(d => d.ValidTo, s => s.Rate.ValidTo)
            .Map(d => d.ExchangeRateValue, s => s.Rate.ExchangeRateValue);
    }
}
