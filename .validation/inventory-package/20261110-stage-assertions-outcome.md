# Scenario 01 stage assertions — outcome

No test job was active when this work started: the preceding regression had written its exit codes and been read.

## What was wrong

Scenario 01 reported six case ids through `InventoryStageReport`, and the register marked all six
`Not verified by Inventory package`. The register was right, and the reason was worse than a missing row.

Three of the six stages could not fail:

| Stage | Defect |
| --- | --- |
| `RCV-01` | Reported `qty, qty` — the same variable as both expected and actual quantity |
| `ISS-01` | The same tautology |
| `CST-04` | `ExpectedValue` of `1080m` against an `ActualValue` of `null`, so the pair was never comparable |

The stage pairs were asserted only by `Inventory_journal_package_reconciles_profit_loss_and_preserves_posted_journals_on_replay`,
whose `[InlineData]` covers scenarios **02–08**; scenario 01 is absent from it, and that theory compares
`ExpectedValue` to `ActualValue` only, never the quantity pair. `RES-01`, `ISS-03` and `CST-01` were genuinely
backed by `Check(...)` calls against queried rows; the other three were reports dressed as verification.

Two documentation defects sat on top of it. `Scenarios/01-ReferenceCycle/README.md` named `RCP-01`, which
exists nowhere in the repository — the id is `RCV-01` — and claimed `ISS-02` and `CST-03`, which no scenario
emits at all.

## Changes

- `RCV-01` asserts that confirming a purchase order registers `InventSum.Ordered >= qty` without changing
  physical on-hand, and that the receipt raises on-hand by the quantity received. The figure is a delta, not a
  constant: `BuyAsync` serves both purchases of the cycle and the second runs from a balance of 40, so
  "zero on hand before receipt" holds only for the first. The generalizable statement is that ordering and
  confirming create no physical stock.
- `ISS-01` asserts that each packing slip lowers on-hand by the delivered quantity and leaves the undelivered
  remainder on `SalesLine.RemainSalesPhysical`.
- `RES-01` now covers the unreserve half of the reference requirement: releasing 60 returns `AvailPhysical` to
  100 without changing on-hand, then the reservation is restored so the deliveries that follow are unaffected.
- `CST-01` asserts, apart from the 1,200 net, that the 60 issued before the second receipt carry `-600` of cost.
  A net value can be correct while its components are wrong and cancel.
- `CST-04` computes its actual closing value instead of reporting `null`.
- A shared `OnHandAsync` helper reads physical on-hand, so every stage measures the same thing.
- `Inventory_reference_package_posts_and_reruns_without_duplicate_movements` asserts every stage's quantity
  **and** value pair, because the aggregate figures alone would pass if an intermediate step posted a wrong
  quantity that a later step compensated for.
- The scenario 01 README is corrected and now states that the stage pairs are asserted rather than reported.

## Outcome

`20261110-stage-assertions.trx` and `20261110-partial-cases.trx`: **12 passed, 0 failed, 0 skipped** each.

Three behaviours were unknown before the run and resolved in the product's favour: `ReleaseAsync` returns
`AvailPhysical` to 100, `InventSum.Ordered` is still at least the ordered quantity at confirm time, and issue
cost carries the negative sign the `-600` assertion expects.

Two defects were found and fixed in the new assertions, not in the product:

- `CST-04`'s closing value was first computed over receipt rows only, giving 1,800 — the gross receipt cost —
  instead of 1,080. Issues carry negative cost, so the sum must span every movement, as `CST-01` already did.
- `ISS-01`'s remainder was captured from the passed `SalesLine` entity, which is the same object for both
  deliveries and therefore stale on the second call: it expected `60 - 20 = 40` where the database correctly
  held 0, because 40 and 20 of a 60 line leave nothing. The remainder is now read from the database.

Both were caught by the assertions themselves, which is the point of adding them.

## Register

Scenario 01's cases were first recorded as `Partial`, each naming the half of its reference requirement the
scenario did not reach, and promoted to `Passed` only once that half was asserted and the run proved it. The
register stands at **14 passed, 1 partial (`CFG-02`), 25 not verified** of 40.

## Regression

`20261110-stage-regression-full.trx`: 940 passed, 1 failed, 18 skipped of 959.
`20261110-stage-regression-cycle.trx`: 146 passed, 0 failed.

The single failure is `AccountsPayableGenericArchitectureTests.Accounts_payable_features_adhere_to_seven_folder_layout`,
unrelated to this work and described in `20261110-tracking-dimensions-outcome.md`. It remains unrepaired.

A further full regression was taken after the four cases were promoted.
`20261110-partial-regression-full.trx`: 940 passed, 1 failed, 18 skipped of 959, duration 11m49s.
`20261110-partial-regression-cycle.trx`: 146 passed, 0 failed. Counts and skips are identical to the stage
regression above, and the one failure is the same unrelated AccountsPayable layout test, so both rounds of
assertions are regression-clean.

## Not certified by this run

The 18 opt-in SQL Server and concurrency skips are unchanged and remain unverified. `CFG-02` stays partial:
the weighted-average include-physical setup passed, and the remaining setup checks have not been reached.
25 in-scope cases remain unverified, so the recurring automation must stay active.
