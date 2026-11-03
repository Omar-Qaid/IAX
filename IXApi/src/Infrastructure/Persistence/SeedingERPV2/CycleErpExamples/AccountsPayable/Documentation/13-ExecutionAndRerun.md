# Execution, progress and safe reruns

## Runner options

Tenant/company; selected module/scenario; include dependencies; demo-data enable flag; base business date; currencies; quantities/prices; tax configuration; cancellation; repeatable/versioned policy. Keep demo scenarios opt-in under the project's environment controls.

## Execution algorithm

1. Discover registered seeders through DI.
2. Validate dependency graph and execution scope before mutation.
3. Resolve required preconditions and compatible existing records.
4. Acquire a scope-specific concurrency guard.
5. Read execution history for seeder ID, version, tenant/company and scenario.
6. Execute or resume steps in deterministic order.
7. Validate persisted results and persist completion with appropriate atomicity.
8. Emit a report and release the guard even on failure.

## History identity

A suggested uniqueness scope is tenant + company + seeder ID + version + scenario key. Use normalized non-null scope keys if the database uniqueness behavior for nulls would permit duplicates. Store status, timestamps, attempt information and error summary. Keep attempt history separately if needed. Version increment does not authorize repeating a posted demo invoice; migrations must resolve existing scenario documents.

## Transaction handling

For simple configuration seeds, commit data and success marker together. For service-controlled posting commits, use a durable operation key, persist returned IDs, and reconcile existing postings after a crash before resuming. Do not mark success before durable business effects exist.

## Recovery tests

Run once, run again, cancel between steps, simulate failure before posting commit and after commit before progress checkpoint. Verify reruns neither duplicate nor omit postings. Concurrent invocation must result in one operation or a controlled conflict.

CLI names and class names must be adapted to the repository. Do not document an executable command until it exists and has been exercised.
