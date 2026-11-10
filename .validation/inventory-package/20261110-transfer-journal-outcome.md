# Same-site transfer journal — actual outcome

package-20261110-transfer-journal.trx: 10 passed, 0 failed, 0 skipped in 1m20s. Tests used isolated SQLite databases
with the real ApplicationDbContext model, foreign keys and uniqueness. Existing package scenarios and preparation
integrity tests also passed. No test job was active when results were collected.

TRF-01 / scenario 07: company dat, year 2026, item IV2-2026-TJOUR, site APV2-SITE.
Opening journal IV2JR-000001 / voucher IV2JV-000001 created 90 units at 12 (1080).
Transfer journal IV2JR-000002 / inventory voucher IV2JV-000002 moved 10 from APV2-WH to APV2-WH2.

Expected/actual source quantity 80 and value 960; destination quantity 10 and value 120; transit zero.
Site totals stayed 90 / 1080, with zero COGS and close adjustment. Exactly three movements existed: opening plus
both transfer legs. The transfer inventory voucher has no GL entry for this same-site configuration; the single
actual GL voucher is the balanced opening adjustment.

Replay preserved movements, GL vouchers and voucher sequence progress. A changed opening line was rejected
without more postings. Historical results are retained. Read_only.md hash is unchanged:
75F2429104DBA6B95372FC1D4DEAFFC30B99DFCF06AE3E581E73103E72F309D4.

Inter-site financial transfers and other open acceptance cases remain unverified; the recurring task stays active.
