using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks
{
    /// <summary>
    /// Legacy composite adapter orchestrating the modularized Finance and Foundation seeders.
    /// Preserved for backward compatibility with existing tests and scripts.
    /// </summary>
    public class ErpSeeder : ISeeder
    {
        public async Task SeedAsync(
            ApplicationDbContext db,
            RoleManager<AspNetRole> roles,
            UserManager<AspNetUser> users,
            CancellationToken ct)
        {
            var context = SeedingContext.CreateDefault(db, roles, users);

            // Reconcile and seed in deterministic dependency order
            await new CompanyInfoSeeder().SeedAsync(context, ct);
            await new CustGroupSeeder().SeedAsync(context, ct);
            await new LogisticsAddressSeeder().SeedAsync(context, ct);
            await new FiscalCalendarSeeder().SeedAsync(context, ct);
            await new CurrencyAndExchangeRateSeeder().SeedAsync(context, ct);
            await new UnitOfMeasureSeeder().SeedAsync(context, ct);
            await new InventItemGroupSeeder().SeedAsync(context, ct);
            await new SalesDeliverySetupSeeder().SeedAsync(context, ct);
            await new SampleProductSeeder().SeedAsync(context, ct);
            await new SampleCustomerVendorSeeder().SeedAsync(context, ct);
        }
    }
}