# Finance — gap analysis index

**Last updated:** 2026-11-01

Each finance module carries its own `GAPS.md`, recording where it stands against **basic (core)
D365 F&O** functionality. Advanced and optional features are deliberately excluded and are not
counted as gaps.

| Module | File | Highest-priority open gap |
| --- | --- | --- |
| General ledger | [GeneralLedger/GAPS.md](GeneralLedger/GAPS.md) | Journal tax calculation, and customer/vendor/bank journal line types |
| Accounts payable | [AccountsPayable/GAPS.md](AccountsPayable/GAPS.md) | Foreign currency; no AP parameters or method-of-payment entity |
| Accounts receivable | [AccountsReceivable/GAPS.md](AccountsReceivable/GAPS.md) | Foreign currency; customer aging |
| Inventory | [Inventory/GAPS.md](Inventory/GAPS.md) | SQL Server verification; four unsupported inventory models |
| Foundation | [Foundation/GAPS.md](Foundation/GAPS.md) | Sales tax settlement — tax posts but can never be settled |
| Common (shared posting) | [Common/GAPS.md](Common/GAPS.md) | Posting-profile `CustVendCode` scoping; posting lock unproven |

## The two findings that cut across every module

1. **Foreign currency is unsupported everywhere.** AP, AR and Inventory each reject any document
   whose currency differs from the ledger accounting currency, with five near-identical messages.
   The root cause is in `Common`: shared posting exposes the ledger's currencies but offers no
   conversion helper, so each module guards instead of converting. Fixing it once in `Common`
   unblocks all three.

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
