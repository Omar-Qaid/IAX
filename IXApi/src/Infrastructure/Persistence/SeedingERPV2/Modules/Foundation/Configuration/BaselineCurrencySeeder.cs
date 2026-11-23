using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;

/// <summary>
/// Currencies, exchange rate types, and for every type a pair from each foreign currency into the accounting
/// currency with one rate. A pair that already has any rate keeps its rates.
/// </summary>
public sealed class BaselineCurrencySeeder : IBaselineSeeder
{
    public const string SeederId = "Finance.GeneralLedger.Currencies";
    public string Id => SeederId;
    public IReadOnlyCollection<string> Dependencies => [BaselineCompanySeeder.SeederId];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var catalog = FoundationBaselineCatalog.Load();
        foreach (var currency in catalog.Currencies)
            await c.AddMissingAsync<Currency>(x => x.CurrencyCode == currency.Code,
                currency.Symbol == null ? new() { CurrencyCode = currency.Code, RoundingPrecision = 0.01m } : new() { CurrencyCode = currency.Code, Symbol = currency.Symbol, RoundingPrecision = 0.01m }, ct);

        var rates = catalog.ExchangeRates;
        foreach (var type in catalog.ExchangeRateTypes)
        {
            var rateType = await c.AddMissingAsync<ExchangeRateType>(x => x.Name == type.Name,
                new() { Name = type.Name, Description = type.Description }, ct);
            foreach (var rate in rates.Rates)
            {
                var pair = await c.AddMissingAsync<ExchangeRateCurrencyPair>(x => x.FromCurrencyCode == rate.From
                        && x.ToCurrencyCode == rates.AccountingCurrency && x.ExchangeRateType == rateType.RecId,
                    new() { FromCurrencyCode = rate.From, ToCurrencyCode = rates.AccountingCurrency, ExchangeRateType = rateType.RecId,
                        ExchangeRateDisplayFactor = ExchangeRateDisplayFactor.One }, ct);
                await c.AddMissingAsync<ExchangeRate>(x => x.ExchangeRateCurrencyPair == pair.RecId,
                    new() { ExchangeRateCurrencyPair = pair.RecId, ExchangeRateValue = rate.Rate, ValidFrom = rates.ValidFrom, ValidTo = rates.ValidTo }, ct);
            }
        }
    }
}
