using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Communication.Notifications.Configuration
{
    public class SysNotificationConfiguration : IEntityTypeConfiguration<SysNotification>
    {
        public void Configure(EntityTypeBuilder<SysNotification> builder)
        {
            builder.ToTable("SysNotifications");

            builder.HasIndex(e => e.Category);
            builder.HasIndex(e => e.Status);
            builder.HasIndex(e => e.Priority);
            builder.HasIndex(e => e.Channel);
            builder.HasIndex(e => new { e.EntityType, e.EntityId });
            builder.HasIndex(e => e.CreatedAt);
            builder.HasIndex(e => e.TenantId);

            builder.HasMany(e => e.Recipients)
                   .WithOne(r => r.Notification)
                   .HasForeignKey(r => r.NotificationId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
