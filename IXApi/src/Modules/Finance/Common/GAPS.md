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
| Posting profile resolution (Table → Group → All) for inventory, customer, vendor and tax accounts | Implemented |
| `GeneralJournalEntry` / `GeneralJournalAccountEntry` / `SubledgerVoucherGeneralJournalEntry` writing | Implemented |
| Finance enumerations (`Enumeration.cs`) mirroring D365 base enums | Implemented |
| Physical posting policy gate (`PostsPhysicalAsync`) | Implemented |
| Application-lock concurrency control for posting keys | Implemented, **unverified under SQL Server** |

## Open gaps

| Area | Existing behavior | Expected behavior | Missing | Affected components | Priority |
| --- | --- | --- | --- | --- | --- |
| Ledger period access modules | `LedgerPostingModule` is `{ Ledger, Tax, Bank, Customer, Vendor }`, matching the columns on `LedgerPeriodModuleAccessControl`. Inventory postings pass `Ledger`. | **Parity gap.** D365's ledger calendar controls the Inventory module independently of Ledger. | An `Inventory` member plus `InventAccessLevel`/`InventUserGroupInfo` columns. Deliberately not invented — it is a schema change. | `SubledgerPostingSupport`, `LedgerPeriodModuleAccessControl`, `InventCloseService`, `InventJournalPostingService` | Medium |
| Posting lock verification | One application-lock namespace (`FinancePosting:`) guards posting keys so unrelated keys cannot alias. | Concurrent posting of the same key must serialize; distinct keys must not block each other. | Verification under SQL Server. SQLite in-memory tests cannot exercise application locks meaningfully, so the concurrency guarantee is **asserted, not proven**. | `SubledgerPostingSupport`, every posting flow | **Critical** |
| Exchange-rate handling in shared posting | `LoadLedgerContextAsync` returns the ledger with its accounting and reporting currency; every caller then *rejects* any mismatch. | Shared posting should convert at the document date and expose the rate to callers, so each module does not reinvent it. | A shared conversion helper. Its absence is why the same foreign-currency restriction is duplicated across AP, AR and Inventory with five near-identical error messages. | `SubledgerPostingSupport`, AP, AR, Inventory | **High** — one shared gap causing a module-wide restriction |
| Enumeration drift | `Enumeration.cs` mirrors D365 base enums and is large and shared. | Enum members and their integer values must match D365, because they are persisted. | No test asserts that persisted enum values are stable. A reordering would silently remap existing data. | `Enumeration.cs`, all finance entities | **High** — cheap to guard, expensive to get wrong |
| Posting profile coverage | Resolution follows Table → Group → All for inventory, customer, vendor and tax accounts. | D365 also resolves by `CustVendCode`/`CustVendRelation` (customer or vendor table/group/all) on the same profile row. | `ResolveInventPostingAsync` filters `CustVendCode == All` only, so a profile row scoped to a specific customer or vendor group is ignored rather than applied. | `SubledgerPostingSupport.ResolveInventPostingAsync` | **High** — a configured profile row would be silently skipped |
| Error surface consistency | Posting failures raise `SubledgerPostingException` or `PostingValidationException`; callers map them to 422. | — | The two exception types overlap and the choice between them is not documented, so similar failures surface differently across modules. | `SubledgerPostingSupport`, `PostingValidationException`, all posting services | Low |

## Flagged uncertainties

- **Whether `CustVendCode` scoping is used in practice.** The filter restricts inventory posting
  resolution to `All`. Whether any seeded or expected configuration uses a narrower scope is not
  established — verify against the posting profile setup before changing resolution, because
  widening it changes which account a posting lands in.
- **`LedgerPostingModule.Tax` and `Bank` usage.** Both members exist and are honored by period
  access, but which flows pass them has not been traced end to end.
- **Reporting currency semantics.** Every flow requires reporting currency to equal accounting
  currency. Whether D365 treats a blank reporting currency as "same as accounting" the way this code
  does is not verified.

## Changelog

| Date | Change |
| --- | --- |
| 2026-11-01 | File created. Recorded the `CustVendCode` posting-profile scoping gap and the absence of shared exchange-rate conversion as the root of the duplicated foreign-currency restrictions across AP, AR and Inventory. Recorded that the posting lock is unproven under SQL Server. |
| 2026-11-01 | `LoadLedgerContextAsync` gained `module` and `userId` parameters for per-module fiscal period access control, affecting General ledger, AP, AR and Inventory callers. |

## Sources

- [Ledger calendar and period module access](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/close-general-ledger-at-period-end)
- [Inventory posting profiles](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/inventory-posting)
- [Subledger journal entries](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/accounting-distributions-subledger-journal-entries-free-text-invoices)
