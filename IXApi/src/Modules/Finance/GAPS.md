# Finance — gap analysis index

**Last updated:** 2026-11-04

Each finance module carries its own `GAPS.md`, recording where it stands against **basic (core)
D365 F&O** functionality. Advanced and optional features are deliberately excluded and are not
counted as gaps.

| Module | File | Highest-priority open gap |
| --- | --- | --- |
| General ledger | [GeneralLedger/GAPS.md](GeneralLedger/GAPS.md) | Three failing journal posting tests (reversal, voucher allocation); journal tax calculation |
| Accounts payable | [AccountsPayable/GAPS.md](AccountsPayable/GAPS.md) | No purchase **discount** agreements (prices now resolve); payment proposal; net unit price / charges matching |
| Accounts receivable | [AccountsReceivable/GAPS.md](AccountsReceivable/GAPS.md) | Return dispositions; settlement priority and cash discount on the settlement path |
| Inventory | [Inventory/GAPS.md](Inventory/GAPS.md) | SQL Server verification; four unsupported inventory models |
| Foundation | [Foundation/GAPS.md](Foundation/GAPS.md) | Tax origins other than percentage-of-net are refused, not calculated; payment schedules |
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

2. **Foreign currency is now complete across every document and payment flow.** Every posting
   flow used to reject a document whose currency differed from the accounting currency. The shared
   answer lives in `Common` as `SubledgerCurrencyTranslation`, and **all five invoice and credit-note
   flows** now use it: sales invoice, free text invoice, customer credit note, vendor invoice and
   vendor credit note, the last two including the inventory cost translation that made them
   non-mechanical. **Both payments were finished on 2026-11-03**: the customer payment's realized
   gain/loss calculation, written on 2026-11-02 and untested, is now covered in both directions, and
   the vendor payment translates and posts its own realized difference instead of refusing a foreign
   currency. Two things are deliberately still refused by name: a **bank account in another
   currency**, which is bank revaluation; and settling two transactions against each other at
   **different rates**, because that path posts no voucher to carry the difference. What is left is
   **revaluation** of open balances, not translation.

3. **Two things that were only "written" are now proven, and the suite is green.** `dotnet test
   Tests/IXApi.Tests.csproj` began 2026-11-03 at **754 passed, 3 failed, 2 skipped**. Both problems
   behind that are closed. The three failures were `LedgerJournalPostingTests` covering journal
   reversal and voucher allocation — red since at least 2026-11-02, recorded in
   `docs/basic-erp-continuation-review-2026-11-02.md` but never in `GeneralLedger/GAPS.md`, whose
   status table claimed reversal as Implemented. All three were **fixture** defects, so no production
   code changed; what had been missing was the coverage itself, which had never once run. The
   realized exchange gain/loss posting was likewise untested for a day, and is now covered in both
   directions; it too was correct as written. Keep the reading rule either way: treat "implemented"
   in these files as "written", not "proven", unless a test is named — and more rows now name one.

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
