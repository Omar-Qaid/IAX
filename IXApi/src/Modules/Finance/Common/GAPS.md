# Common — gap analysis for shared finance infrastructure

**Last updated:** 2026-11-01
**Scope:** the cross-module posting and enumeration infrastructure that every finance module builds
on. This is not a D365 functional module, so its "gaps" are not parity gaps against a D365 feature
— they are correctness and consistency gaps in shared machinery. Rows that *are* parity gaps say so
and name the D365 behavior.

## How this file is maintained

Update this file **in the same change** that alters behavior. Because everything here is shared, a
change in this module can alter the behavior of General ledger, AP, AR and Inventory at once: when
that happens, note the affected modules in the *Changelog* line and update their `GAPS.md` too.

## Status summary

| Area | Status |
| --- | --- |
| `SubledgerPostingSupport`: ledger context, fiscal period and module access, posting profiles, tax accounts, voucher lines, posting lock | Implemented |
| Posting profile resolution: two-axis (item and customer/vendor, each Table → Group → All) | Implemented |
| Document currency translation (`SubledgerCurrencyTranslation`), with exact voucher balancing | Implemented; wired into all five AR and AP invoice/credit flows |
| `GeneralJournalEntry` / `GeneralJournalAccountEntry` / `SubledgerVoucherGeneralJournalEntry` writing | Implemented |
| Finance enumerations (`Enumeration.cs`) mirroring D365 base enums | Implemented |
| Physical posting policy gate (`PostsPhysicalAsync`) | Implemented |
| Application-lock concurrency control for posting keys | Implemented, **unverified under SQL Server** |

## Open gaps

| Area | Existing behavior | Expected behavior | Missing | Affected components | Priority |
| --- | --- | --- | --- | --- | --- |
| Posting lock verification | One application-lock namespace (`FinancePosting:`) guards posting keys so unrelated keys cannot alias. | Concurrent posting of the same key must serialize; distinct keys must not block each other. | Verification under SQL Server. SQLite in-memory tests cannot exercise application locks meaningfully, so the concurrency guarantee is **asserted, not proven**. | `SubledgerPostingSupport`, every posting flow | **Critical** |
| Enumeration drift | Every enum reachable from a finance entity property is pinned against a recorded baseline of 993 members across 224 enums, so a changed value fails with a named diff. | Enum members and their integer values must match D365, because they are persisted. | 183 of the 991 members in `Enumeration.cs` still take their value implicitly from their position, so a reorder *changes* values rather than being rejected outright. The baseline now catches that, but writing the values explicitly would make it impossible; `InventAccountType` is the largest such enum and is persisted. | `Enumeration.cs`, all finance entities, `Tests/PersistedEnumValueTests.cs` | ~~High~~ **Medium** — the guard exists; making the values explicit is the remaining hardening |
| Error surface consistency | Posting failures raise `SubledgerPostingException` or `PostingValidationException`; callers map them to 422. | — | The two exception types overlap and the choice between them is not documented, so similar failures surface differently across modules. | `SubledgerPostingSupport`, `PostingValidationException`, all posting services | Low |

## Flagged uncertainties

- **Axis precedence in inventory posting resolution.** Resolution now ranks the **item** axis above
  the customer/vendor axis, so an item-specific rule beats a customer-specific one. This matches how
  the setup page is organised, but the precedence of a cross-specific pair (item Table + custVend All
  versus item All + custVend Table) was **not verified against a live D365 environment**. If D365
  ranks them the other way, only that one combination resolves differently;
  `The_item_axis_is_more_significant_than_the_customer_axis` pins the current choice and is the test
  to change.
- **`LedgerPostingModule.Tax` and `Bank` usage.** Both members exist and are honored by period
  access, but which flows pass them has not been traced end to end.
- **Reporting currency semantics.** Every flow requires reporting currency to equal accounting
  currency. Whether D365 treats a blank reporting currency as "same as accounting" the way this code
  does is not verified.

## Corrections of record

- An earlier revision of this file recorded that shared posting "offers no conversion helper". That
  was **wrong**: `Foundation/Currency/LedgerCurrencyConverter` and `EffectiveExchangeRateLookup`
  already existed and the general journal already used them. The real gap was that AP, AR and
  Inventory did not, and each guarded instead. The new `SubledgerCurrencyTranslation` is the
  document-level wrapper those flows needed, not a first conversion helper.

## Changelog

| Date | Change |
| --- | --- |
| 2026-11-02 | Persisted enum values are now guarded. `Tests/PersistedEnumValueTests.cs` reflects over every class deriving from `Entity<long>` in the finance assembly, collects each enum reachable from a property (nullable ones included), and compares the whole surface against the embedded baseline `Tests/PersistedEnumValues.txt` — 993 members across 224 enums. A changed value fails with the exact lines that moved and the path of a regenerated baseline to replace it with once the change is confirmed deliberate. A second test pins the rule that produced the baseline, so a new entity property of a new enum type cannot widen the persisted surface unnoticed. Verified by perturbing a value and confirming the named diff. |
| 2026-11-02 | Translation wired into the vendor invoice and vendor credit note, completing all five invoice and credit-note flows. Payments are not translated yet, and exchange gain/loss at settlement is still not started. |
| 2026-11-01 | Translation wired into the free text invoice and the customer credit note. The AP vendor invoice is still pending and is **not** mechanical: its inventory cost arrives from the document, so `CostAmountPosted` and `InventSum.PostedValue` must be translated before they are written, or a foreign-currency purchase would store foreign-currency amounts as inventory value. |
| 2026-11-01 | **Fixed a rounding defect** in the translation shipped earlier the same day: the document's balancing line (the customer or vendor summary entry) was an independent translation of the document total, which can disagree with the sum of the individually translated lines by a rounding unit and leave the voucher out of balance. `SubledgerCurrencyTranslation` now accumulates what it translates and exposes `BalancingEntry`, the exact negation of the lines posted. The subledger's accounting amount is taken from that same figure, so ledger and subledger cannot drift. Covered by `A_foreign_currency_invoice_balances_exactly_when_the_rate_rounds`, which fails without the fix. |
| 2026-11-01 | Added `SubledgerCurrencyTranslation`: resolves a document's accounting and reporting rates once and translates every amount on it, so a flow cannot translate one line and miss another. A document already in the accounting currency short-circuits to pass-through, so same-currency posting is byte-for-byte unchanged. Wired into the AR sales invoice and its charge posting. |
| 2026-11-01 | File created. Recorded the `CustVendCode` posting-profile scoping gap and the absence of shared exchange-rate conversion as the root of the duplicated foreign-currency restrictions across AP, AR and Inventory. Recorded that the posting lock is unproven under SQL Server. |
| 2026-11-01 | **Fixed** the posting-profile scoping defect: `ResolveInventPostingAsync` filtered `CustVendCode == All`, so a rule configured for one vendor/customer or their group could never match and the posting silently used the All rule's account. Resolution is now two-axis, and the AP/AR flows pass the vendor or customer scope. Affects AP, AR and Inventory. Covered by `Tests/InventPostingResolutionTests.cs`. |
| 2026-11-01 | `LoadLedgerContextAsync` gained `module` and `userId` parameters for per-module fiscal period access control, affecting General ledger, AP, AR and Inventory callers. |
| 2026-11-01 | Added the verified Inventory module period fields and switched inventory journal and close posting to independent Inventory access. The migration is generated but awaits SQL Server application. |

## Sources

- [Ledger calendar and period module access](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/close-general-ledger-at-period-end)
- [Inventory posting profiles](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/inventory-posting)
- [Subledger journal entries](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/accounting-distributions-subledger-journal-entries-free-text-invoices)
