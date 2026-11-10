# 06 — Costing and close

The reference figures are a periodic weighted average, and the order of events is the point.

## Receipts and issues

| Event | Quantity | Unit cost | Value |
| --- | --- | --- | --- |
| Receipt 1 | +100 | 10 | 1,000 |
| Issue 1 | -40 | 10 (running) | -400 |
| Issue 2 | -20 | 10 (running) | -200 |
| Receipt 2 | +50 | 16 | 800 |

Before the close, inventory stands at 1,000 - 400 - 200 + 800 = **1,200** and the issues carry the running cost of 10.
The flow asserts this explicitly, so a costing fault is distinguishable from a close fault.

## The period average

The close averages the whole period's receipts: (1,000 + 800) / (100 + 50) = **12** per piece.

| Figure | Calculation | Result |
| --- | --- | --- |
| On hand | 150 received - 60 issued | **90** |
| Inventory value | 90 x 12 | **1,080** |
| Cost of goods sold | 60 x 12 | **720** |
| Close adjustment | 60 x (12 - 10), as a credit to inventory | **-120** |
| …on the 40-piece issue | 40 x -2 | **-80** |
| …on the 20-piece issue | 20 x -2 | **-40** |

The second receipt arrives *after* both sales and still raises their cost. That is what makes this a periodic
average rather than a running one, and it is the reason `CloseAdjustmentValidator` refuses an adjustment of zero:
zero would mean the close did nothing.
