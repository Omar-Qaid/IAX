using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks
{
    /// <summary>
    /// Seeds Vendor Groups (VendGroup) and Vendors (VendTable) for Accounts Payable sub-ledger.
    /// </summary>
    public  class VendSeeder : IAX.IXApi.Infrastructure.Persistence.Seeding.Core.BaseModuleSeeder
{
    public override string Id => "Finance.AccountsPayable.Vendors";
    public override int Version => 1;
    public override IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeederCategory Category => IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeederCategory.Reference;
    public override System.Collections.Generic.IReadOnlyCollection<string> DependsOn => new[] { "Finance.AccountsReceivable.DeliverySetup", "Finance.GeneralLedger.Currencies", "Finance.AccountsReceivable.PaymModes" };
    public override IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeederExecutionPolicy Policy => IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeederExecutionPolicy.Repeatable;
    public override string Scope => "dat";

    public override System.Threading.Tasks.Task SeedAsync(IAX.IXApi.Infrastructure.Persistence.Seeding.Core.SeedingContext context, System.Threading.CancellationToken cancellationToken)
    {
        return SeedAsync(context.Db, context.Roles!, context.Users!, cancellationToken);
    }
        public override Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct) =>
            new IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable.VendorMasterDataExampleService().SeedAsync(db, users, ct);
    }
}
