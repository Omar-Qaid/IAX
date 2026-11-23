using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Currency;
using Xunit;

namespace IAX.IXApi.Tests;

public class EffectiveExchangeRateTests
{
    private static ExchangeRate Rate(int day, decimal value) => new()
    {
        ValidFrom = new DateTime(2026, 1, day), ExchangeRateValue = value, IsActive = true
    };

    [Fact]
    public void Selects_latest_effective_rate_without_rescaling_stored_value()
    {
        var old = Rate(1, 375);
        var latest = Rate(10, 380);
        var future = Rate(20, 390);
        Assert.Same(latest, EffectiveExchangeRateLookup.Select([old, future, latest], new DateTime(2026, 1, 10, 23, 0, 0)).Rate);
        Assert.Equal(380, latest.ExchangeRateValue);
    }

    [Fact]
    public void Honors_expiration_and_rejects_missing_or_ambiguous_rates()
    {
        var rate = Rate(1, 3.75m);
        rate.ValidTo = new DateTime(2026, 1, 10);
        Assert.Same(rate, EffectiveExchangeRateLookup.Select([rate], rate.ValidTo.AddHours(12)).Rate);
        Assert.NotNull(EffectiveExchangeRateLookup.Select([rate], rate.ValidTo.AddDays(1)).Error);
        Assert.NotNull(EffectiveExchangeRateLookup.Select([rate, Rate(1, 4)], rate.ValidFrom).Error);
        Assert.NotNull(EffectiveExchangeRateLookup.Select([Rate(1, 0)], rate.ValidFrom).Error);
    }

    [Fact]
    public void Ignores_deleted_and_inactive_rates()
    {
        var active = Rate(1, 3.75m);
        var deleted = Rate(2, 4); deleted.IsDeleted = true;
        var inactive = Rate(3, 5); inactive.IsActive = false;
        Assert.Same(active, EffectiveExchangeRateLookup.Select([active, deleted, inactive], new DateTime(2026, 1, 4)).Rate);
    }
}
