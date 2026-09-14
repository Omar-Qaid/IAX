# Generic batch framework

The administration page is `/system-administration/batch-jobs`. It reuses the list/details pattern, shared task grid, parameter dialog and execution-history grid.

## Adding a service

All modules share the catalog initialized by `AddBatchFramework()` and the single hosted `BatchWorker`. There is no module enumeration or workflow-specific key in the worker. Workflow, Finance, Inventory, Communication, Integration, Identity, Organization and Reporting use the same registration contract and tables; those names do not imply that every corresponding business processor is implemented.

Implement `IBatchService.ProcessAsync` from `Shared.Application.Batch`, then register it in the owning module:

```csharp
services.AddBatchService<MyBatchService>("MyService", "My service");
```

Registration supplies scoped DI resolution and discovery through `GET /api/v1/SysBackgroundJob/services`. Keys also appear automatically in the existing handler selectors. Do not store CLR type names in task parameters or service keys. Legacy handlers are supported through a compatibility boundary.

Advanced registrations may set `managesCompanyScope: true` only when their implementation validates the execution account and establishes its authorized company scope before business queries. Default registrations remain default-company only. This flag is trusted module metadata, not an admin-editable authorization bypass. Set `supportsWholeJobRetry: false` for services whose retries would repeat non-idempotent work. Both policies are read by the generic registry/worker, without module-name checks.

Alternatively inherit the optional `BatchService` base (which implements `IBatchService`) and override `ProcessAsync`. The worker needs no modification:

```csharp
public sealed class MyBatchService : BatchService
{
    public override async Task<BatchExecutionResult> ProcessAsync(
        BatchExecutionContext context, CancellationToken cancellationToken)
    {
        // Invoke your injected application service here, using context.ParametersJson.
        // Return real counts; throw on failure and honor cancellationToken.
        throw new NotImplementedException("Implement business processing before enabling.");
    }
}
```

The task API uses `serviceKey`; the database column is `BatchJobTasks.ServiceKey`. Task selectors display registration names and keys. After deployment/restart, refresh Batch Administration to discover new registrations. No frontend registration or worker edit is required. Every `IBatchService` must implement `ProcessAsync`; the legacy background-handler adapter invokes this method. New services should not implement `ExecuteAsync` as their batch contract.

Create a disabled job using the `BatchTasks` runner, save, add ordered tasks and parameters, then enable the job. Dependencies must reference earlier enabled tasks. Task retries default to zero; enable them only for idempotent services. Completed tasks are not repeated by a later task's retry. A failed task stops subsequent tasks. The parent timeout covers the complete sequence and retry delays.

## Persistence and deployment

Apply the additional `BatchJobPriority` migration before deploying priority support. Low/Normal/High priority orders due jobs and waiting executions; it does not preempt running work. Existing jobs receive Normal priority.

UI lifecycle labels are Ready (active), Withhold (paused), Cancelled and Completed. Execution history separately shows Waiting, Executing, Completed, Failed and Cancelled. These are separate state machines. Disabled/withheld jobs do not dispatch pending executions. Cancel marks pending work cancelled but does not forcibly abort running business logic. Failed jobs expose `Retry (new run)` with confirmation because that action starts from the beginning; configured task retries remain local to the failed task.

Apply `AddBatchJobTasks`, `GenericBatchFramework`, then `BatchJobPriority` before starting the updated API. These three migrations were applied to local `ERM` on 2026-09-14; production was not changed. The framework migration renames existing job/history tables to `BatchJobs` and `BatchJobHistory`, tasks to `BatchJobTasks` (with `ServiceKey`), and task history to `BatchJobTaskHistory`; it adds `BatchSettings`. Existing CLR/API names remain compatible.

Run the normal settings seeder to provision recurring delivery jobs. Notification delivery now requires `BackgroundJobs:Enabled` as well as `Notifications:BackgroundServiceEnabled`. `BatchSettings` ID 1 supplies an operational enable switch and polling interval through the permission-protected settings API. Disabling scheduling does not abort work already running.

## Seeded activity examples

The settings seeder adds `Example - Activity notification delivery` (system, email and SMS task rows; 60-second recurrence) and `Example - Activity auto-pass` (900-second recurrence). Both are disabled and use empty JSON parameters because these services read their configuration from persisted activity/assignment records. Existing example records are never overwritten. Do not enable examples alongside the production jobs doing the same work.

For an activity with email and system notifications enabled and SMS disabled, request submission queues two `SysScheduledNotifications` records in its transaction. The `EmailDelivery` service consumes the email record; `WorkflowNotification` consumes the in-app record. No SMS record is queued. Actual transport requires configured providers and valid recipient details. This describes the request-submission path; the legacy activity alert dispatcher still calls the notification service directly.

For `IsAutoPassEnabled = true` and `AutoPassAfterHours = 24`, new assignments copy these values to `AutoPassing` and `AutoPassingHrs`. The timeout service checks for eligible open assignments at each sweep. With a 15-minute sweep, an assignment becomes eligible after 24 hours and is normally picked up at the next sweep, subject to backlog and worker availability. Changing the activity later does not rewrite existing assignments.

Important: the existing auto-pass handler marks assignments finished and publishes a notification event. It does **not** execute decision transitions or assign the next step. Full automatic approval/routing needs integration with the normal decision execution path; it must not be inferred from the finished flag alone.

## Legacy Process Builder scheduling retirement

Process Builder no longer exposes the schedule editor, and `/api/v1/WFProcessScheduled` and its registered service have been removed. Use Batch Administration for all new scheduling. `WorkflowRequestSubmissionBatchService.ProcessAsync` remains a scaffold for application-specific submission logic; legacy schedules are not automatically converted to that scaffold.

Deploy `RetireWorkflowProcessScheduling` before starting the updated application. It disables old schedule definitions and affected jobs/tasks and cancels pending executions. It preserves configuration, history and the `WFProcessScheduled` table. Its EF mapping lives in `Persistence/Legacy` with the original CLR namespace solely to preserve schema identity. Rollback does not reactivate jobs automatically. This retirement migration is prepared, not applied by this change.

## Operational boundaries

- SQL Server session application locks serialize schedule/manual queue creation and execution per job across worker instances. Different jobs can execute concurrently; executions of the same job are serialized even when `PreventOverlap` is false. Each running job consumes a dedicated lock connection. The database principal must be allowed to execute `sp_getapplock`/`sp_releaseapplock`.
- Cancellation polls persisted job state approximately every two seconds, then signals the token passed to `ProcessAsync`. It records Cancelled when the service exits cooperatively, and does not automatically retry cancelled work. Resuming waits for running work to stop. Completed external effects are not rolled back.
- Lock ownership is checked by the monitor; lost connectivity cancels work. Services must honor cancellation and use idempotency/fencing for external effects: a non-cooperative service cannot be forcibly stopped safely during a network partition. Recovery only marks running records abandoned after acquiring their job lock and rechecking status.
- Existing default-company execution boundaries remain. Any replacement submission service must implement company/actor validation. Do not schedule arbitrary cross-company task services without implementing their authorization context.
- Notification services reuse `SysScheduledNotifications` as the persisted delivery queue. This is not yet a replacement `CommunicationMessages` outbox. Workflow notification intent and transport are not fully separated into the proposed new schema.
- Email, SMS, push, other channels, in-app reminders/escalations, cleanup and existing workflow auto-pass are registered services. Timeout processing preserves the existing auto-pass policy.
- AX, employee and organization synchronization require actual integration contracts; no placeholder services report fake success.
- The page shows bounded recent history. Full browser/end-to-end delivery validation remains necessary; new labels currently use English.
- Existing unrelated workflow model changes remain pending outside these batch migrations.
