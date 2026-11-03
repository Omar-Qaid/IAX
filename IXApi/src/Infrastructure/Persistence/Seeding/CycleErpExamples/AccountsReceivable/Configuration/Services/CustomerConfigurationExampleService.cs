using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsReceivable;

internal sealed class CustomerConfigurationExampleService
{
    internal Task SeedAsync(DbContext db, CancellationToken ct) => CustParametersSeeder.SeedAsync(db, ct);
}
