# General ledger — gap analysis against basic D365 F&O

**Last updated:** 2026-11-13
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
| Fiscal calendars, years, periods; ledger period status; per-module period access (incl. Inventory) | Implemented |
| Reference cycle (`CycleErpExamples/GeneralLedger/Logic_Readonly.md`) executed end to end | Implemented and **tested** — all fifteen steps through the real services, asserting the 6,300 trial balance, the 1,200 result and the 4,500 year-end opening |
| General journals: names, header, lines, posting, reversal | Implemented |
| Journal line account types: Ledger, Customer, Vendor, Bank | Implemented — writes `CustTrans`/`CustTransOpen`, `VendTrans` + open balance, `BankAccountTrans` |
| Journal reversal and voucher allocation at posting | Implemented and **tested** — the three long-standing failures were fixture defects, fixed 2026-11-03 |
| Posted ledger: `GeneralJournalEntry`, `GeneralJournalAccountEntry`, subledger vouchers | Implemented |
| Trial balance, financial statements | Implemented |
| Year-end close: opening transfer, closing transaction, re-run after adjustment | Implemented; closing-transaction scope and close ordering corrected 2026-11-12 |
| Ledger settlement | Implemented |
| Bank: groups, accounts, transactions, statements, transaction types | Implemented |
| Ledger parameters | Implemented |
| SQL Server behavior | **Unverified for this module** — SQLite tests only. Only the AP cycle has been run on SQL Server (2026-11-04); `BatchDatabaseLock` mutual exclusion is verified there, but no AR/GL/Inventory flow is. |

## Open gaps

| Area | Existing behavior | Expected basic D365 F&O behavior | Missing | Affected components | Priority |
| --- | --- | --- | --- | --- | --- |
| Journal reversal | Reversal must be posted as its own journal; automatic reversal is rejected with a named error. Reversal of a posted journal is implemented via `LedgerJournalReversalService`. | A journal can be marked for automatic reversal on a future date, and the system posts the reversing entry when that date arrives. | The reversing-date flag and the batch that acts on it. | `LedgerJournalTable`, `LedgerJournalReversalService` | Medium |
| Journal tax calculation | **Narrower than this row claimed until 2026-11-12.** One journal tax case *is* implemented: a **Vend** line with an inclusive-tax header, both tax groups and a ledger offset calculates input tax through `VendorJournalTaxPosting`, which reclassifies tax out of the inclusive expense and writes `TaxTrans` and `TaxJournalTrans` against the journal line. Every other combination is refused by `LedgerJournalPosting.EnsureInScope` with the named error. | A general journal line carries a sales tax group and item sales tax group, and tax is calculated and posted with the journal for any line type and for exclusive amounts. | Exclusive-amount tax, **Cust** (output tax) lines, and `Ledger` lines carrying tax. A manual *customer* invoice entered through the journal still cannot carry tax, so it remains usable for net amounts only. Depends on the two `Foundation` tax-calculation gaps, since it would reuse `ApplicableTaxCodesAsync`. | `LedgerJournalTrans`, `LedgerJournalPosting`, `VendorJournalTaxPosting`, `Foundation/Tax` | **High** — a basic general journal is expected to handle tax on both sides |
| Journal line **offset** account types | Offset accounts accept **Ledger** only, refused by name when another type carries an offset dimension (`LedgerJournalTransDtoValidator` line 23). The primary account side now accepts Cust, Vend and Bank. | An offset account may be any of the same types as the primary account, so a one-line journal can move an amount between, say, a customer and a bank. | The subledger offset. The writer it needs already exists — `WriteSubledgerAsync` is already called per `PostingSide` — so this is narrower than it was: the offset side simply never reaches it with a non-Ledger type. | `LedgerJournalTransDtoValidator`, `LedgerJournalPosting.WriteSubledgerAsync` | Medium — the primary-account path covers the common entry; a two-line journal is the workaround |
| §2 cycle as a seeded scenario | `CycleErpExamples/GeneralLedger/` implements `GL-01` only. The reference's fifteen-step §2 cycle is proven by `Tests/GeneralLedgerCycleTests.cs`, not by a seeded scenario, so it cannot be replayed against SQL Server through the example host the way the AP package can. | — (verification shape, not behavior) | A scenario definition, preparation manifest and validator for the §2 cycle, so it runs under `SeedingErpV2SqlServerTests` alongside the other packages. | `CycleErpExamples/GeneralLedger`, `SeedingErpV2SqlServerTests` | Medium — the figures are proven, the SQL Server replay is not |
| Periodic / recurring journals | Not present. | A saved periodic journal is copied forward on a schedule. | `LedgerJournalPeriodic` and the copy process. No such entity exists. | General journal | Medium |
| Year-end close in reporting currency | Year-end transfer rejects a reporting currency that differs from the accounting currency, with a named error. | The close converts balances into the reporting currency as well. | Reporting-currency balance conversion. | `LedgerYearEndCloseController`, year-end close service | Medium |
| Intercompany accounting | Intercompany journal posting is rejected with a named error. | Basic intercompany accounting posts the due-to/due-from legs in both companies. | Intercompany setup and the paired posting. | `LedgerJournalPosting` | Low — borders on optional; confirm before building |
| Currency revaluation | Exchange rates, rate types and currency pairs exist; no revaluation process. | Foreign-currency revaluation of ledger balances posts unrealized gain/loss. | The revaluation process and its posting. Note that nearly every posting flow currently requires the document currency to equal the accounting currency, so there is little foreign-currency exposure to revalue yet. | `Foundation/Currency`, GL posting | Medium |
| SQL Server verification | Covered by SQLite in-memory tests. | — | Posting atomicity, concurrent voucher allocation, and number-sequence contention under SQL Server. | Whole module | **Critical** |

## Deliberate restrictions (verified in code)

| Restriction | Message |
| --- | --- |
| Account type on journal lines | `The basic general journal accepts Ledger, Cust, Vend and Bank account lines.` |
| Offset account type | `The basic general journal accepts Ledger offset accounts only.` |
| Automatic reversal | `Automatic reversal journals are not supported; post the reversal as its own journal.` |
| Journal tax | `{label}: journal tax calculation is not supported; post tax through its own process.` |
| Intercompany | `{label}: intercompany journal posting is not supported.` |
| Ledger settlement toggle | `Ledger settlement is not enabled in the general ledger parameters.` |
| Year-end reporting currency | `Year-end transfer requires reporting-currency balance conversion, which is not supported.` |

## Audited and found sound (2026-11-12)

Recorded so these are not re-investigated. Each was traced and raised no defect.

- **Ledger settlement** (`LedgerSettlementService`): matching data only, no voucher and no balance
  change, which is correct. Same main account and same transaction currency are enforced, an
  out-of-balance selection is refused by name rather than approximated, an already-settled entry is
  refused, reversal marks the rows instead of deleting them, and `OpenItemsAsync` excludes
  standing (non-reversed) matches. `GL-03`'s 500/500 intermediary-account case is supported.
- **Fiscal period status machine** (`FiscalPeriodStatusTransition`): Open ↔ On hold, either to
  permanently Closed, Closed terminal, `YearClosed` neither set nor cleared by a manual change —
  which matches the reference's warning not to use permanent closure as the ordinary month-end
  switch.
- **Period access enforcement** (`SubledgerPostingSupport.LoadLedgerContextAsync`): operating period
  must exist and be Open, and the per-module access level (including Inventory) and its user-group
  rule are both applied. The year-end close deliberately does not go through this path, because it
  must post into the period it is closing.
- **Voucher balance check** (`LedgerVoucherBalanceCheck`): groups by company, voucher, date *and*
  currency and never nets unrelated groups. The per-currency grouping would refuse a genuinely
  multi-currency voucher, which is consistent with the module-wide restriction that a posting's
  document currency equals the accounting currency; it is a boundary, not a defect.
- **Year-end single-amount `AccountEntry`**: the close records one amount as transaction,
  accounting *and* reporting amount. That is safe only because the close refuses a differing
  reporting currency up front, which it does, by name.

## Flagged uncertainties

- **Closing vouchers in the trial balance vs. the income statement.** `LedgerTrialBalanceService`
  includes `YearEndClosing` entries in period movement, so after closing, the closed year's trial
  balance shows the income statement at zero. `LedgerFinancialStatementsController` **excludes**
  them, so the same year still reports its result. Each is defensible for its own purpose and D365
  has reporting options on both sides, but which one this module should present was not verified
  against a live environment. No behavior was changed.

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
| 2026-11-13 | **Made the general ledger reference cycle executable.** `CycleErpExamples/GeneralLedger/` does have a scenario package — contrary to what the open-gap row said before this entry, and the row was corrected rather than left standing — but it implements **`GL-01` only**, a 125 SAR expense reclassification and its reversal, as its own README states. The reference's §2 cycle, the **6,300** trial balance and the **1,200** carried to retained earnings were still proven by nothing, which is why those UAT ids in its §6 read `Not run`. `General_ledger_reference_cycle_balances_at_six_thousand_three_hundred_and_carries_twelve_hundred_to_equity` in `Tests/GeneralLedgerCycleTests.cs` now runs all fifteen through the **real** purchase, receipt, vendor-invoice, demand, reservation, packing-slip, sales-invoice, customer-payment, vendor-payment, inventory-close and general-journal services, then asserts: the per-account trial balance (bank **1,000**, inventory **3,500**, vendors **3,000** Cr, accrued expenses **300** Cr, goods revenue **2,700** Cr, service revenue **300** Cr, cost of sales **1,500**, expense **300**), a flat receivables control account, **6,300** on each side, a net result of **1,200**, and the year-end close carrying that 1,200 to retained earnings with the new year opening balanced at **4,500**. **Unlike the Inventory equivalent it exposed no new production defect** — the figures came out right the first time the cycle ran. What it does add is end-to-end proof of the year-end close fix made the day before: the closing voucher is asserted to zero only the four income-statement accounts and to leave bank, inventory, vendors and accrued expenses untouched, now against real posted documents instead of a hand-built fixture. It also proves two things no other test covered: the item-specific `InventPosting` override (the service item resolves its own revenue account, so the reference's 2,700/300 revenue split is observable) and that automatic customer settlement clears 1,800 then 200 across two invoices and closes the customer at zero. Two of my own expectations were wrong rather than the code: `LedgerTrialBalanceDto.TotalDebit` is the period's **gross movement** (14,800 here, because inventory is debited and then relieved) while the reference's 6,300 is the **balance** column, `TotalDebitBalance`; and the income statement legitimately still reports the closed year's result because it excludes closing vouchers by design. |
| 2026-11-12 | **Second pass over the areas the journal scan did not reach — year-end close, ledger settlement, trial balance, period access — and fixed the close.** **(1) The year-end closing transaction zeroed the balance sheet.** With *Create closing transactions* selected, `LedgerYearEndCloseController` posted the negative of **every** dimension balance, assets and liabilities included. The voucher balanced (the year's balances sum to zero), so nothing looked wrong, and the only existing test asserted the *count* of closing entries, never their lines. The effect was that the closed year reported zero assets and zero liabilities: in the test fixture the asset account's 100 netted to nothing. D365's closing transaction zeroes the **income statement** and books the net result to retained earnings — balance-sheet accounts are carried into the new year by the opening transaction, not closed — and the reference says so explicitly in §5 ("إنشاء سند إقفال **لحسابات الإيرادات والمصروفات** داخل السنة السابقة"). The closing transaction now posts the income-statement dimensions to zero plus the net result to retained earnings, still as a delta against earlier closing runs so a re-run after a late adjustment closes only the difference. The two duplicated closing blocks (one per branch) were replaced by one `AddClosingTransactionAsync`, so the zero-delta branch no longer has its own, weaker, `alreadyClosed`-gated variant. **(2) The close-ordering guard was blind to a zero-delta close.** `MostRecentYearEndClose` is what refuses closing an earlier year after a later one, and only the transfer path recorded it; a year closed through the "opening balances are current" branch left it unset, so an earlier year could then be closed behind it. It is now recorded on that branch too, and only ever moves forward. Verified by a clean build and the full suite; the new line-level assertions in `Year_end_close_transfers_profit_to_equity_and_closes_periods` were **confirmed to fail with the fix reverted**. |
| 2026-11-12 | **Scanned the journal → posted-ledger transition against `CycleErpExamples/GeneralLedger/Logic_Readonly.md` and fixed three defects.** The transition itself — entered journal grouped into one `GeneralJournalEntry` per voucher and accounting date, balance asserted on the *posted* signed accounting and reporting amounts rather than on the entered columns (reference §79), `SubledgerVoucherGeneralJournalEntry` written with company and date so the document is never linked by voucher alone (§80), `Posted`/`NoEdit` set once under a journal-scoped lock (`GL-07`) — is sound. Three things around it were not. **(1) The bank subledger recorded the wrong currency.** `LedgerJournalPosting.WriteSubledgerAsync` wrote `BankAccountTrans.AmountReportingCurrency = accountingAmount` while the correctly computed `reportingAmount` sat unused in the same method, which the customer branch two cases above uses properly; every other bank writer in Finance (`VendPaymentService`, `CustPaymentService`) passes the reporting amount. So a bank journal line under a ledger with a reporting currency different from the accounting currency reported the accounting figure as its reporting figure (`GL-04`). **(2) A journal that posted could not be reversed.** `MainAccountPostingControls` refuses a posted line with no payment reference when the account sets `MandatoryPaymentReference`, but `LedgerJournalReversalService.Mirror` did not carry `PaymReference` onto the mirrored line — so the original posted and its reversal was refused with `requires a payment reference`, blocking `GL-02` for exactly the accounts that control is meant for. The mirror now carries it; settlement selection, the marked invoice and the vendor invoice number are still deliberately dropped, because a reversal must not settle or re-register the document it reverses. **(3) Hardening, not a production defect:** `MarkCorrectionAsync` selected the posted entries by `JournalNumber` alone. The global `DataAreaId` filter scopes that in production, but journal numbers come from a company-scoped sequence and the test context applies no filter, so the company predicate is now explicit, as it is in every other query in that service. Both behavior fixes have a new test in `LedgerJournalPostingTests` — `A_bank_line_records_the_reporting_amount_in_the_reporting_currency_field` (SAR accounting, USD reporting at 0.25, asserting the bank movement's 2,000 / 2,000 / **500**) and `A_journal_on_an_account_requiring_a_payment_reference_can_be_reversed` — and **each was confirmed to fail with the fix reverted and pass with it applied**, so neither is a test written around existing behavior. |
| 2026-11-12 | Source scan corrections, **no behavior changed**. **Removed the inventory ledger-period-access row**, which this file still carried as an open Medium gap: `LedgerPeriodModuleAccessControl.InventAccessLevel` / `InventUserGroupInfo` exist, `SubledgerPostingSupport` reads them for `LedgerPostingModule.Inventory`, and `Inventory/GAPS.md` records the change that added them on 2026-11-01. **Narrowed the journal tax row**, which claimed general journal lines reject tax outright: the inclusive **Vend** case with both tax groups and a ledger offset is implemented through `VendorJournalTaxPosting`, writing `TaxTrans` and `TaxJournalTrans` and a balanced reclassification pair. What is actually missing is exclusive amounts, **Cust** output tax and tax on `Ledger` lines; the row now says so and stays High. |
| 2026-11-03 | **Fixed the three `LedgerJournalPostingTests` failures**, red since at least 2026-11-02 and covering journal reversal and voucher allocation - the two things this file claimed as Implemented while nothing proved either. **All three were defects in the test fixture, not in production code; no production code was changed.** `Allocation_at_posting_replaces_each_temporary_voucher_group_from_configured_sequence` called `Seed(db)`, which only stages its rows, and then queried the database, so it read an empty store and failed with `Sequence contains no elements` before reaching any behavior - it needed a `SaveChangesAsync`. Both reversal tests failed with `Journal name 'GEN' has no voucher number sequence.` because the fixture's `LedgerJournalName` had no `NumberSequenceTable`. That refusal is **correct**: `LedgerJournalReversalService` allocates the reversing journal's voucher from the journal name's sequence, and a reversing journal needs its own number. The fixture now configures one, which leaves the original posting untouched because `AllocateForPostingAsync` returns early when every line already carries a voucher. The suite is 22/22 for this file, and the reversal assertions are substantive - the mirrored entry nets to zero against the original, carries a distinct voucher, and links back through `OriginalJournalNum`. None of that had ever actually run. |
| 2026-11-03 | Source scan. **No behavior changed.** **Closed the journal line account types gap**, which this file still carried as High: `LedgerJournalTransDtoValidator` now accepts Cust, Vend and Bank alongside Ledger, and `LedgerJournalPosting.WriteSubledgerAsync` (lines 260-317) writes `CustTrans` + `CustTransOpen`, `VendTrans` + `VendorOpenBalance.Add`, and `BankAccountTrans` with its source table and record references. Replaced the row with the narrower offset-account restriction that remains, at Medium. Corrected the restriction table, which quoted a message the code no longer raises. Raised the journal tax row's justification: it is now the thing stopping those new Cust/Vend lines from being used for their usual purpose. Added the three standing `LedgerJournalPostingTests` failures as a Critical row — they were recorded in `docs/basic-erp-continuation-review-2026-11-02.md` but never reached this file, while the status table above claimed reversal as Implemented. |
| 2026-11-01 | File created from a source scan. Recorded journal tax and journal account types as the two highest-priority gaps, and the inventory ledger-period-access gap. |

## Sources

- [General ledger home page](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/general-ledger)
- [Financial dimensions](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/financial-dimensions)
- [Fiscal calendars, years and periods](https://learn.microsoft.com/en-us/dynamics365/finance/budgeting/fiscal-calendars-fiscal-years-periods)
- [Ledger settlements](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/ledger-settlements)
- [Year-end close](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/year-end-close)
- [Sales order posting](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/sales-order-posting)
- [Subledger transfer to the general ledger](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/subledger-transfer)
- [Close the general ledger at period end](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/close-general-ledger-at-period-end)
