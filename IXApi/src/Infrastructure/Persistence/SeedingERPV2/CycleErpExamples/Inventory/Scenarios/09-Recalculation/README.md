# Scenario 09 — recalculation keeps the period open

Independent RECALC item: receive 10 at 10, issue 5 at 10, then receive 10 at 20, using adjustment journal services.
Pre-recalculation quantity/value is 15/250. Weighted-average recalculation at November 10 should adjust the issue
by -25, producing total issue loss 75 and inventory value 225. The run must be an adjustment, not a close.

After recalculation, enter another receipt of 1 at 15 dated November 5, inside the recalculation cutoff. Success
proves that recalculation did not close that period. Final quantity/value is 16/240. GL loss must be 75; the
adjustment voucher and settlements must reconcile. Replay retains one recalculation run and four stock movements.

CST-03 is certified only after actual isolated results are recorded. The combined test also reruns this scenario
alongside the other examples; SQL Server concurrency is a separate acceptance gap.
