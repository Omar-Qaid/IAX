# Administration backend architecture refactor

Scope: `IXApi/src/Modules/Administration`. Copied `GeneralLedger/` labels in the attachment were treated as examples; the Administration tree is the target.

## Shared baseline

Inspected Shared `BaseController`, `IBaseService`, `BaseService`, `IUnitOfWork`, `UnitOfWork`, repositories, `APIResponse`, DTO/entity conventions, Mapster scanning, validators, authorization, company context, exception handling, logging, DI, and pagination. The baseline Administration build passed. Existing Communication working-tree edits were left untouched.

## Folder-by-folder results

- **Settings:** Reviewed all entities, EF configuration, DTOs, validators, interface, service, and controller. Existing service already uses `IUnitOfWork`; controller uses Mapster and keeps its four API actions. No change needed.
- **NumberSequences:** Reviewed all entities, configuration, DTOs, validator, interface, service, controller, and auto-code extensions. Existing `BaseController`/`BaseService` and `IUnitOfWork` path retained. Sequence allocation, formatting, and scoped numbering stayed unchanged.
- **AuditLogs:** Reviewed entities, audit interceptor-facing service, log service interface, and controller. Moved record filtering, table-name resolution, and DTO mapping into `SysAuditLogService`; controller retains append-only generic CRUD suppression. Folder compiled.
- **DataManagement:** Reviewed contracts, provider, Excel service, import/export service, and controller. Moved reflection-based generic dispatch and field metadata generation to `SysDataManagementEndpointService`. HTTP file streaming, headers, response shape, permissions, and existing `IUnitOfWork` data service remain. Folder compiled.
- **BackgroundJobs:** Reviewed configuration, entities, DTOs, controllers, services, handlers, batch worker, registry, lock, scheduler, and job context. Added dedicated services for settings, active periods, groups, and tasks; controllers now translate service results to their existing HTTP responses. Worker, manager, handler, and lock use the Shared unit of work while keeping scoped contexts, dedicated SQL lock connection, claim/update order, retries, and saves. Added Mapster setup/job mappings. Folder compiled.
- **Module root/Persistence:** Reviewed module DI, global usings, project references, and `IAdministrationDataContext`. Kept the legacy context interface and Infrastructure registration for compatibility; registered new services and the Administration mapping assembly.

## Complete C# file and declaration inventory

| File | Declarations | Result |
|---|---|---|
| `AdministrationModule.cs` | AdministrationModule | Refactored |
| `AuditLogs/Entities/SysAuditLog.cs` | SysAuditLog | Retained: Entity/schema contract preserved |
| `AuditLogs/Entities/SysExceptionLog.cs` | SysExceptionLog | Retained: Entity/schema contract preserved |
| `AuditLogs/Services/ISysAuditLogService.cs` | ISysAuditLogService | Refactored |
| `AuditLogs/Services/ISysAuditService.cs` | ISysAuditService | Retained: Audit interceptor/service contract retained |
| `AuditLogs/Services/PendingAudit.cs` | PendingAudit | Retained: Audit interceptor/service contract retained |
| `AuditLogs/Services/SysAuditLogService.cs` | SysAuditLogService | Refactored |
| `AuditLogs/Services/SysAuditService.cs` | SysAuditService | Retained: Audit interceptor/service contract retained |
| `AuditLogs/SysAuditLogController.cs` | SysAuditLogController | Refactored |
| `BackgroundJobs/BatchJobActivePeriodController.cs` | BatchJobActivePeriodDto, BatchJobActivePeriodController | Refactored |
| `BackgroundJobs/BatchSettingsController.cs` | BatchSettingsController | Refactored |
| `BackgroundJobs/Configuration/BatchJobActivePeriodConfiguration.cs` | BatchJobActivePeriodConfiguration | Retained: EF mapping preserved |
| `BackgroundJobs/Configuration/BatchSetupMapping.cs` | BatchSetupMapping | Created |
| `BackgroundJobs/Configuration/SysBackgroundJobConfiguration.cs` | SysBackgroundJobConfiguration | Retained: EF mapping preserved |
| `BackgroundJobs/Configuration/SysBackgroundJobExecutionConfiguration.cs` | SysBackgroundJobExecutionConfiguration | Retained: EF mapping preserved |
| `BackgroundJobs/Configuration/SysBackgroundJobGroupConfiguration.cs` | SysBackgroundJobGroupConfiguration | Retained: EF mapping preserved |
| `BackgroundJobs/Configuration/SysBackgroundJobMapping.cs` | SysBackgroundJobMapping | Created |
| `BackgroundJobs/CreateSysBackgroundJobDto.cs` | CreateSysBackgroundJobDto | Retained: API contract preserved |
| `BackgroundJobs/Entities/BatchSettings.cs` | BatchSettings | Retained: Entity/schema contract preserved |
| `BackgroundJobs/Entities/SysBackgroundJob.cs` | SysBackgroundJob, SysBackgroundJobExecution, SysBackgroundJobActivePeriod, SysBackgroundJobGroup, SysBackgroundJobRecurrenceCount | Retained: Entity/schema contract preserved |
| `BackgroundJobs/Entities/SysBackgroundJobEnums.cs` | SysJobScheduleType, SysJobStatus, SysJobExecutionStatus, SysJobTrigger, BatchMonitoringCategory, BatchHistoryLogLevel | Retained: Entity/schema contract preserved |
| `BackgroundJobs/Entities/SysBackgroundJobTask.cs` | SysBackgroundJobTask, SysBackgroundJobTaskExecution | Retained: Entity/schema contract preserved |
| `BackgroundJobs/Services/AxSyncBatchService.cs` | AxSyncBatchService, AxSyncParameters, IAxSyncProcessor, UnconfiguredAxSyncProcessor | Retained: Batch scheduling/handler behavior retained |
| `BackgroundJobs/Services/BatchCancellationMonitor.cs` | BatchCancellationMonitor | Retained: Batch scheduling/handler behavior retained |
| `BackgroundJobs/Services/BatchDatabaseLock.cs` | BatchDatabaseLock | Refactored |
| `BackgroundJobs/Services/BatchJobActivePeriodService.cs` | BatchJobActivePeriodService | Created |
| `BackgroundJobs/Services/BatchServiceHandlerAdapter.cs` | BatchServiceHandlerAdapter | Retained: Batch scheduling/handler behavior retained |
| `BackgroundJobs/Services/BatchSettingsService.cs` | BatchSettingsService | Created |
| `BackgroundJobs/Services/BatchSetupResult.cs` | BatchSetupStatus, BatchSetupResult | Created |
| `BackgroundJobs/Services/BatchWorker.cs` | BatchWorker | Refactored |
| `BackgroundJobs/Services/Handlers/BatchTasksJobHandler.cs` | BatchTasksJobHandler | Refactored |
| `BackgroundJobs/Services/Handlers/ISysBackgroundJobHandler.cs` | ISysBackgroundJobHandler | Retained: Batch scheduling/handler behavior retained |
| `BackgroundJobs/Services/Handlers/SampleEchoJobHandler.cs` | SampleEchoJobHandler, Payload | Retained: Batch scheduling/handler behavior retained |
| `BackgroundJobs/Services/IBatchJobActivePeriodService.cs` | IBatchJobActivePeriodService | Created |
| `BackgroundJobs/Services/IBatchSettingsService.cs` | IBatchSettingsService | Created |
| `BackgroundJobs/Services/ISysBackgroundJobGroupService.cs` | ISysBackgroundJobGroupService | Created |
| `BackgroundJobs/Services/ISysBackgroundJobManager.cs` | ISysBackgroundJobManager | Retained: Batch scheduling/handler behavior retained |
| `BackgroundJobs/Services/ISysBackgroundJobRegistry.cs` | ISysBackgroundJobRegistry | Retained: Batch scheduling/handler behavior retained |
| `BackgroundJobs/Services/ISysBackgroundJobTaskService.cs` | ISysBackgroundJobTaskService | Created |
| `BackgroundJobs/Services/SysBackgroundJobContext.cs` | SysBackgroundJobContext | Refactored |
| `BackgroundJobs/Services/SysBackgroundJobGroupService.cs` | SysBackgroundJobGroupService | Created |
| `BackgroundJobs/Services/SysBackgroundJobManager.cs` | SysBackgroundJobManager | Refactored |
| `BackgroundJobs/Services/SysBackgroundJobOptions.cs` | SysBackgroundJobOptions | Retained: Batch scheduling/handler behavior retained |
| `BackgroundJobs/Services/SysBackgroundJobRegistry.cs` | SysBackgroundJobRegistry | Retained: Batch scheduling/handler behavior retained |
| `BackgroundJobs/Services/SysBackgroundJobTaskService.cs` | SysBackgroundJobTaskService | Created |
| `BackgroundJobs/Services/SysCronExpression.cs` | SysCronExpression | Retained: Batch scheduling/handler behavior retained |
| `BackgroundJobs/Services/SysJobScheduleCalculator.cs` | SysJobScheduleCalculator, RecurrenceSpec | Retained: Batch scheduling/handler behavior retained |
| `BackgroundJobs/SysBackgroundJobController.cs` | SysBackgroundJobController | Retained: Batch scheduling/handler behavior retained |
| `BackgroundJobs/SysBackgroundJobDashboardDto.cs` | SysBackgroundJobDashboardDto | Retained: API contract preserved |
| `BackgroundJobs/SysBackgroundJobDto.cs` | SysBackgroundJobDto | Retained: API contract preserved |
| `BackgroundJobs/SysBackgroundJobExecutionDto.cs` | SysBackgroundJobExecutionDto | Retained: API contract preserved |
| `BackgroundJobs/SysBackgroundJobGroupController.cs` | SysBackgroundJobGroupDto, SysBackgroundJobGroupController | Refactored |
| `BackgroundJobs/SysBackgroundJobTaskController.cs` | SysBackgroundJobTaskDto, SysBackgroundJobTaskController | Refactored |
| `BackgroundJobs/UpdateSysBackgroundJobScheduleDto.cs` | UpdateSysBackgroundJobScheduleDto | Retained: API contract preserved |
| `DataManagement/Contracts/SysImportError.cs` | SysImportError | Retained: API contract preserved |
| `DataManagement/Contracts/SysImportResult.cs` | SysImportResult | Retained: API contract preserved |
| `DataManagement/Providers/SysDataManagementEntityProvider.cs` | SysDataManagementEntityProvider | Retained: Existing provider/export contract retained |
| `DataManagement/Services/ISysDataManagementEndpointService.cs` | ISysDataManagementEndpointService | Created |
| `DataManagement/Services/ISysDataManagementEntityProvider.cs` | ISysDataManagementEntityProvider | Retained: Existing provider/export contract retained |
| `DataManagement/Services/ISysDataManagementService.cs` | ISysDataManagementService | Retained: Existing provider/export contract retained |
| `DataManagement/Services/ISysExcelService.cs` | ISysExcelService | Retained: Existing provider/export contract retained |
| `DataManagement/Services/SysDataManagementEndpointService.cs` | SysDataManagementEndpointService | Created |
| `DataManagement/Services/SysDataManagementService.cs` | SysDataManagementService | Retained: Existing provider/export contract retained |
| `DataManagement/Services/SysExcelService.cs` | SysExcelService | Retained: Existing provider/export contract retained |
| `DataManagement/Services/SysExportColumnDto.cs` | SysExportColumnDto | Retained: API contract preserved |
| `DataManagement/Services/SysExportRequest.cs` | SysExportRequest | Retained: API contract preserved |
| `DataManagement/SysDataManagementController.cs` | SysDataManagementController | Refactored |
| `GlobalUsings.cs` | Global usings | Retained: Existing module behavior retained |
| `NumberSequences/AutoCodeExtensions.cs` | AutoCodeExtensions | Retained: Existing BaseService and UOW pattern |
| `NumberSequences/ISysNumberSequenceService.cs` | ISysNumberSequenceService | Retained: Existing BaseService and UOW pattern |
| `NumberSequences/NextSequenceRequestDto.cs` | NextSequenceRequestDto | Retained: API contract preserved |
| `NumberSequences/NextSequenceResultDto.cs` | NextSequenceResultDto | Retained: API contract preserved |
| `NumberSequences/ResetSequenceRequestDto.cs` | ResetSequenceRequestDto | Retained: API contract preserved |
| `NumberSequences/SysNumberSequence.cs` | SysNumberSequence | Retained: Entity/schema contract preserved |
| `NumberSequences/SysNumberSequenceConfiguration.cs` | SysNumberSequenceConfiguration | Retained: EF mapping preserved |
| `NumberSequences/SysNumberSequenceController.cs` | SysNumberSequenceController | Retained: Existing BaseService and UOW pattern |
| `NumberSequences/SysNumberSequenceDto.cs` | SysNumberSequenceDto | Retained: API contract preserved |
| `NumberSequences/SysNumberSequenceDtoValidator.cs` | SysNumberSequenceDtoValidator | Retained: API contract preserved |
| `NumberSequences/SysNumberSequenceService.cs` | SysNumberSequenceService | Retained: Existing BaseService and UOW pattern |
| `Persistence/IAdministrationDataContext.cs` | IAdministrationDataContext | Retained: Infrastructure compatibility retained |
| `Settings/ISysSettingsService.cs` | ISysSettingsService | Retained: Existing settings service/controller follows Shared pattern |
| `Settings/SysSettings.cs` | SysSettings | Retained: Entity/schema contract preserved |
| `Settings/SysSettingsConfiguration.cs` | SysSettingsConfiguration | Retained: EF mapping preserved |
| `Settings/SysSettingsController.cs` | SysSettingsController | Retained: Existing settings service/controller follows Shared pattern |
| `Settings/SysSettingsDto.cs` | SysSettingsDto | Retained: API contract preserved |
| `Settings/SysSettingsDtoValidator.cs` | SysSettingsDtoValidator | Retained: API contract preserved |
| `Settings/SysSettingsService.cs` | SysSettingsService | Retained: Existing settings service/controller follows Shared pattern |
| `Settings/SysUserSettings.cs` | SysUserSettings | Retained: Entity/schema contract preserved |
| `Settings/SysUserSettingsConfiguration.cs` | SysUserSettingsConfiguration | Retained: EF mapping preserved |
| `Settings/SysUserSettingsDto.cs` | SysUserSettingsDto | Retained: API contract preserved |
| `Settings/SysUserSettingsDtoValidator.cs` | SysUserSettingsDtoValidator | Retained: API contract preserved |

## Requested final checklist

1. **Folders analyzed:** Settings, NumberSequences, AuditLogs, DataManagement, BackgroundJobs, Persistence, and module root, including all nested folders.
2. **Every class analyzed:** The complete 91-file C# inventory above records each declaration and disposition.
3. **Classes modified:** 14 existing Administration files marked Refactored above; `Program.cs` was changed for mapping discovery.
4. **Classes created:** 13 Administration files marked Created above; `AdministrationMappingTests.cs` was added.
5. **Classes removed:** None.
6. **Controller changes:** AuditLogs, DataManagement, batch settings, active-period, group, and task controllers now delegate persistence and business operations. Other controllers already use services. The final controller scan found zero direct DbContext/EF query/save calls.
7. **Service changes:** New setup/endpoint services and updated audit, batch manager, worker, handler, context, and lock paths.
8. **IBaseService usage:** Generic CRUD remains on NumberSequences and AuditLogs where existing. Custom batch, settings, and import/export operations keep dedicated interfaces.
9. **IUnitOfWork usage:** Administration persistence services, worker scopes, and handlers use the Shared scoped unit of work. Direct `Context.SaveChangesAsync` and `ExecuteUpdateAsync` positions were retained for batch execution semantics. `BatchDatabaseLock` keeps a DbContext overload for existing test/caller compatibility; production passes `IUnitOfWork`.
10. **Mapster configuration:** Added `BatchSetupMapping` and `SysBackgroundJobMapping`, registered Administration assembly scanning, and replaced manual entity/DTO mappings in active period, group, task, audit, and job responses. Result/aggregate DTO construction remains explicit.
11. **Validation changes:** Existing data annotations, validators, and status/message behavior remain. Batch setup checks moved into services.
12. **Configuration changes:** Added Mapster configuration; no EF model configuration changed.
13. **DI changes:** Registered batch settings, active period, group, task, and DataManagement endpoint services. Existing UOW registration is reused.
14. **Architecture issues fixed:** Removed controller EF access, moved business rules and reflection dispatch into services, and aligned batch persistence with Shared UOW.
15. **Build/compile result:** Administration rebuilt with 0 module warnings and 0 errors (one warning was in Shared). Final IXApi build passed with 0 errors and 170 project warnings; the Administration rebuild emitted no Administration warnings. `IXApi.Tests` passed 234 tests, with 2 database-dependent tests skipped.
16. **API behavior verification:** Route, HTTP method, authorization, and DomainPermission attributes in all six changed controllers were compared with HEAD and matched. Mapping tests covered IDs, concurrency token, recurrence total, and execution caption. No authenticated live HTTP run was performed.
17. **Database/schema verification:** No entity, EF configuration, migration, or database file was changed. No live schema comparison was performed.
18. **Remaining issues:** Legacy `IAdministrationDataContext` and its Infrastructure registration remain for compatibility. Live batch-worker concurrency, database transactions, and import/export streaming were not exercised against a running service/database.

## Out-of-scope working tree

Pre-existing Communication edits and report were not modified by this Administration task.
