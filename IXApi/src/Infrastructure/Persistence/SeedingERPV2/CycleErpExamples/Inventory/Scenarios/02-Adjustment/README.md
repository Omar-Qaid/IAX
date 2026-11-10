# Scenario 02 — inventory adjustment

The module owns the separate `IV2-{year}-ADJ` stocked item so this scenario cannot change scenario 01's closing value. Run explicitly with `InventoryTransactionOptions(year, actor, ["02"])`.

The journal services create and post two adjustment journals: receive 10 at cost 10, then issue 2 at cost 10. The expected final quantity is 8 and financial inventory value is 80. The journals credit inventory profit by 100 and debit inventory loss by 20; cost of goods sold remains zero.

The execution reads stock, summaries, transaction origins, and ledger vouchers back and runs the shared reconciliation checks plus the profit/loss assertions. The scenario checks exactly two movements and two vouchers. Each journal description carries a stable scenario key. Existing lines are validated before replay; the posting service handles a posted journal without allocating another voucher.

The isolated integration test also checks sequence preservation, no duplicate movements/vouchers, and refusal of a changed posted line. Acceptance evidence is recorded in `Data/acceptance-status.json` only after the test succeeds.

Service-generated records include InventJournalTable, InventJournalTrans, InventTransOrigin, InventTrans, InventSum, InventTransPosting, GeneralJournalEntry, GeneralJournalAccountEntry, and voucher associations. The runner does not insert these rows directly.
