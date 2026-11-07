using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Interceptors;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Services;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IXApi.Tests;

public sealed class OrderToCashAuditTests
{
    [Fact]
    public async Task Customer_correction_audit_records_actor_source_time_and_excludes_rejected_changes()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var actor = new AuditActor();
        var options = new DbContextOptionsBuilder<AuditContext>().UseSqlite(connection)
            .AddInterceptors(new AuditInterceptor(actor, new SysAuditService())).Options;
        await using var db = new AuditContext(options);
        await db.Database.EnsureCreatedAsync();

        var customer = new CustTable { AccountNum = "C-01", Memo = "Original", DataAreaId = "dat" };
        db.AddRange(customer, new CustTable { AccountNum = "C-02", DataAreaId = "dat" });
        var start = DateTime.UtcNow;
        await db.SaveChangesAsync();
        var customerId = customer.RecId;
        var recordId = customerId.ToString();
        var created = await db.Set<SysAuditLog>().SingleAsync(x => x.TableName == "CustTable"
            && x.RecordId == recordId && x.ColumnName == nameof(CustTable.Memo));
        Assert.Equal("Insert", created.Operation);
        Assert.Equal("creator", created.ChangedBy);
        Assert.Equal("Original", created.NewValue);
        Assert.InRange(created.ChangedOn, start, DateTime.UtcNow);

        actor.Id = "corrector";
        customer.AccountNum = "C-02";
        customer.Memo = "Rejected";
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();
        var restored = await db.Set<CustTable>().SingleAsync(x => x.RecId == customerId);
        Assert.Equal("C-01", restored.AccountNum);
        Assert.Equal("Original", restored.Memo);

        start = DateTime.UtcNow;
        restored.Memo = "Corrected";
        await db.SaveChangesAsync();
        var updates = await db.Set<SysAuditLog>().Where(x => x.TableName == "CustTable"
            && x.RecordId == recordId && x.Operation == "Update").ToListAsync();
        var correction = Assert.Single(updates);
        Assert.Equal(nameof(CustTable.Memo), correction.ColumnName);
        Assert.Equal("corrector", correction.ChangedBy);
        Assert.Equal("Original", correction.OldValue);
        Assert.Equal("Corrected", correction.NewValue);
        Assert.InRange(correction.ChangedOn, start, DateTime.UtcNow);
    }

    private sealed class AuditActor : ICurrentUserService
    {
        public string Id { get; set; } = "creator";
        public string GetCurrentUserId() => Id;
        public string GetOwnerAccountId() => Id;
        public string GetDataAreaId() => "dat";
    }

    private sealed class AuditContext(DbContextOptions<AuditContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            var customer = builder.Entity<CustTable>();
            customer.HasKey(x => x.RecId);
            customer.Ignore(x => x.RowVersion);
            foreach (var property in typeof(CustTable).GetProperties().Where(x => !x.PropertyType.IsValueType
                && x.PropertyType != typeof(string) && x.PropertyType != typeof(byte[])))
                customer.Ignore(property.Name);
            customer.HasIndex(x => new { x.DataAreaId, x.AccountNum }).IsUnique();
            builder.Entity<SysAuditLog>();
            foreach (var type in builder.Model.GetEntityTypes().Select(x => x.ClrType)
                .Where(x => x != typeof(CustTable) && x != typeof(SysAuditLog)).ToArray())
                builder.Ignore(type);
        }
    }
}
