using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Communication.Notifications.Jobs;

public sealed class CleanupExpiredNotificationsJobHandler : ISysBackgroundJobHandler
{
    public string JobKey => "CleanupExpiredNotifications";

    public async Task ExecuteAsync(SysBackgroundJobContext context, CancellationToken cancellationToken)
    {
        var db = context.Services.GetRequiredService<ApplicationDbContext>();
        var now = DateTime.UtcNow;
        var expired = await db.Set<SysNotification>()
            .Where(notification => notification.ExpiryDate != null
                && notification.ExpiryDate <= now
                && !notification.IsDeleted
                && notification.Status != SysNotificationStatus.Expired)
            .Take(500)
            .ToListAsync(cancellationToken);

        foreach (var notification in expired)
            notification.Status = SysNotificationStatus.Expired;

        if (expired.Count > 0)
            await db.SaveChangesAsync(cancellationToken);

        context.Output = $"Expired {expired.Count} notification(s).";
    }
}
