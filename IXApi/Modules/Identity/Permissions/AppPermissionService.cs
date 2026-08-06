using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence;
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
    public class AppPermissionService : IAppPermissionService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMemoryCache _cache;

        // Permission keys are cached per-user for 60 seconds.
        // This prevents a DB round-trip on every API request while ensuring
        // that permission changes propagate within 1 minute.
        private static readonly TimeSpan PermissionCacheTtl = TimeSpan.FromSeconds(60);
        private static string UserPermissionCacheKey(string userId) => $"user_perms:{userId}";

        public AppPermissionService(ApplicationDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public Task<List<AppPermission>> GetAllAsync(CancellationToken ct = default)
            => _db.AspNetPermissions
                .OrderBy(p => p.Module).ThenBy(p => p.Action)
                .ToListAsync(ct);

        public Task<List<AppPermission>> GetByRoleAsync(string roleId, CancellationToken ct = default)
            => _db.AspNetRolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission)
                .OrderBy(p => p.Module).ThenBy(p => p.Action)
                .ToListAsync(ct);

        public async Task<List<string>> GetPermissionKeysByUserAsync(string userId, CancellationToken ct = default)
        {
            var cacheKey = UserPermissionCacheKey(userId);

            if (_cache.TryGetValue(cacheKey, out List<string>? cached) && cached is not null)
                return cached;

            var roleIds = await _db.Set<AspNetUserRole>()
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync(ct);

            var keys = await _db.AspNetRolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission.Module + "." + rp.Permission.Resource + "." + rp.Permission.Action)
                .Distinct()
                .ToListAsync(ct);

            _cache.Set(cacheKey, keys, PermissionCacheTtl);

            // Keep the role → users index up-to-date so InvalidateRolePermissionCache
            // can evict all affected users' caches the moment a role changes.
            foreach (var rid in roleIds)
            {
                var indexKey = $"role_users_index:{rid}";
                var existing = _cache.TryGetValue(indexKey, out List<string>? prev) ? prev ?? [] : [];
                if (!existing.Contains(userId))
                {
                    _cache.Set(indexKey, new List<string>(existing) { userId }, PermissionCacheTtl);
                }
            }

            return keys;
        }

        public async Task AssignToRoleAsync(string roleId, IEnumerable<int> permissionIds, CancellationToken ct = default)
        {
            var existing = await _db.AspNetRolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync(ct);

            var toAdd = permissionIds.Except(existing)
                .Select(pid => new AppRolePermission { RoleId = roleId, PermissionId = pid });

            await _db.AspNetRolePermissions.AddRangeAsync(toAdd, ct);
            await _db.SaveChangesAsync(ct);

            InvalidateRolePermissionCache(roleId);
        }

        public async Task RemoveFromRoleAsync(string roleId, IEnumerable<int> permissionIds, CancellationToken ct = default)
        {
            var toRemove = await _db.AspNetRolePermissions
                .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId))
                .ToListAsync(ct);

            _db.AspNetRolePermissions.RemoveRange(toRemove);
            await _db.SaveChangesAsync(ct);

            InvalidateRolePermissionCache(roleId);
        }

        public async Task SetRolePermissionsAsync(string roleId, IEnumerable<int> permissionIds, CancellationToken ct = default)
        {
            var existing = await _db.AspNetRolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync(ct);

            _db.AspNetRolePermissions.RemoveRange(existing);

            var toAdd = permissionIds
                .Select(pid => new AppRolePermission { RoleId = roleId, PermissionId = pid });

            await _db.AspNetRolePermissions.AddRangeAsync(toAdd, ct);
            await _db.SaveChangesAsync(ct);

            InvalidateRolePermissionCache(roleId);
        }

        // When a role's permissions change, we cannot know which users have that role
        // without another DB query. We use a tag-based approach: remove all user permission
        // cache entries by scanning for keys matching the pattern. Since IMemoryCache
        // doesn't support tag-based eviction natively, we track affected users via a
        // role → users index stored in the cache itself.
        private void InvalidateRolePermissionCache(string roleId)
        {
            var indexKey = $"role_users_index:{roleId}";
            if (_cache.TryGetValue(indexKey, out List<string>? userIds) && userIds is not null)
            {
                foreach (var uid in userIds)
                    _cache.Remove(UserPermissionCacheKey(uid));
            }
            _cache.Remove(indexKey);
        }
    }
}

