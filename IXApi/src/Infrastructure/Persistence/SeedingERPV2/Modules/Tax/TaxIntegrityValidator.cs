using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;

/// <summary>
/// Validate the posting chain a taxed document resolves before it can post: the code, its rate for
/// the fiscal year, the group and item-group links, the exemption codes they name, and the authority
/// and reporting periods behind the period head. Mapped references are then swept for the selected
/// company, including relationships owned by other modules.
/// </summary>
public sealed class TaxIntegrityValidator : ISeedValidator
{
    private static readonly MethodInfo LoadMethod = typeof(TaxIntegrityValidator).GetMethod(nameof(Load), BindingFlags.NonPublic | BindingFlags.Static)!;

    public async Task ValidateAsync(SeedContext c, CancellationToken ct)
    {
        // A taxed line resolves the code, then the account group that carries the posting dimension.
        var code = await c.RequireAsync<TaxTable>(x => x.TaxCode == "APV2-15", ct);
        var accounts = await c.RequireAsync<TaxLedgerAccountGroup>(x => x.TaxAccountGroup == code.TaxAccountGroup, ct);
        if (accounts.TaxIncomingLedgerDimension is not > 0)
            throw new InvalidOperationException("Tax account group must carry the input-tax ledger dimension before a taxed document can post.");

        // The rate has to cover the fiscal year the example posts into, or the calculation finds none.
        var rates = await c.Rows<TaxData>().Where(x => x.TaxCode == code.TaxCode).ToListAsync(ct);
        var from = new DateTime(c.FiscalYear, 1, 1);
        var to = new DateTime(c.FiscalYear, 12, 31);
        if (rates.Count == 0 || !rates.Any(x => x.TaxValue > 0m && x.TaxFromDate <= from && x.TaxToDate >= to))
            throw new InvalidOperationException($"Tax code {code.TaxCode} has no nonzero rate covering fiscal year {c.FiscalYear}.");

        // Two rates valid on the same day make the calculated amount depend on row order, so the
        // ranges for a code must not overlap at all.
        var ordered = rates.OrderBy(x => x.TaxFromDate).ThenBy(x => x.TaxToDate).ToList();
        for (var index = 0; index < ordered.Count; index++)
        {
            if (ordered[index].TaxToDate < ordered[index].TaxFromDate)
                throw new InvalidOperationException($"Tax rate for {code.TaxCode} ends before it starts.");
            if (index > 0 && ordered[index].TaxFromDate <= ordered[index - 1].TaxToDate)
                throw new InvalidOperationException($"Tax code {code.TaxCode} has overlapping rates; the applicable rate would be ambiguous.");
        }

        // Group and item group both have to admit the code, and every exemption they name must exist.
        var group = await c.RequireAsync<TaxGroupData>(x => x.TaxCode == code.TaxCode, ct);
        var item = await c.RequireAsync<TaxOnItem>(x => x.TaxCode == code.TaxCode, ct);
        await c.RequireAsync<TaxGroupHeading>(x => x.TaxGroup == group.TaxGroup, ct);
        await c.RequireAsync<TaxItemGroupHeading>(x => x.TaxItemGroup == item.TaxItemGroup, ct);
        foreach (var exempt in new[] { group.TaxExemptCode, item.TaxExemptCode }.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
            await c.RequireAsync<TaxExemptCodeTable>(x => x.ExemptCode == exempt, ct);

        // Reporting rests on the period head, its authority, and periods that tile the fiscal year.
        var period = await c.RequireAsync<TaxPeriodHead>(x => x.TaxPeriod == code.TaxPeriod, ct);
        await c.RequireAsync<TaxAuthorityAddress>(x => x.TaxAuthority == period.TaxAuthority, ct);
        var reported = await c.Rows<TaxReportPeriod>().Where(x => x.TaxPeriod == period.TaxPeriod).OrderBy(x => x.FromDate).ToListAsync(ct);
        if (reported.Count == 0 || reported[0].FromDate != from || reported[^1].ToDate != to)
            throw new InvalidOperationException($"Tax period {period.TaxPeriod} must report across the whole of fiscal year {c.FiscalYear}.");
        for (var index = 0; index < reported.Count; index++)
        {
            if (reported[index].ToDate < reported[index].FromDate)
                throw new InvalidOperationException("A tax reporting period ends before it starts.");
            if (index > 0 && reported[index].FromDate != reported[index - 1].ToDate.AddDays(1))
                throw new InvalidOperationException("Tax reporting periods must be contiguous and must not overlap.");
        }

        var cache = new Dictionary<IEntityType, object[]>();
        async Task<object[]> Rows(IEntityType type)
        {
            if (!cache.TryGetValue(type, out var rows))
            {
                rows = await (Task<object[]>)LoadMethod.MakeGenericMethod(type.ClrType).Invoke(null, [c, ct])!;
                cache[type] = rows;
            }
            return rows;
        }
        foreach (var type in c.Db.Model.GetEntityTypes().Where(x => TaxTableCatalog.MasterTypes.Contains(x.ClrType.Name)))
        {
            var rows = await Rows(type);
            if (rows.Length == 0) throw new InvalidOperationException($"Tax table {type.ClrType.Name} was not seeded in {c.Company}.");
            foreach (var fk in type.GetForeignKeys())
            {
                var parents = await Rows(fk.PrincipalEntityType);
                foreach (var row in rows)
                {
                    var key = fk.Properties.Select(p => p.PropertyInfo?.GetValue(row)).ToArray();
                    if (key.Any(x => x == null)) continue;
                    if (!parents.Any(parent => fk.PrincipalKey.Properties.Select(p => p.PropertyInfo?.GetValue(parent)).SequenceEqual(key)))
                        throw new InvalidOperationException($"Tax reference {type.ClrType.Name} -> {fk.PrincipalEntityType.ClrType.Name} is missing or crosses company {c.Company}.");
                }
            }
        }
    }

    private static async Task<object[]> Load<T>(SeedContext c, CancellationToken ct) where T : class
    {
        var query = c.Db.Set<T>().IgnoreQueryFilters().AsNoTracking();
        if (c.Db.Model.FindEntityType(typeof(T))!.FindProperty("DataAreaId") != null)
            query = query.Where(x => EF.Property<string>(x, "DataAreaId") == c.Company);
        return (await query.ToListAsync(ct)).Cast<object>().ToArray();
    }
}
