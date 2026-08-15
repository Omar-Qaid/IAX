# Number-sequence conversion audit

## Converted frontend create flows

| Entity | Sequence key | Frontend | Status |
|---|---|---|---|
| `WfProcess` | `WfProcess` | Workflow processes | Central list-details metadata |
| `WfStep` | `WfStep` | Workflow steps | Central list-details metadata |
| `WfVariable` | `WfVariable` | Workflow variables | Central list-details metadata |
| `WfActivity` | `WfActivity` | Workflow activities | Central list-details metadata |
| `WfCategory` | `WfCategory` | Workflow categories | Central workflow-setup metadata |
| `WfPriority` | `WfPriority` | Workflow priorities | Central workflow-setup metadata |
| `WfActivityType` | `WfActivityType` | Workflow activity types | Central workflow-setup metadata |
| `WfControl` | `WfControl` | Workflow controls | Central workflow-setup metadata |
| `WfDataType` | `WfDataType` | Workflow data types | Central workflow-setup metadata |

## Backend call-site audit

Workflow services currently contain compatibility hooks for `WfCategory`, `WfPriority`, `WfProcessType`, `WfProcess`, `WfStep`, `WfActivity`, `WfActivityType`, `WfControl`, `WfOperator`, `WfPerformer`, `WfPerformerType`, `WfRequest`, `WfTransition`, and `WfVariable`. Controller creates now allocate centrally first, so these hooks do not consume twice. They remain for non-controller callers until those paths are migrated.

Direct specialized allocations remain in employee, party, location, and legal-entity services because they generate identifiers for related entities inside compound operations. They require a separate domain-specific conversion and must not be mechanically removed.

The seed catalog contains additional ERP keys that do not yet have converted frontend create flows. Run the supplied SQL audit against the target database before adding unique constraints or repairing historical data.

## Historical data policy

No production values are rewritten automatically. Null/empty codes, unresolved `{PREFIX}` values, duplicates, invalid ranges, and scope conflicts must be reviewed using `scripts/audit-number-sequences.sql`. Any repair must be idempotent, transactional, dry-run first, and retain an audit mapping from old to new values.

