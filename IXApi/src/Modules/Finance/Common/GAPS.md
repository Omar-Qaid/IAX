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
| Document currency translation (`SubledgerCurrencyTranslation`) | Implemented; wired into the sales invoice |
| `GeneralJournalEntry` / `GeneralJournalAccountEntry` / `SubledgerVoucherGeneralJournalEntry` writing | Implemented |
| Finance enumerations (`Enumeration.cs`) mirroring D365 base enums | Implemented |
| Physical posting policy gate (`PostsPhysicalAsync`) | Implemented |
| Application-lock concurrency control for posting keys | Implemented, **unverified under SQL Server** |

## Open gaps

| Area | Existing behavior | Expected behavior | Missing | Affected components | Priority |
| --- | --- | --- | --- | --- | --- |
| Ledger period access modules | `LedgerPostingModule` is `{ Ledger, Tax, Bank, Customer, Vendor }`, matching the columns on `LedgerPeriodModuleAccessControl`. Inventory postings pass `Ledger`. | **Parity gap.** D365's ledger calendar controls the Inventory module independently of Ledger. | An `Inventory` member plus `InventAccessLevel`/`InventUserGroupInfo` columns. Deliberately not invented — it is a schema change. | `SubledgerPostingSupport`, `LedgerPeriodModuleAccessControl`, `InventCloseService`, `InventJournalPostingService` | Medium |
| Posting lock verification | One application-lock namespace (`FinancePosting:`) guards posting keys so unrelated keys cannot alias. | Concurrent posting of the same key must serialize; distinct keys must not block each other. | Verification under SQL Server. SQLite in-memory tests cannot exercise application locks meaningfully, so the concurrency guarantee is **asserted, not proven**. | `SubledgerPostingSupport`, every posting flow | **Critical** |
| Enumeration drift | `Enumeration.cs` mirrors D365 base enums and is large and shared. | Enum members and their integer values must match D365, because they are persisted. | No test asserts that persisted enum values are stable. A reordering would silently remap existing data. | `Enumeration.cs`, all finance entities | **High** — cheap to guard, expensive to get wrong |
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
| 2026-11-01 | Added `SubledgerCurrencyTranslation`: resolves a document's accounting and reporting rates once and translates every amount on it, so a flow cannot translate one line and miss another. A document already in the accounting currency short-circuits to pass-through, so same-currency posting is byte-for-byte unchanged. Wired into the AR sales invoice and its charge posting. |
| 2026-11-01 | File created. Recorded the `CustVendCode` posting-profile scoping gap and the absence of shared exchange-rate conversion as the root of the duplicated foreign-currency restrictions across AP, AR and Inventory. Recorded that the posting lock is unproven under SQL Server. |
| 2026-11-01 | **Fixed** the posting-profile scoping defect: `ResolveInventPostingAsync` filtered `CustVendCode == All`, so a rule configured for one vendor/customer or their group could never match and the posting silently used the All rule's account. Resolution is now two-axis, and the AP/AR flows pass the vendor or customer scope. Affects AP, AR and Inventory. Covered by `Tests/InventPostingResolutionTests.cs`. |
| 2026-11-01 | `LoadLedgerContextAsync` gained `module` and `userId` parameters for per-module fiscal period access control, affecting General ledger, AP, AR and Inventory callers. |

## Sources

- [Ledger calendar and period module access](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/close-general-ledger-at-period-end)
- [Inventory posting profiles](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/inventory-posting)
- [Subledger journal entries](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/accounting-distributions-subledger-journal-entries-free-text-invoices)
