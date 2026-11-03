# Foundation — gap analysis against basic D365 F&O

**Last updated:** 2026-11-03
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
| Sales tax: codes, rates, groups, item groups, ledger account groups, exempt codes, authorities, periods | Setup implemented; see the two calculation rows below |
| Sales tax settlement (settle and post sales tax, with preview) | Implemented |
| Posted sales tax report, by date range or settlement period | Implemented |
| Payment terms (Net, current month, COD), cash discount | Implemented |
| Payment schedules (installments) | **Setup only** — configuring one makes the invoice unpostable |
| Sales tax calculation | **Partial** — percentage of net only; other origins silently ignored |
| Zero-rated codes: base recorded on `TaxTrans`, no ledger line | Implemented |
| Charges (`MarkupTable`, `MarkupTrans`) with AP/AR posting | Implemented |
| Delivery terms and delivery modes | Implemented |
| Financial dimensions and account structures | Implemented |
| HR reference data: workers, organization structure, showrooms, nationalities, occupations, genders | Implemented |
| SQL Server behavior | **Unverified** — SQLite tests only |

## Open gaps

| Area | Existing behavior | Expected basic D365 F&O behavior | Missing | Affected components | Priority |
| --- | --- | --- | --- | --- | --- |
| Realized exchange gain/loss accounts | No account storage exists. `LedgerParameters.EnableLedgerSettlementPostRealizedGainLoss` is present but unused. The transaction side is already D365-shaped: `CustTrans.CustExchAdjustmentRealized`, `ReportingExchAdjustmentRealized`, and `LedgerPostingType.ExchRateGain`/`ExchRateLoss`. | **Identified:** D365 holds them in `CurrencyLedgerGainLossAccount`, confirmed against the live table, with columns `AccountType`, `CurrencyCode`, `Ledger`, `LedgerDimension` (plus RecId/RecVersion/Partition). So the accounts are per ledger **and** per currency, keyed by an account-type enum - not columns on `Ledger`, which has none. | The table, and the settlement logic that posts the realized difference to it. Still needed before building: the **`AccountType` enum member values** (a sample row carries 1). Everything else is known. | `CurrencyLedgerGainLossAccount` (absent), `CustPaymentService`, `VendPaymentService` | **High** - one enum mapping away from implementable |
| Exchange rate adjustment | Rates, rate types and currency pairs exist and are stored. | Foreign-currency revaluation adjusts open customer/vendor balances and ledger accounts, posting unrealized gain/loss. | The adjustment process. Note that AP/AR currently reject any document currency other than the accounting currency, so there is little exposure to adjust yet — this gap unblocks only after those restrictions are lifted. | `Foundation/Currency`, AP/AR, GL | **High** (after AP/AR foreign currency) |
| **Tax calculation controls are stored but never read** | `SubledgerPostingSupport.ApplicableTaxCodesAsync` (lines 255-272) intersects the tax group with the item tax group and takes `TaxData.TaxValue` as a **percentage of the net line amount**. `order.InclTax` correctly backs tax out of a gross price. Nothing else is consulted: `TaxTable.TaxBase`, `TaxCalcMethod`, `TaxLimitBase`, `TaxOnTax` and `TaxData.TaxLimitMin`/`TaxLimitMax` have **no reader anywhere in `src/`** (verified by grep for `.TaxBase`, `.TaxCalcMethod`, `.TaxLimitBase`, `.TaxOnTax`, `.TaxLimitMin`, `.TaxLimitMax`). | Origin determines the base: percentage of net amount, percentage of gross, amount per unit, or percentage of tax on another code; `TaxCalcMethod` selects whole-amount versus interval; the limits bound the base. [Sales tax calculation](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/indirect-taxes-overview) | Every origin except percentage-of-net, interval calculation, limits, and tax-on-tax. **This gap is silent**: no error is raised, so a code configured as amount-per-unit or percentage-of-gross posts a *wrong* tax amount rather than being refused. Unlike the rest of this repository's boundaries, the setup is accepted and then ignored. | `SubledgerPostingSupport.ApplicableTaxCodesAsync`, `SalesInvoiceService`, `FreeTextInvoiceService`, `PurchaseTaxPosting`, `CustReturnService`, `PurchReturnService`, `TaxCodeRateService` | **High** — silently wrong amounts, not a refused boundary |
| Exempt-flagged codes post no `TaxTrans` row | **Narrowed 2026-11-03.** The zero-rate half is closed — see the *Changelog*. What remains: `ApplicableTaxCodesAsync` still filters `TaxGroupData.ExemptTax != NoYes.Yes` when collecting group codes, so a code flagged exempt **in that tax group** is dropped before any calculation and writes no tax record. | An exempt transaction still records its taxable base against the code, with zero tax, so exempt turnover can be reported and the exempt code shown. | The exempt-flagged path. Deliberately **not** changed with the zero-rate fix: a zero *rate* is unambiguously a rate of zero, whereas what D365 does with the `ExemptTax` flag — whether it suppresses the code entirely or records it at zero with its `TaxExemptCode` — is **not verified**, and the two filters mean different things. Tied to the `TaxExemptCode` row below; resolve both together. | `SubledgerPostingSupport.ApplicableTaxCodesAsync`, `TaxGroupData.ExemptTax`, `TaxReportService` | Medium — narrower than the zero-rate defect it was recorded with |
| Tax exempt / reverse charge behavior | **Resolved as inert.** `TaxExemptCodeTable` has an entity, configuration, DTO, validator, service and controller, but the only non-CRUD references to `TaxExempt*` in `src/` are the field-length constant and the two DI registrations. No posting or calculation path reads it. | Exempt and reverse-charge handling changes how tax posts on the document, and the exempt code is carried onto the transaction. | The whole effect. The setup can be maintained and has no consequence. Reverse charge is additionally absent. | `TaxExemptCodeService`, `SubledgerPostingSupport`, `PurchaseTaxPosting`, AR tax posting | Medium |
| `TaxTrans.TaxOrigin` is not recorded | **Narrowed 2026-11-03.** `TaxValue` is now stamped by all five flows; the purchase side already did. `TaxOrigin` and `TaxBaseQty` are still left at their defaults. | The posted tax transaction carries the origin applied as well as the rate. | `TaxOrigin`, which is meaningless until origin is honored at all (first row above), and `TaxBaseQty`, which only applies to a quantity-driven origin. Blocked behind that row, not independently fixable. | `SalesInvoiceService.TaxTransaction`, `TaxTrans` | Low — dependent on the origin row |
| Payment term methods | **Measured.** `PaymentDueDateCalculator.Calculate` supports `PaymMethod.Net` (`AddMonths` then `AddDays`), `CurrentMonth` (month end, then the offsets) and `COD` (invoice date). The `PaymMethod` enum has exactly four members, so **`CutOffDate` is the only unsupported method**, and it is refused by name. D365's current-quarter and current-year variants are not enum members in this repository at all. | Terms support net days, current month, and a cut-off day; D365 also offers current quarter and current year. | `CutOffDate`, and the two variants absent from the enum. Adding an enum member is a persisted-value change guarded by `PersistedEnumValueTests`. | `PaymentDueDateCalculator`, `PaymMethod` enum, `PaymTerm` | Medium — downgraded from High: three of the four configurable methods work, and the fourth is refused by name rather than approximated |
| **Payment schedules are setup-only and block invoicing** | `PaymSched`/`PaymSchedLine` have entities, configurations, DTOs, controllers, services and validators. But `PaymentDueDateCalculator` refuses any term carrying `PaymSched`, `PaymDayId`, `CutOffDay` or `AdditionalMonths` with `Payment-day calendars and installment schedules require a supported payment schedule before invoicing.`, and all three invoice flows call it. So a payment schedule can be configured and then makes its customers' and vendors' invoices **unpostable**. | A term with a payment schedule splits the invoice into installments, each with its own due date and its own open transaction, and the invoice settles installment by installment. [Terms of payment](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/configure-customer-payments) | Installment generation: one open customer/vendor transaction per schedule line instead of one for the invoice total. Also the payment-day calendar. Note the invoice flows currently write exactly one `CustTransOpen`/`VendTransOpen` per invoice, so this is a shape change in the open-transaction writer, not just a date calculation. | `PaymSched`, `PaymSchedLine`, `PaymentDueDateCalculator`, `SalesInvoiceService`, `FreeTextInvoiceService`, `VendorInvoiceService` | **High** — configurable setup that makes documents unpostable |
| Withholding tax | Not present. | Withholding tax is a basic AP feature in several regions. | The whole feature. Region-dependent; confirm whether it is in scope. | — | Low — confirm scope |
| Number sequences | `SysNumberSequence` with an allocation policy is used by all finance modules. | D365 number sequences support scope, continuity, manual/automatic, and preallocation. | Continuity and preallocation behavior under concurrency are unverified. | `SysNumberSequence`, `InventoryNumberSequencePolicy` | **High** — correctness under concurrent posting is unproven |
| SQL Server verification | Covered by SQLite in-memory tests. | — | Concurrency behavior of number sequences and tax posting. | Whole module | **Critical** |

## Deliberate restrictions (verified in code)

| Restriction | Message |
| --- | --- |
| Payment term method | `The configured payment-term method is not supported for invoice due-date calculation.` |
| Corrective settlement | `The interval {from} to {to} is already settled. Corrective settlement of a settled interval is outside the basic scope.` |
| Mixed settlement accounts | `The tax codes in this interval point at different sales tax settlement accounts, so the interval cannot be settled as one voucher.` |
| Settlement reporting currency | `Sales tax settlement with a different reporting currency requires exchange-rate processing, which is not supported.` |

Foundation carries far fewer explicit restrictions than the transactional modules. That is expected
for setup data, but it also means its gaps are **silent**: a missing process such as tax settlement
raises no error, it simply never runs. Treat the *Open gaps* table, not the error messages, as the
authority for this module.

## Flagged uncertainties

- **Corrective settlement runs.** D365 distinguishes an *Original* settlement of an interval from
  *Latest corrections* and *Total list* runs. Only the Original run is implemented, because the
  specification enum's D365 values could not be verified and were not invented. A second run over a
  settled interval is refused with a named error. `TaxReportPeriod.VersionNum` already tracks run
  iterations, so the field a corrective run needs is in place.

- **Payment-term methods — resolved 2026-11-03.** Measured off `PaymentDueDateCalculator`, not
  `PaymTermService`, which is the CRUD service and calculates nothing. Net, current month and COD
  work; `CutOffDate` is refused. No longer an uncertainty; see the *Open gaps* row.
- **`TaxExemptCode` — resolved 2026-11-03.** It is inert. Verified by grepping every `TaxExempt*`
  reference in `src/`: entity, configuration, DTO, validator, service, controller, one field-length
  constant and two DI registrations, and nothing else. No longer an uncertainty.
- **Whether a zero-amount `TaxTrans` row is what D365 writes for an `ExemptTax`-flagged line.**
  Partly resolved. The **zero-rate** case was implemented on 2026-11-03, because a rate row that
  says zero is unambiguously a rate of zero and dropping it discarded a configured code. The
  **`ExemptTax` flag** was deliberately left alone: whether D365 suppresses the code entirely or
  records it at zero alongside its `TaxExemptCode` is still **not verified against a live
  environment**, and guessing would mean inventing posting behavior. Confirm before changing that
  filter.
- **Whether a zero-rated code should post a zero-amount ledger line.** The implementation posts
  none, on the grounds that a zero ledger entry conveys nothing and resolving a payable account for
  it would newly refuse a company whose zero-rated code has no account configured. Whether D365
  writes a zero `GeneralJournalAccountEntry` here is **unverified**; if it does, the voucher's
  amounts are unaffected either way.
- **Whether the installment open transactions are `CustTransOpen` rows or a schedule-specific
  table.** D365 splits a scheduled invoice into installments; that one open transaction per
  installment is the mechanism is inferred from the settlement model, **not verified**. Confirm
  before changing the open-transaction writer.
- **Withholding tax scope.** Region-specific. Microsoft documents it under Accounts payable, but it
  is not universally basic.
- **HR reference data scope.** Workers, occupations, nationalities, genders and showrooms are
  present. These support finance dimensions and responsibility assignment rather than HR proper;
  whether they should grow further is a product decision, not a parity gap.

## Changelog

| Date | Change |
| --- | --- |
| 2026-11-03 | **Closed the zero-rate half of the tax reporting gap.** `ApplicableTaxCodesAsync` no longer discards a code because its rate is zero: it now distinguishes a code with **no `TaxData` row covering the date** (not in force — still dropped) from one whose row says **zero** (in force at zero — kept). All five invoice and credit-note flows now write that code's `TaxTrans` row with its taxable base and zero tax, while posting **no** ledger line and resolving **no** payable or input-tax account for it, so a zero-rated code needs no account configured and the voucher gains no zero-amount entries. `AllocateTax` returns one zero per code when every rate is zero, where it previously divided by zero. The two charge flows stopped returning early on a zero rate, so a zero-rated charge's base is reported too. `TaxValue` is now stamped on the AR rows, which the purchase side already did — with zero-tax rows being written it is what distinguishes zero-rated from a rate never recorded. Covered by `Tests/ZeroRatedTaxTests.cs`; verified by reverting the two shared changes and confirming three of the six tests fail. The `ExemptTax`-flagged path is **unchanged and still open** at Medium: that flag's D365 posting behavior is not verified and is a different question from a zero rate. |
| 2026-11-03 | Source scan of tax calculation and payment terms. **No behavior changed.** Four findings recorded, two of them High: the tax calculation reads only `TaxData.TaxValue` as a percentage of net and silently ignores `TaxBase`, `TaxCalcMethod`, `TaxLimitBase`, `TaxOnTax` and the limit columns — the module's first *silent* wrong-amount gap rather than a named refusal; and zero-rate and exempt-flagged codes are filtered out before any `TaxTrans` row is written, so exempt turnover never reaches the posted sales tax report added on 2026-11-02. Also recorded that payment schedules are setup-only and make a configured customer's invoices unpostable. Resolved two standing uncertainties by measurement: the payment-term row now names `CutOffDate` as the only unsupported method (downgraded High to Medium), and `TaxExemptCode` is confirmed inert. |
| 2026-11-02 | Identified the real D365 home for realized gain/loss accounts: `CurrencyLedgerGainLossAccount` (`AccountType`, `CurrencyCode`, `Ledger`, `LedgerDimension`), confirmed against the live table. Ruled out `Ledger` itself and the `_RU`/`_W`/`_PL` localization tables. |
| 2026-11-02 | Corrected the gain/loss gap: `Ledger` does not hold the realized gain/loss accounts in D365 (confirmed against the real table), so the earlier suggestion to add columns there was wrong. This repo's `Ledger` already matches D365 column for column. |
| 2026-11-02 | Added the **posted sales tax report** (`GET api/v1/tax/reports/posted-sales-tax`), by date range or settlement period interval, per tax code and direction, in accounting-currency amounts, showing settled versus unsettled tax. It excludes the offsetting transactions settlement writes, which would otherwise make a settled period total zero. Recorded that realized exchange gain/loss has nowhere to store its accounts. |
| 2026-11-01 | **Implemented sales tax settlement** (`POST api/v1/tax/settlement`, with a `preview`). Clears the input and output tax accounts for a period interval and moves the net to `TaxLedgerAccountGroup.TaxReportLedgerDimension`, posts offsetting `TaxTrans` rows unless the period sets `NotGenerateOffsetTaxTrans`, and stamps each settled row with its period and run number. No schema change: every field and table it needs already existed. Covered by `Tests/TaxSettlementTests.cs`. |
| 2026-11-01 | File created from a source scan. Recorded sales tax settlement as the critical gap — tax posts but can never be settled — and noted that this module's gaps are silent rather than error-raising. |

## Sources

- [Sales tax overview](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/indirect-taxes-overview)
- [Settle and post sales tax](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/tasks/settle-post-sales-tax)
- [Foreign currency revaluation](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/foreign-currency-revaluation-general-ledger)
- [Terms of payment](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/configure-customer-payments)
- [Charges overview](https://learn.microsoft.com/en-us/dynamics365/supply-chain/procurement/charges-purchase-orders)
- [Global address book](https://learn.microsoft.com/en-us/dynamics365/fin-ops-core/fin-ops/organization-administration/overview-global-address-book)
