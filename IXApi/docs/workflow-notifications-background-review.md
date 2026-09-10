# Workflow, Process Builder, Notifications and Background Execution

Current implementation update (2026-09-10): see [request execution implementation plan](workflow-request-execution-implementation-plan.md), section 5, for implemented behavior and remaining runtime gates. The inspection table below records the earlier baseline. Builder now persists schedules through `WFProcessScheduled`, exposes activity templates, and the drawer uses central notification APIs. Submission stages alerts in the existing scheduled-notification queue; its worker now carries company/account context and uses claim leases. No database migration or external delivery has been performed by this review.

Review date: 2026-09-10. Status: source review and initial corrections; full integration is not complete.

## Existing connections

| Configuration or component | Existing behavior | Remaining connection |
| --- | --- | --- |
| AppNotificationDrawer | Uses local MOCK_NOTIFICATIONS state; the separate NotificationProvider displays transient toast messages | Connect authenticated central notification list/read/archive operations and realtime updates; backend delivery alone does not populate this drawer |
| Builder activity notification switches | API adapter loads and saves InApp, Email, SMS and WhatsApp flags | Submission startup does not dispatch activity notifications yet |
| Activity notification template | Backend dispatcher resolves SysNotificationTemplateId and sends by template code | Builder activity model does not expose the template selection; verify preservation during all activity save paths |
| WfActivityNotificationDispatcher | Reuses central SysNotificationService and activity channel switches | Queue assignment-created delivery rather than call external senders inside submission |
| Central notification service | Renders templates, resolves account recipients, applies preferences, stores notification/recipient rows, invokes senders and updates realtime unread counts | Delivery failure returned by a sender is different from an exception; callers must not equate completion with delivery |
| SysNotificationBackgroundService | Registered hosted service; polls persisted scheduled notifications every minute; retries exceptions with 2/4/8 minute delays | Extend existing scheduling for trusted company context and atomic claiming/deduplication before using it as reliable workflow delivery |
| SysBackgroundJobProcessor | Existing job registry, schedules, execution history, progress, timeout and retry infrastructure | TenantId is passed in job context; the inspected execution path does not establish a company/actor context for scoped workflow queries |
| WfActivityAutoPass | Finishes due assignments and publishes notification events after saving | It does not execute activity/step completion policies or create subsequent assignments; persisted completion and event delivery are not atomic |
| Process Scheduled in Builder | Browser draft configuration | No server schedule registration or recurring request creation is connected |

## Initial corrections

1. Activity dispatch explicitly preserves the selected channel. A template whose default is Email must not turn an explicitly enabled InApp channel into Email. Existing callers retain template-default behavior unless they opt in.
2. Auto-pass notification resolves HcmWorker.UserId from the assignment employee ID. An employee ID string is not an identity account ID. Missing accounts are skipped.
3. Auto-pass checks AssignDate + configured hours against UTC now. Counting crossed hour boundaries could finish a 10:59 assignment at 11:00 for a one-hour SLA.
4. Auto-pass excludes inactive/deleted assignments and inactive/deleted, stopped or finished requests.

## Integration sequence

The visible notification drawer must also be connected to the authenticated central notification APIs. Keep persistent inbox records separate from transient toast messages. Verify user-specific unread counts and read/archive actions before claiming end-to-end delivery in the UI.

1. Preserve and expose the configured activity notification template in Builder, using the existing notification-template lookup.
2. Extend existing scheduled notifications with company/actor ownership and a stable workflow occurrence key. Use a claim/lease to prevent two workers processing the same occurrence concurrently.
3. Stage assignment-created delivery records in the same transaction as request, details, variables and assignments. Resolve employee IDs into identity account IDs; store the configured channels and template context.
4. Process committed delivery through the central notification service. Track suppressed, failed and delivered outcomes separately. Retry must reuse the delivery identity rather than create duplicate notifications.
5. Move auto-pass and human completion through one completion operation with concurrency checks, step policy and route progression. Persist follow-up delivery with completion state.
6. Persist Builder recurring-process configuration and register it with the existing job manager. Run each occurrence under a trusted company and actor context and an idempotent submission key.

No new background framework is needed. Do not treat the existing scheduled-notification table as a complete outbox: the reviewed worker sends before marking completion, has no atomic claim in its selection loop, and retries exceptions rather than every failed channel result.

## Verification boundary

Compilation and characterization checks do not prove hosted workers are running, providers are configured, notifications reached accounts, or concurrent executions are deduplicated. Those require database-backed and authenticated runtime checks.
