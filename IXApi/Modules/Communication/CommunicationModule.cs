using IAX.IXApi.Modules.Communication.Notifications.Services;
using IAX.IXApi.Modules.Communication.Notifications.Services.Channels;

namespace IAX.IXApi.Modules.Communication;

public static class CommunicationModule
{
    public static IServiceCollection AddCommunicationModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (configuration.GetValue("Notifications:BackgroundServiceEnabled", true))
        {
            services.AddHostedService<SysNotificationBackgroundService>();
        }
        services.AddScoped<ISysNotificationChannelSender, SysInAppNotificationChannelSender>();
        services.AddScoped<ISysNotificationChannelSender, SysEmailNotificationChannelSender>();
        services.AddScoped<ISysNotificationChannelSender, SysSmsNotificationChannelSender>();
        services.AddScoped<ISysNotificationChannelSender, SysPushNotificationChannelSender>();
        services.AddScoped<ISysNotificationChannelSender, SysWhatsAppNotificationChannelSender>();
        services.AddScoped<ISysNotificationChannelSender, SysTeamsNotificationChannelSender>();
        services.AddScoped<ISysNotificationChannelSender, SysSlackNotificationChannelSender>();
        services.AddScoped<ISysNotificationChannelSender, SysWebhookNotificationChannelSender>();
        return services;
    }
}
