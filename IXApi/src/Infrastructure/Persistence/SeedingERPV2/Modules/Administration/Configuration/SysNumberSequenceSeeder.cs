using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Administration.NumberSequences;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Administration;

/// <summary>
/// The allocation sequences every generated document code is drawn from. SysNumberSequence carries a
/// DataAreaId but its mapping declares a unique index on NumberSequence alone, so a key belongs to the
/// installation and the first company to seed owns the row. The seeder therefore reads without a
/// company filter and does not use <see cref="SeedContext.EnsureAsync"/>, which would propose a second
/// row per company and break that index.
/// </summary>
public sealed class SysNumberSequenceSeeder : IDataSeeder
{
    public string Id => "ERPV2.SysNumberSequenceSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CompanyInfoSeeder"];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var existing = await c.Db.Set<SysNumberSequence>().IgnoreQueryFilters().ToListAsync(ct);
        var byKey = existing.ToDictionary(x => x.NumberSequence, StringComparer.OrdinalIgnoreCase);
        foreach (var definition in AdministrationNumberSequenceCatalog.Definitions)
        {
            if (byKey.TryGetValue(definition.Key, out var row))
            {
                if (row.IsDeleted)
                    throw new InvalidOperationException($"Number sequence {definition.Key} is deleted; no code can be allocated for it.");

                // Converge only what allocation requires, so an operator's own format survives a rerun.
                // A sequence left unusable can never allocate: consumers select on InUse != 0, which in
                // SQL excludes NULL, and the service refuses a sequence whose range cannot produce a value.
                if (row.InUse == null) row.InUse = 1;
                if (row.Lowest == null) row.Lowest = AdministrationNumberSequenceCatalog.Lowest;
                if (row.Highest == null) row.Highest = AdministrationNumberSequenceCatalog.Highest;
                if (row.NextRec == null) row.NextRec = row.Lowest;

                // A cyclic sequence is reset once a calendar day. When the format carries a year or a
                // month, that reset re-issues a code already used inside the period, and the service
                // then refuses the document. Never lower the counter while repairing it.
                if (row.Cyclic == 1 && AdministrationNumberSequenceCatalog.CarriesPeriod(row.AnnotatedFormat))
                    row.Cyclic = 0;
                continue;
            }
            c.Db.Set<SysNumberSequence>().Add(new SysNumberSequence
            {
                NumberSequence = definition.Key,
                Txt = definition.Text,
                Format = definition.Format,
                AnnotatedFormat = definition.AnnotatedFormat,
                Lowest = AdministrationNumberSequenceCatalog.Lowest,
                Highest = AdministrationNumberSequenceCatalog.Highest,
                NextRec = AdministrationNumberSequenceCatalog.Lowest,
                Cyclic = 0,
                InUse = 1,
                Blocked = 0,
                Manual = 0,
                DataAreaId = c.Company,
                CreatedBy = c.Actor,
                LastModifiedBy = c.Actor,
                OwnerAccountId = c.Actor
            });
        }
        await c.Db.SaveChangesAsync(ct);
    }
}
