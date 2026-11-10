# Counting shortage — actual outcome

`package-20261110-counting.trx`: 6 passed, 0 failed, 0 skipped, execution duration 1m33s. Includes reference-cycle replay, adjustment and counting scenarios, preparation repeat/conflict/counter preservation, journal setup and late-failure rollback. Test databases were isolated SQLite with the real ApplicationDbContext model, relationships and uniqueness retained.

CNT-01: company dat, year 2026, item IV2-2026-COUNT, site APV2-SITE, warehouse APV2-WH.

- Opening journal IV2JR-000001 / voucher IV2JV-000001: expected/actual +90 units and +1080 value.
- Counting journal IV2JR-000002 / voucher IV2JV-000002: on hand 90, counted 88, expected/actual difference -2 and value -24.
- Final expected/actual quantity 88, inventory value 1056, COGS zero. Loss account debit 24; balanced vouchers. Two movements and two vouchers.
- Replay preserved movement/voucher counts and voucher sequence progress. A changed opening line was rejected without additional postings.

Read_only.md SHA256 is unchanged: 75F2429104DBA6B95372FC1D4DEAFFC30B99DFCF06AE3E581E73103E72F309D4.

CNT-02 overage/matching counts remain unverified by this package, as do other open cases in the acceptance register. No full-acceptance claim or automation shutdown is warranted. No test process remained active when this result was collected.
