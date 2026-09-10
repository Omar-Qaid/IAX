# Process Builder persistence matrix

Date: 2026-09-10. C1 source audit following data-type and operator alignment.

## Latest implementation corrections

The following supersedes earlier baseline caveats in this inventory:

- Request option JSON preserves unknown properties, validates stored objects before form writes, and removes superseded legacy aliases for owned flags.
- Inactive request/activity options omitted from the editor are preserved. Blank activity option labels are removed together with their positional aliases and feature metadata.
- Both operator selectors use the same catalog-code resolver as persistence; unsupported catalog operators are not offered as selectable equality comparisons.
- Full-save ownership checks run before header mutation. Section saves validate persisted identities against the appropriate parent, including validation/control identities. Duplicate local identities cannot overwrite generated-ID maps.
- Full-save adapter coverage verifies generated variable/step/activity/request-control IDs for both transition trigger kinds.
- The transition service validates process, variable, target step, operator and trigger references before create/update/bulk-update; it rejects dual triggers and moving an existing transition between processes. This does not add foreign keys, aggregate atomicity, or complete master-service ownership enforcement.

Live database save/reload, provider constraints, concurrency, and the remaining C1 acceptance work are unverified. C2-C10 remain pending in the [configuration plan](workflow-process-configuration-plan.md).

## Evidence and meaning

This matrix follows the [builder document types](../../IXApp/src/modules/process-builder/types/processBuilderTypes.ts), [store](../../IXApp/src/modules/process-builder/store/useProcessBuilderStore.ts), [load/save adapter](../../IXApp/src/modules/process-builder/api/processBuilderApi.ts), Workflow DTOs, and EF mappings. It describes source behavior, not authenticated database round-trip results.

Editor values enter the Zustand document through the workspace/settings panels. Browser drafts and JSON export retain the document; that does not establish backend persistence. Full save calls multiple APIs and then reloads the server document. Section saves merge selected results into the store.

Statuses: **Mapped** means a load/save mapping exists; **Derived** means reconstructed on load; **Draft only** means no matching persistence path was found; **Lossy** means normalization, discarded metadata, or incomplete reload changes the value. Runtime configuration consumers must be tested independently.

## Process and variables

| Builder fields | DTO / storage | Save/reload behavior | Validation and runtime boundary |
| --- | --- | --- | --- |
| Process `id`, `code` | `WfProcess.RecId`, `Code` | Mapped; generated identity/code returned by backend. | Existing number-sequence API; no predicted IDs. |
| `name`, `description` | `Name`, `Description` | Mapped, trimmed on save. | Process validator requires name. |
| `categoryId`, `priorityId`, `processType` | `CategoryId`, `PriorityId`, `ProcessTypeId` | Mapped as numbers. | Builder requires positive IDs; process validator alone does not prove related-record eligibility. |
| `score`, `canRepeat`, `repeatIntervalHours`, `mandatoryDocs`, `active` | `Score`, `IsRepeatable`, `RepeatIntervalHours`, `MandatoryDocuments`, `IsActive` | Mapped. | Configuration persistence is not proof of automatic recurrence or workflow activation. |
| `dataTypeCatalogVersion` | Browser/export document only | Draft compatibility marker; not a workflow definition version. | Old drafts restore existing variable types from server metadata. |
| `variables`, `requestControls`, `steps`, `transitions` | Separate configuration entities | Saved through separate calls; no aggregate transaction. | A later error can follow earlier committed writes. |
| Variable `id`, `code`, `name`, `description`, `sortOrder`, `active` | `WfVariable` identity/master fields, `SortOrder`, `IsActive` | Mapped; new codes follow number-sequence policy. | Builder checks names, duplicate names, and byte-range order; variable DTO validator is empty. |
| Variable `dataType` | `DataTypeId` → `WfDataTypes.Code` | Mapped through INT/STR/DT/BOOL; existing equivalent IDs preserved. | Unknown semantics and unsupported Object save fail; semantic type IDs are not assumed. |
| Variable `required`, `scope`, `defaultValue` | No corresponding fields in `WfVariableDto` | Draft only; reload sets false, process, and empty string. | No runtime default initialization can be inferred from these editor values. |

Process eligibility (`UsersProcesses`), aliases, and other master metadata outside the builder document can exist on standalone Workflow pages. Full save spreads the existing process record, but new Builder processes start with empty eligibility rows. Do not replace those pages or assume all backend fields are editable in Builder.

## Steps and activities

| Builder fields | DTO / storage | Save/reload behavior | Validation and runtime boundary |
| --- | --- | --- | --- |
| Step `id`, `code`, `name`, `order`, `score`, `active`, `systemField` | `WfStep` identity/master fields, `SortOrder`, `Score`, `IsActive`, `IsSystemDefined` | Mapped. | Step DTO validator is empty; process FK establishes existence, not the whole graph's validity. |
| Step `allMandatory` | `MustCompleteAll` | Mapped. | Not a recipient majority policy or AND/OR condition group. |
| Step `condition` | No field in step DTO | Draft only; reload sets null. | No executable conditional step contract. |
| Step `activities` | `WfActivities.StepId` | Mapped through activity saves and generated-ID remapping. | Separate calls, not atomic graph persistence. |
| Activity `id`, `code`, `name`, `activityTypeId`, `performer`, `score`, `sortOrder`, `active` | `WfActivity` master fields, `ActivityTypeId`, `PerformerId`, `Score`, `SortOrder`, `IsActive` | Mapped. | Save resolves activity type and requires a performer; actor/employee resolution is a later runtime concern. |
| Activity `type` | Derived from activity-type metadata | Derived; explicit backend ID takes precedence on save, with NORMAL fallback for new modes. | Approval/review/API designer modes are not independent backend behavior contracts. |
| `mandatoryDocs`, `autoPassEnabled`, `autoPassingHours` | `MandatoryDocuments`, `IsAutoPassEnabled`, `AutoPassAfterHours` | Mapped. | Auto-pass is distinct from calendar-aware SLA and escalation. |
| Four `is*NotificationEnabled` flags | Matching activity channel flags | Mapped. | Notification configuration does not prove delivery. |
| `canViewPreviousSteps`, `canViewPreviousDocuments` | Matching activity flags | Mapped. | Live authorization enforcement requires separate verification. |
| `assignmentMode`, `required` | No typed counterpart in activity DTO | Draft only; reload sets any and true. | No implemented quorum/round-robin behavior established by these fields. |
| `actions` including `id`, `type`, `label`, `nextStepId`, `condition` | No action rows/DTO mapping in adapter | Draft only; reload sets empty array. | Dynamic decisions need a defined persistence/runtime contract. |
| Activity `validations`, `condition` | No activity-level validation/condition save mapping | Draft only; reload sets empty array/null. | Actual validation rows target activity controls. |
| `config.apiMethod`, `apiUrl`, `notifyEmails` | No matching save mapping | Draft only; reload restores GET/empty values. | Does not implement API execution or notification dispatch. |
| `controls` | `WfActivityControls.ActivityId` | Saved by `syncActivityControls`, including normalized options and validations. | Generated control IDs feed dependent writes. |

## Request and activity controls

| Builder fields | Request-control path | Activity-control path | Caveat |
| --- | --- | --- | --- |
| `id`, `code`, `label`, `labelAR` | `RecId`, `Code`, `Name`, `NameAlias` | Same identity/master mapping | Request codes honor number-sequence metadata; activity control code initializes on create. |
| `type`, `controlId` | `WfControls` lookup | Same lookup | Type derived from code/name/controlType; unknown control labels may still fall back to text in `builderControlType`. |
| `sortOrder` | `SortOrder` | `SortOrder` | Load normalizes control positions to sequential values; ordering survives, exact original numeric spacing does not. |
| `score`, `required`, `uniqueKey`, `usedAsCriteria` | Columns plus selected JSON mirrors | Columns plus selected JSON mirrors | Required maps to `Mandatory`; visibility is separately represented. |
| `labelColor`, `columnSpan`, `readOnly`, `defaultValue` | `ExtendedProperties` JSON | `ExtendedProperties` JSON | Mapped presentation/configuration values; storage alone does not verify every runtime consumer. |
| `visible` | JSON plus `IsActive` on save | JSON plus `IsActive` on save | Conflates hidden presentation and inactive configuration. Review before changing visibility semantics. |
| `canFilter`, `canGroup`, `canSort`, `referenceType`, `fieldRole`, `dataType`, `defaultAggregation` | Typed reporting columns | Reload hard-codes defaults; not mapped by activity control save | Request DTO validates allowed enums and EF adds check constraints; no activity parity implied. |
| `visibilityCondition.variableId`, `operator`, `value` | JSON uses `sourceControlId`; IDs remapped after controls save | Reload sets null; save omits condition | Despite its name, request visibility `variableId` identifies a source request control, not `WfVariable`. |
| `validations` | Normalized request validation rows plus `ValidationRules` JSON | Normalized activity validation rows plus `ValidationRules` JSON | Two representations require consistent interpretation; see validation matrix. |
| Existing JSON properties outside Builder fields | Existing object merged before owned fields are updated | Existing object merged before owned fields are updated | Fixed in the metadata-preservation slice for valid JSON objects in `ExtendedProperties` and `ValidationRules`. Builder-owned fields still update normally; malformed JSON recovery is unchanged. |

### Options

| Builder fields | Storage | Save/reload behavior |
| --- | --- | --- |
| `options`, `optionAliases` | Request/activity option `Value`, `Name`, `NameAlias` | Loads label (`Name` or `Value`); existing options now retain stored Value while Name is edited. Matching labels preserve row identity on reorder/delete; one unambiguous rename preserves its row. New options initialize Value from the label. Ambiguous mixed/bulk renames are rejected; explicit option IDs are still absent from the editor document. |
| Option order and active state | Option `SortOrder`, `IsActive` | Loads active options; save normalizes order to multiples of ten and writes active=true. Rows absent from resolved matches are deleted. Inactive rows remain a compatibility consideration when resolving ambiguous edits. |
| `optionScores` | Request option `Score` | Mapped on requests; activity option entity has no equivalent score column. |
| `optionFeatureConfigurations.requireFileUpload`, `sendAlertMessage`, `alertMessage`, `performerIds`, `showOtherControls`, `visibleControlIds` | Request option JSON | Mapped; visible control IDs remapped to generated request-control IDs. Request submission consumes configured option features. |
| Same activity option feature fields | Activity control JSON | Restored and normalized on load in the activity option reload repair. Existing positional association follows active options sorted by order. No equivalent activity option runtime behavior is established. |

### Control validation fields

| Builder field | DTO field | Behavior |
| --- | --- | --- |
| `id` | `RecId` | Determines create/update; generated control identity determines parent. |
| `type` | `ValidationType` | Normalized by alias mapper on load; unknown values currently become custom. |
| `value`, `secondaryValue` | `Value`, `ValidationExpression` | Trimmed; empty becomes null. |
| `operator`, `mask` | `Operator`, `MaskInput` | Trimmed; empty becomes null. |
| `message` | `ErrorMessage` | Resolved default/custom message on save. |
| `messageAlias` | Request `ErrorMessageAlias` | Mapped for request validation; not saved as an activity validation message alias. |
| `severity`, `active` | `Severity`, `IsActive` | Mapped; DTO validators require nonempty severity rather than proving all allowed semantics. |
| `sortOrder` | `SortOrder` | Loaded from storage; save recalculates `(index + 1) * 10`. |

Request validation rows target `RequestControlId`; activity validation rows target **`ActivityControlId`**. Neither is an activity-level decision rule. Backend DTO validators require parent ID/type/message/severity. Form-definition/submission validation and the separate `/validate` engine remain distinct consumers; this source inventory does not assert parity for every validation type.

## Transitions and mappings

| Builder field | DTO / storage | Behavior and boundary |
| --- | --- | --- |
| `id`, `variableId`, `targetStepId`, `value`, `sortOrder`, `active` | `RecId`, `VariableId`, `StepId`, `Value`, `SortOrder`, `IsActive` | Mapped. Value trimmed; order byte-checked. DTO requires positive IDs and limits Value to 255 characters. |
| `operator`, `operatorId` | `OperatorId` referencing catalog metadata | Code-based resolution; stale IDs corrected when comparison changes; unknown/ambiguous mappings rejected. No new routing evaluator is introduced. |
| `triggerSource`, `triggerId` | `ActivityId` or `RequestControlId` | One selected trigger is written, other is null. Activity trigger uses **`ActivityId`**, not `ActivityControlId`. |
| `name`, `sourceStepId` | No corresponding transition DTO fields | Derived on load; designer labels/source state cannot be assumed to round-trip independently. |
| Nested AND/OR, terminal/fallback policy | No current Builder/DTO aggregate contract | Future C4 work; one scalar comparison is not grouped routing. |
| Request/activity control-variable mappings | `WfRequestMappingVariables`, `WfActivityMappingVariables` | Backend tables/services exist; absent from Builder document and adapter save paths. EF ignores request `SortOrder` and activity `VariableOrder`. |

Remaining scalar validation gaps: numeric input currently accepts decimal values despite INT semantics; DT editing uses date-only strings; Between has no two-operand editor/contract; operator/type combinations are not centrally validated. Do not invent a range delimiter or datetime conversion while calling it behavior-preserving alignment.

## Full save versus section save

| Operation | Writes and return behavior | Remaining concern |
| --- | --- | --- |
| Full process save | Process → variables → request controls → steps → activities → activity controls/options/validations → transitions → full reload | Separate commits; draft-only values disappear on reload. New step/variable/request-control/activity IDs are remapped into transitions before save. |
| Variables | Validate all type mappings, delete removed rows, write variables, reload variables; store remaps variable selection and transition references | No atomic delete/update operation; required/default/scope reload to defaults. |
| Steps | Save step records; store merges persisted steps and remaps step references | Must preserve activities and unrelated dirty slices; standalone DTO has no step condition. |
| Activities / Activity Form | Save activities and synchronize their controls, options, validations; merge returned activity data | Activity-level draft fields are not server contracts. |
| Request Form | Save controls, validations, options, then visibility links; store merges controls and remaps transition triggers | Multiple writes; JSON and option identity losses described above. |
| Transitions | Resolve candidate rows before deletes/writes; reload; store applies transitions | Frontend checks do not replace server same-process authorization or missing foreign keys. |

## C1 findings and remaining acceptance work

Update: `BuilderControl.optionIds` now carries stored identities independently of labels. Loaded documents use explicit control-scoped IDs for saves, with null entries for new options. The label-matching limitations above apply to legacy drafts without IDs. Store edits preserve IDs and reorder associated metadata. This does not make multi-call saves atomic.

Option identity resolution now also runs during the initial form validation pass before control, validation, and option mutations. The activity-header and full-process save stages can still commit earlier writes. Concurrent changes after catalog reads still require backend transaction/concurrency enforcement.

Stored control JSON now receives strict save-time object validation: blank fields are accepted; malformed/non-object `ExtendedProperties` or `ValidationRules` block form writes instead of being silently replaced. The permissive read parser remains unchanged for compatibility. This supersedes the malformed-metadata caveat in the earlier inventory.

1. Data-type and operator catalog identity fixes are implemented and have focused tests. They do not establish all typed comparison semantics.
2. This matrix completes the source inventory of Builder document fields and save paths. Exact live database round-trips, provider constraints, and full-save failure injection remain unverified.
3. Before C2, add targeted characterization for unknown JSON preservation, option label/value identity, activity option reload, and full/section-save ID propagation. Use those findings to scope repairs rather than silently dropping data.
4. Define supported numeric/date/range semantics from current backend contracts and actual lookup values before expanding type choices or enforcing new rules.
5. Retain unsupported draft state while making persistence limitations visible in the eventual validation panel. Do not invent endpoints for activity-level validation or mark designer modes executable.

No application or database behavior was changed while producing this matrix. C1 remains open for the remaining validation/characterization work; C2 and subsequent feature checkpoints remain planned.
# Scheduling and notification persistence update

`ProcessScheduleSettings` loads and saves `WFProcessScheduled` through a dedicated GET/PUT endpoint after the process is saved. Activation records the authenticated company and saving account, creates/enables its existing-framework background job, validates source mappings and the request form, and protects updates with a concurrency version. Local edits remain unactivated until saved. Supported dynamic table sources are `HcmWorker` and `Showroom` only.

Activity `sysNotificationTemplateId` round-trips through both full-process and activity-only saves, including explicit clearing. Request option/assignment alerts are staged in the existing `SysScheduledNotifications` queue. See [execution plan](workflow-request-execution-implementation-plan.md) for migration, verification and delivery limits. Earlier audit rows below describe the original inspection where explicitly marked draft-only.
