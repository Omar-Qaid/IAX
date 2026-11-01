# General ledger — gap analysis against basic D365 F&O

**Last updated:** 2026-11-01
**Scope:** basic (core) D365 F&O General ledger only. Budgeting, budget control, allocations,
consolidation, elimination, financial reporting designer, and intercompany accounting are out of
scope by request and are not listed as gaps.

## How this file is maintained

Update this file **in the same change** that alters behavior. Closing a gap moves the row out of
*Open gaps* and adds a *Changelog* line. A new deliberate restriction is recorded with the **exact**
message the code raises, so the table stays verifiable by grep. A gap whose standard D365 behavior
cannot be verified goes under *Flagged uncertainties* rather than being guessed at.

## Status summary

| Area | Status |
| --- | --- |
| Ledger, chart of accounts, main accounts and categories | Implemented |
| Account structures / financial dimensions (`DimensionAttributeValueCombination`) | Implemented |
| Fiscal calendars, years, periods; ledger period status; per-module period access | Implemented |
| General journals: names, header, lines, posting, reversal | Implemented |
| Posted ledger: `GeneralJournalEntry`, `GeneralJournalAccountEntry`, subledger vouchers | Implemented |
| Trial balance, financial statements, year-end close | Implemented |
| Ledger settlement | Implemented |
| Bank: groups, accounts, transactions, statements, transaction types | Implemented |
| Ledger parameters | Implemented |
| SQL Server behavior (atomicity, concurrency, idempotency) | **Unverified** — SQLite tests only |

## Open gaps

| Area | Existing behavior | Expected basic D365 F&O behavior | Missing | Affected components | Priority |
| --- | --- | --- | --- | --- | --- |
| Journal reversal | Reversal must be posted as its own journal; automatic reversal is rejected with a named error. Reversal of a posted journal is implemented via `LedgerJournalReversalService`. | A journal can be marked for automatic reversal on a future date, and the system posts the reversing entry when that date arrives. | The reversing-date flag and the batch that acts on it. | `LedgerJournalTable`, `LedgerJournalReversalService` | Medium |
| Journal tax calculation | General journal lines reject tax calculation with a named error; tax posts through the AP/AR document flows. | A general journal line carries a sales tax group and item sales tax group, and tax is calculated and posted with the journal. | Tax calculation on general journal lines. | `LedgerJournalTrans`, `LedgerJournalPosting`, `Foundation/Tax` | **High** — a basic general journal is expected to handle tax |
| Journal line account types | The basic general journal accepts **Ledger** account and offset lines only, rejecting other types with a named error. | Account type may be Ledger, Customer, Vendor, Bank, Fixed asset or Project, which posts to that subledger. | Customer, Vendor and Bank line types, which are the basic ones. Fixed asset and Project belong to modules not present here. | `LedgerJournalTrans`, `LedgerJournalPosting` | **High** — customer/vendor/bank journal lines are core basic function |
| Periodic / recurring journals | Not present. | A saved periodic journal is copied forward on a schedule. | `LedgerJournalPeriodic` and the copy process. No such entity exists. | General journal | Medium |
| Year-end close in reporting currency | Year-end transfer rejects a reporting currency that differs from the accounting currency, with a named error. | The close converts balances into the reporting currency as well. | Reporting-currency balance conversion. | `LedgerYearEndCloseController`, year-end close service | Medium |
| Intercompany accounting | Intercompany journal posting is rejected with a named error. | Basic intercompany accounting posts the due-to/due-from legs in both companies. | Intercompany setup and the paired posting. | `LedgerJournalPosting` | Low — borders on optional; confirm before building |
| Ledger period access: inventory | `LedgerPeriodModuleAccessControl` carries Ledger, Tax, Bank, Customer and Vendor access levels. | The ledger calendar also controls the **Inventory** module independently. | `InventAccessLevel` / `InventUserGroupInfo`. Inventory postings therefore ride on the Ledger module's access. Not invented here because it is a schema change. | `LedgerPeriodModuleAccessControl`, `SubledgerPostingSupport.LedgerPostingModule` | Medium |
| Currency revaluation | Exchange rates, rate types and currency pairs exist; no revaluation process. | Foreign-currency revaluation of ledger balances posts unrealized gain/loss. | The revaluation process and its posting. Note that nearly every posting flow currently requires the document currency to equal the accounting currency, so there is little foreign-currency exposure to revalue yet. | `Foundation/Currency`, GL posting | Medium |
| SQL Server verification | Covered by SQLite in-memory tests. | — | Posting atomicity, concurrent voucher allocation, and number-sequence contention under SQL Server. | Whole module | **Critical** |

## Deliberate restrictions (verified in code)

| Restriction | Message |
| --- | --- |
| Account type on journal lines | `The basic general journal accepts Ledger account lines only.` |
| Offset account type | `The basic general journal accepts Ledger offset accounts only.` |
| Automatic reversal | `Automatic reversal journals are not supported; post the reversal as its own journal.` |
| Journal tax | `{label}: journal tax calculation is not supported; post tax through its own process.` |
| Intercompany | `{label}: intercompany journal posting is not supported.` |
| Ledger settlement toggle | `Ledger settlement is not enabled in the general ledger parameters.` |
| Year-end reporting currency | `Year-end transfer requires reporting-currency balance conversion, which is not supported.` |

## Flagged uncertainties

- **Financial statements scope.** `LedgerFinancialStatementsController` exists, but how closely its
  output should match D365's financial reporting (which is a separate reporting tool, arguably
  outside basic scope) is not established. Treat its shape as local, not as parity.
- **Intercompany as "basic".** Intercompany accounting is listed under basic General ledger setup in
  Microsoft's documentation but is only meaningful with more than one legal entity in use. Confirm it
  is wanted before building.
- **Ledger settlement semantics.** Implemented and gated by a parameter; whether its matching rules
  equal D365's ledger settlement is not verified against a live environment.

## Changelog

| Date | Change |
| --- | --- |
| 2026-11-01 | File created from a source scan. Recorded journal tax and journal account types as the two highest-priority gaps, and the inventory ledger-period-access gap. |

## Sources

- [General ledger home page](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/general-ledger)
- [Financial dimensions](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/financial-dimensions)
- [Fiscal calendars, years and periods](https://learn.microsoft.com/en-us/dynamics365/finance/budgeting/fiscal-calendars-fiscal-years-periods)
- [Ledger settlements](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/ledger-settlements)
- [Year-end close](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/year-end-close)
