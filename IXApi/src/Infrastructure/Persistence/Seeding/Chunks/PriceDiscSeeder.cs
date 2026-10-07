using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

/// <summary>Seeds repeatable starter setup for sales price and discount journals.</summary>
public sealed class PriceDiscSeeder : ISeeder
{
    private const string DataAreaId = "dat";

    public async Task SeedAsync(
        ApplicationDbContext db,
        RoleManager<AspNetRole> roles,
        UserManager<AspNetUser> users,
        CancellationToken ct)
    {
        var sysUser = await users.FindByNameAsync("sys");
        var createdBy = sysUser?.Id ?? "sys";

        var journalNameSeeds = new[]
        {
            new PriceDiscAdmName { JournalName = "SALEPRICE", Name = "Sales prices", DefaultRelation = PriceType.PriceSales, DataAreaId = DataAreaId },
            new PriceDiscAdmName { JournalName = "LINEDISC", Name = "Sales line discounts", DefaultRelation = PriceType.LineDiscSales, DataAreaId = DataAreaId },
            new PriceDiscAdmName { JournalName = "TOTALDISC", Name = "Sales total discounts", DefaultRelation = PriceType.EndDiscSales, DataAreaId = DataAreaId },
        };
        var existingNames = await db.Set<PriceDiscAdmName>().IgnoreQueryFilters()
            .Where(x => x.DataAreaId == DataAreaId).Select(x => x.JournalName).ToListAsync(ct);
        var namesToAdd = journalNameSeeds.Where(x => !existingNames.Contains(x.JournalName)).ToList();
        if (namesToAdd.Count > 0)
        {
            foreach (var row in namesToAdd) SetAudit(row, createdBy);
            await db.Set<PriceDiscAdmName>().AddRangeAsync(namesToAdd, ct);
            await db.SaveChangesAsync(ct);
        }

        var groupSeeds = new[]
        {
            new PriceDiscGroup { GroupId = "CUSTDISC", Name = "Customer discount group", Module = ModuleInventCustVend.Cust, Type = PriceType.LineDiscSales, DataAreaId = DataAreaId },
            new PriceDiscGroup { GroupId = "ITEMDISC", Name = "Item discount group", Module = ModuleInventCustVend.Invent, Type = PriceType.LineDiscSales, DataAreaId = DataAreaId },
            new PriceDiscGroup { GroupId = "CUSTPRICE", Name = "Customer sales price group", Module = ModuleInventCustVend.Cust, Type = PriceType.PriceSales, DataAreaId = DataAreaId },
        };
        var existingGroups = await db.Set<PriceDiscGroup>().IgnoreQueryFilters()
            .Where(x => x.DataAreaId == DataAreaId)
            .Select(x => new { x.GroupId, x.Module, x.Type }).ToListAsync(ct);
        var groupsToAdd = groupSeeds.Where(seed => !existingGroups.Any(existing =>
            existing.GroupId == seed.GroupId && existing.Module == seed.Module && existing.Type == seed.Type)).ToList();
        if (groupsToAdd.Count > 0)
        {
            foreach (var row in groupsToAdd) SetAudit(row, createdBy);
            await db.Set<PriceDiscGroup>().AddRangeAsync(groupsToAdd, ct);
            await db.SaveChangesAsync(ct);
        }

        const string journalNum = "SEED-TA-001";
        var journalExists = await db.Set<PriceDiscAdmTable>().IgnoreQueryFilters()
            .AnyAsync(x => x.DataAreaId == DataAreaId && x.JournalNum == journalNum, ct);
        if (!journalExists)
        {
            var journal = new PriceDiscAdmTable
            {
                JournalNum = journalNum,
                JournalName = "SALEPRICE",
                Name = "Sample sales price agreement journal",
                DefaultRelation = PriceType.PriceSales,
                Posted = NoYes.No,
                DataAreaId = DataAreaId,
            };
            SetAudit(journal, createdBy);
            db.Set<PriceDiscAdmTable>().Add(journal);
            await db.SaveChangesAsync(ct);
        }
    }

    private static void SetAudit(IAX.IXApi.Shared.Domain.Entities.AuditableEntity entity, string userId)
    {
        entity.CreatedBy = userId;
        entity.OwnerAccountId = userId;
        entity.CreatedAt = DateTime.UtcNow;
    }
}
