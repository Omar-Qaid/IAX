using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Currency;

/// <summary>Selects persisted rate data only. Does not interpret storage scaling or convert money.</summary>
public sealed class EffectiveExchangeRateLookup(DbContext db)
{
    public async Task<EffectiveExchangeRateDto> FindAsync(string area, long rateType, string fromCurrency,
        string toCurrency, DateTime date, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(area) || rateType <= 0 || date == default ||
            string.IsNullOrWhiteSpace(fromCurrency) || string.IsNullOrWhiteSpace(toCurrency))
            return new(null, null, "Company, rate type, currencies and date are required.");
        var from = fromCurrency.Trim().ToUpperInvariant();
        var to = toCurrency.Trim().ToUpperInvariant();
        if (!await db.Set<ExchangeRateType>().AsNoTracking().AnyAsync(x => x.DataAreaId == area && x.RecId == rateType && !x.IsDeleted && x.IsActive, ct))
            return new(null, null, "Exchange rate type was not found in the selected company.");
        var pairs = await db.Set<ExchangeRateCurrencyPair>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.ExchangeRateType == rateType && x.FromCurrencyCode == from && x.ToCurrencyCode == to && !x.IsDeleted && x.IsActive)
            .ToListAsync(ct);
        if (pairs.Count != 1) return new(null, null, pairs.Count == 0 ? "No direct currency pair was found." : "Duplicate currency pairs require reconciliation.");
        var pair = pairs[0];
        var rates = await db.Set<ExchangeRate>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.ExchangeRateCurrencyPair == pair.RecId).ToListAsync(ct);
        var selection = Select(rates, date);
        return new(pair, selection.Rate, selection.Error);
    }

    public static (ExchangeRate? Rate, string? Error) Select(IEnumerable<ExchangeRate> rates, DateTime date)
    {
        var eligible = rates.Where(x => !x.IsDeleted && x.IsActive && x.ValidFrom.Date <= date.Date
            && (x.ValidTo == default || x.ValidTo.Date >= date.Date)).OrderByDescending(x => x.ValidFrom.Date).ToList();
        if (eligible.Count == 0) return (null, "No exchange rate is effective on the requested date.");
        var latest = eligible[0];
        if (eligible.Count(x => x.ValidFrom.Date == latest.ValidFrom.Date) != 1)
            return (null, "Multiple exchange rates have the same effective start date.");
        if (latest.ExchangeRateValue <= 0) return (null, "The effective exchange rate must be positive.");
        return (latest, null);
    }
}
