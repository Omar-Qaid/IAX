# Scenario 01 — Reference cycle

The eleven steps of `Read_only.md`, executed through the owning purchase, sales, reservation and closing services.

Buy 100 at 10. Reserve 60 against a sales order that also carries a non-stocked service line. Deliver and invoice
40, then deliver and invoice the remaining 20 together with the service. Buy 50 more at 16 — **after** both sales —
then run the periodic weighted-average close.

## Why this scenario exists

It is the only case that exercises inventory close after *real* document postings. Of the existing close tests, 36
of 37 build `InventTrans` rows by hand; this one reaches the same rows through the receipt, invoice, delivery and
close services, so a defect in any of them surfaces as a wrong closing value rather than passing unnoticed.

It also proves the crux of periodic weighted average: the second receipt arrives after the sales and still lifts the
period average, so already-invoiced issues are adjusted at close.

## Documents

| Key | Document |
| --- | --- |
| `IV2-{year}-01-P1` | first purchase order, with receipt `-R` and vendor invoice `-I` |
| `IV2-{year}-01` | sales order |
| `IV2-{year}-01-D1` / `-D2` | packing slips for 40 and 20 |
| `IV2-{year}-01-I1` / `-I2` | sales invoices for 40, and for 20 plus the service |
| `IV2-{year}-01-P2` | second purchase order, 50 at 16 |

Each key is checked for before its step runs, so an interrupted run resumes. A completed close for the cutoff is
reconciled rather than repeated.

## Case ids touched

`RCV-01` (purchase receipt), `RES-01` (reservation), `ISS-01` (delivery), `ISS-03` (stock and service invoicing),
`CST-01` (running cost before close), `CST-04` (weighted-average close). These are the ids this scenario reports in
`InventoryStageReport`; `Data/acceptance-status.json` remains the authority on what the package certifies overall,
and it records four of these six as partial because the scenario reaches only part of their reference requirement.

Every stage pair is compared by the scenario-01 test, not merely reported. Before that assertion existed the
`RCV-01` and `ISS-01` stages carried the same variable as both expected and actual, and `CST-04` reported an
expected value with no actual at all, so none of the three could fail.
