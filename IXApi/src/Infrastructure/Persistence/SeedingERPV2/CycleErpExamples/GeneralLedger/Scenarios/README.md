# General Ledger scenarios against the catalog

Sources: `AllModules/Secnarios.md` §4 (GL-01 … GL-26), `AllModules/FullExample.md` part 6 (F1 … F6) and
`Logic_Readonly.md` §2. The catalog IDs are different from the seeded scenario IDs: seeded `01` is **not**
catalog GL-01. A row counts as **seeded** only when `GeneralLedgerTransactionExamples.Scenarios` posts it through
the owning services. **Test only** means a service test proves the behavior, but no seeded example exists.

## Seeded scenarios

| Seeded | Folder | Catalog rows |
| --- | --- | --- |
| 01 | [01-ReclassificationAndReversal](01-ReclassificationAndReversal/README.md) | GL-02 (ledger to ledger), GL-04 (dimensions), GL-15 (reverse a posted journal) |
| 02 | [02-ReferenceCycle](02-ReferenceCycle/README.md) | `Logic_Readonly.md` §2 steps 1–15, GL-25 (trial balance over its own vouchers), and the GL-11 accrual without automatic reversal |

## Catalog rows not seeded

| Catalog | Status | Evidence or note |
| --- | --- | --- |
| GL-01 opening balances / F1 | Not seeded | — |
| GL-03 multi-line voucher | Test only | `LedgerJournalPostingTests`. |
| GL-05, GL-06 journal with VAT / F2 | Not seeded | Tax examples post VAT through AP and AR documents, not the daily journal. |
| GL-07 journal to vendor or customer | Not seeded | — |
| GL-08 … GL-10 bank journals | Not seeded | GL-02 moves the bank only through AR and AP payments. |
| GL-11 accrual with automatic reversal | Test only | `LedgerJournalPostingTests` (`ReverseEntry`, `ReverseDate`). |
| GL-12 prepaid amortization | Not seeded | — |
| GL-13 ⚙️ periodic journal | Not covered | Optional. |
| GL-14 foreign currency journal | Test only | `LedgerJournalPostingTests`. |
| GL-16 ⚙️ workflow approval | Not covered | Optional. |
| GL-17 tax settlement / F3 | Test only | `TaxSettlementTests`. |
| GL-18 tax payment to the authority | Not seeded | — |
| GL-19 bank reconciliation / F4 | Test only | `FinanceCoreCloseAndReconciliationTests`. |
| GL-20 ⚙️ currency revaluation | Not covered | Optional. |
| GL-21 … GL-23 period status / F6 | Test only | `LedgerFiscalCalendarPeriodStatusTests`. Seeding period changes would affect every example in the company. |
| GL-24 year-end close | Test only | `GeneralLedgerCycleTests`. Not seeded for the same reason. |
| GL-26 subledger reconciliation / F5 | Test only | `FinanceCoreCloseAndReconciliationTests`. |

The catalog's negative rows NEG-13 … NEG-16 are not seeded. `LedgerVoucherBalanceTests` and
`LedgerFiscalCalendarPeriodStatusTests` cover the unbalanced-voucher and closed-period refusals.
