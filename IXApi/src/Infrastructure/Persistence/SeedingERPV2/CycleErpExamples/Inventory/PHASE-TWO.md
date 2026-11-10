# Phase two — journal setup and executable scenarios

The initial package executed scenario 01 only. Scenario 02 now implements positive and negative adjustments through
the journal services, with a separate module-owned item, reconciliation and repeat/conflict checks. Its verification
outcome belongs in `Data/acceptance-status.json`. Journal services are registered by the opt-in Inventory entry point.

That setup has now been seeded. What remains is scenario work.

## Done — the prerequisites

| Record | Seeded by | Note |
| --- | --- | --- |
| `481004` inventory profit, `681006` inventory loss main accounts | `GeneralLedgerSeedExample` → `MainAccountSeeder` | the head of the chain; nothing below resolves without them |
| The `IV2` account structure and its ledger dimensions | `InventoryAccountStructureSeeder` | a third structure, because an activated one cannot be redefined |
| `InventPosting` for `InventReceipt`, `InventIssue`, `InventProfit`, `InventLoss` | `InventoryCycleSeedExample.PostingProfiles` → `InventPostingSeeder` | `InventJournalPostingService` resolves exactly these four |
| `SysNumberSequence` for `InventJournalTable` and the `IV2-JVOUCH` voucher | `InventoryNumberSequenceSeeder` | `InventDim` and `InventTransId` stay owned by the payable seeder |
| Four `InventJournalName` rows | `InventJournalNameSeeder` | Movement, Adjustment, Transfer **and** Counting — readiness requires all four, not three |
| `APV2-WH2` destination and `APV2-TRN` transit warehouses | `InventLocationSeeder` | `InventTransferOrderService` requires Standard → Transit → Standard |
| The four `InventParameters` journal selections | `InventParametersSeeder` | readiness checks the selection, not just the name's existence |
| `InventCountGroup` `IV2-COUNT` | `InventJournalNameSeeder` | counting policy is resolved from it |

Two things are worth recording about *why* they look like this.

**The account structure ranges must stay disjoint.** `InventPostingSeeder` and `InventoryCycleValidator` both resolve
a profile's account with `RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == code)`, which
demands exactly one row. Overlapping `APV2`, `ARV2` and `IV2` ranges would produce a second combination for the same
code and break that resolution — so a new inventory account goes in the `IV2` range, not into a widened one.

**The parameter selections belong to the seeder that owns the row.** `SeedContext.EnsureAsync` validates every field
it owns on each rerun, so a selection written onto `InventParameters` by a *later* seeder is read back by
`InventParametersSeeder` on the next run as conflicting data. Filling the four ids in `InventJournalNameSeeder` was
tried and failed exactly that way; they are now part of the `InventParametersSeeder` proposal. The consequence is
deliberate: a database already seeded with empty selections will report a conflict on that field rather than being
silently rewritten, which is the contract this package advertises for every changed fixture.

**A pre-existing gap closed on the way.** `InventParametersService.GetReadiness` lists `InventReceipt`,
`InventIssue`, `InventProfit` and `InventLoss` among its *required* posting types. Before this change the seeded demo
company reported four `MISSING_*_ACCOUNT` issues and was never ready for item setup — a real setup defect, not only a
phase-two prerequisite.

`Inventory_preparation_seeds_the_journal_setup_the_readiness_check_requires` asserts all of the above against the
seeded rows.

## Remaining — the scenarios

In order:

1. Adjustment journal — implemented in scenario 02; consult the acceptance register for the actual test outcome.
2. Counting journal — shortage, overage and matching counts passed isolated SQLite verification in scenarios 03/04/05. Consult CNT-01/CNT-02 for evidence; SQL Server concurrency remains separate.
3. Transfer order — scenario 06 passed shipment, partial/final receipt, transit and warehouse value reconciliation in isolated SQLite. See TRF-02 evidence. Transfer journals and inter-site cases remain separate.
4. Marking — mark an issue against a chosen receipt before close, and assert the settlement picks it.
5. Serial sales issue — serial receipt, per-serial on-hand and the one-unit-per-line rule are
   verified; no scenario issues a serial-tracked item through a packing slip. The batch issue path
   is covered, and both share one resolver, so this is a scenario gap rather than a service gap.

Batch and serial tracking (MD-04) are implemented outside the numbered scenarios, because it changes an item'"'"'s
dimensions rather than adding a journal flow. The resolver guard that previously refused every
tracking dimension now admits batch and serial only, and the purchase line applies the same
`InventoryDimensionPolicy` the inventory journal uses, so a tracked item cannot receive a blank or
undefined batch. Evidence is in `md04-batch-tracking.trx`.

Each joins `InventoryTransactionFlow.Scenarios` only once it asserts its figures against posted rows, and updates its
own row in `Data/acceptance-status.json` with the evidence. Listing an id there before that point would claim
coverage the package does not have.
