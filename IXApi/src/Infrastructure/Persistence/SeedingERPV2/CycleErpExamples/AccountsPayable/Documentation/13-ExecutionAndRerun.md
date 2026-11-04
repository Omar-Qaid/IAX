# Configuration/master preparation and safe reruns

Register `AddAccountsPayableConfigurationAndMasterDataExamples`, resolve
`AccountsPayableCycleSeeder` in a fresh DI scope, and call `PrepareAsync` with fiscal year and
audit actor. The existing ApplicationDbContext determines the selected company.

1. Read and validate the embedded master-data definitions before mutation.
2. Run the existing seed dependency graph under the shared database guard and serializable transaction.
3. Execute the existing module-owned configuration/master seeders.
4. Run phase-one integrity checks and the cycle manifest/reference validator before commit.
5. Commit the masters and existing seed-history records together.
6. Return the captured references and ordered preparation report, with TransactionsExecuted=false.

The report is in memory and can be serialized by its caller. It is not an ERP status table or a
posted reconciliation result. On a failed prepare, return no success report. On a rerun, validate
and reuse compatible records; do not recreate them or reset number-sequence counters.

Cancellation and any validation failure roll back the configuration/master writes. Transaction
posting recovery, operation checkpoints and purchase-to-settlement reconciliation remain future
work. No command-line posting runner or executable transaction scenario is provided here.
