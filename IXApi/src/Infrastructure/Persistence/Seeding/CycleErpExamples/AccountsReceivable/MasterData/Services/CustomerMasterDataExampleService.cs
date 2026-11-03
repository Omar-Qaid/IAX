using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsReceivable;

internal sealed class CustomerMasterDataExampleService
{
    internal Task SeedAsync(DbContext db, CancellationToken ct) => CustTableSeeder.SeedAsync(db, ct);
}
