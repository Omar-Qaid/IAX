# Execution and safe reruns

Register `AddAccountsReceivableConfigurationAndMasterDataExamples`, resolve
`AccountsReceivableCycleSeeder` in a fresh DI scope, and call `PrepareAsync` with the fiscal year and
the audit actor. The existing ApplicationDbContext determines the selected company.

1. Read and validate the embedded master-data definitions before mutation.
2. Run the existing seed dependency graph under the shared database guard and transaction.
3. Execute the existing module-owned configuration and master seeders.
4. Run the phase-one integrity checks and the cycle manifest validator before commit.
5. Commit the masters and the seed-history records together.
6. Return the captured references and the ordered preparation report, with TransactionsExecuted=false.

For transactions, register `AddAccountsReceivableTransactionExamples` and call
`AccountsReceivableTransactionExamples.RunAsync`. It prepares masters first, takes the database seeder
lock, runs each selected scenario, and records it in the seed history.

The report is in memory and can be serialized by its caller. It is not an ERP status table.

On a rerun, validate and reuse compatible records; do not recreate them or reset number-sequence
counters. Cancellation or any validation failure rolls back the configuration and master writes.
Each transaction scenario commits through its own services, so a rerun resumes from what is already
posted instead of starting over.

Run the purchase scenarios for the same fiscal year first, or in the same process, so the demo item
has been received and financially updated before a sale tries to deliver it.
