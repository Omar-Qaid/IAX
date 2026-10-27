using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance
{
    public class CustGroupSeeder : BaseModuleSeeder
    {
        public override string Id => "Finance.AccountsReceivable.CustGroups";
        public override int Version => 1;
        public override SeederCategory Category => SeederCategory.Reference;
        public override IReadOnlyCollection<string> DependsOn => new[] { "Finance.Foundation.CompanyInfo" };
        public override SeederExecutionPolicy Policy => SeederExecutionPolicy.Repeatable;
        public override string Scope => "dat";

        public override async Task SeedAsync(SeedingContext context, CancellationToken cancellationToken)
        {
            var db = context.Db;
            var createdBy = "sys";

            var custGroupSeeds = new[]
            {
                new CustGroup { CustGroupId = "Consultant",   Name = "Consultant Customers", DataAreaId = "dat", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "Contractor",   Name = "Contractor Customers", DataAreaId = "dat", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "Government",   Name = "Government Customers", DataAreaId = "dat", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "IKKAff",       Name = "IKK Affiliates",      DataAreaId = "dat", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "SisComp",      Name = "Sister Companies",    DataAreaId = "dat", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "Traders",      Name = "Trading Customers",   DataAreaId = "dat", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "DOM",          Name = "Domestic",            DataAreaId = "dat", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "INT",          Name = "International",       DataAreaId = "dat", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "CUST-RTL",     Name = "Retail Customers",    DataAreaId = "dat", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "CUST-WHL",     Name = "Wholesale Customers", DataAreaId = "dat", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "CUST-VIP",     Name = "VIP Customers",       DataAreaId = "dat", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "CUST-GOV",     Name = "Government",          DataAreaId = "dat", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
            };

            var existingCustGroupCodes = await db.CustGroups.IgnoreQueryFilters().Where(x => x.DataAreaId == "dat").Select(g => g.CustGroupId).ToListAsync(cancellationToken);
            var custGroupsToAdd = custGroupSeeds.Where(g => !existingCustGroupCodes.Contains(g.CustGroupId)).ToList();

            if (custGroupsToAdd.Any())
            {
                await db.CustGroups.AddRangeAsync(custGroupsToAdd, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
