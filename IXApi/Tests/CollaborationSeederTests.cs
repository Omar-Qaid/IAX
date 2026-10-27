using IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;
using IAX.IXApi.Modules.Administration.Settings;
using IAX.IXApi.Modules.Communication.Chat.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.Announcements;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IXApi.Tests;

public sealed class CollaborationSeederTests
{
    [Fact]
    public async Task Examples_recover_missing_children_preserve_customizations_and_never_queue_delivery()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<Fixture>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.Add(new AspNetUser { Id = "test-system-user", UserName = "sys" });
        await db.SaveChangesAsync();
        var seed = new CollaborationExampleSeeder();
        await seed.SeedExamplesAsync(db);
        var announcement = await db.Set<Announcement>().FirstAsync();
        announcement.Name = "Customized title";
        var preference = await db.Set<SysNotificationPreference>().SingleAsync();
        preference.EnableInApp = false;
        db.Remove(await db.Set<SysNotificationRecipient>().FirstAsync());
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await seed.SeedExamplesAsync(db);
        Assert.Equal(3, await db.Set<Announcement>().CountAsync());
        Assert.Equal(3, await db.Set<SysNotification>().CountAsync());
        Assert.Equal(3, await db.Set<SysNotificationRecipient>().CountAsync());
        Assert.Equal(3, await db.Set<SysNotificationAuditLog>().CountAsync());
        Assert.Equal(2, await db.Set<SysChatMessage>().CountAsync());
        Assert.Equal(1, await db.Set<SysChatReadState>().CountAsync());
        Assert.Equal("Customized title", (await db.Set<Announcement>().FindAsync(announcement.RecId))!.Name);
        Assert.False((await db.Set<SysNotificationPreference>().SingleAsync()).EnableInApp);
        Assert.All(await db.Set<SysScheduledNotification>().ToListAsync(), x => Assert.Equal(SysScheduledJobStatus.Cancelled, x.Status));
        Assert.All(await db.Set<SysNotification>().ToListAsync(), x => Assert.Equal(SysNotificationChannel.InApp, x.Channel));
        Assert.Contains(await db.Set<SysNotificationRecipient>().ToListAsync(), x => x.IsRead);
        Assert.Contains(await db.Set<SysNotificationRecipient>().ToListAsync(), x => !x.IsRead);
    }

    private sealed class Fixture(DbContextOptions<Fixture> options) : DbContext(options)
    {
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries().Where(x => x.State == EntityState.Added && x.Metadata.FindProperty("RowVersion") != null))
                entry.Property("RowVersion").CurrentValue = Array.Empty<byte>();
            return base.SaveChangesAsync(cancellationToken);
        }
        protected override void OnModelCreating(ModelBuilder b)
        {
            var types = new[] { typeof(AspNetUser), typeof(Announcement), typeof(SysNotification), typeof(SysNotificationRecipient),
                typeof(SysNotificationAuditLog), typeof(SysNotificationPreference), typeof(SysScheduledNotification),
                typeof(SysUserSettings), typeof(SysChatMessage), typeof(SysChatReadState) };
            foreach (var type in types) b.Entity(type);
            foreach (var type in b.Model.GetEntityTypes().Where(x => !types.Contains(x.ClrType)).Select(x => x.ClrType).ToList()) b.Ignore(type);
            foreach (var type in types.Where(t => t != typeof(AspNetUser)))
            {
                b.Entity(type).HasKey("RecId");
                if (b.Model.FindEntityType(type)!.FindProperty("RowVersion") != null)
                    b.Entity(type).Property<byte[]>("RowVersion").IsConcurrencyToken(false).ValueGeneratedNever();
            }
            b.Entity<SysNotificationRecipient>().HasIndex(x => new { x.NotificationId, x.UserId }).IsUnique();
            b.Entity<Announcement>().HasIndex(x => new { x.DataAreaId, x.Code }).IsUnique();
        }
    }
}
