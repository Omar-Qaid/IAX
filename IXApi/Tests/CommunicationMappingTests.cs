using IAX.IXApi.Modules.Communication.Notifications;
using IAX.IXApi.Modules.Communication.Notifications.Configuration;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Mapster;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class CommunicationMappingTests
{
    [Fact]
    public void Notification_and_recipient_map_to_existing_response_shape()
    {
        var config = new TypeAdapterConfig();
        new SysNotificationMapping().Register(config);
        var notification = new SysNotification
        {
            RecId = 101,
            Title = "Approval",
            Message = "Ready",
            Category = "Workflow",
            Recipients = [new SysNotificationRecipient { RecId = 202, UserId = "u1" }]
        };
        var recipient = new SysNotificationRecipient
        {
            RecId = 202,
            NotificationId = 101,
            IsRead = true,
            ReadDate = new DateTime(2026, 10, 12, 0, 0, 0, DateTimeKind.Utc),
            IsArchived = true
        };

        var result = notification.Adapt<SysNotificationDto>(config);
        recipient.Adapt(result, config);

        Assert.Equal(101, result.RecId);
        Assert.Equal("Approval", result.Title);
        Assert.Equal("Ready", result.Message);
        Assert.Equal("Workflow", result.Category);
        Assert.True(result.IsRead);
        Assert.True(result.IsArchived);
        Assert.Equal(recipient.ReadDate, result.ReadDate);
        Assert.Null(result.Recipients);
    }
}
