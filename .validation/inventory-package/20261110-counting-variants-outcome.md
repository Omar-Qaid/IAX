# Counting variants — verified outcome

Retest `package-20261110-counting-variants-fixed.trx`: 8 passed, 0 failed, 0 skipped, execution duration 1m01s.
The real ApplicationDbContext SQLite model retained relationships and uniqueness. The fixture-length failure
and repair are preserved separately in `package-20261110-counting-variants.trx` and `20261110-counting-variants-repair.md`.

Company dat, fiscal year 2026, site APV2-SITE, warehouse APV2-WH. Each scenario used an independent database/item.

- Scenario 04, IV2-2026-CPLUS: opening 90 / 1080 via journal IV2JR-000001, voucher IV2JV-000001.
  Count 93 via IV2JR-000002, voucher IV2JV-000002. Expected/actual variance +3 / +36; final 93 / 1116.
  Additional inventory profit credit 36, no loss or COGS. Two movements and two balanced vouchers.
- Scenario 05, IV2-2026-CMATCH: opening 90 / 1080 via journal IV2JR-000001, voucher IV2JV-000001.
  Matching count IV2JR-000002 posted with expected/actual zero quantity and value variance and no variance voucher.
  Final 90 / 1080; only the opening movement/voucher remained.
- Both replayed without additional movements/vouchers or voucher sequence consumption. A changed opening line
  was rejected without further postings. Existing reference, adjustment, shortage and preparation tests also passed.

Read_only.md SHA256 remains 75F2429104DBA6B95372FC1D4DEAFFC30B99DFCF06AE3E581E73103E72F309D4.
No test jobs were active when results were collected. Full acceptance remains open; transfer scenarios and other
unverified cases still require implementation/verification. The recurring task remains active.
