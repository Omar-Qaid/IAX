# Recalculation verification in progress

Session 13120 runs InventorySeedIntegrityTests, including scenario 09 and combined nine-scenario replay, in isolated
real-model SQLite databases. Inspect package-20261110-recalculation.log/.trx before starting another run.
No Inventory test job was active before launch; the existing unrelated dotnet process was left untouched.

Expected: receive 10@10, issue 5@10, receive 10@20; recalculation adjusts issue -25. Then a new receipt 1@15 dated
inside the recalculation cutoff succeeds, leaving quantity 16/value 240 and GL loss 75. One adjustment run and
four stock movements are preserved on replay. CST-03 remains unverified pending the actual result.
