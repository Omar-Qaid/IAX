using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Currency;
using Microsoft.EntityFrameworkCore;
using CurrencyEntity = IAX.IXApi.Modules.Finance.Entities.Currency;

namespace IAX.IXApi.Modules.Finance.Common;

/// <summary>
/// One document's currency translation: the transaction currency it was entered in, and the rates
/// that carry each amount into the ledger's accounting and reporting currencies.
///
/// The rate is resolved once per document, from the ledger's exchange rate types on the document's
/// date, and then applied to every amount on it. That is deliberate: all lines of one invoice must
/// translate at the same rate, or the voucher's accounting amounts would not balance even though
/// its transaction amounts do.
///
/// Every posting flow previously rejected a document whose currency differed from the accounting
/// currency, each with its own message, because translating at the call site is easy to get subtly
/// wrong. This type is the shared answer, so the rule lives in one place.
/// </summary>
internal sealed class SubledgerCurrencyTranslation
{
    private readonly decimal _accountingRate;
    private readonly decimal _reportingRate;
    private readonly decimal _accountingPrecision;
    private readonly decimal _reportingPrecision;
    private readonly bool _accountingIsIdentity;
    private readonly bool _reportingIsIdentity;

    /// <summary>The currency the document was entered in.</summary>
    public string TransactionCurrency { get; }
    public string AccountingCurrency { get; }
    public string ReportingCurrency { get; }

    /// <summary>True when all three currencies match, so no amount is scaled or re-rounded.</summary>
    public bool IsIdentity => _accountingIsIdentity && _reportingIsIdentity;

    private SubledgerCurrencyTranslation(string transactionCurrency, string accountingCurrency, string reportingCurrency,
        decimal accountingRate, decimal reportingRate, decimal accountingPrecision, decimal reportingPrecision)
    {
        TransactionCurrency = transactionCurrency;
        AccountingCurrency = accountingCurrency;
        ReportingCurrency = reportingCurrency;
        _accountingRate = accountingRate;
        _reportingRate = reportingRate;
        _accountingPrecision = accountingPrecision;
        _reportingPrecision = reportingPrecision;
        _accountingIsIdentity = transactionCurrency == accountingCurrency;
        _reportingIsIdentity = transactionCurrency == reportingCurrency;
    }

    /// <summary>
    /// Resolves the document's rates. Throws <see cref="SubledgerPostingException"/> when a needed
    /// rate is missing, so a caller's existing posting-failure handling reports it as a 422.
    /// </summary>
    public static async Task<SubledgerCurrencyTranslation> LoadAsync(DbContext db, string area, Ledger ledger,
        string documentCurrency, DateTime date, CancellationToken ct)
    {
        var accounting = Normalize(ledger.AccountingCurrency);
        if (accounting.Length == 0) throw new SubledgerPostingException("The ledger accounting currency is required.");
        var transaction = Normalize(documentCurrency);
        if (transaction.Length == 0) throw new SubledgerPostingException("The document currency is required.");
        var reporting = Normalize(ledger.ReportingCurrency);
        if (reporting.Length == 0) reporting = accounting;

        // A document already in the accounting currency needs no rate, and - importantly - no
        // re-rounding: it must post exactly the amounts it was entered with.
        if (transaction == accounting && transaction == reporting)
            return new SubledgerCurrencyTranslation(transaction, accounting, reporting, 1m, 1m, 0m, 0m);

        var accountingRate = await RateAsync(db, area, transaction, accounting, ledger.DefaultExchangeRateType, date, ct);
        var reportingRate = reporting == accounting
            ? accountingRate
            : await RateAsync(db, area, transaction, reporting,
                ledger.ReportingCurrencyExchangeRateType == 0
                    ? ledger.DefaultExchangeRateType
                    : ledger.ReportingCurrencyExchangeRateType, date, ct);

        return new SubledgerCurrencyTranslation(transaction, accounting, reporting, accountingRate, reportingRate,
            await PrecisionAsync(db, area, accounting, ct), await PrecisionAsync(db, area, reporting, ct));
    }

    /// <summary>The amount in the ledger's accounting currency.</summary>
    public decimal ToAccounting(decimal transactionAmount) => _accountingIsIdentity
        ? transactionAmount
        : Round(transactionAmount * _accountingRate, _accountingPrecision);

    /// <summary>The amount in the ledger's reporting currency.</summary>
    public decimal ToReporting(decimal transactionAmount) => _reportingIsIdentity
        ? transactionAmount
        : Round(transactionAmount * _reportingRate, _reportingPrecision);

    /// <summary>
    /// A ledger line for one transaction-currency amount, with its accounting and reporting
    /// amounts translated. Callers use this instead of the three-amount overload so a flow cannot
    /// translate one line and forget another.
    /// </summary>
    public GeneralJournalAccountEntry AccountEntry(string area, long entryId,
        DimensionAttributeValueCombination dimension, LedgerPostingType type, decimal transactionAmount,
        string text, decimal qty, string paymentReference = "") =>
        SubledgerPostingSupport.AccountEntry(area, entryId, dimension, type, transactionAmount,
            ToAccounting(transactionAmount), ToReporting(transactionAmount), text, qty,
            TransactionCurrency, paymentReference);

    private static async Task<decimal> RateAsync(DbContext db, string area, string from, string to,
        long rateType, DateTime date, CancellationToken ct)
    {
        if (from == to) return 1m;
        if (rateType <= 0)
            throw new SubledgerPostingException(
                $"The ledger has no exchange rate type configured, so {from} cannot be translated to {to}.");
        var effective = await new EffectiveExchangeRateLookup(db).FindAsync(area, rateType, from, to, date, ct);
        if (effective.Error != null || effective.Pair == null || effective.Rate == null)
            throw new SubledgerPostingException(
                $"No exchange rate is available to translate {from} to {to} on {date:d}: {effective.Error}");
        var factor = (int)effective.Pair.ExchangeRateDisplayFactor;
        return effective.Rate.ExchangeRateValue / (factor <= 0 ? 1m : factor);
    }

    private static async Task<decimal> PrecisionAsync(DbContext db, string area, string currency, CancellationToken ct) =>
        await db.Set<CurrencyEntity>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.CurrencyCode == currency)
            .Select(x => x.RoundingPrecision).FirstOrDefaultAsync(ct);

    private static decimal Round(decimal amount, decimal precision) => precision <= 0
        ? decimal.Round(amount, 2, MidpointRounding.AwayFromZero)
        : Math.Round(amount / precision, 0, MidpointRounding.AwayFromZero) * precision;

    private static string Normalize(string? value) => value?.Trim().ToUpperInvariant() ?? string.Empty;
}
