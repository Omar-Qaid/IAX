using System.Reflection;
using FluentValidation;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Interfaces;
using IAX.IXApi.Modules.Administration.AuditLogs.Services;
using IAX.IXApi.Modules.Administration.AuditLogs.Validation;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Interfaces;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Validation;
using IAX.IXApi.Modules.Administration.DataManagement.Dtos;
using IAX.IXApi.Modules.Administration.DataManagement.Interfaces;
using IAX.IXApi.Modules.Administration.DataManagement.Services;
using IAX.IXApi.Modules.Administration.DataManagement.Validation;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Administration.Settings.Dtos;
using IAX.IXApi.Modules.Administration.Settings;
using IAX.IXApi.Modules.Administration.Settings.Interfaces;
using IAX.IXApi.Modules.Administration.Settings.Services;
using IAX.IXApi.Modules.Administration.Settings.Validation;
using IAX.IXApi.Shared.Application.Batch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Modules.Administration;

public static class AdministrationModule
{
    public static IServiceCollection AddAdministrationModule(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly assembly)
    {
        services.Configure<SysBackgroundJobOptions>(configuration.GetSection("BackgroundJobs"));
        services.AddBatchFramework();
        services.TryAddScoped<IAxSyncProcessor, UnconfiguredAxSyncProcessor>();
        services.AddBatchService<AxSyncBatchService>("AxSync", "Dynamics AX synchronization");
        services.AddHostedService<BatchWorker>();

        foreach (var handlerType in typeof(AdministrationModule).Assembly.GetTypes().Where(type =>
                     type is { IsClass: true, IsAbstract: false, IsPublic: true } &&
                     typeof(ISysBackgroundJobHandler).IsAssignableFrom(type)))
        {
            services.AddScoped(handlerType);
            services.AddScoped(typeof(ISysBackgroundJobHandler), provider => provider.GetRequiredService(handlerType));
        }

        // AuditLogs feature
        services.AddScoped<ISysAuditLogService, SysAuditLogService>();
        services.AddScoped<IBaseService<SysAuditLog>>(sp => sp.GetRequiredService<ISysAuditLogService>());
        services.AddScoped<ISysExceptionLogService, SysExceptionLogService>();
        services.AddScoped<IBaseService<SysExceptionLog>>(sp => sp.GetRequiredService<ISysExceptionLogService>());
        services.AddScoped<ISysAuditService, SysAuditService>();

        // BackgroundJobs feature
        services.AddScoped<ISysBackgroundJobService, SysBackgroundJobService>();
        services.AddScoped<IBaseService<SysBackgroundJob>>(sp => sp.GetRequiredService<ISysBackgroundJobService>());
        services.AddSingleton<ISysBackgroundJobRegistry, SysBackgroundJobRegistry>();
        services.AddScoped<ISysBackgroundJobManager, SysBackgroundJobManager>();
        services.AddScoped<IBatchSettingsService, BatchSettingsService>();
        services.AddScoped<IBaseService<BatchSettings>>(sp => sp.GetRequiredService<IBatchSettingsService>());
        services.AddScoped<IBatchJobActivePeriodService, BatchJobActivePeriodService>();
        services.AddScoped<IBaseService<SysBackgroundJobActivePeriod>>(sp => sp.GetRequiredService<IBatchJobActivePeriodService>());
        services.AddScoped<ISysBackgroundJobGroupService, SysBackgroundJobGroupService>();
        services.AddScoped<IBaseService<SysBackgroundJobGroup>>(sp => sp.GetRequiredService<ISysBackgroundJobGroupService>());
        services.AddScoped<ISysBackgroundJobTaskService, SysBackgroundJobTaskService>();
        services.AddScoped<IBaseService<SysBackgroundJobTask>>(sp => sp.GetRequiredService<ISysBackgroundJobTaskService>());

        // DataManagement feature
        services.AddScoped<ISysDataManagementEntityProvider, SysDataManagementEntityProvider>();
        services.AddScoped<ISysDataManagementService, SysDataManagementService>();
        services.AddScoped<ISysDataManagementEndpointService, SysDataManagementEndpointService>();
        services.AddScoped<ISysExcelService, SysExcelService>();

        // NumberSequences feature
        services.AddScoped<ISysNumberSequenceService, SysNumberSequenceService>();
        services.AddScoped<IBaseService<SysNumberSequence>>(sp => sp.GetRequiredService<ISysNumberSequenceService>());
        services.AddScoped<Shared.Application.NumberSequences.INumberSequenceRuntime>(provider =>
            provider.GetRequiredService<ISysNumberSequenceService>());

        // Settings feature
        services.AddScoped<ISysSettingsService, SysSettingsService>();
        services.AddScoped<IBaseService<SysSettings>>(sp => sp.GetRequiredService<ISysSettingsService>());
        services.AddScoped<ISysUserSettingsService, SysUserSettingsService>();
        services.AddScoped<IBaseService<SysUserSettings>>(sp => sp.GetRequiredService<ISysUserSettingsService>());

        // Validators explicit registration
        services.AddScoped<IValidator<SysNumberSequenceDto>, SysNumberSequenceDtoValidator>();
        services.AddScoped<IValidator<SysSettingsDto>, SysSettingsDtoValidator>();
        services.AddScoped<IValidator<SysUserSettingsDto>, SysUserSettingsDtoValidator>();
        services.AddScoped<IValidator<AuditLogs.Dtos.SysAuditLogDto>, SysAuditLogDtoValidator>();
        services.AddScoped<IValidator<AuditLogs.Dtos.SysExceptionLogDto>, SysExceptionLogDtoValidator>();
        services.AddScoped<IValidator<SysBackgroundJobDto>, SysBackgroundJobDtoValidator>();
        services.AddScoped<IValidator<CreateSysBackgroundJobDto>, CreateSysBackgroundJobDtoValidator>();
        services.AddScoped<IValidator<UpdateSysBackgroundJobScheduleDto>, UpdateSysBackgroundJobScheduleDtoValidator>();
        services.AddScoped<IValidator<SysBackgroundJobGroupDto>, SysBackgroundJobGroupDtoValidator>();
        services.AddScoped<IValidator<SysBackgroundJobTaskDto>, SysBackgroundJobTaskDtoValidator>();
        services.AddScoped<IValidator<BatchJobActivePeriodDto>, BatchJobActivePeriodDtoValidator>();
        services.AddScoped<IValidator<BatchSettingsDto>, BatchSettingsDtoValidator>();
        services.AddScoped<IValidator<SysExportRequest>, SysExportRequestValidator>();
        return services;
    }
}
