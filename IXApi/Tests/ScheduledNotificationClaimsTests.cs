using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Services;
using Xunit;

namespace IXApi.Tests;

public class ScheduledNotificationClaimsTests
{
    [Theory]
    [InlineData(SysScheduledJobStatus.Pending, -1, true)]
    [InlineData(SysScheduledJobStatus.Pending, 1, false)]
    [InlineData(SysScheduledJobStatus.Processing, -1, true)]
    [InlineData(SysScheduledJobStatus.Processing, 1, false)]
    [InlineData(SysScheduledJobStatus.Completed, -1, false)]
    [InlineData(SysScheduledJobStatus.Cancelled, -1, false)]
    [InlineData(SysScheduledJobStatus.Failed, -1, false)]
    public void OnlyDuePendingOrExpiredClaimsCanBeAcquired(SysScheduledJobStatus status, int minutes, bool expected)
    {
        var now = new DateTime(2026, 9, 10, 10, 0, 0, DateTimeKind.Utc);
        Assert.Equal(expected, ScheduledNotificationClaims.Due(now).Compile()(new()
        {
            Status = status, SendAt = now.AddMinutes(minutes)
        }));
    }
}
