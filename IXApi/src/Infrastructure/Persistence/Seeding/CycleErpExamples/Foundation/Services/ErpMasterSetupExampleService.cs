using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.HumanResources;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Foundation;

internal sealed class ErpMasterSetupExampleService
{
    internal async Task SeedAsync(DbContext db, CancellationToken ct)
    {
        await new LedgerMasterSetupExampleService().SeedAsync(db, ct);
        if (!await db.Set<VendLedgerAccounts>().IgnoreQueryFilters().AnyAsync(x =>
                x.DataAreaId == "dat" && x.PostingProfile == "DEMO" && !x.IsDeleted, ct))
            throw new InvalidOperationException("The example vendor posting profile DEMO is missing.");
        await VendParametersSeeder.SeedAsync(db, ct);
        await LedgerParametersSeeder.SeedAsync(db, ct);
        await VendPaymModeTableSeeder.SeedAsync(db, "ELECTRONIC", "Electronic vendor payment", ct);
        await VendPaymModeTableSeeder.SeedAsync(db, "CHECK", "Vendor check payment", ct);
        await HcmWorkerOrganizationAssignmentSeeder.SeedAsync(db, ct);
    }
}
