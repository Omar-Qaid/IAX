using System.Reflection;
using IAX.IXApi.Modules.Communication.Notifications;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Services;
using IAX.IXApi.Modules.Workflow.Activities;
using Xunit;

namespace IAX.IXApi.Tests;

public class WorkflowNotificationConfigurationTests
{
    [Theory]
    [InlineData(true, SysNotificationChannel.InApp)]
    [InlineData(false, SysNotificationChannel.Email)]
    public void Explicit_workflow_channel_overrides_template_but_legacy_default_is_preserved(bool preserve, SysNotificationChannel expected)
        => Assert.Equal(expected, new CreateSysNotificationDto { Channel = SysNotificationChannel.InApp, PreserveChannel = preserve }.ResolveChannel(SysNotificationChannel.Email));

    [Fact]
    public async Task Activity_dispatch_uses_only_enabled_channels_and_identity_recipient()
    {
        var service = DispatchProxy.Create<ISysNotificationService, NotificationRecorder>();
        var recorder = (NotificationRecorder)service;
        // No template is configured, so dispatch must not access the database.
        var dispatcher = new WfActivityNotificationDispatcher(service, null!);
        await dispatcher.DispatchActivityAlertAsync(new WfActivity
        {
            RecId = 7, IsSystemNotificationEnabled = true, IsEmailNotificationEnabled = true,
            IsSmsNotificationEnabled = false, IsWhatsAppNotificationEnabled = false,
        }, "account-guid", url: "/workflow/requests/10", fallbackTitle: "Assigned", fallbackMessage: "Review request 10");
        Assert.Equal(new[] { SysNotificationChannel.InApp, SysNotificationChannel.Email }, recorder.Sent.Select(item => item.Channel));
        Assert.All(recorder.Sent, item =>
        {
            Assert.Equal(new[] { "account-guid" }, item.UserIds);
            Assert.True(item.PreserveChannel);
            Assert.Equal("7", item.EntityId);
            Assert.Equal("/workflow/requests/10", item.Url);
        });
        recorder.Sent.Clear();
        await dispatcher.DispatchActivityAlertAsync(new WfActivity(), "account-guid");
        Assert.Empty(recorder.Sent);
    }

    public class NotificationRecorder : DispatchProxy
    {
        public List<CreateSysNotificationDto> Sent { get; } = [];
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name != nameof(ISysNotificationService.SendAsync)) throw new NotSupportedException();
            Sent.Add((CreateSysNotificationDto)args![0]!);
            return Task.FromResult(new SysNotificationDto());
        }
    }
}
