using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Identity.Persistence;

public interface IIdentityDataContext
{
    DbSet<AspNetUser> Users { get; }
    DbSet<AppPermission> AspNetPermissions { get; }
    DbSet<AppRolePermission> AspNetRolePermissions { get; }
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
