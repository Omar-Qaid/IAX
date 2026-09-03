# How Process Builder configuration connects to workflow transaction/history data

## Purpose

This guide explains how a workflow designed in Process Builder becomes request transaction and history data. It is based on the current IXApi EF model, the workflow seeders, and the legacy snapshot at `src/Infrastructure/Persistence/Seeding/wfData/wf.sql`.

Process `590` in `wf.sql` is useful as a reference graph, but it must not be copied as a fixed set of IDs. A process must be reconstructed by following relationships and remapping generated keys.

## The hard boundary

Workflow data has two different lifecycles:

1. **Configuration/definition** describes what future requests should do.
2. **Transaction/history** records what a particular request actually contained and what happened to it.

Process Builder owns configuration. Submitting and processing a request creates transaction/history. Saving a process definition must not create fake requests, assignments, or history rows.

## Key architecture

The workflow must be understood as the following dependency graph. The arrows represent creation and execution dependencies; they do not always mean that the child table contains a direct foreign key to the table immediately above it.

```text
                         WfProcesses
                              |
          +-------------------+-------------------+
          |                   |                   |
          v                   v                   v
 WfRequestControls        WfVariables          WfSteps
          |                   |                   |
          |                   |                   v
          |                   |              WfActivities
          |                   |              /          \
          |                   |             v            v
          |                   |     WfActivityControls  WfPerformers
          |                   |             |            |
          v                   v             v            v
 WfRequestMappingVariables  WfActivityMappingVariables  WfUsersPerformers
          |                   |
          +---------+---------+
                    |
                    v
               WfTransitions
                    |
                    v
                 Routing

 WfProcesses
      |
      +----------------------> WfUsersProcesses
                                Process access
```

The essential routing chain is:

```text
control value -> mapping -> workflow variable -> transition condition -> next step/activity
```

Request-control mappings supply values captured during submission. Activity-control mappings supply values captured during workflow execution. `WfTransitions` consumes the relevant variable and operator to select the next route.

After somebody submits a request, the runtime graph begins:

```text
Process Definition
      |
      v
  WfRequests
      |
      +-------------> WfProcessVariables
      |
      v
 WfAssignments
      |
      v
 WfProcessData
      |
      v
Next transition / assignment
```

The current schema adds normalized historical detail and printing around that core runtime graph:

```text
WfRequests
  -> WfRequestDetails
  -> WfRequestVariables
  -> WfAssignments -> WfActivityDetails
  -> ReportEntityVersions
```

These additional tables preserve submitted values, completed activity values, mapped request values, and the print-template version associated with a request. They extend the core graph; they do not change the configuration/runtime boundary.

## Configuration tables

| Area | Tables | Responsibility |
|---|---|---|
| Shared metadata | `WfCategories`, `WfPriorities`, `WfProcessTypes` | Classify a process. |
| Shared control metadata | `WfControls`, `WfDataTypes` | Define supported UI/control types and value types. |
| Shared routing metadata | `WfActivityTypes`, `WfOperators` | Define activity behavior and transition operators. |
| Process root | `WfProcesses` | Stores the workflow identity and activation settings. |
| Request form | `WfRequestControls`, `WfRequestControlsOptions`, `WfRequestControlsValidations` | Defines inputs completed by the requester. |
| Variables | `WfVariables` | Defines values that controls and routing rules can use. |
| Request mapping | `WfRequestMappingVariables` | Copies a request-control value into a workflow variable. |
| Flow graph | `WfSteps`, `WfActivities`, `WfTransitions` | Defines stages, work items, and conditional routing. |
| Activity form | `WfActivityControls`, `WfActivityControlsOptions`, `WfActivityControlsValidations` | Defines inputs completed during an approval/task activity. |
| Activity mapping | `WfActivityMappingVariables` | Copies an activity-control value into a workflow variable. |
| Performers/access | `WfPerformers`, `WfPerformerType`, `WfUsersPerformers`, `WfUsersProcesses` | Defines who can submit or perform workflow work. |
| Printing | `ReportTemplates`, `ReportTemplateVersions` | Defines versioned print layouts for a process. |

`WfRequestControls` and `WfActivityControls` are not interchangeable. Request controls collect the initial request. Activity controls collect decisions or data while the request is being processed.

## Transaction and history tables

| Table | Created/updated when | Historical meaning |
|---|---|---|
| `WfRequests` | A user submits or saves a request. | Root transaction, linked to the process used at submission. |
| `WfRequestDetails` | Request values are captured. | Normalized request-control values for that request. |
| `WfRequestVariables` | Request mappings are evaluated. | Request-specific mapped variable values. |
| `WfProcessVariables` | Runtime variables are initialized or changed. | Despite its name, the current and legacy schemas relate it to `RequestId`; treat it as request-scoped runtime state. |
| `WfAssignments` | Work is assigned to an approver/performer. | The actionable and completed assignment trail. |
| `WfProcessData` | A request advances through processing. | Runtime step/activity/status information. |
| `WfActivityDetails` | An activity form is completed. | Normalized activity-control values for the executed activity. |
| `ReportEntityVersions` | A request is associated with a print version. | Pins the intended template version for stable historical output. |

These tables must not be populated merely to define a process.

## End-to-end lifecycle

### 1. Build and activate

Process Builder saves the process root, controls, variables, steps, activities, performers, and transitions. Before activation, validate that every foreign key resolves, orders are valid, every transition points to a valid destination step, and every mapped variable has a compatible data type.

The current frontend directly maintains process, variable, step, activity, request/activity control, option, validation, and transition records. The backend mapping tables exist, but the current `processBuilderApi.ts` does not yet load/save both mapping collections. That gap matters when a route depends on a value captured by a control.

### 2. Submit a request

A submission creates `WfRequests` and captures the request-form values in `WfRequestDetails`. Mapping rules project selected control values into request/runtime variables. The workflow engine then evaluates routing using those captured values, not by treating the live designer form as request history.

### 3. Assign and execute work

The selected activity and performer produce an assignment. Activity input is stored in `WfActivityDetails`; runtime movement is recorded in `WfProcessData`; variable mappings can update request-scoped values before the next transition is evaluated.

### 4. Preserve history and print

Historical output must read the request's captured details, activity details, runtime state, and pinned print version. It must not rebuild an old request exclusively from the latest process controls or latest template.

This rule prevents a newly added designer control from appearing on an old request unless that request actually contains a historical value/snapshot for that control. It also prevents renaming or deleting a current control from rewriting past activity history.

## What `wf.sql` demonstrates

The legacy export contains 22 tables that still exist in the current project and 2,615 matching INSERT rows. Relevant observations are:

- Process `590` has request controls linked through legacy `WfRequestControls.RelatedObjectId = 590`.
- The legacy request-control rows include control metadata and sometimes XML `ExtendedProperties`, such as lookup/list items.
- Requests for Process `590` retain a serialized `WfRequests.RequestDetails` payload containing the submitted control IDs, labels, types, options, and values.
- Legacy `WfProcessVariables` is linked by `RequestId`, confirming that these values are runtime/request state rather than process-definition rows.
- The export contains assignments and process data, showing that execution history is downstream of a request, not part of Process Builder configuration.
- `WfRequestVariables` exists but has no exported rows; absence of rows is not proof that mapping is unnecessary because the legacy snapshot also uses serialized request details and process-variable rows.

The current schema has evolved from that export:

| Legacy `wf.sql` | Current project interpretation |
|---|---|
| `WfRequestControls.RelatedObjectId` | Current entity uses the explicit process relationship (`ProcessId`). |
| Options/validation may be embedded in `ExtendedProperties` | Options and validations have normalized tables. |
| Request/activity values may be embedded in XML payloads | Current schema also provides `WfRequestDetails` and `WfActivityDetails`. |
| No print-template tables in the export | Current schema has template, template-version, and request-version tables. |

Do not import a legacy row by column position without mapping it to the current entity and migration. The SQL snapshot is evidence about legacy behavior, not a current migration script.

## Dependency-safe process creation order

Use one database transaction and keep an old-ID to new-ID map:

1. Reuse or create shared master rows: category, types, controls, data types, and operators.
2. Create `WfProcesses` as inactive.
3. Create process variables.
4. Create request controls, then their options and validations.
5. Create request-control-to-variable mappings.
6. Create steps in `StepOrder`.
7. Create activities for each step.
8. Create activity controls, options, validations, performers, and mappings.
9. Create transitions after all referenced steps, activities, variables, and controls have new IDs.
10. Create process access rows.
11. Validate the whole graph, then activate the process.

Never copy identity values such as `ProcessId`, `StepId`, `ActivityId`, or control IDs into a different database and assume they still point to the same records.

## Process 590 inspection query pattern

Use recursive relationship queries rather than searching for the number `590` in every table:

```sql
DECLARE @ProcessId bigint = 590;

SELECT * FROM WfProcesses WHERE ProcessId = @ProcessId;
SELECT * FROM WfRequestControls WHERE RelatedObjectId = @ProcessId; -- legacy column
SELECT * FROM WfVariables WHERE ProcessId = @ProcessId;
SELECT * FROM WfSteps WHERE ProcessId = @ProcessId ORDER BY StepOrder;
SELECT * FROM WfActivities
WHERE StepId IN (SELECT StepId FROM WfSteps WHERE ProcessId = @ProcessId);
SELECT * FROM WfActivityControls
WHERE ActivityId IN (
    SELECT ActivityId FROM WfActivities
    WHERE StepId IN (SELECT StepId FROM WfSteps WHERE ProcessId = @ProcessId)
);
SELECT * FROM WfTransitions WHERE ProcessId = @ProcessId;
SELECT * FROM WfUsersProcesses WHERE ProcessId = @ProcessId;

SELECT * FROM WfRequests WHERE ProcessId = @ProcessId;
SELECT * FROM WfAssignments
WHERE RequestId IN (SELECT RequestId FROM WfRequests WHERE ProcessId = @ProcessId);
SELECT * FROM WfProcessData
WHERE RequestId IN (SELECT RequestId FROM WfRequests WHERE ProcessId = @ProcessId);
SELECT * FROM WfProcessVariables
WHERE RequestId IN (SELECT RequestId FROM WfRequests WHERE ProcessId = @ProcessId);
```

For the current database, use current EF column names and normalized detail tables instead of assuming all legacy names still exist.

## Acceptance rules

- Saving a process changes only configuration tables.
- Submitting a request creates a request snapshot/details and runtime state.
- Completing an activity creates durable activity/assignment history.
- Transitions use mapped request-specific values.
- Adding, renaming, or deleting a designer control does not rewrite an old request.
- Printing an old request uses captured historical data and its pinned template version.
- A process is activated only after its entire graph passes validation.
- Imports are transactional and remap every generated identity.

See [Workflow table catalog](README.md) for the individual table documentation.
