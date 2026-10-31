using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

/// <summary>
/// The Department financial dimension and its values, as the core general ledger cycle defines
/// them (section 4, step 4: Department with ADMIN, SALES and SERVICE).
///
/// No financial dimension existed in this database before: the dimension tables had no EF
/// configuration and so were never created, and the legacy Chunks/DimensionSeeder is an empty,
/// unregistered ISeeder. This is a reference seeder rather than a demo one, because account
/// structures and posting validation are built on it.
/// </summary>
public sealed class FinancialDimensionSeeder : BaseModuleSeeder
{
    public override string Id => "Finance.Foundation.FinancialDimensions";
    public override int Version => 1;
    public override SeederCategory Category => SeederCategory.Reference;
    public override string Scope => "dat";
    public override SeederExecutionPolicy Policy => SeederExecutionPolicy.Repeatable;
    public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.Foundation.CompanyInfo" };

    private const string Area = "dat";
    private const string CreatedBy = "sys";

    public override Task SeedAsync(SeedingContext context, CancellationToken ct) => SeedDataAsync(context.Db, ct);

    public async Task SeedDataAsync(DbContext db, CancellationToken ct = default)
    {
        var department = await db.Set<DimensionAttribute>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.DataAreaId == Area && x.Name == "Department", ct);
        if (department == null)
        {
            department = new DimensionAttribute
            {
                DataAreaId = Area, Name = "Department", Type = DimensionAttributeType.Custom,
                HashKey = Guid.NewGuid(), IsActive = true, CreatedBy = CreatedBy, OwnerAccountId = CreatedBy
            };
            await db.Set<DimensionAttribute>().AddAsync(department, ct);
            await db.SaveChangesAsync(ct);
        }

        foreach (var value in new[] { "ADMIN", "SALES", "SERVICE" })
        {
            if (await db.Set<DimensionAttributeValue>().IgnoreQueryFilters()
                    .AnyAsync(x => x.DataAreaId == Area && x.DimensionAttribute == department.RecId
                        && x.DisplayValue == value, ct))
                continue;
            await db.Set<DimensionAttributeValue>().AddAsync(new DimensionAttributeValue
            {
                DataAreaId = Area, DimensionAttribute = department.RecId, DisplayValue = value,
                HashKey = Guid.NewGuid(), IsActive = true, CreatedBy = CreatedBy, OwnerAccountId = CreatedBy
            }, ct);
        }
        await db.SaveChangesAsync(ct);
    }
}
