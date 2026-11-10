# Movement journal — actual outcome

package-20261110-movement-retry.trx: 11 passed, 0 failed, 0 skipped, execution 1m12s. The retry used one build
worker and a fresh compiler process after the preserved Roslyn access-violation crash. No implementation change
was required to recover compilation. Tests used isolated SQLite databases with the real model and its constraints.

MOV-01 / scenario 08: company dat, year 2026, item IV2-2026-MOVE, site APV2-SITE, warehouse APV2-WH.
Journal IV2JR-000001 / voucher IV2JV-000001 received expected/actual +10 units, value +100.
Journal IV2JR-000002 / voucher IV2JV-000002 issued expected/actual -2 units, value -20.
Final expected/actual stock 8, inventory value 80, COGS zero, two movements and two balanced GL vouchers.

The explicitly selected maintenance expense account 681001 had entries -100 and +20 (net credit 80). Adjustment
profit/loss accounts remained zero. Replay preserved movements, ledger vouchers and voucher sequence progress;
a changed receipt line was rejected without further posting. All prior package scenarios and preparation tests passed.

Read_only.md hash remains 75F2429104DBA6B95372FC1D4DEAFFC30B99DFCF06AE3E581E73103E72F309D4.
No test job remained active when the result was collected. Other acceptance cases are still open; automation remains active.
