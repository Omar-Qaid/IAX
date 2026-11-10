# Transfer journal verification in progress

No dotnet/testhost job was active before this run. Session 84608 runs InventorySeedIntegrityTests in isolated
SQLite databases using the real model. Check package-20261110-transfer-journal.log/.trx before another run.

Scenario 07 transfers 10 of an independent opening 90 at cost 12 between warehouses in one site. Assertions
cover 80/10 warehouse quantities, 960/120 values, conserved site total, two transfer movements, reconciliation,
same-database replay and conflict preservation. TRF-01 is not yet certified.
