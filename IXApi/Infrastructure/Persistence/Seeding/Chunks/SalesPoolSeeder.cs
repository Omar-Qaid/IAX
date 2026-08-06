using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks
{
    public class SalesPoolSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct)
        {
            var seeds = new[]
            {
                new SalesPool { SalesPoolId = "RETAIL", Name = "Retail Sales Pool", DataAreaId = "dat" },
                new SalesPool { SalesPoolId = "WHOLESALE", Name = "Wholesale Sales Pool", DataAreaId = "dat" },
                new SalesPool { SalesPoolId = "ONLINE", Name = "Online E-Commerce Channel", DataAreaId = "dat" },
                new SalesPool { SalesPoolId = "DIRECT", Name = "Direct Sales Force", DataAreaId = "dat" },
                new SalesPool { SalesPoolId = "EXPORT", Name = "Export Sales Pool", DataAreaId = "dat" },
                new SalesPool { SalesPoolId = "GOV", Name = "Government Contracts", DataAreaId = "dat" }
            };

            var existingIds = await db.SalesPools
                .IgnoreQueryFilters()
                .Select(p => p.SalesPoolId)
                .ToListAsync(ct);

            var toAdd = seeds.Where(s => !existingIds.Contains(s.SalesPoolId)).ToList();
            if (toAdd.Count > 0)
            {
                await db.SalesPools.AddRangeAsync(toAdd, ct);
                await db.SaveChangesAsync(ct);
            }
        }
    }
}

