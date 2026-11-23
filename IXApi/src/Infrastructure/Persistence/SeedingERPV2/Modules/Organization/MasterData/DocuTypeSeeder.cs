using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Organization.DocumentManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Organization;

/// <summary>
/// The document types an attachment can be filed under. DocuType carries a DataAreaId but its
/// mapping declares an alternate key on TypeId alone, so the identifier is unique across the whole
/// installation rather than per company: the first company to seed owns the row and every later
/// company reuses it. That is why this seeder reads without a company filter and never calls
/// <see cref="SeedContext.EnsureAsync"/>, which would propose a second row per company and break the
/// alternate key.
/// </summary>
public sealed class DocuTypeSeeder : IDataSeeder
{
    public string Id => "ERPV2.DocuTypeSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CompanyInfoSeeder"];

    /// <summary>TypeId, name and the type group the document service resolves the attachment with.</summary>
    private static readonly (string TypeId, string Name, int TypeGroup)[] Types =
    [
        ("File", "File", 0),
        ("Note", "Note", 1),
        ("URL", "URL", 2),
        ("Image", "Image", 3)
    ];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var existing = await c.Db.Set<DocuType>().IgnoreQueryFilters().ToListAsync(ct);
        foreach (var (typeId, name, group) in Types)
        {
            var row = existing.SingleOrDefault(x => string.Equals(x.TypeId, typeId, StringComparison.Ordinal));
            if (row != null)
            {
                if (row.IsDeleted)
                    throw new InvalidOperationException($"Document type {typeId} is deleted; the document service cannot file an attachment under it.");
                if (row.TypeGroup != group)
                    throw new InvalidOperationException($"Conflicting DocuType.TypeGroup for {typeId}; existing data was preserved.");
                continue;
            }
            c.Db.Set<DocuType>().Add(new DocuType
            {
                TypeId = typeId,
                Name = name,
                TypeGroup = group,
                FilePlace = 0,
                DataAreaId = c.Company,
                CreatedBy = c.Actor,
                LastModifiedBy = c.Actor,
                OwnerAccountId = c.Actor
            });
        }
        await c.Db.SaveChangesAsync(ct);
    }
}
