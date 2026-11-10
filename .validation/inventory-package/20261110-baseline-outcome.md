# Inventory package baseline — 2026-11-10

The existing test jobs were checked before this run. No other Inventory test was active.

`package-20261110-baseline.trx` records **7 passed, 0 failed, 0 skipped**, duration 1m08s (test execution, excluding build). The run used isolated SQLite databases with the real ApplicationDbContext model, relationships and uniqueness retained. Build output went to this directory's `bin` folder to avoid the running application's assemblies.

The filter included InventorySeedIntegrityTests and the CFG02_/CST02_ physical-value acceptance tests. It verified the current master/report package, journal configuration prerequisites, core reference cycle and repeat execution, late-validation rollback, conflict/counter preservation, and weighted-average Include physical value on/off. This was before the new adjustment scenario build.

This result does not certify the remaining reference cases, SQL Server concurrency, or batch/serial implementation. Read_only.md SHA256 remained 75F2429104DBA6B95372FC1D4DEAFFC30B99DFCF06AE3E581E73103E72F309D4.
