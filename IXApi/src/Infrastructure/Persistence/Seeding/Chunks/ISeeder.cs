using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Identity.Authentication;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Impersonation;
using Microsoft.AspNetCore.Identity;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks
{
    public interface ISeeder
    {
        Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct);
    }
}

