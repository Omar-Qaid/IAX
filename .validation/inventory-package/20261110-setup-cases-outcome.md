# CFG-01 and CFG-03 — outcome

No test job was active when this work started; the preceding regression had written its exit codes and been read.

## Seeder audit (requested check of `SeedingERPV2/Modules/Inventory`)

The mechanically verifiable defect class is whether each seeder's declared `Dependencies` covers what it reads
with `RequireAsync`, since a missing declaration lets a seeder run before its prerequisite. **24 Inventory
seeders, no defects.** Five candidates were raised and all five were false positives:

- `EcoResStorageDimensionGroupItemSeeder`, `EcoResTrackingDimensionGroupItemSeeder` and two requirements in
  `InventoryCycleMasterSeeder` declare the matching `FldSetup` seeder, which declares the `Group` seeder.
  `SeedDependencyResolver` is a depth-first topological sort with cycle detection, so transitive ordering holds.
- `InventPostingSeeder` requiring `DimensionAttributeValueCombination` does declare
  `ERPV2.DimensionAttributeValueCombinationSeeder`; the audit missed it because that seeder writes the rows
  through a service rather than `EnsureAsync`.

No dependency names a seeder that does not exist, which the resolver would surface as `Missing seed dependency`.
This is a negative result, recorded so the same ground is not re-covered.

## CFG-01 — site and warehouse

The reference asks that a correct dimension combination be accepted and a warehouse not linked to the required
site be refused. The refusal message existed in `InventDimensionResolver` and **no test anywhere asserted it**.

Now asserted in `20261110-cfg01-site-warehouse.trx`, **3 passed, 0 failed**:

| Step | Expected | Actual |
| --- | --- | --- |
| Resolve `S01`/`WH-01`, then resolve it again | One dimension, reused on the second call | As expected; a single `InventDim` row |
| Resolve `S02`/`WH-01` | Refused | `The warehouse does not belong to the selected site and company.` |
| Resolve unknown site `S99` | Refused | Same message |
| Resolve a blank warehouse | Refused | `Company, site, and warehouse are required for basic inventory dimensions.` |
| After the refusals | No dimension row left behind | As expected |
| Purchase line with `S02`/`WH-01` | 422 and no line written | As expected |

The reuse assertion matters on its own: on-hand is keyed by `InventDimId`, so a duplicate dimension for the same
combination would silently split a balance.

## CFG-03 — posting accounts and no partial posting

The reference asks for a balanced voucher to the expected accounts per operation, and for a missing required
account to be tested without a partial posting. The repository held each half separately: the atomic rollback
test in `InventoryArchitectureTests` refuses for insufficient available inventory, and
`InventPostingResolutionTests.Missing_setup_is_reported_rather_than_resolved_to_nothing` proves resolution
reports rather than defaulting. Neither asserted the two together.

`20261110-cfg03-missing-account.trx`: **1 passed, 0 failed.** A packing slip posted with the offset account
configured and the packing-slip account omitted is refused with
`Inventory posting setup is missing for SalesPackingSlip and item 'ITEM-001'.`, and leaves no
`CustPackingSlipJour`, no new `InventTrans`, no `GeneralJournalEntry`, no on-hand change and no deducted quantity.

The balanced-voucher half is already asserted for every scenario by `LedgerBalanceValidator`, which requires at
least one posted voucher and that each one sums to zero in the accounting and the reporting currency, and by
`InventoryValueValidator`, which ties financial inventory to the expected GL account. The register cites both
that evidence and the new test rather than claiming the single test proves the whole case.

### The first attempt failed, and the product was right

The packing slip posted successfully with status 200 and no inventory account configured. That is correct
behaviour, not a defect: the fixture's `FIFO` model group leaves `PostOnHandPhysical` and `PostPhysicalRevenue`
unset, so `SubledgerPostingSupport.PostsPhysicalAsync` returns false, the delivery posts no ledger entry at all,
and a missing inventory account is irrelevant to it. The test premise was wrong. Opting the model group into
physical revenue posting is what makes the account required, and the refusal then appears as expected.

A flaw in the test itself was fixed at the same time: asserting `NotEqual(200, ...)` inside the `try` meant
xunit's own failure was caught and compared as a message, so the first failure read as two assertion texts
rather than the real outcome. The status is now captured inside the `try` and asserted outside it.

## Regression

`20261110-cfg01-regression-full.trx`: 943 passed, 1 failed, 18 skipped of 962.
`20261110-cfg03-regression-full.trx`: 944 passed, 1 failed, 18 skipped of 963, duration 12m17s.
`20261110-cfg01-regression-cycle.trx` and `20261110-cfg03-regression-cycle.trx`: 146 passed, 0 failed each.

Each round's passed count rises by exactly the tests added and the skip count never moves. The single failure in
both is `AccountsPayableGenericArchitectureTests.Accounts_payable_features_adhere_to_seven_folder_layout`,
unrelated to this work and described in `20261110-tracking-dimensions-outcome.md`; it remains unrepaired and
outside the Inventory scope.

## Register

**16 passed, 1 partial (`CFG-02`), 23 not verified** of 40.

## Not certified by this run

`CFG-04` (number sequences, journals and closed-period refusal) is untouched. The 18 opt-in SQL Server and
concurrency skips are unchanged. 23 in-scope cases remain, so the recurring automation must stay active.
