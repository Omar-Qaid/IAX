using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Core
{
    /// <summary>
    /// Base class providing sensible defaults and legacy ISeeder interoperability.
    /// </summary>
    public abstract class BaseModuleSeeder : IModuleSeeder, ISeeder
    {
        public abstract string Id { get; }
        public virtual int Version => 1;
        public abstract SeederCategory Category { get; }
        public virtual IReadOnlyCollection<string> DependsOn => Array.Empty<string>();
        public virtual SeederExecutionPolicy Policy => SeederExecutionPolicy.Repeatable;
        public virtual string Scope => "global";

        public virtual Task SeedAsync(SeedingContext context, CancellationToken cancellationToken)
        {
            return SeedAsync(context.Db, context.Roles!, context.Users!, cancellationToken);
        }

        public virtual Task SeedAsync(
            ApplicationDbContext db,
            RoleManager<AspNetRole> roles,
            UserManager<AspNetUser> users,
            CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
}
