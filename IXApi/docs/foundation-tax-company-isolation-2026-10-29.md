# Foundation tax company isolation

Confirmed defects: tax rate synchronization selected by TaxCode alone and created new rates in hardcoded `dat`; tax code detail/update/delete lookups lacked explicit company predicates; DTO mapping could change company on update.

Changes use the existing company context and fields. Tax code custom read/create/update/delete paths now select the current company, ignore an incoming create company, preserve the original company on update, and return not found for another company's identifiers. Rate reads and synchronization include the company, and new TaxData rows use that company. Missing company context is rejected instead of defaulting to `dat`.

No migrations, schema changes, live data rewrites, rate-date policy changes or posted transaction changes are included. Existing rate selection ordering remains unchanged; this does not certify effective-dated rate editing, direct generic service writes, or full D365 tax parity.

`TaxCompanyIsolationTests` uses SQLite without global company filters. It covers duplicate tax codes across companies, rate isolation, foreign RecId reads/updates/deletes, incoming company spoofing on create/update, original code preservation, and missing company context.

## Atomic code/rate saves

The custom DTO create/update operations now own a serializable database transaction covering both code and rate saves. Rate synchronization failures and cancellation propagate instead of being logged and suppressed. Before-commit failures roll back and restore previously tracked scalar values, detaching entities introduced during the failed attempt. Existing successful save ordering and rate dates are preserved.

Following the existing workflow submission transaction pattern, these endpoint operations require transaction ownership. They reject an ambient EF transaction or unrelated pending tracked changes before writing, rather than committing a caller's work. A failure after commit starts is reported as an uncertain outcome requiring reload, without automatic replay. This is an intentional change to internal calling requirements; the HTTP create/update contract is unchanged.

Fault-injection tests fail after the second SaveChanges for both create and update, including cancellation. They verify absence of partial rows, original tracked/database values, unchanged rate dates, and a successful subsequent retry. SQL Server concurrency and commit-connection-failure behavior are not claimed as live-tested.
