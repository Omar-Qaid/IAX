# Finance — gap analysis index

**Last updated:** 2026-11-03

Each finance module carries its own `GAPS.md`, recording where it stands against **basic (core)
D365 F&O** functionality. Advanced and optional features are deliberately excluded and are not
counted as gaps.

| Module | File | Highest-priority open gap |
| --- | --- | --- |
| General ledger | [GeneralLedger/GAPS.md](GeneralLedger/GAPS.md) | Three failing journal posting tests (reversal, voucher allocation); journal tax calculation |
| Accounts payable | [AccountsPayable/GAPS.md](AccountsPayable/GAPS.md) | No purchase trade agreements; vendor payment currency; invoice matching tolerances |
| Accounts receivable | [AccountsReceivable/GAPS.md](AccountsReceivable/GAPS.md) | Realized gain/loss still untested; return dispositions |
| Inventory | [Inventory/GAPS.md](Inventory/GAPS.md) | SQL Server verification; four unsupported inventory models |
| Foundation | [Foundation/GAPS.md](Foundation/GAPS.md) | Tax calculation ignores every configured origin, silently; payment schedules make invoices unpostable |
| Common (shared posting) | [Common/GAPS.md](Common/GAPS.md) | Posting lock unproven under SQL Server; two overlapping posting exception types |

## The four findings that cut across every module

1. **Three themes recur, and they are worth naming before the module files.** First, several features
   stop at setup: a table, controller and validator exist, nothing reads them. `SpecTrans`,
   `TaxExemptCodeTable`, `PaymSched` and most of `TaxTable`'s calculation columns are all in this
   state, and `PaymSched` is the sharp case — configuring one makes its customers' invoices
   unpostable. Second, **documents can be started but not ended**: a purchase order cannot be
   cancelled at all, a partly delivered sales order cannot be cancelled either, and a credit note or
   on-account payment on either side creates an open row that no process can ever apply. **This
   whole theme was closed on 2026-11-03**: both cancellation halves, so an order can be ended on
   either side, and *Settle open transactions* on both subledgers, so a credit note and an
   on-account payment have an exit. What is left of it is narrower - stored `SpecTrans` marking,
   settlement priority, and cash discount on the settlement path. Third, this
   repository's habit is to **refuse** what it does not support, by name — which is why the tax
   calculation gap stands out as the exception worth fixing first: it accepts configuration it then
   ignores, and posts a wrong amount silently. The zero-rate half of that third theme was closed on
   2026-11-03; the origin half remains, and is the one that produces wrong amounts.

2. **Foreign currency is being lifted, flow by flow, and is nearly done on documents.** Every posting
   flow used to reject a document whose currency differed from the accounting currency. The shared
   answer lives in `Common` as `SubledgerCurrencyTranslation`, and **all five invoice and credit-note
   flows** now use it: sales invoice, free text invoice, customer credit note, vendor invoice and
   vendor credit note, the last two including the inventory cost translation that made them
   non-mechanical. **Payments are where it stands now.** The customer payment translates and posts a
   realized gain or loss, but that calculation has no test. The **vendor payment** still refuses a
   foreign currency outright. Both sides' bank-currency mismatch is still refused, which is bank
   revaluation and a separate feature.

3. **The test suite is not green, and the gap files said it was.** `dotnet test
   Tests/IXApi.Tests.csproj` on 2026-11-03: **754 passed, 3 failed, 2 skipped**. All three failures
   are `LedgerJournalPostingTests` covering journal reversal and voucher allocation, and they are the
   same three recorded in `docs/basic-erp-continuation-review-2026-11-02.md` the day before — a
   standing baseline, not a regression. They had never reached `GeneralLedger/GAPS.md`, whose status
   table claimed reversal as Implemented. Separately, the realized exchange gain/loss posting written
   on 2026-11-02 still has **no test at all**. Treat "implemented" in these files as "written", not
   "proven", unless a test is named.

4. **Nothing is verified against SQL Server.** Every finance module is covered by SQLite in-memory
   tests only. Posting atomicity, concurrent posting of the same key, number-sequence contention and
   migration application are all unproven, and the posting application-lock cannot be meaningfully
   exercised on SQLite. No module here should be described as operationally ready until this is
   addressed.

## How these files are maintained

They are part of the modules, not reports about them. The rules are the same in every file:

- Update the module's `GAPS.md` **in the same change** that alters its behavior.
- Closing a gap moves the row out of *Open gaps*, states what now exists, and adds a *Changelog* line.
- A new deliberate restriction is recorded with the **exact** message the code raises, so every row
  stays checkable by grep.
- A gap whose standard D365 behavior cannot be verified goes under *Flagged uncertainties*. Never
  guess at D365 behavior, and never invent entities, fields or business rules to close a gap.
- A change in `Common` can alter several modules at once. When that happens, update each affected
  module's file and say so in the `Changelog`.
- Keep the `Last updated` date current, and correct earlier entries that turn out to be wrong rather
  than leaving them to stand — both `Inventory/GAPS.md` and its `README.md` carry *Corrections of
  record* sections for exactly this reason.
