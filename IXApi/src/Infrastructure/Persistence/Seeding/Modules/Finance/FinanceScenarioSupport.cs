using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;

internal static class FinanceScenarioSupport
{
    /// <summary>
    /// Preserve company filters and independent service transactions for resumable posting examples.
    /// Their durable business keys protect reruns; these writes cannot share the seed-history transaction.
    /// </summary>
    internal static ApplicationDbContext CreatePostingDb(SeedingContext context, ICompanyExecutionContext company)
    {
        return new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(context.Db.Database.GetConnectionString()).Options, companyContext: company);
    }
}
