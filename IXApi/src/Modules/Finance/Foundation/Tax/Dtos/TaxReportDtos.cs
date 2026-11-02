namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

/// <summary>
/// One tax code and direction in the posted sales tax report: the taxable base and the tax, in the
/// accounting currency, which is what a return is filed on.
/// </summary>
public sealed record TaxReportLineDto(
    string TaxCode,
    string Direction,
    int TransactionCount,
    decimal TaxableBase,
    decimal TaxAmount,
    decimal SettledTaxAmount,
    decimal UnsettledTaxAmount);

/// <summary>
/// The posted sales tax report for a date range or a settlement period interval.
///
/// It reports the tax transactions that documents posted. Settlement writes an offsetting
/// transaction against each one it clears, so summing every row for a settled period nets to zero -
/// the report therefore excludes those offsets and reports settlement status per code instead, which
/// is the view a return is prepared from.
/// </summary>
public sealed record TaxReportDto(
    DateOnly FromDate,
    DateOnly ToDate,
    string? TaxPeriod,
    string? TaxAuthority,
    decimal OutgoingTax,
    decimal IncomingTax,
    decimal NetTax,
    decimal SettledTax,
    decimal UnsettledTax,
    IReadOnlyList<TaxReportLineDto> Lines);
