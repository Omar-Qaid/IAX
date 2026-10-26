// Compatibility forwarding for callers using IAX.IXApi.Modules.Administration.BackgroundJobs
global using SysBackgroundJobDto = IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos.SysBackgroundJobDto;
global using CreateSysBackgroundJobDto = IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos.CreateSysBackgroundJobDto;
global using UpdateSysBackgroundJobScheduleDto = IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos.UpdateSysBackgroundJobScheduleDto;
global using SysBackgroundJobExecutionDto = IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos.SysBackgroundJobExecutionDto;
global using SysBackgroundJobDashboardDto = IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos.SysBackgroundJobDashboardDto;
global using SysBackgroundJobGroupDto = IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos.SysBackgroundJobGroupDto;
global using SysBackgroundJobTaskDto = IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos.SysBackgroundJobTaskDto;
global using BatchJobActivePeriodDto = IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos.BatchJobActivePeriodDto;
global using BatchSettingsDto = IAX.IXApi.Modules.Administration.BackgroundJobs.Dtos.BatchSettingsDto;

global using ISysBackgroundJobService = IAX.IXApi.Modules.Administration.BackgroundJobs.Interfaces.ISysBackgroundJobService;
global using ISysBackgroundJobGroupService = IAX.IXApi.Modules.Administration.BackgroundJobs.Interfaces.ISysBackgroundJobGroupService;
global using ISysBackgroundJobTaskService = IAX.IXApi.Modules.Administration.BackgroundJobs.Interfaces.ISysBackgroundJobTaskService;
global using IBatchJobActivePeriodService = IAX.IXApi.Modules.Administration.BackgroundJobs.Interfaces.IBatchJobActivePeriodService;
global using IBatchSettingsService = IAX.IXApi.Modules.Administration.BackgroundJobs.Interfaces.IBatchSettingsService;
global using ISysBackgroundJobManager = IAX.IXApi.Modules.Administration.BackgroundJobs.Interfaces.ISysBackgroundJobManager;
global using ISysBackgroundJobRegistry = IAX.IXApi.Modules.Administration.BackgroundJobs.Interfaces.ISysBackgroundJobRegistry;

global using SysBackgroundJobController = IAX.IXApi.Modules.Administration.BackgroundJobs.Controllers.SysBackgroundJobController;
global using SysBackgroundJobGroupController = IAX.IXApi.Modules.Administration.BackgroundJobs.Controllers.SysBackgroundJobGroupController;
global using SysBackgroundJobTaskController = IAX.IXApi.Modules.Administration.BackgroundJobs.Controllers.SysBackgroundJobTaskController;
global using BatchJobActivePeriodController = IAX.IXApi.Modules.Administration.BackgroundJobs.Controllers.BatchJobActivePeriodController;
global using BatchSettingsController = IAX.IXApi.Modules.Administration.BackgroundJobs.Controllers.BatchSettingsController;
