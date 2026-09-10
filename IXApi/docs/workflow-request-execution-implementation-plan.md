# Request execution: configuration audit and implementation plan

Date: 2026-09-10. Baseline: working tree, including the earlier uncommitted submission and notification changes. This document was created before implementation for this review.

## 1. Start with request submission

The supported entry point is WfRequestController.SubmitDynamic -> WfRequestService.SubmitDynamicAsync. DynamicForm submits JSON through dynamicRequestFormApi, then uploads file bytes after the request commits. The attached request.txt is historical design reference: its view/XML conversion, numeric type IDs, sentinel steps and repeated SQL/save loops must not replace the current implementation.

Target sequence:

1. Resolve trusted actor/company; load an active process and its controls.
2. Normalize values once, reject foreign/duplicate IDs, calculate visibility, validate visible values and option metadata.
3. Build an execution plan from variables, mappings, ordered submission transitions, destination activities and resolved employees. Configuration failures should happen before request/runtime writes.
4. Begin the submission transaction; generate request code and save the request.
5. Save immutable submitted values and control labels; stage runtime variables and assignment rows in batches.
6. Stage notification delivery intents using the existing Communication infrastructure once its ownership and retry gaps are closed.
7. Commit; return request code, score, attachment owners, starting step and assignment IDs. File uploads remain separate until an explicit activation contract exists.
8. Background execution processes committed delivery and later workflow commands under trusted company/actor context, with durable occurrence identity.

The current first-match/no-match-error policy is preserved. This review does not interpret the legacy -1/0 step sentinels as authorization to introduce fallback, completion or fan-out semantics.

## 2. Configuration map

Paths below are relative to IXApi unless prefixed IXApp. Transactions means both database transaction ownership and configured workflow transitions; they are separate concerns.

### Process

Definition: src/Modules/Workflow/Processes/WfProcess.cs, WfUsersProcess.cs; Builder types and api/processBuilderApi.ts in IXApp/src/modules/process-builder.

| Settings | Read/application | Passed downstream | Gaps/conflicts |
| --- | --- | --- | --- |
| Name, description, active/deleted state, company | GetFormDefinitionAsync reads active process; scoped EF filters apply company/deletion | Form heading, request name/details, request company | Eligibility is not established merely by process visibility |
| CategoryId, PriorityId, ProcessTypeId, SortOrder, Score, IsSystemDefined | Master/Builder persistence and presentation | Configuration records | Startup does not select routing based on these fields; request score is calculated from submitted controls |
| IsRepeatable, RepeatIntervalHours | Stored on process; Builder save paths | Process record | No authoritative repeat/idempotency enforcement in submission; lifetime versus interval semantics need definition |
| MandatoryDocuments | Stored on process | Form/design configuration | Submit payload has no verified uploaded-document stage; cannot claim mandatory file activation is enforced |
| Employee/occupation/department eligibility | WfUsersProcess relational records; NotMapped process lists | Process setup | Submit does not evaluate these restrictions; precedence and administrator behavior need explicit policy |

### Request controls

Definition: Requests/WfRequestControl.cs, DynamicRequestFormDtos.cs, WfRequestControlsOption/Validation records. Reader: GetFormDefinitionAsync and PrepareSubmissionAsync. Frontend: DynamicForm and DynamicControlRenderer.

| Settings | Applied behavior | Passed downstream | Gaps/conflicts |
| --- | --- | --- | --- |
| ControlId, code, name/alias, order, score | Definition and display; detail snapshot uses ControlLabel/ControlLabelAlias | Prepared visible controls and WfRequestDetail rows | Old rows fall back to current labels; no complete definition-version snapshot |
| ExtendedProperties: required, readOnly, defaultValue, visibility, layout, uniqueKey | Parsed into runtime DTO; normalization and visibility precede validation | One normalized value dictionary; visible detail values | Metadata comments still mention XML while current builder uses JSON |
| Option values/labels/score; show controls | Selection checks, option-driven visibility and scoring | Detail values and total request score | Hidden controls must not map into execution variables |
| Option required upload | Validates file metadata | Attachment-owner detail IDs | Metadata is not proof of file upload |
| Option alert message/performer IDs | Submit currently calls central notification service inside transaction | Notification recipients | Static performer IDs are employee IDs; conversion to account IDs must be explicit; external delivery before commit is a reliability gap |
| Validation type/value/expression/operator/severity | Shared PrepareSubmissionAsync for submit and validate-submission | Structured field errors; Error blocks save | Legacy /validate remains separate; unknown rules can pass; browser/server rule parity is incomplete |
| CanFilter/CanGroup/CanSort/ReferenceType/FieldRole/DataType/DefaultAggregation | Reporting metadata | Reporting consumers | Not transition-variable types and must not be used as a substitute |

### Variables

Definition: Variables/WfVariable, WfDataType; Requests/WfRequestMappingVariable; runtime: Processes/WfProcessVariable.

| Settings | Applied behavior | Passed downstream | Gaps/conflicts |
| --- | --- | --- | --- |
| ProcessId, DataTypeId, active state, order | Startup loads active variables and semantic type codes | Runtime value per active variable | Runtime SortOrder/RowVersion are ignored by EF; do not order queries by ignored fields |
| RequestControlId -> VariableId | Visible normalized values populate variables | Evaluator inputs and persisted values | Multiple inputs per variable rejected; one control may feed multiple variables; mapping SortOrder is ignored by EF |
| INT/DEC/STR/BOOL/DT | WorkflowConditionEvaluator | Typed route result | Numeric-looking strings retain string semantics; date-only means UTC midnight; datetime requires an offset |
| WfRequestVariable | Separate existing entity | No startup consumer identified | Do not duplicate runtime writes into it without identifying consumers |

### Transitions, steps, activities and database transaction

Definitions: Transitions/WfTransition; Steps/WfStep; Activities/WfActivity; Performers/WfPerformer and WfPerformerUsers. Runtime: WfRequestService.StartWorkflow.cs. Transaction: Infrastructure/Persistence/Repositories/UnitOfWork.cs.

| Settings | Applied behavior | Passed downstream | Gaps/conflicts |
| --- | --- | --- | --- |
| VariableId, OperatorId, Value, SortOrder, StepId | Evaluates scalar submission routes; selects first match by SortOrder/RecId | Selected step | Current implementation evaluates later routes too; invalid later configuration may block an earlier match |
| ActivityId / RequestControlId trigger | Activity routes excluded at submission; control triggers require a visible process control | Candidate routes | Distinct IDs/scopes; do not confuse activity with activity-control validation |
| AND/OR additional conditions | Builder browser draft/export | No runtime contract | Server save rejects compound groups; nested group persistence/evaluation is not implemented |
| Step active/process identity, MustCompleteAll | Selected step validated; active activities loaded | Assignment step IDs | MustCompleteAll applies to completion, not startup; completion engine missing |
| Activity performer, score, auto-pass enabled/hours | Resolves employees; copies score and SLA to assignments | WfAssignment | Activity type does not yet execute API/notification actions; all active activities currently create human assignment rows |
| Activity mandatory documents / history flags | Persisted Builder/backend configuration | Activity record | Not a submission-stage completion policy |
| Performer type, applicant/employee/related field, manager flags, memberships | Resolves active employees and management levels | Employee IDs in assignments | SQL types rejected; explicit memberships union with subject rules; no completion quorum semantics |
| Begin/Complete/Commit/Rollback; EF retry | Request/details/variables/assignments share transaction | Committed IDs | Configuration is currently checked after some writes; request object lives outside retry closure; durable idempotency and database uniqueness absent |

### Notifications

Definitions/readers: Communication/Notifications/CreateSysNotificationDto, SysNotificationService, SysNotificationTemplate, recipient/preferences/audit entities; Workflow/Activities/WfActivityNotificationDispatcher.

| Settings | Applied behavior | Passed downstream | Gaps/conflicts |
| --- | --- | --- | --- |
| Activity SysNotificationTemplateId, four channel flags | Dispatcher resolves template code and sends selected channels; PreserveChannel prevents template overriding explicit InApp | Central DTO with account IDs | Template missing from Builder activity model; assignment-created path does not call dispatcher |
| Template subject/body/placeholders/default channel/category/priority/icon | Central service renders; defaults apply | Stored notification and recipient records | Persist exact selected channel in delayed work; template existence must be validated |
| Recipient preferences | Central service filters channel recipients | Sender per permitted recipient | Suppressed and failed delivery are different outcomes |
| Sender success, audit, realtime unread count | Central service stores recipient delivery status, audit, realtime updates | UI/provider | Scheduled worker treats returned failure as completed unless an exception occurs |
| AppNotificationDrawer / NotificationProvider | Drawer uses MOCK_NOTIFICATIONS; provider uses transient toast store | Local display | No authenticated persistent notification inbox connection in the inspected drawer |

### Background and Scheduled

Definitions/readers: Administration/BackgroundJobs entities, SysBackgroundJobManager/Processor/Registry/ScheduleCalculator; Communication/Notifications/Services/SysScheduledNotificationService.cs; Workflow/Execution/WfActivityAutoPassJobHandler.cs; IXApp ProcessScheduleSettings.tsx.

| Settings | Applied behavior | Passed downstream | Gaps/conflicts |
| --- | --- | --- | --- |
| General JobKey, enabled/status, one-time/interval/CRON/run-at, next run | Existing scheduler finds handler and records executions | SysBackgroundJobContext | Reuse this scheduler; do not add another recurring workflow scheduler |
| PreventOverlap, retries/backoff, timeout, payload, TenantId | Processor supplies job context and cancellation/progress | Scoped handler services | TenantId in a DTO does not set CompanyExecutionContext; absent HTTP defaults company; cross-instance claim behavior needs verification |
| Notification SendAt/type/status/recurrence/max occurrences/escalation read check | Hosted worker polls 50 pending jobs each minute; retries exceptions 2/4/8 minutes | CreateSysNotificationDto then central service | No company/actor fields on scheduled notification entity; no atomic claim in selection loop; duplicates possible on send/crash/retry |
| Auto-pass copied hours/enabled | Checks full elapsed duration; excludes stopped/finished requests; saves finish then publishes event | Activity dispatcher via event handler | Event delivery is not durable; finishing flags does not perform routing/progression |
| Builder Process Scheduled frequency/start/timezone/owner/source/mappings | Local storage per process; draft key migrated after save | Browser draft/export only | No server contract; arbitrary tables must not become SQL execution; source allowlist and authorization required |

## 3. Reviewed implementation checkpoints

Implement in order using existing owners. An item is not complete solely because it compiles.

- [ ] A. Submission execution planning: split reading/evaluation/performer resolution from persistence, preflight before runtime writes, reuse the resulting plan for batch persistence. Preserve API, scoring, visibility, scalar routing and attachment behavior.
- [ ] B. Builder template round trip: expose existing SysNotificationTemplateId through activity model, both save paths and existing lookup pattern. Preserve unset versus explicit clear semantics and validate references using the current template API.
- [ ] C. Notification policy tests: verify explicit selected channel survives a conflicting template default; verify dispatch honors disabled channels and maps intended template/context without invoking live senders.
- [ ] D. Integration verification and documentation: run focused backend/frontend tests, compile affected projects, review final diffs, update this plan with evidence and limitations.

The following are designed dependencies, not safe assumptions to silently enable in this checkpoint:

- E. Durable notification scheduling: extend the existing scheduled record with trusted company/actor and occurrence identity; atomically claim records; persist send outcomes and reuse notification identity on retry. Queue assignment and option intents in submission transaction; eliminate pre-commit send calls only when delivery is connected and tested.
- F. Recurring submission: define source allowlist, actor eligibility and recurrence/DST/missed-run policy; then register a handler in existing scheduler and call the same submission coordinator with an idempotency key.
- G. Complete workflow progression: agree terminal/fallback/quorum/attachment activation policies, then route human completion and auto-pass through one transactional executor. Do not claim auto-pass currently implements this.
- H. Notification inbox: connect existing drawer to authenticated list/read/archive APIs and realtime events; keep transient toast infrastructure independent.

These dependencies require database-backed concurrency/rollback and identity tests. No live database credentials, approved runtime fixtures or external sender configuration are assumed from the IDE tabs. Schema migrations may be prepared, but database application and actual delivery must be reported separately.

## 4. Acceptance and test matrix

| Scenario | Required evidence |
| --- | --- |
| Valid submitted values and labels | Same prepared snapshot used for details, variables and scoring |
| Invalid mappings/routes/performers | Planning fails before request/detail/runtime repository writes |
| Hidden mapped field | Does not populate runtime value |
| Scalar types, ordering, no match | Deterministic result or explicit configuration error |
| Template + channels | Load/save round trip; channel precedence test with template default conflict |
| Database failure/retry | Atomic rollback and no duplicate variables/assignments/delivery; live integration gate |
| Background company/actor | Two-company isolation, missing actor rejection; integration gate |
| Concurrent scheduled workers | Claim/lease and duplicate suppression; integration gate |
| Email/SMS/provider failure | Failed status and retry, distinct from submission failure; provider/mock integration gate |

## 5. Execution log

User clarification: persist recurrence in a new table named `WFProcessScheduled`, configure it from Process Builder and activate it through existing background execution. The schedule must store company, execution account, recurrence and source mappings. Saving configuration alone must not be described as recurring execution. The account saving/activating the schedule is the initial explicit execution account; background work must validate that account and establish a scoped identity. Arbitrary SQL/table access is not authorized by a table name in the draft.

Notification inbox review: the existing list query excludes archives. Add an optional archive filter with the current non-archived default preserved, then connect the drawer to paged current-user list/read/archive/delete operations. Refresh while open; do not claim a SignalR subscription where none exists.

Plan reviewed against current entity/DTO/adapter/transaction/worker sources before implementation. Existing uncommitted changes are retained. Checkpoint evidence will be appended as work completes.
