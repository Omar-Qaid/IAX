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
        public override string Scope => "global";

        public override async Task SeedAsync(SeedingContext context, CancellationToken cancellationToken)
        {
            var db = context.Db;
            var createdBy = "sys";

            var groupCodes = new[] { "RAW", "FIN", "CONS", "SPR" };
            var existingGroupCount = await db.InventItemGroups.IgnoreQueryFilters().CountAsync(cancellationToken);

            if (existingGroupCount < groupCodes.Length)
            {
                var groupsToAdd = Enumerable.Range(0, groupCodes.Length - existingGroupCount)
                    .Select(_ => new InventItemGroup { IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy })
                    .ToList();
                await db.InventItemGroups.AddRangeAsync(groupsToAdd, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
