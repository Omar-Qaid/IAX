# Combined package verification

No test job was active before starting package-20261110-combined.log/.trx. The new test runs all eight implemented
scenarios in one isolated real-model SQLite database, then runs all eight again. It compares each scenario's
quantity/value/COGS/adjustment and preserves all sequence counters, stock movements, GL vouchers, settlements and
transfer-journal counts. This checks coexistence, which individual scenario databases cannot prove.

The test build reuses the previously verified project assemblies (BuildProjectReferences=false); no production
code changed for this run. A fresh compiler and one build worker avoid the earlier compiler-process failure.
This does not substitute for SQL Server simultaneous-poster tests or certify remaining unimplemented cases.
