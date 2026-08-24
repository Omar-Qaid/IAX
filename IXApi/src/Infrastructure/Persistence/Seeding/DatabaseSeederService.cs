using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding
{
    public class DatabaseSeederService : IDatabaseSeederService
    {
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<AspNetRole> _roles;
        private readonly UserManager<AspNetUser> _users;

        public DatabaseSeederService(
            ApplicationDbContext db,
            RoleManager<AspNetRole> roles,
            UserManager<AspNetUser> users)
        {
            _db = db;
            _roles = roles;
            _users = users;
        }

        public async Task SeedAsync(CancellationToken ct = default)
        {
            var seeders = new List<ISeeder>
            {
                new IdentitySeeder(),
                new DocumentManagementSeeder(),
                new OrganizationSeeder(),
                new WorkflowRequestTrackingSeeder(),
                new ErpSeeder(),
                new VendSeeder(),
                new TaxSeeder(),
                new PostingProfileSeeder(),
                new DimensionSeeder(),
                new NumberSequenceSeeder(),
                new SettingsSeeder(),
                new CustLedgerSeeder(),
                new MainAccountSeeder(),
                new CustPaymModeSeeder(),
                new SalesPoolSeeder(),
                new MarkupTableSeeder(),
            };

            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync(_db, _roles, _users, ct);
            }
        }

        /// <summary>
        /// Utility to run a block of code only once based on a persistent log.
        /// </summary>
        public async Task RunOnceAsync(string seedName, Func<Task> action, CancellationToken ct)
        {
            var already = await _db.SysDataSeedLogs.AnyAsync(s => s.TableName == seedName, ct);
            if (already) return;

            await action();

            _db.SysDataSeedLogs.Add(new SysDataSeedLog { TableName = seedName });
            await _db.SaveChangesAsync(ct);
        }
    }
}



