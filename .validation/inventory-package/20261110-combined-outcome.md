# Combined Inventory package — actual outcome

package-20261110-combined.trx: 12 passed, 0 failed, 0 skipped, execution duration 2m39s. Includes all individual
package cases plus the combined first-run/replay check using a single isolated SQLite database and the real model.

All eight implemented scenarios completed in one company/year without changing another scenario's assertions.
They then replayed together with identical per-scenario quantities, inventory values, COGS and close adjustments.
All number-sequence counters, stock movement count, GL voucher count, cost-settlement count and transfer journal
count remained unchanged. This is sequential replay evidence, not SQL Server concurrent-poster evidence.

Reference close scenario 01 reconciled quantity 90, financial inventory 1080, COGS 720, issue adjustment -120,
cost settlements, transaction origins and balanced GL vouchers. It remained correct alongside the seven other
journal/count/transfer scenarios, and on combined replay. CST-04 is now explicitly linked to this evidence.

The test build reused the previously compiled production assemblies; only the combined integration test was new.
No Inventory test job remained active when results were collected. Other dotnet processes were not Inventory
test jobs and were left untouched. Acceptance gaps remain, including SQL Server concurrency and batch/serial;
the recurring task stays active.
