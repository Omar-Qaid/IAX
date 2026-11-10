# Transfer order — actual outcome

`package-20261110-transfer.trx`: 9 passed, 0 failed, 0 skipped in 1m18s. Real ApplicationDbContext model,
isolated SQLite databases with relationships and uniqueness retained. Other Inventory package scenarios and
preparation integrity tests remained green. No test process was active when these results were collected.

TRF-02 / scenario 06: company dat, year 2026, item IV2-2026-TRANS, order IV2-2026-06, site APV2-SITE.
Opening journal IV2JR-000001 / voucher IV2JV-000001 created 90 units at 12, value 1080.

- Shipment of 10: APV2-WH 80 / value 960; APV2-TRN 10 / 120; APV2-WH2 0 / 0.
- Receipt of 6: source 80 / 960; transit 4 / 48; destination 6 / 72.
- Final receipt of 4: source 80 / 960; transit 0 / 0; destination 10 / 120.
- All stage assertions passed. Site total stayed 90 / 1080; COGS and close adjustment remained zero.
- Transfer journals/voucher identifiers IV2TJ-000001, IV2TJ-000002, IV2TJ-000003. There was one actual GL voucher
  (the opening adjustment); this same-site transfer did not add GL entries. Transfer voucher identifiers are not
  counted as ledger vouchers when they have no GeneralJournalEntry.
- Replay preserved movement and ledger-voucher counts, with exactly three transfer journals. A changed opening
  line was rejected without more postings.

The package now seeds transfer order/journal sequences through the Inventory owner and registers the existing
transfer service in the opt-in entry point. No direct balance or transaction inserts were used by the runner.
Read_only.md hash is unchanged: 75F2429104DBA6B95372FC1D4DEAFFC30B99DFCF06AE3E581E73103E72F309D4.

Full acceptance remains incomplete, including transfer-journal/inter-site cases, batch/serial and other open
register entries. This result does not certify SQL Server concurrency. The recurring task remains active.
