using IAX.IXApi.Modules.Communication;
using IAX.IXApi.Modules.Communication.Notifications.Controllers;
using IAX.IXApi.Modules.Communication.Chat.Services;
using IAX.IXApi.Modules.Communication.Notifications;
using IAX.IXApi.Modules.Communication.Notifications.Interfaces;
using IAX.IXApi.Modules.Communication.Notifications.Services;
using IAX.IXApi.Modules.Communication.Notifications.Services.Channels;
using IAX.IXApi.Infrastructure.Realtime;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class CommunicationCharacterizationTests
{
    [Fact]
    public void Notification_controller_preserves_authenticated_v1_route()
    {
        var controllerType = typeof(SysNotificationController);

        Assert.NotEmpty(controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true));
        var route = Assert.Single(
            controllerType.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>());
        Assert.Equal("api/v1/[controller]", route.Template);
    }

    [Fact]
    public void Notification_generic_lookup_is_disabled_and_recipient_crud_requires_system_permission()
    {
        var getById = typeof(SysNotificationController).GetMethod(nameof(SysNotificationController.GetById))!;
        Assert.Single(getById.GetCustomAttributes(typeof(NonActionAttribute), inherit: false));

        var permission = Assert.Single(typeof(SysNotificationRecipientController)
            .GetCustomAttributes(typeof(DomainPermissionAttribute), inherit: false)
            .Cast<DomainPermissionAttribute>());
        Assert.Equal("System.Notifications.View", permission.GetRequiredPermission("GET"));
    }

    [Fact]
    public void Realtime_hubs_preserve_authorization_requirement()
    {
        Assert.Single(typeof(SysRealtimeHub).GetCustomAttributes(typeof(AuthorizeAttribute), true));
        Assert.Single(typeof(SysChatHub).GetCustomAttributes(typeof(AuthorizeAttribute), true));
    }

    [Fact]
    public void Communication_module_registers_all_channel_strategies_and_core_services()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Notifications:BackgroundServiceEnabled"] = "false"
            })
            .Build();

        services.AddCommunicationModule(configuration);

        Assert.Equal(
            8,
            services.Count(x => x.ServiceType == typeof(ISysNotificationChannelSender)));
        Assert.Contains(services, x => x.ServiceType == typeof(ISysNotificationService));
        Assert.Contains(services, x => x.ServiceType == typeof(ISysNotificationPreferenceService));
        Assert.Contains(services, x => x.ServiceType == typeof(ISysNotificationRecipientService));
        Assert.Contains(services, x => x.ServiceType == typeof(ISysNotificationTemplateService));
        Assert.Contains(services, x => x.ServiceType == typeof(ISysChatService));
    }
}
