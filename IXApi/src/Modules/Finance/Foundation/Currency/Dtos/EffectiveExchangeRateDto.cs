using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.Currency;

public sealed record EffectiveExchangeRateDto(ExchangeRateCurrencyPair? Pair, ExchangeRate? Rate, string? Error);
