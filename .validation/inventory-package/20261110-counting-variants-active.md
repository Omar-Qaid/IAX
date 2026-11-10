# Counting variants verification in progress

No dotnet/testhost jobs were active before starting this run. Session 90218 runs InventorySeedIntegrityTests with
isolated SQLite databases and output in this folder's bin directory. Inspect `package-20261110-counting-variants.log`
and `.trx` before starting another run. This extends the verified shortage case with independent overage (93 against
90) and matching-count (90 against 90) scenarios. CNT-02 remains unverified until actual results are recorded.

The test theory now has scenarios 02/03/04/05; it validates exact final balances, stage values, number of vouchers,
repeat counts, voucher counter preservation and rejection of a changed opening journal line.
