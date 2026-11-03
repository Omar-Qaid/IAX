using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance
{
    public class SampleCustomerVendorSeeder : BaseModuleSeeder
    {
        public override string Id => "Finance.AccountsReceivable.DemoCustVend";
        public override int Version => 1;
        public override SeederCategory Category => SeederCategory.Demo;
        public override IReadOnlyCollection<string> DependsOn => new[]
        {
            "Finance.AccountsReceivable.CustGroups",
            "Finance.Foundation.Logistics", "Finance.GeneralLedger.Currencies", "Finance.GeneralLedger.Tax"
        };
        public override SeederExecutionPolicy Policy => SeederExecutionPolicy.Repeatable;
        public override string Scope => "dat";

        public override async Task SeedAsync(SeedingContext context, CancellationToken cancellationToken)
        {
            await new IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsReceivable.CustomerMasterDataExampleService().SeedAsync(context.Db, cancellationToken);
        }
    }
}
