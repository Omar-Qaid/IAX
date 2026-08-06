using System.Reflection;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;

namespace IAX.IXApi.Modules.Administration;

public static class AdministrationModule
{
    public static IServiceCollection AddAdministrationModule(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly assembly)
    {
        services.Configure<SysBackgroundJobOptions>(configuration.GetSection("BackgroundJobs"));
        services.AddHostedService<SysBackgroundJobProcessor>();

        foreach (var handlerType in assembly.GetTypes().Where(type =>
                     type is { IsClass: true, IsAbstract: false } &&
                     typeof(ISysBackgroundJobHandler).IsAssignableFrom(type)))
        {
            services.AddScoped(handlerType);
            services.AddScoped(typeof(ISysBackgroundJobHandler), provider => provider.GetRequiredService(handlerType));
        }

        // Explicit Administration registrations
        services.AddScoped<AuditLogs.Services.ISysAuditLogService, AuditLogs.Services.SysAuditLogService>();
        services.AddScoped<AuditLogs.Services.ISysAuditService, AuditLogs.Services.SysAuditService>();
        services.AddSingleton<BackgroundJobs.Services.ISysBackgroundJobRegistry, BackgroundJobs.Services.SysBackgroundJobRegistry>();
        services.AddScoped<BackgroundJobs.Services.ISysBackgroundJobManager, BackgroundJobs.Services.SysBackgroundJobManager>();
        services.AddScoped<DataManagement.Services.ISysDataManagementEntityProvider, DataManagement.Providers.SysDataManagementEntityProvider>();
        services.AddScoped<DataManagement.Services.ISysDataManagementService, DataManagement.Services.SysDataManagementService>();
        services.AddScoped<DataManagement.Services.ISysExcelService, DataManagement.Services.SysExcelService>();
        services.AddScoped<NumberSequences.ISysNumberSequenceService, NumberSequences.SysNumberSequenceService>();
        services.AddScoped<Settings.ISysSettingsService, Settings.SysSettingsService>();

        return services;
    }
}
