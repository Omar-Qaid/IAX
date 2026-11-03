using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IXApi.Tests;

public sealed class ErpCycleSeedIntegrityTests
{
    [Fact]
    public async Task Retry_after_confirmation_commits_does_not_create_another_snapshot()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new IntegrityDb(new DbContextOptionsBuilder<IntegrityDb>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var (order, line) = Example();
        var calls = 0;
        async Task<PurchResult> Confirm(CancellationToken ct)
        {
            calls++;
            await AddSnapshotAsync(db, order, line, ct);
            return new(200, null);
        }
        var service = new PurchaseCycleExampleService();
        await service.EnsureConfirmationAsync(db, order, line, Confirm);
        // Receipt can fail after the real confirmation service has already committed its snapshot.
        db.ChangeTracker.Clear();
        await service.EnsureConfirmationAsync(db, order, line, Confirm);
        Assert.Equal(1, calls);
        Assert.Single(await db.Set<VendPurchOrderJour>().ToListAsync());
        Assert.Single(await db.Set<VendPurchOrderTrans>().ToListAsync());
    }

    [Theory]
    [InlineData("header-amount")]
    [InlineData("header-deleted")]
    [InlineData("line-quantity")]
    [InlineData("line-dimension")]
    [InlineData("line-deleted")]
    public async Task Conflicting_confirmation_is_preserved_and_never_replaced(string conflict)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new IntegrityDb(new DbContextOptionsBuilder<IntegrityDb>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var (order, line) = Example();
        await AddSnapshotAsync(db, order, line);
        var header = await db.Set<VendPurchOrderJour>().SingleAsync();
        var snapshot = await db.Set<VendPurchOrderTrans>().SingleAsync();
        switch (conflict)
        {
            case "header-amount": header.Amount = 999m; break;
            case "header-deleted": header.IsDeleted = true; break;
            case "line-quantity": snapshot.Qty = 99m; break;
            case "line-dimension": snapshot.InventDimId = "CUSTOM"; break;
            case "line-deleted": snapshot.IsDeleted = true; break;
        }
        await db.SaveChangesAsync();
        var beforeHeader = db.Entry(header).CurrentValues.Clone();
        var beforeLine = db.Entry(snapshot).CurrentValues.Clone();
        var calls = 0;
        await Assert.ThrowsAsync<InvalidOperationException>(() => new PurchaseCycleExampleService()
            .EnsureConfirmationAsync(db, order, line, _ => { calls++; return Task.FromResult(new PurchResult(200, null)); }));
        Assert.Equal(0, calls);
        Assert.Equal(beforeHeader.Properties.Select(x => beforeHeader[x]), db.Entry(header).CurrentValues.Properties.Select(x => db.Entry(header).CurrentValues[x]));
        Assert.Equal(beforeLine.Properties.Select(x => beforeLine[x]), db.Entry(snapshot).CurrentValues.Properties.Select(x => db.Entry(snapshot).CurrentValues[x]));
    }

    [Fact]
    public async Task Another_companys_confirmation_does_not_suppress_the_DAT_example()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new IntegrityDb(new DbContextOptionsBuilder<IntegrityDb>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var (order, line) = Example();
        db.Add(new VendPurchOrderJour { DataAreaId = "other", PurchId = order.PurchId,
            PurchOrderDocNum = "CONF1", Amount = 500m });
        await db.SaveChangesAsync();
        var calls = 0;
        await new PurchaseCycleExampleService().EnsureConfirmationAsync(db, order, line, async ct =>
        {
            calls++;
            await AddSnapshotAsync(db, order, line, ct);
            return new(200, null);
        });
        Assert.Equal(1, calls);
        Assert.Equal(2, await db.Set<VendPurchOrderJour>().CountAsync());
        Assert.Equal(500m, (await db.Set<VendPurchOrderJour>().SingleAsync(x => x.DataAreaId == "other")).Amount);
    }

    [Fact]
    public async Task Missing_confirmation_after_receipt_is_reported_without_rewriting_history()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new IntegrityDb(new DbContextOptionsBuilder<IntegrityDb>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var (order, line) = Example();
        await Assert.ThrowsAsync<InvalidOperationException>(() => new PurchaseCycleExampleService()
            .EnsureConfirmationAsync(db, order, line, _ => throw new Xunit.Sdk.XunitException("Must not reconfirm received history"), allowCreate: false));
        Assert.Empty(await db.Set<VendPurchOrderJour>().ToListAsync());
    }

    [Theory]
    [InlineData("site")]
    [InlineData("warehouse")]
    [InlineData("deleted")]
    [InlineData("company")]
    public async Task Storage_conflicts_are_rejected_without_changing_dimension_rows(string conflict)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new IntegrityDb(new DbContextOptionsBuilder<IntegrityDb>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var dimension = new InventDim { DataAreaId = "dat", InventDimId = "D1", InventSiteId = "JED", InventLocationId = "JED-1" };
        switch (conflict)
        {
            case "site": dimension.InventSiteId = "OTHER"; break;
            case "warehouse": dimension.InventLocationId = "OTHER"; break;
            case "deleted": dimension.IsDeleted = true; break;
            case "company": dimension.DataAreaId = "other"; break;
        }
        db.Add(dimension);
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => ErpCycleStorageIntegrity.RequireAsync(db, "D1"));
        Assert.Equal(EntityState.Unchanged, db.Entry(dimension).State);
    }

    [Fact]
    public async Task Storage_accepts_expected_company_even_when_another_company_reuses_the_dimension_key()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new IntegrityDb(new DbContextOptionsBuilder<IntegrityDb>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.Add(new InventDim { DataAreaId = "dat", InventDimId = "D1", InventSiteId = "JED", InventLocationId = "JED-1" });
        db.Add(new InventDim { DataAreaId = "other", InventDimId = "D1", InventSiteId = "OTHER", InventLocationId = "OTHER" });
        await db.SaveChangesAsync();
        await ErpCycleStorageIntegrity.RequireAsync(db, "D1");
        Assert.Equal(2, await db.Set<InventDim>().CountAsync());
    }

    private static (PurchTable, PurchLine) Example() => (
        new PurchTable { DataAreaId = "dat", PurchId = "P1", OrderAccount = "VEND-100", InvoiceAccount = "VEND-100", CurrencyCode = "SAR" },
        new PurchLine { DataAreaId = "dat", PurchId = "P1", ItemId = "ITEM-001", PurchQty = 12m, PurchPrice = 100m,
            PriceUnit = 1m, PurchUnit = "PCS", LineAmount = 1200m, InventDimId = "D1", InventTransId = "IT1" });

    private static async Task AddSnapshotAsync(DbContext db, PurchTable order, PurchLine line, CancellationToken ct = default)
    {
        db.Add(new VendPurchOrderJour { DataAreaId = order.DataAreaId, PurchId = order.PurchId,
            PurchOrderDocNum = "CONF1", OrderAccount = order.OrderAccount, InvoiceAccount = order.InvoiceAccount,
            CurrencyCode = order.CurrencyCode, Qty = line.PurchQty, Amount = line.LineAmount });
        db.Add(new VendPurchOrderTrans { DataAreaId = line.DataAreaId, PurchId = line.PurchId,
            PurchOrderDocNum = "CONF1", ItemId = line.ItemId, Qty = line.PurchQty, PurchPrice = line.PurchPrice,
            PriceUnit = line.PriceUnit, PurchUnit = line.PurchUnit, LineAmount = line.LineAmount,
            InventDimId = line.InventDimId, InventTransId = line.InventTransId });
        await db.SaveChangesAsync(ct);
    }

    private sealed class IntegrityDb(DbContextOptions<IntegrityDb> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder b)
        {
            foreach (var type in new[] { typeof(VendPurchOrderJour), typeof(VendPurchOrderTrans), typeof(InventDim) })
            {
                b.Entity(type).HasKey("RecId");
                b.Entity(type).Ignore("RowVersion");
            }
        }
    }
}
