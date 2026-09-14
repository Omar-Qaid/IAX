using IAX.IXApi.Modules.Communication.Notifications.Services;
using IAX.IXApi.Modules.Communication.Notifications.Services.Channels;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;
using IAX.IXApi.Modules.Communication.Notifications.Jobs;
using IAX.IXApi.Shared.Application.Batch;

namespace IAX.IXApi.Modules.Communication;

public static class CommunicationModule
{
    public static IServiceCollection AddCommunicationModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ScheduledNotificationBatchProcessor>();
        services.AddBatchService<EmailDeliveryBatchService>("EmailDelivery", "Email delivery");
        services.AddBatchService<SmsDeliveryBatchService>("SmsDelivery", "SMS delivery");
        services.AddBatchService<PushNotificationBatchService>("PushNotification", "Push notification delivery");
        services.AddBatchService<NotificationDeliveryBatchService>("NotificationDelivery", "Other notification delivery");
        services.AddBatchService<WorkflowNotificationBatchService>("WorkflowNotification", "Workflow notifications");
        services.AddBatchService<WorkflowReminderBatchService>("WorkflowReminder", "Workflow reminders");
        services.AddBatchService<WorkflowEscalationBatchService>("WorkflowEscalation", "Workflow escalations");
        services.AddBatchService<WorkflowCleanupBatchService>("WorkflowCleanup", "Expired notification cleanup");
        services.AddScoped<ISysNotificationChannelSender, SysInAppNotificationChannelSender>();
        services.AddScoped<ISysNotificationChannelSender, SysEmailNotificationChannelSender>();
        services.AddScoped<ISysNotificationChannelSender, SysSmsNotificationChannelSender>();
        services.AddScoped<ISysNotificationChannelSender, SysPushNotificationChannelSender>();
        services.AddScoped<ISysNotificationChannelSender, SysWhatsAppNotificationChannelSender>();
        services.AddScoped<ISysNotificationChannelSender, SysTeamsNotificationChannelSender>();
        services.AddScoped<ISysNotificationChannelSender, SysSlackNotificationChannelSender>();
        services.AddScoped<ISysNotificationChannelSender, SysWebhookNotificationChannelSender>();

        // Explicit Communication services
        services.AddScoped<Chat.Services.ISysChatService, Chat.Services.SysChatService>();
        services.AddScoped<Notifications.Services.ISysNotificationService, Notifications.Services.SysNotificationService>();
        services.AddScoped<ISysBackgroundJobHandler, CleanupExpiredNotificationsJobHandler>();

        return services;
    }
}
