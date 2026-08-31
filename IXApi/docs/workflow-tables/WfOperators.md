# WfOperators

Master list of comparison operators used by workflow transitions.

- Primary key: `RecId` (`byte`).
- Referenced by: `WfTransitions.OperatorId`.
- Process Builder resolves records to `=`, `!=`, `>`, `<`, `>=`, `<=`, `between` and related comparisons.
- Legacy seed imports operators from the workflow JSON snapshot.
- It defines operator identity only; `WfTransitions.Value` stores the comparison operand.

## `wf.sql` snapshot

- Exported rows: **7**.
- The SQL data represents `>`, `<`, `>=`, `<=`, `=`, `<>`, and `Between` operators.
- Process Builder normalizes legacy `<>` to `!=`.
