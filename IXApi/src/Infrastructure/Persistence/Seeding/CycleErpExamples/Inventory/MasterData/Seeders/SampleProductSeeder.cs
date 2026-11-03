using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance
{
    public class SampleProductSeeder : BaseModuleSeeder
    {
        public override string Id => "Finance.Inventory.DemoProducts";
        public override int Version => 1;
        public override SeederCategory Category => SeederCategory.Demo;
        public override IReadOnlyCollection<string> DependsOn => new[]
        {
            "Finance.Inventory.ItemGroups",
            "Finance.Inventory.UnitOfMeasure",
            "Finance.GeneralLedger.Tax"
        };
        public override SeederExecutionPolicy Policy => SeederExecutionPolicy.Repeatable;
        public override string Scope => "dat";

        public override Task SeedAsync(SeedingContext context, CancellationToken cancellationToken)
            => SeedDataAsync(context.Db, cancellationToken);

        public Task SeedDataAsync(DbContext db, CancellationToken cancellationToken = default) =>
        new IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory.ProductExampleService().SeedDataAsync(db, cancellationToken);
    }
}
