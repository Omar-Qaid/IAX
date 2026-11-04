# Current implementation contract

Prepare Accounts Payable configuration and master data only, reusing the existing module-owned
phase-one seeders and integrity validators. CycleErpExamples owns the opt-in preparation entry
point, embedded example descriptions and configuration/master reference report.

Keep JSON and module fixtures consistent, validate definitions before mutation, validate stored
references before commit, resolve RecIds at runtime by company/business key, and report the real
completed seeder order. Preserve existing data and fail on incompatible setup. Prove rerun safety
and the absence of transaction writes on SQLite and disposable SQL Server.

Do not implement or call purchase-order, receipt, invoice, payment, settlement, inventory-movement
or ledger-posting operations in this scope. The Scenarios directory and Documentation/06–12 are
future transaction specifications, not an instruction to execute them in the current package.
