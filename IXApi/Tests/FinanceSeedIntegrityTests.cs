using IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IXApi.Tests;

public sealed class FinanceSeedIntegrityTests
{
    [Fact]
    public async Task Products_repair_partial_seeds_and_preserve_other_companies_and_customizations()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new SeedContext(new DbContextOptionsBuilder<SeedContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.Add(new InventItemGroup { ItemGroupId = "CONS", Name = "Other company", DataAreaId = "other" });
        var placeholder = new InventItemGroup { DataAreaId = "dat", CreatedBy = "sys", OwnerAccountId = "sys" };
        db.Add(placeholder);
        db.Add(new InventTable { ItemId = "ITM-1001", Name = "Other company item", DataAreaId = "other" });
        db.Add(new InventTable { ItemId = "ITM-1002", Name = "Custom paper", DataAreaId = "dat" });
        foreach (var symbol in new[] { "BTL", "CTN", "PCK", "BOX", "M", "TON", "PCS", "DOZ" })
            db.Add(new UnitOfMeasure { Symbol = symbol });
        await db.SaveChangesAsync();

        var groups = new InventItemGroupSeeder();
        var products = new SampleProductSeeder();
        await groups.SeedDataAsync(db);
        await products.SeedDataAsync(db);
        db.ChangeTracker.Clear();
        var customModule = await db.Set<InventTableModule>().FirstAsync(x => x.ItemId == "ITM-1002");
        customModule.Price = 99;
        var deletedModule = await db.Set<InventTableModule>().FirstAsync(x => x.ItemId == "ITM-1001");
        db.Remove(deletedModule);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await groups.SeedDataAsync(db);
        await products.SeedDataAsync(db);

        Assert.Equal(4, await db.Set<InventItemGroup>().CountAsync(x => x.DataAreaId == "dat"));
        Assert.Equal("RAW", (await db.Set<InventItemGroup>().FindAsync(placeholder.RecId))!.ItemGroupId);
        Assert.Equal(5, await db.Set<EcoResProduct>().CountAsync());
        Assert.Equal(5, await db.Set<EcoResDistinctProduct>().CountAsync());
        Assert.Equal(5, await db.Set<EcoResProductTranslation>().CountAsync());
        Assert.Equal(15, await db.Set<InventTableModule>().CountAsync());
        Assert.Equal(5, await db.Set<InventItemGroupItem>().CountAsync());
        Assert.Equal(4, await db.Set<UnitOfMeasureConversion>().CountAsync());
        Assert.Equal(0, (await db.Set<InventTable>().SingleAsync(x => x.DataAreaId == "other")).Product);
        Assert.Equal("Custom paper", (await db.Set<InventTable>().SingleAsync(x => x.ItemId == "ITM-1002")).Name);
        Assert.Equal(99m, (await db.Set<InventTableModule>().FindAsync(customModule.RecId))!.Price);
        var productIds = await db.Set<EcoResProduct>().Select(x => x.RecId).ToListAsync();
        Assert.All(await db.Set<InventTable>().Where(x => x.DataAreaId == "dat").ToListAsync(), x => Assert.Contains(x.Product, productIds));
        Assert.All(await db.Set<UnitOfMeasureConversion>().ToListAsync(), x => Assert.Contains(x.Product, productIds));
        Assert.All(await db.Set<InventTableModule>().Where(x => x.ItemId == "ITM-1003").ToListAsync(), x => Assert.Equal("M", x.UnitId));
    }

    private sealed class SeedContext(DbContextOptions<SeedContext> options) : DbContext(options)
    {
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries().Where(x => x.State == EntityState.Added))
                entry.Property("RowVersion").CurrentValue = Array.Empty<byte>();
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder b)
        {
            var allowed = new[] { typeof(InventItemGroup), typeof(InventTable), typeof(InventTableModule),
                typeof(InventItemGroupItem), typeof(UnitOfMeasure), typeof(UnitOfMeasureConversion),
                typeof(EcoResProduct), typeof(EcoResDistinctProduct), typeof(EcoResProductTranslation) };
            foreach (var type in allowed) b.Entity(type);
            foreach (var type in b.Model.GetEntityTypes().Where(x => !allowed.Contains(x.ClrType)).Select(x => x.ClrType).ToList())
                b.Ignore(type);
            foreach (var type in allowed)
            {
                b.Entity(type).HasKey("RecId");
                b.Entity(type).Property<byte[]>("RowVersion").IsConcurrencyToken(false)
                    .ValueGeneratedNever().HasDefaultValue(Array.Empty<byte>());
            }
            b.Entity<InventTable>().HasIndex(x => new { x.DataAreaId, x.ItemId }).IsUnique();
            b.Entity<InventItemGroup>().HasIndex(x => new { x.DataAreaId, x.ItemGroupId }).IsUnique();
            b.Entity<EcoResProduct>().HasIndex(x => x.DisplayProductNumber).IsUnique();
            b.Entity<InventTableModule>().HasIndex(x => new { x.DataAreaId, x.ItemId, x.ModuleType }).IsUnique();
            b.Entity<EcoResProductTranslation>().HasOne<EcoResProduct>().WithMany().HasForeignKey(x => x.Product);
            b.Entity<UnitOfMeasureConversion>().HasOne<EcoResProduct>().WithMany().HasForeignKey(x => x.Product);
        }
    }
}
