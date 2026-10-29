using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Currency;

/// <summary>Read-only rate availability checks; does not calculate or approve posting amounts.</summary>
public sealed class LedgerExchangeRateCheck(DbContext db)
{
    public async Task<(IReadOnlyList<string> Errors, IReadOnlyList<string> Unverified)> CheckAsync(
        string area, Ledger ledger, IEnumerable<LedgerJournalTrans> lines, CancellationToken ct)
    {
        var errors = new List<string>();
        var unverified = new HashSet<string>();
        var requests = new HashSet<(long Type, string From, string To, DateTime Date, string Purpose)>();
        foreach (var line in lines)
        {
            if ((!string.IsNullOrWhiteSpace(line.Company) && line.Company != area) ||
                (!string.IsNullOrWhiteSpace(line.OffsetCompany) && line.OffsetCompany != area))
            {
                unverified.Add("Intercompany exchange rates require destination ledger validation.");
                continue;
            }
            Request(ledger.AccountingCurrency, ledger.DefaultExchangeRateType,
                line.ExchRate != 0 || line.ExchRateSecond != 0, "Accounting");
            Request(ledger.ReportingCurrency, ledger.ReportingCurrencyExchangeRateType != 0
                ? ledger.ReportingCurrencyExchangeRateType : ledger.DefaultExchangeRateType,
                line.ReportingCurrencyExchRate != 0 || line.ReportingCurrencyExchRateSecondary != 0,
                "Reporting");

            void Request(string target, long type, bool overridden, string purpose)
            {
                if (string.IsNullOrWhiteSpace(target) || string.IsNullOrWhiteSpace(line.CurrencyCode) || line.TransDate == default) return;
                if (string.Equals(line.CurrencyCode.Trim(), target.Trim(), StringComparison.OrdinalIgnoreCase)) return;
                unverified.Add("Exchange-rate storage scaling, conversion factors and currency rounding are not validated.");
                if (overridden)
                {
                    unverified.Add($"{purpose} journal exchange-rate overrides require validation before posting.");
                    return;
                }
                requests.Add((type, line.CurrencyCode.Trim().ToUpperInvariant(), target.Trim().ToUpperInvariant(), line.TransDate.Date, purpose));
            }
        }
        var lookup = new EffectiveExchangeRateLookup(db);
        foreach (var request in requests.OrderBy(x => x.Date).ThenBy(x => x.Purpose).ThenBy(x => x.From))
        {
            if (request.Type <= 0)
            {
                errors.Add($"{request.Purpose}: no exchange rate type is configured on the ledger.");
                continue;
            }
            var result = await lookup.FindAsync(area, request.Type, request.From, request.To, request.Date, ct);
            if (result.Error != null)
                errors.Add($"{request.Purpose} {request.From}/{request.To} on {request.Date:yyyy-MM-dd}: {result.Error}");
        }
        return (errors.Distinct().ToList(), unverified.OrderBy(x => x).ToList());
    }
}
