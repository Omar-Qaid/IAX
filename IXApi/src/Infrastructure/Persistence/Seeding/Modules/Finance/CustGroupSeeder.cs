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
        public override string Scope => "global";

        public override async Task SeedAsync(SeedingContext context, CancellationToken cancellationToken)
        {
            var db = context.Db;
            var createdBy = "sys";

            var custGroupSeeds = new[]
            {
                new CustGroup { CustGroupId = "Consultant",   Name = "Consultant Customers", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "Contractor",   Name = "Contractor Customers", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "Government",   Name = "Government Customers", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "IKKAff",       Name = "IKK Affiliates",      IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "SisComp",      Name = "Sister Companies",    IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "Traders",      Name = "Trading Customers",   IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "DOM",          Name = "Domestic",            IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "INT",          Name = "International",       IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "CUST-RTL",     Name = "Retail Customers",    IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "CUST-WHL",     Name = "Wholesale Customers", IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "CUST-VIP",     Name = "VIP Customers",       IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
                new CustGroup { CustGroupId = "CUST-GOV",     Name = "Government",          IsActive = true, CreatedBy = createdBy, OwnerAccountId = createdBy },
            };

            var existingCustGroupCodes = await db.CustGroups.IgnoreQueryFilters().Select(g => g.CustGroupId).ToListAsync(cancellationToken);
            var custGroupsToAdd = custGroupSeeds.Where(g => !existingCustGroupCodes.Contains(g.CustGroupId)).ToList();

            if (custGroupsToAdd.Any())
            {
                await db.CustGroups.AddRangeAsync(custGroupsToAdd, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
