# Foundation — gap analysis against basic D365 F&O

**Last updated:** 2026-11-01
**Scope:** the shared setup and master data that the finance modules depend on — legal entities,
organization, global address book, currency and exchange rates, sales tax, payment terms and
schedules, cash discount, charges (markup), delivery terms and modes, and the HR reference data used
for finance dimensions. Human resources proper, payroll, and tax localizations are out of scope.

## How this file is maintained

Update this file **in the same change** that alters behavior. Closing a gap moves the row out of
*Open gaps* and adds a *Changelog* line. A new deliberate restriction is recorded with the **exact**
message the code raises, so the table stays verifiable by grep. A gap whose standard D365 behavior
cannot be verified goes under *Flagged uncertainties* rather than being guessed at.

## Status summary

| Area | Status |
| --- | --- |
| Legal entities (`CompanyInfo`), organization units, departments | Implemented |
| Global address book: parties, locations, postal and electronic addresses, contacts | Implemented |
| Currencies, exchange rate types, exchange rates, currency pairs | Implemented |
| Sales tax: codes, rates, groups, item groups, ledger account groups, exempt codes, authorities, periods | Implemented |
| Payment terms, payment schedules, cash discount | Implemented |
| Charges (`MarkupTable`, `MarkupTrans`) with AP/AR posting | Implemented |
| Delivery terms and delivery modes | Implemented |
| Financial dimensions and account structures | Implemented |
| HR reference data: workers, organization structure, showrooms, nationalities, occupations, genders | Implemented |
| SQL Server behavior | **Unverified** — SQLite tests only |

## Open gaps

| Area | Existing behavior | Expected basic D365 F&O behavior | Missing | Affected components | Priority |
| --- | --- | --- | --- | --- | --- |
| Sales tax settlement and payment | Tax codes, rates, groups, periods and ledger account groups are configured, and tax posts with AP/AR documents into `TaxTrans`. | The **Settle and post sales tax** periodic process closes a tax period, posts the net tax to the settlement account, and produces the tax payment. | The settlement process and `TaxReportVoucher`. No service exists. | `Foundation/Tax`, `TaxPeriodService`, GL posting | **Critical** — tax is collected and posted but can never be settled, so the tax accounts only ever grow |
| Sales tax reporting | Tax transactions are recorded per document. | Basic tax reporting lists tax per code, period and authority for a return. | Reporting over `TaxTrans` by period and authority. | `Foundation/Tax` | **High** — depends on settlement above |
| Exchange rate adjustment | Rates, rate types and currency pairs exist and are stored. | Foreign-currency revaluation adjusts open customer/vendor balances and ledger accounts, posting unrealized gain/loss. | The adjustment process. Note that AP/AR currently reject any document currency other than the accounting currency, so there is little exposure to adjust yet — this gap unblocks only after those restrictions are lifted. | `Foundation/Currency`, AP/AR, GL | **High** (after AP/AR foreign currency) |
| Payment term methods | Payment terms compute an invoice due date; an unsupported method is rejected with a named error. | Terms support net days, current month, current quarter, current year, specific day of month, and COD. | The method variants that are rejected. The exact set still missing should be read off `PaymTermService` before building. | `PaymTermService`, `PaymTerm` | **High** — due dates drive aging, cash discount and payment proposal |
| Tax exempt / reverse charge behavior | `TaxExemptCode` setup exists. | Exempt and reverse-charge handling changes how tax posts on the document. | Whether the exempt code affects posting is not established; the setup may be inert. Verify before claiming either way. | `TaxExemptCodeService`, `PurchaseTaxPosting`, AR tax posting | Medium |
| Withholding tax | Not present. | Withholding tax is a basic AP feature in several regions. | The whole feature. Region-dependent; confirm whether it is in scope. | — | Low — confirm scope |
| Number sequences | `SysNumberSequence` with an allocation policy is used by all finance modules. | D365 number sequences support scope, continuity, manual/automatic, and preallocation. | Continuity and preallocation behavior under concurrency are unverified. | `SysNumberSequence`, `InventoryNumberSequencePolicy` | **High** — correctness under concurrent posting is unproven |
| SQL Server verification | Covered by SQLite in-memory tests. | — | Concurrency behavior of number sequences and tax posting. | Whole module | **Critical** |

## Deliberate restrictions (verified in code)

| Restriction | Message |
| --- | --- |
| Payment term method | `The configured payment-term method is not supported for invoice due-date calculation.` |

Foundation carries far fewer explicit restrictions than the transactional modules. That is expected
for setup data, but it also means its gaps are **silent**: a missing process such as tax settlement
raises no error, it simply never runs. Treat the *Open gaps* table, not the error messages, as the
authority for this module.

## Flagged uncertainties

- **Which payment-term methods are actually supported.** The rejection message does not name the
  supported set. Read `PaymTermService` before recording the missing variants precisely.
- **Whether `TaxExemptCode` affects posting.** The setup exists; its effect on tax calculation has
  not been traced. Do not assume either way.
- **Withholding tax scope.** Region-specific. Microsoft documents it under Accounts payable, but it
  is not universally basic.
- **HR reference data scope.** Workers, occupations, nationalities, genders and showrooms are
  present. These support finance dimensions and responsibility assignment rather than HR proper;
  whether they should grow further is a product decision, not a parity gap.

## Changelog

| Date | Change |
| --- | --- |
| 2026-11-01 | File created from a source scan. Recorded sales tax settlement as the critical gap — tax posts but can never be settled — and noted that this module's gaps are silent rather than error-raising. |

## Sources

- [Sales tax overview](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/indirect-taxes-overview)
- [Settle and post sales tax](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/tasks/settle-post-sales-tax)
- [Foreign currency revaluation](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/foreign-currency-revaluation-general-ledger)
- [Terms of payment](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/configure-customer-payments)
- [Charges overview](https://learn.microsoft.com/en-us/dynamics365/supply-chain/procurement/charges-purchase-orders)
- [Global address book](https://learn.microsoft.com/en-us/dynamics365/fin-ops-core/fin-ops/organization-administration/overview-global-address-book)
