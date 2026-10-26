using System;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Core
{
    /// <summary>
    /// Contextual execution state provided to seeders.
    /// Standardizes tenant/company scope, environment, logging, and identity services.
    /// </summary>
    public class SeedingContext
    {
        public ApplicationDbContext Db { get; }
        public RoleManager<AspNetRole>? Roles { get; }
        public UserManager<AspNetUser>? Users { get; }
        public IConfiguration Configuration { get; }
        public IHostEnvironment? Environment { get; }
        public ILogger Logger { get; }
        public IServiceProvider? ServiceProvider { get; }
        public string CurrentCompany { get; }
        public IIdentityInsertService IdentityInsert { get; }

        public SeedingContext(
            ApplicationDbContext db,
            RoleManager<AspNetRole>? roles,
            UserManager<AspNetUser>? users,
            IConfiguration? configuration,
            IHostEnvironment? environment,
            ILogger? logger,
            IServiceProvider? serviceProvider,
            string currentCompany = "dat",
            IIdentityInsertService? identityInsert = null)
        {
            Db = db ?? throw new ArgumentNullException(nameof(db));
            Roles = roles;
            Users = users;
            Configuration = configuration ?? new ConfigurationBuilder().Build();
            Environment = environment;
            Logger = logger ?? NullLogger.Instance;
            ServiceProvider = serviceProvider;
            CurrentCompany = string.IsNullOrWhiteSpace(currentCompany) ? "dat" : currentCompany;
            IdentityInsert = identityInsert ?? new IdentityInsertHelper();
        }

        public static SeedingContext CreateDefault(
            ApplicationDbContext db,
            RoleManager<AspNetRole>? roles = null,
            UserManager<AspNetUser>? users = null,
            string company = "dat")
        {
            return new SeedingContext(
                db,
                roles,
                users,
                configuration: null,
                environment: null,
                logger: null,
                serviceProvider: null,
                currentCompany: company);
        }
    }
}
