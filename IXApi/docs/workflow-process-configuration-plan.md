# Workflow Process Configuration: Analysis and Implementation Plan

Date: 2026-09-10. Status: C1 contract repairs and characterization are in progress; C2-C10 are not implemented.

## Current execution status

The user authorized continuing through the plan without checkpoint approval prompts. Individual settings or JSON fields do not establish runtime support; completion requires the acceptance gates below.

- **Implemented:** semantic data-type and operator resolution, shared operator selection in both editor panes, option identity/value/metadata preservation, malformed metadata protection, visibility checks and deletion cleanup, record ownership preflight, and generated-ID transition characterization.
- **Backend changes:** transition create/update/bulk-update validate process ownership of variables, target steps and triggers. DTO validation rejects invalid and dual triggers. Existing transitions cannot be moved to another process through the service.
- **Still required for C1:** deployed lookup and historical definition verification, complete numeric/date/range semantics, remaining master-service ownership checks, full-save failure/deletion characterization, unsupported-setting diagnostics, and live save/reload checks.
- **C2-C10 remain pending:** aggregate transactions, server drafts/publication/version pinning, expanded form policies, grouped rules/simulation, execution/approvals, SLA, subprocesses, automation, reporting and AI. No schema migration or runtime activation was performed.
- **Environment:** a development database/test environment has been requested for authenticated round trips and migration verification. Code and automated validation continue independently.

### Latest persistence repairs

Request option saves preserve unknown JSON properties and remove superseded legacy flag aliases so disabled features remain disabled. Invalid stored option JSON fails before form writes. Blank activity option labels no longer shift aliases or feature metadata; inactive options omitted from the editor remain stored.

Full-save ownership checks run before the process header write. Section saves reject stale/foreign identities in their relevant parent scope. These checks do not provide a transaction or concurrency guarantee. A full-save adapter regression verifies generated variable, step, activity and request-control IDs reach both transition trigger kinds.

Validation: 105 frontend tests across 12 Process Builder/loader files passed; the production TypeScript/Vite build and focused ESLint checks passed. The backend and five focused DTO tests passed with isolated build output after the normal test build encountered DLL locks from the running application. The existing Vite large-chunk warning remains. These are automated checks, not authenticated CRUD or database-provider verification of the service queries.

## 1. Scope and documentation basis

Update the existing Process Builder so business users can configure the requirements in [Workflow Request Submission](workflow-request-submission.md), including its nine submission requirements and 61 extended requirements. Use the [business proposal](workflow-business-analysis-product-proposal.md) for product ownership and priorities.

Supporting documentation reviewed includes the [workflow architecture analysis](workflow-architecture-analysis.md), [deeper review](workflow-deep-review-and-enhancements.md), [Process Builder integration](../../IXApp/docs/process-builder.md), and [module README](../../IXApp/src/modules/process-builder/README.md). Related design constraints are recorded in the [AI implementation plan](../../AIWorkflowProcessBuilderPlan.md), [request display specification](../../request-from.md), and [print-template design](dynamic-print-template-engine.md). The existing AI plan already proposes canonical definition contracts and transactional creation; converge on those capabilities instead of building a second configuration engine.

This is a focused review of relevant workflow Markdown and selected current source paths, not an audit of every Markdown file or every runtime capability in the repository. Existing architecture reports provide wider findings; those not independently checked here remain prior-review evidence.

Preserve the existing module paths, compact editor layout, Arabic/English support, current integrations, saved data, and uncommitted work. Configuration development must distinguish a setting that can be saved from a capability that the runtime actually executes.

## 2. Findings checked against current source

| Finding | Evidence | Consequence for the plan |
| --- | --- | --- |
| The established editor covers process metadata, variables, request forms, steps, activities, activity forms, transitions, and diagram state. | [Builder types](../../IXApp/src/modules/process-builder/types/processBuilderTypes.ts), [integration guide](../../IXApp/docs/process-builder.md) | Extend the current workspaces and contextual settings rather than replace the editor. |
| Original finding: Builder's hard-coded type IDs disagreed with backend seed semantics. The first C1 slice replaced those IDs with catalog-code resolution and rejects unsupported Object mappings. | [Builder adapter](../../IXApp/src/modules/process-builder/api/processBuilderApi.ts), [WorkflowSeeder](../src/Infrastructure/Persistence/Seeding/Chunks/WorkflowSeeder.cs) | Application mapping corrected; deployed lookup data and historical definitions still need verification. No stored values were migrated. |
| A transition DTO carries one variable, operator, and value with a target step; it has no nested group contract. | [WfTransitionDto](../src/Modules/Workflow/Transitions/WfTransitionDto.cs) | Introduce explicit AND/OR groups and typed operands with backward-compatible conversion. |
| Full builder save writes process metadata, variables, controls, steps, activities, and transitions through separate API calls. | `saveProcessBuilder` in [builder adapter](../../IXApp/src/modules/process-builder/api/processBuilderApi.ts) | A later failure can leave earlier calls committed. Add a server-owned aggregate operation before publication or complex graph editing. |
| Designer drafts use localStorage. Process DTO has no explicit publication version or effective-date fields. | [Draft hook](../../IXApp/src/modules/process-builder/hooks/useProcessBuilderDraft.ts), [WfProcessDto](../src/Modules/Workflow/Processes/WfProcessDto.cs) | Browser recovery, server drafts, and immutable published definitions need separate semantics. |
| Builder activity types include assignment modes and action definitions; the activity DTO has performer, notification, document, auto-pass, and extended-properties fields, but no corresponding typed decision/completion contracts. | [Builder types](../../IXApp/src/modules/process-builder/types/processBuilderTypes.ts), [WfActivityDto](../src/Modules/Workflow/Activities/WfActivityDto.cs) | Audit every displayed option through load/save/runtime. Do not equate designer state or opaque JSON with implemented approval behavior. |
| Step completion exposes `MustCompleteAll`; performers expose applicant/employee flags, four manager levels, and configured users. | [WfStepDto](../src/Modules/Workflow/Steps/WfStepDto.cs), [WfPerformerDto](../src/Modules/Workflow/Performers/WfPerformerDto.cs) | Separate activity recipient quorum, step completion, and routing conditions; introduce generic resolution policies incrementally. |
| Submission persists requests/details, sends option alerts, and commits; this inspected path does not initialize variables or create workflow assignments. | `SubmitDynamicAsync` in [WfRequestService](../src/Modules/Workflow/Requests/WfRequestService.cs) | Configuration delivery needs a separate execution integration gate. Saving a process must never create runtime requests or assignments. |

No authenticated CRUD, database inspection, build, or runtime tests were performed for this planning task. These are source findings, not live-system verification.

## 3. Configuration ownership and requirement coverage

Every requirement remains in scope, with delivery phased below. Some require supporting modules or runtime operations rather than additional Process Builder fields.

| Configuration area / owner | Requirements covered | Proposed configuration or integration | Checkpoint |
| --- | --- | --- | --- |
| Process settings / Workflow | Request for Entity; Multi-Company / Multi-Branch; Process Ownership | Allowed subject types, authorized reference providers, company/branch applicability, business owner. Keep actor, requester, and request subject distinct. | C2–C3 |
| Request experience / Workflow and user preferences | Favorite / Frequent Requests; Save as Draft; Copy Previous Request; Request Templates | Process policy where needed; user-owned favorites, usage, request drafts, and personal template data outside the process definition. Revalidate copied data against the selected version. | C3 |
| Request and activity form settings / Workflow | Conditional Fields; Real-Time Validation; Form Wizard; Dynamic Forms Rules; Calculated Fields; Cross-Field Validation; Dynamic Reference Data; Form Sections & Steps; Digital Signature | Typed visibility/required rules, calculation dependencies, reference catalogs, sections, wizard order, and signature requirements. Runtime rendering and server validation consume the same semantics. | C3 |
| Transitions and rule editor / Workflow | Dynamic Business Rules; Rule Groups (AND/OR); Conditional Approval; Rule Designer | Typed conditions, nested groups, explicit fallback/terminal outcomes, route priority, and explainable evaluation. | C4 |
| Steps, activities, and approvers / Workflow with Organization | Parallel Approval; Approval Matrix; Dynamic Performer Resolution; Role-Based Approval; Multi-Level Management; Delegation; Substitute Approver | Recipient selection, quorum, matrix references, role/hierarchy policies, delegation validity, substitution rules, and resolution failure outcomes. | C5 |
| Decisions and lifecycle / Workflow | Dynamic Decisions; Decision Requirements; Return for Correction / Rework; Request More Information; Withdraw Request; Cancel / Suspend / Resume; Reopen Completed Request | Configured decision definitions, reasons/documents, permitted actors/states, correction scope, and resume point. Runtime records carry occurrences and audit history. | C5 |
| SLA settings / Workflow with calendar and Communication services | SLA Management; Business Calendar; Automatic Reminders; Escalation | Calendar reference, duration, warning/reminder thresholds, escalation target and schedule, and pause/resume rules. Auto-pass remains a distinct policy. | C6 |
| Definition lifecycle / Workflow | Process Versioning; Draft → Publish; Effective Dates; Governance & Change Approval | Server drafts, immutable published revisions, effective periods, review permissions, and change approval. Existing requests remain pinned to their version. | C2 |
| Reuse and process families / Workflow | Workflow Templates; Reusable Sub-Processes; Parent Request; Parent/Child Requests | Reusable definition templates; child invocation, inputs/outputs, wait policy, cancellation/failure policy, and recursion limits. | C7 |
| Automation / Workflow with scheduler and integrations | Automatic Actions; Event-Based Triggers; Scheduled Triggers; Recurring Processes | Authorized trigger definitions, recurrence/timezone, action catalog, input mapping, retries, and duplicate prevention. | C8 |
| Collaboration and history / request experience | Comments & Discussions; Mentions; Complete Timeline; Audit Trail | Visibility and participation policies where needed; comments, mentions, transitions, and decision events remain runtime records. | C9 |
| Analytics / reporting and Workflow | Process Monitoring; Bottleneck Analysis; SLA Performance; Approver Performance; Dynamic Reporting; Process-Specific KPIs; Saved Reports / Views; Scheduled Reports | Reporting metadata and metric definitions in configuration; authorized projections, personal views, and report delivery in reporting services. | C9 |
| Documents / existing reporting and document services | Print Template Designer; Document Generation | Reference existing published print templates; define completion output policy and authorized generation. Formal generated output requires its own verification gate. | C9 |
| Designer tooling / Workflow | Process Designer; Form Designer; Simulation / Test Mode; Process Clone; Import/Export Process | Extend existing editors; validated portable definitions with logical keys; dry-run preview using the shared evaluator. | C2, C4, C7 |
| Future assistance / generic AI platform | AI Assistance | Summaries, missing-data suggestions, routing suggestions, search, and explanations through approved shared contracts. Business rules retain decision authority. | C10 |

## 4. Proposed editor organization

Keep the existing tabs and contextual panes. Add grouped settings only as their contracts become available:

- **Process:** General, ownership/scope, request policies, version/publication, triggers, and reporting/output references.
- **Variables:** Semantic type, default, scope, and input/output mappings.
- **Request Form / Activity Form:** Controls, sections/steps, conditional rules, validation, calculations, references, and signature requirements.
- **Steps:** Completion policy and stage SLA.
- **Activities:** Performer resolution, recipient quorum, decisions, documents, delegation/substitution, reminders/escalation, and child/action configuration.
- **Transitions:** Condition groups, source/trigger, ordering, destination, fallback, and terminal outcomes.
- **Diagram / validation panel:** Reachability diagnostics, unsupported capabilities, publication blockers, and test-route explanations.

Use explicit statuses such as browser draft, server draft, validated, and published. A saved setting whose runtime is unavailable must not be presented as active executable behavior. Reuse current shared controls and compact spacing; do not introduce a parallel page framework.

## 5. Contract and persistence direction

These are proposed contracts, not existing endpoints or approved table schemas:

1. Define one versioned process-definition contract for manual editing, clone/import, simulation, and later AI proposals. Reconcile it with the existing `WorkflowBlueprintDto` proposal. Use logical keys before persistence and server-generated IDs after save.
2. Resolve data types, operators, controls, performer types, and other metadata by stable semantic codes with explicit supported combinations. Reject unknown semantics rather than falling back to text or equality.
3. Introduce typed contracts for condition groups, decision requirements, completion policies, performer resolution, lifecycle operations, SLA policies, invocation mappings, and triggers. Distinguish form rules from route rules while sharing type semantics.
4. Keep all graph members within one definition/version boundary. Reject references to unrelated processes or companies unless a separately authorized child/integration contract allows them.
5. Use one server transaction for aggregate definition save with pre-validation, generated-ID remapping, optimistic concurrency, and idempotency. Reuse repository conventions without calling independently committing CRUD operations as though they were staging helpers.
6. Preserve existing JSON properties during migration. Classify which are presentation metadata and which need typed execution contracts; do not discard unrecognized legacy properties silently.
7. Publish an immutable, validated definition. Version pinning must cover controls, rules, steps, activities, mappings, and referenced policy versions needed for reproducibility, not just the process header.
8. Keep runtime requests, assignments, comments, delivery records, personal drafts, and user favorites outside definition persistence. Reuse existing print-template versioning as a separate subsystem.

## 6. Delivery checkpoints

Each checkpoint delivers a reviewable change with its own validation. Complete the initial contract checkpoint before scheduling the remaining schema and UI changes. No dates are committed before scope and migration evidence are agreed.

### C1 — Contract alignment and persistence inventory

- Produce a field-by-field matrix: editor → store → adapter → DTO → validator → entity/EF mapping → runtime consumer.
- Cover both full save and individual workspace saves, including activity controls and logical-ID remapping.
- Correct semantic type/operator resolution after auditing existing values; never mass-relabel stored variables based solely on seed IDs.
- Verify activity-control validation uses `ActivityControlId`, while activity transition triggers use `ActivityId`.
- Inventory unsupported settings and preserve them without suggesting runtime support.
- Reconcile conflicting module ownership wording between the Process Builder README and integration/architecture guidance.

**Acceptance:** supported metadata round-trips correctly, unknown types/operators report actionable errors, and existing definitions retain their meaning. Existing load, save, draft, and UI behavior stays compatible.

**Initial files:** `IXApp/src/modules/process-builder/api/processBuilderApi.ts`, `types/processBuilderTypes.ts`, relevant Workflow API adapters, lookup DTOs/validators, seed semantics, and focused adapter tests. Extend this list only from the completed persistence matrix.

### C2 — Reliable server drafts and publication

- Define aggregate read/validate/save contracts and transactional persistence, preserving current numbering and authorization.
- Add version identity, draft/publication lifecycle, effective dates, ownership, concurrency, and change-review policy.
- Specify migration for existing active definitions and historical requests before applying schema changes. Do not rewrite historical meaning from current mutable definitions.
- Move editor server saves to the aggregate boundary incrementally; retain browser recovery as a separate capability.

**Acceptance:** an injected failure leaves no partial graph; retries do not duplicate nodes; stale edits conflict; published revisions cannot be edited; effective-date boundaries are deterministic. No requests or assignments are created by configuration save.

### C3 — Request policies and form configuration

- Add allowed entity subjects, request draft/copy policies, form sections/wizard order, conditional required/visibility rules, calculations, and reference dependencies.
- Extend shared form validation semantics for input-time feedback and authoritative submission validation.
- Implement user-owned favorites, frequent requests, and personal templates in their appropriate request-experience storage, with process eligibility checks.
- Preserve existing normal and print-template form behavior and supported structured controls.

**Acceptance:** hidden/required and cross-field rules agree across UI/server; calculation cycles fail clearly; draft/copy/template data is revalidated; subject selection respects access. Existing attachments, signatures, scores, and print-bound controls remain compatible.

### C4 — Rules, routes, and simulation

**Current editor draft:** Transitions can combine their existing scalar condition with additional typed variable/operator/value conditions using AND (all) or OR (any). The workspace and request/activity/transition settings reuse one editor. Additional references follow variable ID remapping. Browser drafts and JSON export retain these settings. Full and transition-section saves reject compound groups before any writes because the current server contract remains scalar. Nested groups, server persistence, validation and execution remain pending; this editor is not a completed C4 implementation.

- Add atomic nested AND/OR configuration with typed operands and compatible handling of existing single-condition routes.
- Define route priority, fallback, terminal outcomes, and first-match versus fan-out behavior explicitly.
- Add Rule Designer controls and read-only simulation using the same evaluator intended for execution.

**Acceptance:** nested groups survive save/reload/export; invalid references, cycles, empty groups, and unsupported operators fail with element-specific errors. Simulation makes no runtime writes and explains matching/no-match outcomes.

### C5 — Approvals, decisions, and execution integration

- Configure activity recipient quorum separately from step completion. Add approver matrix/role/hierarchy policies, delegation, and substitution.
- Add dynamic decisions, reason/document requirements, correction and more-information paths, withdrawal, cancellation, suspension/resumption, and controlled reopening.
- Connect validated definitions to variable initialization, mappings, assignment creation, and progression through a shared executor. Integrate existing auto-pass with that completion path.
- Resolve attachment activation and durable notification behavior before enabling execution.

**Acceptance:** configured approvals produce authorized assignments; repeated completion cannot advance twice; rework returns to the correct execution occurrence; suspension differs from cancellation; every lifecycle action records actor/reason and respects permissions.

### C6 — SLA calendars and escalation

- Add reusable calendar policy, task/stage durations, warning/reminder schedules, escalation resolution, and waiting-time rules.
- Reuse scheduler and Communication infrastructure; keep auto-pass distinct from deadline escalation.

**Acceptance:** working hours, weekends, holidays, timezone boundaries, pauses, and repeated job delivery yield correct deadlines without duplicate effects.

### C7 — Reuse, portability, and child processes

- Add definition templates, clone, import/export validation and logical-key remapping.
- Configure child target/version, input/output mappings, dependency policy, and failure/cancellation handling.
- Compose authorized parent/child history using shared request components.

**Acceptance:** imported definitions do not retain environment-specific IDs; cloning changes identities consistently; child creation/resumption is idempotent; nested cycles are bounded; child permissions remain independent.

### C8 — Automated initiation and actions

**Schedule editor draft:** Process Information now includes Process Scheduled with daily, weekly, monthly or yearly recurrence, start date/time and time zone. Schedule intent is stored separately in this browser and follows a new process when it receives its saved ID. It is not persisted by the process API or executed by a scheduler.

The requested ownership choices are system administrator, process owner, employee or showroom. Form values can be mapped from a predefined employee, showroom or table record to configured request fields. Current source/owner references are draft text inputs, not verified lookup selections. Changing the source clears incompatible mappings. Missing request fields are shown as unavailable and require reselection.

Execution must resolve an authorized creator independently of the owner/subject, expose authorized data providers and field catalogs, validate required mapped values, and create requests idempotently for each due occurrence. A showroom is an owner/subject reference, not an authenticated account. Server persistence, provider resolution, schedule validation and automatic creation remain pending.

- Add event, scheduled, and recurring trigger contracts and a controlled business-action catalog.
- Use the shared executor with explicit company/actor context, delivery retries, and idempotency.

**Acceptance:** duplicate events or retries create no duplicate requests/actions; recurrence respects timezone and effective version; failed external actions can recover without restarting completed work.

### C9 — Collaboration, reporting, and output

- Add request discussions, mentions, and accurate occurrence-based timelines.
- Add process metrics and reporting configuration, saved views, scheduled reports, and authorized monitoring projections.
- Reuse the existing print designer and template selection; implement official output generation only with authorized data resolution and historical version handling.

**Acceptance:** all parallel pending tasks remain visible; metric definitions state timing and parent/child counting rules; report access matches request access; generated documents represent the intended request/version.

### C10 — Future AI assistance

Integrate the generic AI platform only after deterministic definition validation and execution are established. Reuse its existing plan and shared configuration contracts. Suggestions must not bypass business rules, publication control, or authorization.

**Acceptance:** assistance is distinguishable from official decisions and cannot directly activate an unvalidated process or approve a request.

## 7. Decisions to settle at the owning checkpoint

| Decision | Proposed direction for review | Needed by |
| --- | --- | --- |
| Existing inconsistent type IDs | Inspect actual lookup and variable values; use semantic codes and a reviewed migration report. No blind conversion. | C1 |
| Definition version storage | One immutable aggregate snapshot or fully versioned graph; compare query, reference, and migration costs before schema design. | C2 |
| Publication authority | Process owner with configured review permissions for sensitive changes. | C2 |
| Subject identity | Typed entity reference separate from actor/requester; define supported providers first. | C3 |
| Rule routing | Explicit ordered first-match default is proposed; fan-out must be a separate configured policy. | C4 |
| Quorum and rejection | Define majority denominator, abstention, ties, rejection veto, and outstanding-task handling. | C5 |
| Missing approver/delegation conflict | Explicit recoverable configuration failure or approved substitute policy; no silent skipping. | C5 |
| Correction/reopen semantics | Preserve stage occurrences and reasons; agree editable fields and valid resume points. | C5 |
| Mandatory attachments | Agree staging/finalization before activating tasks; metadata is not proof of uploaded bytes. | C5 |
| SLA pause and calendar changes | Agree which waits pause time and whether running deadlines retain their original calendar. | C6 |
| Child outcomes | Define wait-all/any-success, remaining children, cancellation, and output conflict ownership. | C7 |

## 8. Validation and rollout

- **Configuration tests:** semantic lookup mapping, full/section save round-trips, ID remapping, invalid references, localization, and draft recovery. Reuse `ProcessBuilderPage.test.tsx`, `ProcessBuilderActivityFormApi.test.ts`, and loader tests where relevant.
- **Backend tests:** DTO/domain validation, EF mapping, transaction rollback, concurrency, version isolation, authorization, and migration compatibility against the database provider where needed.
- **Runtime tests:** typed routing, parallel decisions, rework, calendar deadlines, child retries, notification delivery boundaries, and automation idempotency at the relevant checkpoints.
- **UI validation:** focused Process Builder tests plus `e2e/process-builder-responsive.pw.ts`, preserving current compact panes and RTL behavior.
- **Build gates:** run affected tests, TypeScript checking, and relevant .NET build/tests for each implementation slice; run repository-required verification before handoff. Report code, build, authenticated CRUD, and E2E evidence separately.
- **Rollout:** validate migrated definitions in an isolated environment, publish selected pilot processes, and enable their execution deliberately. Retain the existing submission path for unmigrated processes. Rollback stops new activation without deleting historical definitions or in-flight data.

## 9. Immediate next implementation slice

Start with **C1: semantic type/operator alignment and a complete persistence matrix**. It addresses a verified mismatch and establishes which settings can safely be extended. Deliver bounded changes and their evidence before proceeding to aggregate persistence and publication.

### First C1 implementation slice

Inspected the existing Workflow page patterns before editing: setup pages reuse `WorkflowSetupListPage`; process, variable, step, and activity pages reuse enterprise `ListDetailsPage` and shared lookups; request entry composes `DynamicForm`; Process Builder retains its workspace/store/adapter structure.

The first slice reuses the Workflow Variables page's `wfDataTypeApi` catalog in Process Builder, resolves supported semantic codes instead of fixed IDs, validates type mappings before variable mutations, and preserves existing identities where possible. Older drafts recover persisted variable types from server metadata while retaining other edits. See [Process Builder documentation](../../IXApp/docs/process-builder.md) for the draft compatibility behavior.

This first slice did not complete C1. It introduced no schema migration or page redesign.

### Plan review and second C1 implementation slice

Confirmed the dependency order: finish contract inspection and alignment before aggregate persistence/publication (C2). The original findings above describe the pre-change baseline where identified; implementation status must be read with this progress section.

- Operator resolution now uses supported semantic codes, with a display-name compatibility fallback only when a code is absent. Unknown codes no longer silently become equality.
- Changing a transition comparison resolves the corresponding catalog ID instead of retaining an ID whose semantics differ from the selected operator.
- Ambiguous or unavailable operator mappings fail before transition writes. Existing matching IDs, including inactive definitions, are preserved.
- Reconciled the Process Builder module README with its documented Workflow ownership.
- Focused operator and existing adapter tests passed: 14 tests across two files.

**C1 remains in progress:** complete the field-by-field persistence matrix and characterize full/section saves and supported metadata combinations. Mapping editors and broader relationship repairs require their own reviewed implementation slices; aggregate transactions remain C2. Publication, runtime execution, and later feature checkpoints have not been implemented.

### C1 persistence inventory completed

The [field-by-field persistence matrix](workflow-process-builder-persistence-matrix.md) now covers every Builder document interface, request/activity control differences, normalized options and validations, transition identities, and full versus section saves. This completes the source inventory, not live database verification.

Newly isolated gaps include draft-only variable defaults/scope and activity decisions, activity option features that are written but not reloaded, replacement of unknown control JSON properties, and conflation of option labels with stored values. These need targeted characterization and bounded repairs before claiming complete round-trip fidelity.

**Next C1 work:** preserve existing control metadata and option semantics with regression tests, characterize full/section-save identity propagation, and settle numeric/date/range semantics against the backend. C1 remains open; C2 has not started.

### C1 control metadata preservation

Request and activity control saves now merge existing `ExtendedProperties` and `ValidationRules` objects before updating Builder-owned fields. Regression assertions verify unrelated nested metadata survives activity saves and both request-control write stages. Seven adapter tests passed. Option identity/value semantics, activity option feature reload, malformed JSON handling, and broader C1 characterization remain separate outstanding work.

### C1 activity option feature reload

Activity control loading now restores its saved option feature array using the existing feature normalizer and sorted active option order. Missing or malformed arrays produce default feature settings. A mocked save/reopen regression verifies persistence through the adapter; all nine adapter tests passed. This repairs editor persistence only and does not implement activity option alert/file/visibility execution. Stable option identity and distinct label/value preservation remain outstanding.

### C1 option matching and stored values

Request and activity option saves now match existing labels before mutation, retain matched record IDs and stored values, and support an unambiguous single rename. Reordering/deleting options no longer reassigns rows by sorted position. Ambiguous bulk or mixed edits fail instead of guessing. New options still initialize their value from their label. Twelve focused tests cover matching and both adapter payloads; production build passed.

This is a compatibility repair, not the final explicit-ID option editor. Matching is checked before option mutations for each control, but earlier control writes can already have committed under the existing multi-call save. Explicit identities in drafts, arbitrary bulk renames, and full aggregate atomicity remain outstanding.

### C1 explicit option identities

Builder controls now load `optionIds` alongside option labels. Existing IDs survive individual label edits, additions/deletions, reordering, browser drafts, and export. Save resolves explicit IDs only within the current control and rejects duplicate or stale identities; null denotes a new option. Older drafts without IDs continue using the conservative label fallback. Multiple label edits can now retain their separate existing records.

Activity reordering now moves aliases, scores, and feature configuration alongside IDs, matching the request editor's metadata handling. Regression tests cover identity matching, store edits and reorder, and request/activity adapter saves. Production build passed. Aggregate atomicity remains C2; comma-separated edits still interpret same-length changed labels as renames, so replacing records should use delete/add actions.

### C1 unsaved option metadata regression

Follow-up review found that multiple new options share null server IDs, so looking up metadata by those IDs reset unsaved aliases, scores, and feature settings. The editor now tracks source positions independently of server identity when applying option edits. Regressions cover renaming, deletion, and adding another option with multiple unsaved entries. All 18 focused option/adapter tests passed. This repairs the preceding option-ID slice; it does not complete the remaining C1 characterization or start C2.

### C1 draft-to-save replacement regression

Added request and activity adapter scenarios that recover option IDs from browser drafts and replace one existing option while renaming another. The request scenario exposed a missed positional lookup in the write loop: deletion used resolved IDs, but updates still used the original array position. Corrected request writes to use the resolved match, as activity writes already did. Tests verify the exact delete/update/create identities and preservation of the retained stored value. All 20 focused tests passed. These are mocked adapter assertions; live database transactions remain unverified.

### C1 option identity preflight

Both form-save paths now resolve option identities during their initial control validation pass, before deleting or writing any controls, validation rows, or option rows. Stale identities fail before those mutations; 13 adapter tests passed, including request/activity no-control-mutation regressions. Activity header saves and earlier full-save operations still precede this check. This is a bounded preflight improvement, not an aggregate transaction or concurrency guarantee; C2 remains necessary.

### C1 malformed metadata protection

Control saves now validate stored `ExtendedProperties` and `ValidationRules` as JSON objects before form mutations. Blank metadata is allowed; malformed JSON, arrays, null literals, and scalar literals produce a control-specific error instead of being overwritten through an empty-object fallback. Valid unknown properties remain preserved. Read-time compatibility behavior is unchanged. Twenty-two focused tests passed, including request/activity rejection before control writes. Earlier activity-header/full-save writes still require the planned aggregate transaction boundary.

### C1 transition document references

Transition save now requires its target step and selected request-control/activity trigger to exist in the current Builder document, in addition to the existing variable check. All transitions are checked before transition deletions or writes. Eighteen adapter tests passed, including absent step/control/activity cases with zero transition mutations. This is frontend graph validation only: a manipulated document or concurrent server change still requires server ownership validation, foreign keys, and transaction/concurrency enforcement.

### C1 request visibility references

Request-form validation now rejects a visibility condition whose source control is absent from the submitted Builder document, before control mutations. This prevents missing draft IDs from silently clearing the condition and missing numeric IDs from being stored as references. Valid new source controls remain supported through generated-ID remapping. Twenty-one adapter tests passed, including missing numeric/draft references and a successful new-source remap. Cycles, option-feature visibility references, and backend ownership enforcement remain separate outstanding checks.

### C1 closed visibility cycles

Reviewed the current dynamic form's monotonic visibility expansion before implementing cycle validation. Request-form save now rejects condition cycles with no option-based entry into the cycle, including self-references, before form mutations. Cycles targeted by option features remain allowed because options can activate them; actual option-source reachability and values require fuller simulation. Twenty-four focused tests passed, covering ordinary chains, closed cycles, and option entry points. This is a conservative frontend check, not complete graph/runtime validation.

### C1 option visibility target references

Request-form validation now checks option feature `visibleControlIds` against the current form before writes. It rejects missing targets rather than silently dropping unresolved draft IDs or retaining unrelated numeric IDs. A positive adapter test verifies remapping a new target to its generated control ID. Twenty-three adapter tests passed. The older option-feature fixture now references a control actually included in its form. This does not validate performer recipients, complete visibility reachability, or backend ownership.

### C1 request-control deletion cleanup

Removing a request control in the editor now clears visibility conditions whose source was that control and removes its ID from option visibility targets. This extends the existing transition-trigger cleanup. Unrelated conditions, feature settings, and remaining targets are preserved. Dependent fields become unconditional when their deleted source condition is cleared; the change is part of the dirty draft and persists only on save. Six focused store tests passed. Database deletion/history policy remains unchanged and requires separate review.
