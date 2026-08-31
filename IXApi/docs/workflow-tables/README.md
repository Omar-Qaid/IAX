# Workflow table catalog

This catalog documents the 34 `Wf*` tables in the current EF migration. Each linked file explains what the table stores, where it is used, its principal relationships, and how the workflow seeders populate it.

For the complete configuration-to-runtime lifecycle, see [How Process Builder configuration connects to workflow transaction/history data](How-Process-Builder-Configuration-Connects-to-Workflow-Transaction-History-Data.md).

## Legacy `wf.sql` snapshot

`src/Infrastructure/Persistence/Seeding/wfData/wf.sql` is a 4.9 MB SQL Server export from database `db_a8e163_aljazerasoftfp`, scripted on 2026-08-31. It defines 24 legacy workflow tables. This catalog analyzes only the 22 tables that also exist in the current project; those matching tables contain 2,615 exported rows. The matching `WfRequestVariables` table has no exported rows.

The snapshot is not the current EF schema. Legacy-only tables are intentionally skipped. The export also omits newer normalized option, validation, request/activity-detail, and print-template tables. Every current-project table file records whether it exists in `wf.sql` and, when present, its exported row count.

| Legacy table | INSERT rows |
|---|---:|
| WfActivities | 52 |
| WfActivityControls | 108 |
| WfActivityMappingVariables | 17 |
| WfActivityTypes | 2 |
| WfAssignments | 493 |
| WfCategories | 12 |
| WfControls | 15 |
| WfDataTypes | 4 |
| WfOperators | 7 |
| WfPerformers | 39 |
| WfProcessData | 315 |
| WfProcesses | 7 |
| WfProcessVariables | 1,141 |
| WfRequestControls | 35 |
| WfRequestMappingVariables | 4 |
| WfRequests | 203 |
| WfRequestVariables | 0 |
| WfSteps | 47 |
| WfTransitions | 30 |
| WfUsersPerformers | 25 |
| WfUsersProcesses | 38 |
| WfVariables | 21 |

The two main sources are `LegacyWorkflowMasterDataSeeder` (definition/master data) and `WorkflowRequestTrackingSeeder` (requests, execution history, and print versions). Both are currently commented out in `DatabaseSeederService`, so these notes describe their behavior when enabled; they do not assert that those rows currently exist in the database.

## Definition and master data

- [WfCategories](WfCategories.md), [WfPriorities](WfPriorities.md), [WfProcessTypes](WfProcessTypes.md)
- [WfControls](WfControls.md), [WfDataTypes](WfDataTypes.md), [WfOperators](WfOperators.md), [WfActivityTypes](WfActivityTypes.md)
- [WfProcesses](WfProcesses.md), [WfSteps](WfSteps.md), [WfActivities](WfActivities.md)
- [WfVariables](WfVariables.md), [WfTransitions](WfTransitions.md)
- [WfRequestControls](WfRequestControls.md), [WfRequestControlsOptions](WfRequestControlsOptions.md), [WfRequestControlsValidations](WfRequestControlsValidations.md)
- [WfActivityControls](WfActivityControls.md), [WfActivityControlsOptions](WfActivityControlsOptions.md), [WfActivityControlsValidations](WfActivityControlsValidations.md)
- [WfRequestMappingVariables](WfRequestMappingVariables.md), [WfActivityMappingVariables](WfActivityMappingVariables.md)
- [WfPerformers](WfPerformers.md), [WfPerformerType](WfPerformerType.md), [WfUsersPerformers](WfUsersPerformers.md), [WfUsersProcesses](WfUsersProcesses.md)

## Runtime and history

- [WfRequests](WfRequests.md), [WfRequestDetails](WfRequestDetails.md)
- [WfAssignments](WfAssignments.md), [WfProcessData](WfProcessData.md), [WfActivityDetails](WfActivityDetails.md)
- [WfRequestVariables](WfRequestVariables.md), [WfProcessVariables](WfProcessVariables.md)

## Printing

- [WfPrintTemplates](WfPrintTemplates.md), [WfPrintTemplateVersions](WfPrintTemplateVersions.md), [WfRequestPrintVersions](WfRequestPrintVersions.md)

## Important boundaries

- Process Builder directly loads/saves process, variable, step, activity, control, option, validation, and transition tables.
- Mapping tables exist in the backend but are not currently maintained by `processBuilderApi.ts`.
- Runtime/history tables are created after request submission or processing; they are not required merely to save a process definition.
- Print tables extend a process but are not part of the executable workflow definition.
