# Scenario 01 — Expected results

## Period average

Receipts are 100 at 10 and 50 at 16, so the period average is (1,000 + 800) / 150 = **12**.

| Figure | Expected | Checked by |
| --- | --- | --- |
| On hand | 90 | `OnHandQuantityValidator` |
| On-hand summary (`InventSum.PhysicalInvent`) | 90 | `InventSumConsistencyValidator` |
| Inventory value | 1,080 | `InventoryValueValidator` |
| Cost of goods sold | 720 | `CostOfGoodsSoldValidator` |
| Close adjustment on the issues | -120 | `CloseAdjustmentValidator` |
| …on the 40-piece issue | -80 | derived: 40 x (12 - 10) |
| …on the 20-piece issue | -40 | derived: 20 x (12 - 10) |
| Reserved after delivery | 0 | `ReservationValidator` |
| Sales invoices | 2, totaling 1,800 | the flow, before reconciliation |
| Service inventory movements | 0 | `NonStockedItemValidator` |

## Intermediate states the flow asserts

| Point | Expectation |
| --- | --- |
| After reserving 60 | on hand 100, reserved 60, available 40 |
| Before the close | inventory value 1,200, the issues still at the running cost of 10 |

The pre-close assertion matters: 1,200 is the correct value at a running cost, and 1,080 is the correct value at the
period average. Asserting both separates a costing fault from a close fault.

## What a failure means

Every figure above is read back from persisted rows. A mismatch is a defect in a posting or closing service, not in
the expectation — the arithmetic above is fixed by the reference and was verified independently.
