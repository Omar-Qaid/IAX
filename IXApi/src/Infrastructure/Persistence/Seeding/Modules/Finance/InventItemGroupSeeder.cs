using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance
{
    public class InventItemGroupSeeder : BaseModuleSeeder
    {
        public override string Id => "Finance.Inventory.ItemGroups";
        public override int Version => 1;
        public override SeederCategory Category => SeederCategory.Reference;
        public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.Foundation.CompanyInfo" };
        public override SeederExecutionPolicy Policy => SeederExecutionPolicy.Repeatable;
        public override string Scope => "dat";

        public override Task SeedAsync(SeedingContext context, CancellationToken cancellationToken)
            => SeedDataAsync(context.Db, cancellationToken);

        public async Task SeedDataAsync(DbContext db, CancellationToken cancellationToken = default)
        {
            var createdBy = "sys";

            var seeds = new[] { (Code: "RAW", Name: "Raw materials"), (Code: "FIN", Name: "Finished goods"),
                (Code: "CONS", Name: "Consumables"), (Code: "SPR", Name: "Spare parts") };
            var existingCodes = await db.Set<InventItemGroup>().IgnoreQueryFilters()
                .Where(x => x.DataAreaId == "dat").Select(x => x.ItemGroupId).ToListAsync(cancellationToken);
            // Earlier seed versions inserted four anonymous rows. Give only those
            // untouched seed placeholders their intended codes, preserving record IDs.
            var placeholders = new Queue<InventItemGroup>(await db.Set<InventItemGroup>().IgnoreQueryFilters()
                .Where(x => x.DataAreaId == "dat" && x.ItemGroupId == "" && x.Name == ""
                    && x.CreatedBy == createdBy && x.OwnerAccountId == createdBy)
                .OrderBy(x => x.RecId).ToListAsync(cancellationToken));
            foreach (var seed in seeds.Where(x => !existingCodes.Contains(x.Code)))
            {
                if (placeholders.Count == 0) break;
                var group = placeholders.Dequeue();
                group.ItemGroupId = seed.Code;
                group.Name = seed.Name;
                existingCodes.Add(seed.Code);
            }
            var groupsToAdd = seeds.Where(x => !existingCodes.Contains(x.Code))
                .Select(x => new InventItemGroup { ItemGroupId = x.Code, Name = x.Name, DataAreaId = "dat",
                    IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy }).ToList();
            if (groupsToAdd.Count > 0)
            {
                await db.Set<InventItemGroup>().AddRangeAsync(groupsToAdd, cancellationToken);
            }
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
