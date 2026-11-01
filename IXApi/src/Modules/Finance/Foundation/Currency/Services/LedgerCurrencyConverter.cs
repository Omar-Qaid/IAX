using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using CurrencyEntity = IAX.IXApi.Modules.Finance.Entities.Currency;

namespace IAX.IXApi.Modules.Finance.Foundation.Currency;

internal sealed class LedgerCurrencyConverter(DbContext db)
{
    public async Task<decimal> ConvertAsync(string area, decimal amount, string from, string to,
        long rateType, DateTime date, CancellationToken ct)
    {
        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase)) return await RoundAsync(area, amount, to, ct);
        var effective = await new EffectiveExchangeRateLookup(db).FindAsync(area, rateType, from, to, date, ct);
        if (effective.Error != null || effective.Pair == null || effective.Rate == null)
            throw new InvalidOperationException($"Exchange rate {from}/{to} is unavailable: {effective.Error}");
        var factor = (int)effective.Pair.ExchangeRateDisplayFactor;
        if (factor <= 0) factor = 1;
        return await RoundAsync(area, amount * effective.Rate.ExchangeRateValue / factor, to, ct);
    }

    private async Task<decimal> RoundAsync(string area, decimal amount, string currency, CancellationToken ct)
    {
        var precision = await db.Set<CurrencyEntity>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.CurrencyCode == currency)
            .Select(x => x.RoundingPrecision).FirstOrDefaultAsync(ct);
        if (precision <= 0) return amount;
        return Math.Round(amount / precision, 0, MidpointRounding.AwayFromZero) * precision;
    }
}
