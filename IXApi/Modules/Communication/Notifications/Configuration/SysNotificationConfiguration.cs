using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Communication.Notifications.Configuration
{
    /// <summary>
    /// EF Core configuration for the notification module entities.
    /// Configures indexes, relationships, and constraints.
    /// </summary>
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

    public class SysNotificationRecipientConfiguration : IEntityTypeConfiguration<SysNotificationRecipient>
    {
        public void Configure(EntityTypeBuilder<SysNotificationRecipient> builder)
        {
            builder.ToTable("SysNotificationRecipients");

            // Composite index for fast lookups by user + read status
            builder.HasIndex(e => new { e.UserId, e.IsRead, e.IsArchived });
            builder.HasIndex(e => new { e.NotificationId, e.UserId }).IsUnique();
            builder.HasIndex(e => e.DeliveryStatus);
        }
    }

    public class SysNotificationTemplateConfiguration : IEntityTypeConfiguration<SysNotificationTemplate>
    {
        public void Configure(EntityTypeBuilder<SysNotificationTemplate> builder)
        {
            builder.ToTable("SysNotificationTemplates");

            builder.HasIndex(e => e.Code).IsUnique();
        }
    }
}

