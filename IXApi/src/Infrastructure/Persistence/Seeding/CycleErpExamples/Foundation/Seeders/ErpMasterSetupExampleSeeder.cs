using System.Linq.Expressions;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Foundation;

/// <summary>Complete missing setup for the existing example company; existing values always win.</summary>
public sealed partial class ErpMasterSetupExampleSeeder : BaseModuleSeeder
{
    public override string Id => "ERP.MasterSetupExamples";
    public override SeederCategory Category => SeederCategory.Demo;
    public override string Scope => "dat";
    public override IReadOnlyCollection<string> DependsOn => new[] {
        "Finance.OperationalExamples", "Finance.Foundation.AccountStructures", "Finance.Foundation.HcmExamples" };

    public override async Task SeedAsync(SeedingContext context, CancellationToken ct)
    {
        await SeedDataAsync(context.Db, ct);
    }

    public Task SeedDataAsync(DbContext db, CancellationToken ct = default) =>
        new ErpMasterSetupExampleService().SeedAsync(db, ct);
}
