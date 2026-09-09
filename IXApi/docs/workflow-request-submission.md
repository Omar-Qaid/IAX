# Workflow Request Submission

Status: current behavior documentation and proposed functional design.  
Application: IAX / Workflow.  
Date: 2026-09-10.

## 1. Purpose

Define how a user submits a workflow request, how configuration should determine its execution, and how the system should display the main request and any child requests.

The submission mechanism must be generic, reusable, testable, and independent of specific license, permit, payment, or other business processes. Preserve the existing module structure and reusable infrastructure. Implement changes incrementally.

**Implementation boundary:** the current application saves dynamic requests and details, sends optional form-option alerts, and uploads attachments separately. Configured transition execution, assignment creation during submission, AND/OR condition groups, and child-request generation are proposed capabilities, not completed features.

See the [architecture analysis](workflow-architecture-analysis.md) for source findings, responsibilities, risks, and the current-to-proposed mapping. The [source inventory](workflow-source-inventory.md) lists Workflow source files.

See also the [Arabic deep review and enhancements](workflow-deep-review-and-enhancements.md), which records second-pass persistence findings and practical design refinements.

## 2. Current submission flow

### Entry points

- Frontend page: `IXApp/src/modules/workflow/pages/WfRequestFromPage.tsx`.
- Form component: `IXApp/src/modules/workflow/components/DynamicForm.tsx`.
- API adapter: `IXApp/src/modules/workflow/api/dynamicRequestFormApi.ts`.
- Backend controller: `Workflow/Requests/WfRequestController.cs`.
- Backend service: `Workflow/Requests/WfRequestService.cs`, method `SubmitDynamicAsync`.

Current endpoints:

- `GET /api/v1/WfRequest/form-definition/{processId}`: load the configured form.
- `POST /api/v1/WfRequest/submit`: validate and persist the submission.
- `POST /api/v1/WfRequest/validate`: separate validation implementation; its rules are not fully aligned with submission.
- `GET /api/v1/WfRequest/{requestId}/mail-details`: load authorized request details and tracking history.

### What happens today

1. The user chooses a process and opens its dynamic form.
2. The application loads active controls, options, validations, and presentation metadata.
3. The user fills visible controls and selects attachments.
4. The browser validates input and sends JSON control values and option-file metadata.
5. The server rejects duplicate or unknown controls, applies read-only defaults, determines visibility, validates configured rules, and calculates score.
6. Within a database transaction, the service generates the request code and saves `WfRequest`.
7. It batches and saves `WfRequestDetail` rows, including selected-option file metadata where applicable.
8. It sends alerts for selected options configured with alert messages and static performer recipients.
9. It commits and returns the request ID, code, score, and attachment-owner detail IDs.
10. The browser uploads file bytes separately against the saved request/detail records. Failed uploads are reported while the request remains saved.

```mermaid
flowchart TD
    A[User selects process] --> B[Load dynamic form]
    B --> C[Enter controls and select files]
    C --> D[Submit JSON values]
    D --> E[Server validation and scoring]
    E --> F[Begin transaction]
    F --> G[Save WfRequest]
    G --> H[Save WfRequestDetail rows]
    H --> I[Send configured option alerts]
    I --> J[Commit and return IDs]
    J --> K[Upload files separately]
```

Current submission does **not** initialize workflow variables, evaluate transitions, determine the first step, or create activity assignments. A successful response currently confirms request persistence, not workflow task delivery.

New submissions write relational details directly. Legacy XML is supported on read paths; the XML/view conversion in `IXApp/docs/request.txt` is historical reference code, not the current submission path.

## 3. Configuration and runtime records

### Workflow configuration

- `WfProcess`: process definition.
- `WfStep`: configured stage and completion policy.
- `WfActivity`: configured task, performer, SLA, and notification settings.
- `WfPerformer` / `WfPerformerUsers`: recipient-resolution configuration.
- `WfVariable` / `WfDataType`: declared variables and semantic types.
- `WfOperator` / `WfTransition`: comparison and routing configuration.
- Request/activity controls, options, validations, and variable mappings: form and data-transfer definitions.

### Runtime data

- `WfRequest`: one submitted process instance.
- `WfRequestDetail`: submitted request control values.
- `WfProcessVariable`: request-specific variable values intended for execution.
- `WfAssignment`: one runtime task assigned to an employee.
- `WfActivityDetail`: values entered while performing an activity.
- `WfProcessData`: activity completion/process data.
- Communication notification records: delivery intent, recipients, and delivery history.

`WfRequestVariable` also exists. Confirm its deployed consumers and meaning before consolidating it with `WfProcessVariable`.

## 4. Proposed generic submission pipeline

The shared execution service should perform the following operation:

1. **Authorize:** verify the actor, company, process eligibility, and permission to submit.
2. **Load configuration:** obtain an active, internally consistent process definition and its identity/version.
3. **Normalize and validate:** apply defaults, visibility, control types, options, required fields, configured rules, and attachment policy.
4. **Protect against duplicates:** establish an idempotency key for this submission.
5. **Begin the transaction:** one coordinator owns the complete database operation.
6. **Create the request:** generate its code and persist original request values.
7. **Initialize variables:** create runtime values from configured variable definitions.
8. **Apply mappings:** map normalized request controls to the correct variables.
9. **Evaluate transitions:** use typed comparisons and configured AND/OR groups.
10. **Select the next state:** determine a target step, explicit completion, or a defined no-match outcome.
11. **Resolve activities and performers:** obtain all required active tasks and eligible employees.
12. **Create assignments:** stage assignments in batches with copied runtime SLA settings.
13. **Prepare notifications:** persist delivery intents alongside workflow state.
14. **Commit:** save the operation atomically and return the execution result.
15. **Deliver notifications:** dispatch committed intents through the existing Communication channels with retry and deduplication.

```mermaid
flowchart TD
    A[Authorize and validate] --> B[Begin transaction and establish idempotency]
    B --> C[Create request and details]
    C --> D[Initialize and map variables]
    D --> E[Evaluate typed AND/OR conditions]
    E --> F{Routing outcome}
    F -->|Target step| G[Load activities and resolve performers]
    G --> H[Create assignments]
    F -->|Explicit completion| I[Complete request]
    F -->|Invalid configuration| X[Rollback and return error]
    H --> J[Persist notification intents]
    I --> J
    J --> K[Commit and return result]
    K --> L[Deliver queued notifications]
```

Normal requests, automatic triggers, and child requests must use the same execution components. Do not maintain separate copies of submission logic.

## 5. Variables and typed evaluation

The evaluator reads an actual value from the request's runtime variables, interprets it using the variable's declared data type, and compares it with the configured transition operand using a supported operator.

```text
Submitted control
    -> configured control-variable mapping
    -> WfProcessVariable value
    -> declared WfDataType + WfOperator + expected value
    -> condition result
    -> group result
    -> transition destination
```

### Existing support and gaps

- Current submission has no connected workflow transition evaluator for any type.
- Legacy reference code calls `ValidateValues<long>` for Integer and `ValidateValues<string>` for String.
- Legacy Boolean and DateTime branches are empty.
- The implementation of `ValidateValues<T>` was not found in the reviewed source; its exact operator behavior is unverified.
- Backend seeds define type IDs 1–4 as Integer, String, DateTime, Boolean. Process Builder currently maps those IDs as Text, Number, Boolean, Date. Resolve this mismatch before enabling execution.

### Proposed rules

- Integer/Decimal: equality, inequality, ordering, and inclusive Between with two operands.
- String: equality, inequality, Contains, and explicit empty/present checks with documented case handling.
- Boolean: equality/inequality using canonical true/false values.
- DateTime: comparisons and ranges using a documented UTC/offset contract; date-only values require their own explicit semantics.
- Reject unsupported types/operators, malformed operands, and invalid configuration. Do not silently treat evaluation errors as false or default to equality.
- Preserve declared types: a numeric-looking String must not automatically become a number.
- Resolve semantic type/operator codes from configuration; avoid assuming database IDs or editable labels define behavior.

## 6. Configurable AND and OR conditions

Each transition owns a condition group with a selectable evaluation mode:

- **All conditions (AND):** every condition must match.
- **Any condition (OR):** at least one condition must match.

Support nested groups when both modes are needed in one route.

Example:

```text
Target step: Senior approval

All conditions (AND)
    Amount > 10000
    Any condition (OR)
        Department = Finance
        Department = Legal
```

This represents:

```text
Amount > 10000 AND (Department = Finance OR Department = Legal)
```

Proposed storage uses `WfTransitionConditionGroup` and `WfTransitionCondition` under the existing transition. Groups define All/Any and parent grouping; conditions define variable, operator, operands, and order. AND/OR are group modes, not comparison operators in `WfOperators`.

Transition priority remains separate from grouping. The proposed first-match policy evaluates candidate transitions by `(SortOrder, RecId)` and selects the first matching route. Confirm this policy before implementation; OR does not mean execute every matching destination.

Reject empty groups, cycles, cross-process references and invalid conditions. Use an explicit fallback rule rather than interpreting an empty group as success. Save a transition and its groups atomically, and preserve grouping through designer drafts, API round trips, and import/export.

## 7. Activities, performers, and assignments

A step can contain multiple activities. Resolve performers from configuration, including:

- Request employee/applicant.
- An employee selected in a request field.
- A configured manager level.
- Explicitly configured employees.

Reuse Organization's `HcmWorkerManager` hierarchy. Distinguish employee IDs used by assignments from account user IDs used by notifications.

Each required activity must either resolve valid performers or return an explicit configuration failure. Do not leave a request silently unassigned. SQL-based performers remain unsupported until a scoped, allowlisted and parameterized resolution contract is defined.

On assignment completion, the same executor validates actor/concurrency, persists activity values and completion data, updates variables, applies activity/step completion policy, evaluates the next route, and creates subsequent assignments or completes the request.

`WfStep.MustCompleteAll` and multiple-recipient completion rules must be defined independently of transition AND/OR conditions. The current auto-pass job must eventually call the shared completion operation rather than only setting assignment finish flags.

## 8. Child requests using different processes

A main request may generate one or more child requests. Each child may use a different process and has its own form, variables, steps, assignments, notifications, and history.

```text
Main request: License process
    Child request: Inspection process
    Child request: Payment process
```

The main request retains its process. A child receives the configured target process and executes through the same workflow engine.

### Required configuration

- Source activity and invocation trigger.
- Target process.
- Explicit parent-value to child-control input mappings.
- Requester/employee policy and initiating actor audit.
- Continue independently or wait for child completion.
- For several children: wait for all required children or any successful child, with an explicit policy for remaining children.
- Failure, retry, cancellation, and optional child-output to parent-variable mappings.

Use a runtime relationship such as `WfRequestLink` to record parent, child, source execution occurrence, configuration identity, invocation key, and dependency state. Enforce idempotent creation: retries must not generate duplicate children. Multiple legitimate children may use the same process.

Default to the same company. A different process must not bypass target-process eligibility, request access, or attachment permissions. Bound nesting and detect unsafe recursive invocation configurations.

Child completion resumes a waiting parent exactly once through normal engine progression. Do not rely solely on an in-memory notification event. Parent cancellation must have an explicit child policy; retain historical records.

## 9. Main and child requests on one page

The unified request page should show the main request and expandable child sections together. Users should not need another page to inspect a child's details.

```text
Main request header: code, process, requester, status, dates
    Original details and attachments
    Completed stages
    Pending assignments

    Child request: Inspection
        Status and whether it blocks the main request
        Original details and attachments
        Completed/pending stages and performers

    Child request: Payment
        Status and whether it blocks the main request
        Original details and attachments
        Completed/pending stages and performers
```

Allow multiple expanded children, show nested children under their parent, and load detailed content on demand. Each stage should expose performer, assignment date, completion date, outcome, controls, and documents. Preserve all parallel pending assignments.

Reuse common request-detail/history/attachment components for every process. Every action must identify the request/assignment it affects. Enforce permissions per child; access to the main request alone does not grant child access. A combined timeline may be offered, with request/process identity attached to every event.

## 10. Transactions, attachments, and notifications

- One coordinator owns the database transaction. Helpers stage changes; they must not independently commit a parent operation.
- Batch detail, variable, and assignment writes. Keep necessary saves for generated IDs; avoid saves per control or recipient where practical.
- Persist request state and notification intents together, then dispatch externally after commit. The current synchronous alert delivery inside submission is a reliability concern.
- Use idempotency keys and concurrency checks for submission, child creation, assignment completion, and background triggers.
- Enforce configured uniqueness with a database-backed mechanism under a documented company/control scope; a read-before-write check alone is insufficient.
- Actual attachment uploads currently happen after request commit. Before making mandatory files a prerequisite for workflow activation, define staging/finalization and recovery behavior. Metadata alone does not prove files were uploaded.
- Failed delivery must be distinguishable from failed submission. Queued notification intent is not delivered email/SMS.

## 11. Execution result and errors

Preserve the existing public result fields: request ID, code, score, and attachment owners. Extend the result compatibly when runtime execution is introduced to expose status, selected/current step or assignments, and notification-queue state where useful.

Return structured validation errors with control/condition identifiers and stable error codes. Distinguish input errors, eligibility failures, invalid configuration, concurrency conflicts, and unexpected failures. Log request/process/assignment and correlation IDs without routinely logging complete submitted values.

Do not report a request as assigned when no assignments were created, a child as created before its relationship is committed, or a notification as delivered merely because it was queued.

## 12. Implementation order

1. Characterize current submission, details, scoring, visibility, attachments and legacy reads.
2. Align type/operator contracts and unify authoritative server validation.
3. Extract focused form, query, and execution responsibilities while preserving existing API behavior.
4. Establish transaction ownership, idempotency, eligibility and durable notification intents.
5. Add typed variable mapping and AND/OR evaluation with atomic configuration storage.
6. Connect step selection, performer resolution and assignment creation.
7. Add completion/progression and route auto-pass through the same engine.
8. Add configured child invocation, dependency policies and parent-child history.
9. Compose the unified main/child page from shared request components.

Enabling workflow routing is new functionality in this checkout, not only a behavior-preserving refactor.

## 13. Acceptance criteria

- A valid request persists its details and starts the configured workflow atomically under the agreed attachment policy.
- Invalid submissions leave no partial runtime state or externally delivered pre-commit alerts.
- AND, OR, nested groups and route ordering produce deterministic results using declared data types.
- Unsupported comparisons and missing required performers return clear errors.
- Duplicate submission/completion/child triggers do not create duplicate runtime effects.
- Assignments respect company, actor, performer and completion policies.
- Child requests can run different processes, receive mapped inputs and report completion under configured wait policies.
- Main and child details, stages and authorized attachments appear on the same page with accurate independent statuses.
- Existing form, localization, legacy history and shared UI behavior remain compatible.

## 14. Decisions required before enabling execution

Confirm no-match/default routing, explicit terminal-state representation, first-match versus fan-out behavior, multiple-recipient completion, mandatory-file activation rules, missing-manager behavior, definition snapshot/version policy, child outcome/wait/cancellation policies, and eligibility for automatic submissions.

These decisions should be configuration or documented engine policies. Do not introduce process IDs, form labels, or business-specific conditions into the generic executor.

## 15. Refinements from the deeper review

- Produce one normalized value set for visibility, validation, uniqueness, scoring, variable mapping and persistence. The current omitted-default uniqueness gap must be covered by a regression scenario.
- For mandatory files, consider a prepared request followed by an idempotent activation operation after authorized uploads are verified. Keep ordinary submissions seamless when no staged files are required.
- Distinguish execution state, business outcome, and waiting reason; completed child work is not necessarily successful business approval.
- Associate assignments and child invocations with the specific stage execution occurrence, so retries and late results cannot advance another occurrence.
- Validate and version complete workflow definitions before publication; provide a read-only condition/routing preview using the actual evaluator.
- Establish trusted company/actor context for background commands; existing HTTP-derived context is insufficient as an implicit multi-company job contract.
- Verify EF mappings before relying on ordering or concurrency. Runtime variables ignore `RowVersion`/`SortOrder`; mapping orders are also ignored in the relevant current configurations.
- Preserve the legacy completion-data relationship: `WfActivityDetail.ProcessId` maps to `TaskID` and is used for a `WfProcessData` identity in seeding. `WfProcessData.ActivityDetails` remains XML-typed.
- Define explicit child input/output ownership and conflict handling; do not synchronize all parent/child variables automatically.
- Add operational recovery for a failed command or delivery rather than restarting an entire request and repeating its effects.

These are proposed requirements and verified source caveats, not completed implementation. Detailed evidence, priorities and failure scenarios are in the linked deep review.
