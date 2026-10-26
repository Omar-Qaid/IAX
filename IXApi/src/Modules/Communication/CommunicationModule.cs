using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;
using IAX.IXApi.Modules.Communication.Chat.Entities;
using IAX.IXApi.Modules.Communication.Chat.Services;
using IAX.IXApi.Modules.Communication.Chat.Validation;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Interfaces;
using IAX.IXApi.Modules.Communication.Notifications.Jobs;
using IAX.IXApi.Modules.Communication.Notifications.Services;
using IAX.IXApi.Modules.Communication.Notifications.Services.Channels;
using IAX.IXApi.Modules.Communication.Notifications.Validation;
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
        services.AddScoped<ISysChatService, SysChatService>();
        services.AddScoped<ISysNotificationService, SysNotificationService>();
        services.AddScoped<ISysNotificationPreferenceService, SysNotificationPreferenceService>();
        services.AddScoped<ISysNotificationTemplateService, SysNotificationTemplateService>();
        services.AddScoped<ISysNotificationRecipientService, SysNotificationRecipientService>();

        // Generic IBaseService registrations for architecture resolution
        services.AddScoped<IBaseService<SysChatMessage>>(sp => sp.GetRequiredService<ISysChatService>());
        services.AddScoped<IBaseService<SysNotification>>(sp => sp.GetRequiredService<ISysNotificationService>());
        services.AddScoped<IBaseService<SysNotificationPreference>>(sp => sp.GetRequiredService<ISysNotificationPreferenceService>());
        services.AddScoped<IBaseService<SysNotificationTemplate>>(sp => sp.GetRequiredService<ISysNotificationTemplateService>());
        services.AddScoped<IBaseService<SysNotificationRecipient>>(sp => sp.GetRequiredService<ISysNotificationRecipientService>());

        // Validators
        services.AddScoped<SysChatMessageDtoValidator>();
        services.AddScoped<SendChatMessageRequestValidator>();
        services.AddScoped<SysNotificationDtoValidator>();
        services.AddScoped<CreateSysNotificationDtoValidator>();
        services.AddScoped<SysNotificationTemplateDtoValidator>();
        services.AddScoped<SysNotificationPreferenceDtoValidator>();
        services.AddScoped<SysNotificationRecipientDtoValidator>();

        services.AddScoped<ISysBackgroundJobHandler, CleanupExpiredNotificationsJobHandler>();

        return services;
    }
}
