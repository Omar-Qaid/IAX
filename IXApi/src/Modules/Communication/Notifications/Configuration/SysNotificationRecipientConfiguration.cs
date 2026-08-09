using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Communication.Notifications.Configuration
{
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
}
