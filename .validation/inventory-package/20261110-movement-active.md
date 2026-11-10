# Movement scenario verification in progress

No dotnet/testhost jobs were active before the run. Session 36128 runs InventorySeedIntegrityTests, including new
scenario 08, against isolated real-model SQLite databases. Inspect package-20261110-movement.log/.trx before
starting another run. MOV-01 remains unverified until actual results are recorded.

Expected result: receipt +10/100, issue -2/-20, final 8/80. Explicit maintenance-expense offset entries -100/+20,
no adjustment profit/loss entries, zero sales COGS, two movements/vouchers. Replay and conflict checks included.
