using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

/// <summary>
/// The posted sales tax report: taxable base and tax per tax code and direction, for a date range
/// or for one settlement period interval.
///
/// Amounts are the accounting-currency amounts (TaxBaseAmount / TaxAmount), because that is what a
/// return is filed on; a foreign-currency document's own amounts stay on the transaction.
///
/// Settlement posts an offsetting transaction against each transaction it clears, so the report
/// excludes those offsets - otherwise a settled period would total zero. Settlement status is
/// reported per code instead, from the period stamp settlement writes.
/// </summary>
public sealed class TaxReportService(IUnitOfWork unitOfWork, ICompanyExecutionContext company) : ITaxReportService
{
    private DbContext Db => unitOfWork.Context;

    public async Task<(TaxReportDto? Result, string? Error)> RunAsync(DateOnly? fromDate, DateOnly? toDate,
        string? taxPeriod, string? taxCode, CancellationToken ct = default)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A company must be selected.");
        taxPeriod = taxPeriod?.Trim();
        taxCode = taxCode?.Trim();

        DateTime from, to;
        string? authority = null;
        if (!string.IsNullOrEmpty(taxPeriod))
        {
            // A period interval is identified by its start date, as settlement identifies it.
            if (fromDate is null)
                return (null, "A period start date is required when reporting on a settlement period.");
            var interval = await Db.Set<TaxReportPeriod>().AsNoTracking().FirstOrDefaultAsync(
                x => x.DataAreaId == companyId && x.TaxPeriod == taxPeriod
                     && x.FromDate == fromDate.Value.ToDateTime(TimeOnly.MinValue), ct);
            if (interval == null)
                return (null, $"Settlement period '{taxPeriod}' has no interval starting on {fromDate:d}.");
            from = interval.FromDate.Date;
            to = interval.ToDate.Date;
            authority = await Db.Set<TaxPeriodHead>().AsNoTracking()
                .Where(x => x.DataAreaId == companyId && x.TaxPeriod == taxPeriod)
                .Select(x => x.TaxAuthority).FirstOrDefaultAsync(ct);
        }
        else
        {
            if (fromDate is null || toDate is null)
                return (null, "A from and to date are required, or a settlement period and its start date.");
            from = fromDate.Value.ToDateTime(TimeOnly.MinValue);
            to = toDate.Value.ToDateTime(TimeOnly.MinValue);
        }
        if (to < from) return (null, "The end date cannot precede the start date.");
        var exclusiveEnd = to.AddDays(1);

        var rows = await Db.Set<TaxTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId
                && x.TransDate >= from && x.TransDate < exclusiveEnd
                // The offsetting transactions settlement writes are excluded: they exist to net the
                // settled tax out of the ledger, not to be reported as tax collected or paid.
                && x.OrigTaxTransRecId == 0
                && (taxCode == null || taxCode == "" || x.TaxCode == taxCode))
            .Select(x => new { x.TaxCode, x.TaxDirection, x.TaxBaseAmount, x.TaxAmount, x.TaxPeriod })
            .ToListAsync(ct);

        var lines = rows
            .GroupBy(x => (x.TaxCode, x.TaxDirection))
            .Select(g => new TaxReportLineDto(
                g.Key.TaxCode,
                g.Key.TaxDirection.ToString(),
                g.Count(),
                g.Sum(x => x.TaxBaseAmount),
                g.Sum(x => x.TaxAmount),
                g.Where(x => x.TaxPeriod.Length > 0).Sum(x => x.TaxAmount),
                g.Where(x => x.TaxPeriod.Length == 0).Sum(x => x.TaxAmount)))
            .OrderBy(x => x.TaxCode, StringComparer.Ordinal).ThenBy(x => x.Direction, StringComparer.Ordinal)
            .ToList();

        return (new TaxReportDto(
            DateOnly.FromDateTime(from),
            DateOnly.FromDateTime(to),
            string.IsNullOrEmpty(taxPeriod) ? null : taxPeriod,
            string.IsNullOrEmpty(authority) ? null : authority,
            lines.Where(x => x.Direction == nameof(TaxDirection.Outgoing)).Sum(x => x.TaxAmount),
            lines.Where(x => x.Direction == nameof(TaxDirection.Incoming)).Sum(x => x.TaxAmount),
            lines.Sum(x => x.TaxAmount),
            lines.Sum(x => x.SettledTaxAmount),
            lines.Sum(x => x.UnsettledTaxAmount),
            lines), null);
    }
}
