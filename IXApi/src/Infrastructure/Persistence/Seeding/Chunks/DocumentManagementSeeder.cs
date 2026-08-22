using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.DocumentManagement.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

public sealed class DocumentManagementSeeder : ISeeder
{
    public async Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct)
    {
        var existing = await db.DocuTypes.IgnoreQueryFilters().Select(x => x.TypeId).ToListAsync(ct);
        var existingIds = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var createdBy = (await users.FindByNameAsync("sys"))?.Id ?? "sys";
        var types = new[]
        {
            new DocuType { TypeId = "File", Name = "File", TypeGroup = 0, FilePlace = 0 },
            new DocuType { TypeId = "Note", Name = "Note", TypeGroup = 1, FilePlace = 0 },
            new DocuType { TypeId = "URL", Name = "URL", TypeGroup = 2, FilePlace = 0 },
            new DocuType { TypeId = "Image", Name = "Image", TypeGroup = 3, FilePlace = 0 },
        };

        var missing = types.Where(x => !existingIds.Contains(x.TypeId)).ToList();
        foreach (var type in missing)
        {
            type.DataAreaId = "dat";
            type.CreatedBy = createdBy;
            type.LastModifiedBy = createdBy;
            type.OwnerAccountId = createdBy;
        }

        if (missing.Count == 0) return;
        db.DocuTypes.AddRange(missing);
        await db.SaveChangesAsync(ct);
    }
}
