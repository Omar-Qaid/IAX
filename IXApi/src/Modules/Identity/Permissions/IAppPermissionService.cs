using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IAX.IXApi.Modules.Identity.Permissions
{
    public interface IAppPermissionService
    {
        Task<List<AppPermission>> GetAllAsync(CancellationToken ct = default);
        Task<List<AppPermission>> GetByRoleAsync(string roleId, CancellationToken ct = default);
        Task<List<string>> GetPermissionKeysByUserAsync(string userId, CancellationToken ct = default);
        Task AssignToRoleAsync(string roleId, IEnumerable<int> permissionIds, CancellationToken ct = default);
        Task RemoveFromRoleAsync(string roleId, IEnumerable<int> permissionIds, CancellationToken ct = default);
        Task SetRolePermissionsAsync(string roleId, IEnumerable<int> permissionIds, CancellationToken ct = default);
    }
}
