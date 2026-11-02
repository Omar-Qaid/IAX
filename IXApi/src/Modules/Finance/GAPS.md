# Finance — gap analysis index

**Last updated:** 2026-11-01

Each finance module carries its own `GAPS.md`, recording where it stands against **basic (core)
D365 F&O** functionality. Advanced and optional features are deliberately excluded and are not
counted as gaps.

| Module | File | Highest-priority open gap |
| --- | --- | --- |
| General ledger | [GeneralLedger/GAPS.md](GeneralLedger/GAPS.md) | Journal tax calculation, and customer/vendor/bank journal line types |
| Accounts payable | [AccountsPayable/GAPS.md](AccountsPayable/GAPS.md) | Foreign currency; payment proposal; invoice matching tolerances |
| Accounts receivable | [AccountsReceivable/GAPS.md](AccountsReceivable/GAPS.md) | Customer aging; exchange gain/loss on settlement |
| Inventory | [Inventory/GAPS.md](Inventory/GAPS.md) | SQL Server verification; four unsupported inventory models |
| Foundation | [Foundation/GAPS.md](Foundation/GAPS.md) | Realized gain/loss accounts have nowhere to live (schema decision); payment-term methods |
| Common (shared posting) | [Common/GAPS.md](Common/GAPS.md) | Posting lock unproven under SQL Server; implicit enum values still renumber on reorder |

## The two findings that cut across every module

1. **Foreign currency is being lifted, flow by flow.** Every posting flow used to reject a document
   whose currency differed from the accounting currency. The shared answer lives in `Common` as
   `SubledgerCurrencyTranslation`, and **all three accounts receivable invoice flows** now use it:
   sales invoice, free text invoice and credit note. Two things remain, and neither is mechanical:
   the **vendor invoice**, because its inventory cost arrives from the document and must be
   translated before it is written to inventory value; and **exchange-rate gain/loss at settlement**,
   which has not been started - translating a document is not the same as recognising the gain or
   loss when it is paid at a different rate.

2. **Nothing is verified against SQL Server.** Every finance module is covered by SQLite in-memory
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
