using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Identity.Authentication;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Roles;
 
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;


namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks
{
    /// <summary>Seeds the default ledger posting profiles (one main account per posting type).</summary>
    public class PostingProfileSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct)
        {
      

            await db.SaveChangesAsync(ct);
        }
    }
}


