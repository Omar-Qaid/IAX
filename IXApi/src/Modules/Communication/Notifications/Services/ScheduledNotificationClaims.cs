using System.Linq.Expressions;
using IAX.IXApi.Modules.Communication.Notifications.Entities;

namespace IAX.IXApi.Modules.Communication.Notifications.Services;

public static class ScheduledNotificationClaims
{
    public static Expression<Func<SysScheduledNotification, bool>> Due(DateTime now) => job =>
        (job.Status == SysScheduledJobStatus.Pending || job.Status == SysScheduledJobStatus.Processing) && job.SendAt <= now;
}
