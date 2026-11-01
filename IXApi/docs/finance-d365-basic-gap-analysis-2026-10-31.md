# Finance modules — D365 F&O basic parity gap analysis

Date: 2026-10-31. Branch `main` at `36c018bc`. **Revised 2026-11-01 at `f4439405`** — see
§6, which re-verifies every open entry against the code and supersedes this header's commit.

Scope: `IXApi/src/Modules/Finance` (Foundation, GeneralLedger, AccountsReceivable, AccountsPayable,
Inventory, Common/Posting). **Basic D365 F&O only.** Advanced and optional features are excluded by
design and are listed in "Deliberately out of scope" at the end rather than reported as gaps.

Baselines used: `src/Modules/Finance/D365_FO_Core_General_Ledger_Full_Cycle.md`,
`AccountsPayable/APTables.md`, `AccountsPayable/CyclePurchorder.md`,
`AccountsReceivable/ARTables.md`, `AccountsReceivable/CycleSalesorder.md`,
`Inventory/CycleInvnetory.md`, plus the `d365-parity` skill's basic-scope table. Where a claim
depends on target-version D365 metadata that is not in the repository, it is marked
**UNCERTAIN** instead of asserted. The predecessor review
`docs/d365-alignment-review-2026-10-28.md` is superseded in part: transfer orders, ledger
settlement, trial balance and journal posting/reversal have since been implemented.

---

## 1. What exists and works

Established by reading the code, not inferred from file names.

| Area | Verified behavior |
|---|---|
| Posting engine | `Common/Posting/SubledgerPostingSupport.cs` is the single posting path: ledger + fiscal period resolution, posting profiles `Table → Group → All` for customer/vendor/inventory, item model group policy, tax code resolution and allocation, dimension/main-account validation, `GeneralJournalEntry`/`GeneralJournalAccountEntry` construction, and an `sp_getapplock` per-key lock. |
| GL journal posting | `LedgerJournalPosting` posts `LedgerJournalTable`/`Trans` → posted entries + `SubledgerVoucherGeneralJournalEntry`, groups by voucher + accounting date, balances on **posted signed amounts**, and refuses out-of-scope lines (non-`Ledger` account type, intercompany, FX, journal tax, auto-reversal) rather than approximating them. |
| GL reversal | `LedgerJournalReversalService` posts the reversal as its own journal with `OriginalJournalNum`, new vouchers, its own open-period date, once-only enforcement, and `IsCorrection` when `LedgerParameters.TransactionReversalCorrection` is set. |
| Posted-data protection | Posted-entry controllers and generic services are read-only (`PostedLedgerReadService<T>`, `NonAction` conventions). |
| Period control at posting | `LoadLedgerContextAsync` requires a unique **Operating** period and `LedgerFiscalCalendarPeriod.Status == Open`; On hold and Closed both block posting. |
| Ledger settlement | `LedgerSettlementService` records `LedgerTransSettlement` matches with open-item query and reversal; refuses out-of-balance, cross-currency and cross-account selections. |
| Trial balance | `LedgerTrialBalanceService` + controller. |
| Manual-entry block | `MainAccountLegalEntity.DoNotAllowManualEntry` is enforced per legal entity. |
| AR cycle | Quotation → order → confirmation → packing slip (with physical posting) → invoice → credit note/return; `CustTrans`/`CustTransOpen`, `CustInvoiceJour`/`Trans`, `TaxTrans`; customer payment with `CustSettlement`, oldest-due-first or marked-invoice settlement, remainder left open, `BankAccountTrans` movement, idempotency by request ID. |
| AP cycle | PO → confirmation → product receipt (with receipt accrual) → invoice (3-way quantity match via `VendInvoicePackingSlipQuantityMatch`) → return/credit note; `VendTrans`/`VendTransOpen`, `VendSettlement`, vendor payment. |
| Tax | `TaxTable`/`TaxData`/`TaxGroup*`/`TaxOnItem`/`TaxLedgerAccountGroup` setup; rate-by-date resolution; incl./excl. tax; multi-code allocation that sums to the rounded total; `TaxTrans` on sales invoice, purchase invoice and both return paths. |
| Inventory | `InventTrans`/`InventTransOrigin`/`InventSum`, dimensions, on-hand, reservation and picking, all four basic journal types (Movement, Adjustment, Transfer, Counting) with counting freeze/confirm protection, transfer orders on standard `InventTransferTable`/`InventTransferLine`, FIFO close with `InventSettlement` and cost adjustment, FIFO preview, physical as-of and reconciliation. |
| Foundation | Legal entity, currencies and exchange rates, fiscal calendar, payment terms and schedules, global address book, UOM with conversions, number sequences. |

---

## 2. Gap register

Priority: **P1** blocks a documented basic cycle or lets wrong accounting through ·
**P2** basic behavior missing, workaround exists · **P3** basic completeness.

### P1 — Account structures are not modelled or validated — CORRECTED AND ENLARGED 2026-10-31; PHASE A DONE

| | |
|---|---|
| **This entry was wrong about the scale.** | It said `DimensionHierarchy` "exists as a header only". True of the C# class — but investigation found **it had no database table**, and nor did nine other dimension entities. Only `DimensionAttributeValueCombination` was in the EF model, and only because other entities' foreign keys reach it. There was **no `IEntityTypeConfiguration`, no `DbSet`, no migration and no table** for `DimensionAttribute`, `DimensionAttributeValue`, `DimensionHierarchy`, `DimensionAttributeValueGroup`, `DimensionAttributeValueGroupCombination`, `DimensionAttributeLevelValue`, `DimensionAttributeSet`, `DimensionAttributeSetItem`, `DimensionAttributeValueSet` or `DimensionAttributeValueSetItem`. Every dimension feature folder held only `.gitkeep` outside `Entities`. `Chunks/DimensionSeeder.SeedAsync` was empty and unregistered, so no dimension or dimension value existed. Every seeded combination was main-account-only: display value = the account code, `AccountStructure = 0`, no segment values. <br>**So financial dimensions did not exist in this database, and an account structure would have had nothing to validate.** |
| **Expected (D365)** | GL cycle doc §4 Step 4: account structures define which main-account ranges allow or require which dimensions; both are activated and **assigned to the ledger**. Acceptance: `611000 ADMIN` is valid, a **blank Department on 611000 must fail**, a blank Department on `130100` is valid. |
| **Phase A done** | Six dimension entities now have configurations, indexes and a table (`20261031123628_AddFinancialDimensionTables`); `LedgerDimensionResolver` creates a combination with its `DimensionAttributeValueGroup` → `DimensionAttributeLevelValue` chain and the group-combination link, populating the denormalised per-dimension columns too, and reads the segments back for validation; `FinancialDimensionSeeder` seeds the Department dimension with ADMIN / SALES / SERVICE. Also fixed: `DimensionAttributeValue` shadowed the base `bool IsDeleted` with `new NoYes IsDeleted`, which would have silently disabled soft-delete filtering once the type was mapped (the global filter only applies when the property is a `bool`). |
| **Phase B done** | The four constraint tables are in, with the field lists verified from CDM, plus `LedgerStructure` (`Ledger` + `DimensionHierarchy`, unique together — confirmed from the AX 2012 table reference, whose relation set matches current D365's "Ledger page → Account structures" behaviour). `AccountStructureBuilder` defines a structure as a draft, writing its `DimensionHierarchyLevel` segments and its `DimensionConstraintTree` / `DimensionConstraintNode` / `DimensionConstraintNodeCriteria` tree; activates it, refusing when its main account values overlap an already-active structure; and assigns it to the ledger. `DimensionCriteriaMatcher` implements the documented criteria rules. `AccountStructureSeeder` defines and activates the cycle doc's BS and PL structures. 33 tests across `DimensionCriteriaMatcherTests` and `AccountStructureBuilderTests`. |
| **Decision taken in phase B** | The seeded BS and PL structures are **activated but not assigned to the ledger**. Validation (phase C) only consults assigned structures, and every ledger account combination in this database is main-account-only with no Department — so assigning PL, whose Department is mandatory, would refuse every existing demo revenue and expense combination. Assignment is left as a deliberate setup act, after departments are filled in where they are needed. **This is a decision to revisit**: either backfill departments onto the demo combinations and then assign, or relax PL's Department to `*;""` and accept that the cycle doc's acceptance example is only exercised by its own test fixture. |
| **Phase C done — gap 1 closed** | `AccountStructureValidator` is wired into `SubledgerPostingSupport.LoadDimensionAsync`, the single chokepoint `ResolveCustomerSummaryAsync`, `ResolveVendorSummaryAsync`, `ResolveInventPostingAsync`, `ResolveTaxPayableAsync` and the journal path all already call — so GL, AR, AP, tax and inventory posting are all covered by one edit. The main account value selects the structure; each remaining segment must satisfy that structure's criteria for its level; a blank segment is allowed only where the segment is optional. Segment values come from the value group chain, falling back to the combination's own per-dimension column so a combination written without a chain is validated rather than read as all blanks. The structures are loaded once per posting operation, not per line. The cycle doc's acceptance example is a test: `611000 ADMIN` posts, a blank Department on `611000` is refused, a blank Department on `130100` posts. |
| **Fail-open, as decided** | A ledger with no assigned structure is **not validated** and posts exactly as before. `LedgerJournalSetupCheck` now reports either "No account structure is assigned to the ledger, so dimension combinations are not validated" or the names of the structures that are validating, replacing the old blanket "Account structures … not verified" line. This is a deliberate deviation from D365, which requires a structure on the ledger. |
| **Fixture cost worth knowing** | Because `LoadDimensionAsync` now reads `LedgerStructure` and `DimensionHierarchy`, every hand-built test context that posts had to model them — six fixtures. A new posting test needs those two types in its context even when it never defines a structure. |
| **Scope limit to note** | The builder writes a **single-path** structure: one set of allowed values per segment, which is exactly what the cycle doc's BS and PL structures are. A structure whose grid holds several rows, where a later segment's allowed values depend on the path taken through the earlier ones, needs a constraint node per branch. The tables model that; the builder does not construct it yet. |
| **Affected** | `Foundation/Dimensions/*`, `GeneralLedger/Ledger`, `Common/Posting/SubledgerPostingSupport.cs`, `LedgerJournalSetupCheck`, every posting service via `LoadDimensionAsync`. |
| **Note** | Matching must compare criteria **as strings, never numerically** — `1000000` falls inside the string range `100000..399999`. `*` allows all non-blank values; blank needs `""` separately. The plan at `.claude/plans/bubbly-jumping-cookie.md` carries the full rule set and sources. |

### P2 — `MarkupTrans.IsDeleted` silently disables soft-delete filtering (found 2026-10-31)

| | |
|---|---|
| **Existing** | `Foundation/Markup/Entities/MarkupTrans.cs:131` declares `public new NoYes IsDeleted`, shadowing the base entity's `bool IsDeleted`. The global soft-delete query filter in `ApplicationDbContext.SetGlobalQueryFilters` only applies when the `IsDeleted` property is a `bool`, so **`MarkupTrans` has no soft-delete filter today**. The model snapshot confirms the column is mapped as `int`. |
| **Expected** | Soft-deleted charge transactions should be filtered out of every query, as they are for every other entity. |
| **Missing** | The fix is not a one-liner: removing the shadow changes the column from `int` to `bit`, so it needs a migration with data conversion. Reported rather than changed in passing. The same defect on `DimensionAttributeValue` was fixed while that type was still unmapped, which cost nothing. |
| **Affected** | `Foundation/Markup/Entities/MarkupTrans.cs`, plus a migration. |

### P1 — Main account posting controls are stored but never enforced — PARTLY DONE 2026-10-31

| | |
|---|---|
| **Existing** | `MainAccount` carries `DebitCreditCheck`, `DebitCreditProposal`, `DebitCreditBalanceDemand`, `CurrencyCode`, `ValidateCurrency`, `ValidatePosting`, `ValidateUser`, `MandatoryPaymentReference`, `UnitOfMeasure`, `PostingType`. Before this change no posting path read any of them, and `GeneralJournalAccountEntry.PaymentReference` was written as `string.Empty` by `SubledgerPostingSupport.AccountEntry` even though `LedgerJournalTrans.PaymReference` carried a value. |
| **Expected (D365)** | Posting validates the posted side against the account's debit/credit requirement, restricts the account to its specified currency when `ValidateCurrency` requires it, restricts allowed posting types via `ValidatePosting`, and refuses a line with no payment reference when `MandatoryPaymentReference` is set. |
| **Enum semantics — now verified, and two repo enums were wrong** | Resolved from the AX 2012 `MainAccount` table reference, whose field-to-enum mapping is carried forward by the CDM entity (display names match: "DB/CR requirement", "DB/CR proposal", "Validate currency code"). <br>• **`DebitCreditCheck` and `DebitCreditProposal` share one enum**, `DebCredProposal` = None 0 / Debit 1 / Credit 2. `DebitCreditCheck` is "whether the transaction **must** be a debit or a credit"; `DebitCreditProposal` is "whether the typical transaction is **usually** a debit or a credit". So **the required side lives on `DebitCreditCheck`**, and `DebitCreditProposal` restricts nothing. The repo's `DebitCreditCheck` enum was `None`/`Check` — wrong shape, and it made the earlier implementation read the side from the wrong field. Corrected: `MainAccount.DebitCreditCheck` is now typed `DebitCreditProposal` and the bad enum is deleted. <br>• **`ValidateCurrency`, `ValidatePosting`, `ValidateUser` all use `FieldControl`** = Optional 0 / FillIn 1 / Table 2 / List 3: no check, require a value, compare against the value on the main account, compare against an allowed-value list. They were `int` with `// Map to … Enum if matching`; now typed `FieldControl`. <br>Both corrections are **schema-neutral** — the columns were already `int` and no seed data sets these fields. `dotnet ef migrations has-pending-model-changes` reports no model drift, so no migration. |
| **Done** | `MainAccountPostingControls` takes the posted entry and applies, on the **posted signed amount**: <br>• **Debit/credit requirement** from `DebitCreditCheck`. <br>• **`MandatoryPaymentReference`**. <br>• **`ValidateCurrency`** in FillIn and Table modes. <br>• **`ValidatePosting`** in FillIn and Table modes. <br>The payment reference is also **carried through** to `GeneralJournalAccountEntry.PaymentReference` instead of being dropped — a data-loss fix independent of the control. 10 tests in `Tests/MainAccountPostingControlsTests.cs` plus two end-to-end tests in `LedgerJournalPostingTests`. |
| **Still missing, with the reason** | • **List mode** for currency and posting type — no allowed-value list table exists in this module. <br>• **`ValidateUser`** — `MainAccount` has no `UserInfoId` to compare against (CDM has one; this repo's entity does not), and `LedgerJournalPosting` carries no posting-user identity. <br>• **`DebitCreditBalanceDemand`** — uses its own enum `DebCredBalanceDemand`, not checked here; needs a balance basis and refuse-versus-warn semantics. Still **UNCERTAIN**. <br>• **`UnitOfMeasure`** on the account. <br>All are reported per account by the setup-check. Separately, `GeneralJournalAccountEntry.PostingType` is always written as `LedgerJournal` from this path rather than from the source document, which limits how useful a `ValidatePosting` Table check can be. |
| **Affected** | `GeneralLedger/LedgerAccount/Services/MainAccountPostingControls.cs` (new), `Common/Enums/Enumeration.cs`, `GeneralLedger/LedgerAccount/Entities/MainAccount.cs`, `Common/Posting/SubledgerPostingSupport.cs`, `LedgerJournalPosting.cs`, `LedgerJournalSetupCheck.cs`. |
| **Revised priority** | **P3** for the remainder: each outstanding item is blocked on setup that does not exist (list tables, `UserInfoId`, posting-type derivation) rather than on undone work. |

**Sources for the enum semantics** (public Microsoft metadata — evidence, not certification of your
target version, per the standing caveat in §5):

- [MainAccount table, AX 2012 reference](https://learn.microsoft.com/en-us/previous-versions/dynamics/ax-2012/reference/gg901309(v=ax.60)) — field-to-enum mapping and the "must be" versus "usually" wording
- [DebCredProposal enumeration](https://learn.microsoft.com/en-us/previous-versions/dynamics/ax-2012/reference/gg847554(v=ax.60)) — None 0, Debit 1, Credit 2
- [FieldControl enumeration](https://learn.microsoft.com/en-us/previous-versions/dynamics/ax-2012/reference/gg864586(v=ax.60)) — Optional 0, FillIn 1, Table 2, List 3
- [MainAccount in Main, Common Data Model](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/financialdimensions/main/mainaccount) — current display names, confirming the fields carry forward

### P1 — `LedgerParameters` voucher and rounding controls are not enforced — PARTLY ADDRESSED 2026-10-31; REPRIORITIZED

| | |
|---|---|
| **Existing** | `LedgerParameters` persists `CheckContinuous`, `CheckVoucher`, `CheckJournalizing`, `AllowMultipleTransactionsWithinOneVoucher`, `MaxRoundingDifferenceMst`, `MaxRoundingDifferenceMstSecond`, `AbortErrorAccount`, `ReverseOppositeSignAmounts`, `LimitedOpenFiscalPeriods`. Before this change only two parameters were read anywhere: `TransactionReversalCorrection` and `EnableLedgerSettlement`. |
| **Expected (D365)** | GL cycle doc §4 Step 5 requires these controls to govern posting. §9.12 adds: "Use permitted rounding configuration and inspect generated rounding accounts. Never rely on silent plugs to a suspense account as a routine balancing method." |
| **Done** | `LedgerJournalParameterControls` closes the silent-skip half. A **switched-on restriction** (`CheckVoucher`, `CheckContinuous`, `CheckJournalizing`) now **refuses** the journal naming the parameter, in both `LedgerJournalPosting` and the setup-check, instead of posting looser than the company configured. All three are off in the default record, so only a company that deliberately switched one on is affected. The remaining parameters are reported as not applied by the setup-check, so the diagnostic no longer implies they are in force. |
| **Still missing, and why it is not P1** | The original P1 rating was wrong. Actually *applying* these controls is **blocked**, not merely undone: <br>• **Rounding tolerance** has nowhere to post a residual. This module has no automatic transaction account setup (`LedgerPostingAccounts` / `LedgerAutoTransType` do not exist; the only penny-difference accounts are `TaxLedgerAccountGroup`'s customer/vendor tax ones, which are not a GL voucher rounding account). Spending a tolerance without that setup means either dropping the residual or plugging it to an invented account — the silent plug §9.12 forbids. The current exact-balance requirement is **stricter than configured and never wrong**, so leaving it is correct until the account setup exists. <br>• **`AllowMultipleTransactionsWithinOneVoucher`** needs D365's rule for what counts as one transaction inside a voucher (its line/offset grouping). **UNCERTAIN** — not established from the repository or the cycle docs. It is also `No` in the default record, so enforcing a guessed reading would refuse ordinary journals. <br>• **`CheckVoucher` / `CheckContinuous` / `CheckJournalizing`** exact check semantics are **UNCERTAIN** against target metadata; hence refusal rather than a guessed implementation. <br>• **`ReverseOppositeSignAmounts`** governs reversal presentation (mirror versus negate) in `LedgerJournalReversalService`, which currently always mirrors. Not covered by this change. |
| **Affected** | `GeneralLedger/LedgerJournalTable/Services/LedgerJournalParameterControls.cs` (new), `LedgerJournalPosting.cs`, `LedgerJournalSetupCheck.cs`. 9 tests in `Tests/LedgerJournalParameterControlsTests.cs` plus one end-to-end refusal test in `LedgerJournalPostingTests`. |
| **Revised priority** | **P3, and blocked** on the automatic transaction account setup and on target metadata for the check semantics. Confirm both before scheduling; the silent-skip defect that made it feel P1 is gone. |

### ~~P1 — A permanently closed fiscal period can be reopened through generic CRUD~~ — FIXED 2026-10-31

| | |
|---|---|
| **Was** | `LedgerFiscalCalendarPeriodService` was 20 lines of generic `BaseService<LedgerFiscalCalendarPeriod>` CRUD; its controller exposed inherited update, and nothing restricted a `Status` transition. |
| **Expected (D365)** | GL cycle doc §11: On hold is the normal reversible restriction; **permanently Closed cannot be reopened**. Period status change is a controlled action, and module-level and user-group period access apply in addition to overall status. |
| **Fixed** | `FiscalPeriodStatusTransition` holds the status machine (`Open ⇄ OnHold`, either `→ Closed`, `Closed` terminal, `YearClosed` neither set nor left since year end close is out of scope). `SetStatusAsync` is the only way to change the status; generic update/delete are sealed on the service and `[NonAction]` on the controller; `POST /api/v1/LedgerFiscalCalendarPeriod/{recId}/status` is the guarded action. A create may not duplicate a ledger period or be born `Closed`/`YearClosed`, so the status posting reads stays unambiguous. 17 tests in `Tests/LedgerFiscalCalendarPeriodStatusTests.cs`. |
| **Still missing** | Module-level and user-group period access (`LedgerPeriodCloseStatus` / period access by module), which `LedgerJournalSetupCheck` still reports as unverified and `LedgerFiscalCalendarPeriodDto` still fakes with hard-coded `"Open"` helper properties (`LedgerModule`, `SalesTax`, `Bank`, `Customer`, `Vendor`). Tracked as its own gap below. |

### P1 — No pending vendor invoice (`VendInvoiceInfoTable` / `VendInvoiceInfoLine`)

| | |
|---|---|
| **Existing** | `VendorInvoiceService` posts a vendor invoice directly from the purchase order in one call. No pending-invoice entities exist anywhere in the module (`grep VendInvoiceInfo` → nothing). |
| **Expected (D365)** | The project's own `APTables.md` §8.1 documents `VendInvoiceInfoTable` as "Pending invoice header", lists it as **"Required for pending invoices"** in its table register (row 23), and names `VendInvoiceInfoTable`, `VendInvoiceInfoLine`, `VendInvoiceJour`, `VendInvoiceTrans` as the invoice chain. A vendor invoice is entered, matched and held, then posted. |
| **Missing** | `VendInvoiceInfoTable`, `VendInvoiceInfoLine`, the entry/matching surface, and posting from the pending invoice. |
| **Affected** | New `AccountsPayable/Invoice` entities + service; `VendorInvoiceService` would post *from* the pending invoice rather than from the PO. |

### P2 — Period access by module and user group is not modelled

| | |
|---|---|
| **Existing** | Period access is a single `LedgerFiscalCalendarPeriod.Status` for the whole ledger period. `LedgerFiscalCalendarPeriodDto` carries `LedgerModule`, `SalesTax`, `Bank`, `Customer`, `Vendor` properties hard-coded to `"Open"` — they are not persisted and no posting path reads them. `LedgerJournalSetupCheck` reports "module-level period access" as unverified. |
| **Expected (D365)** | GL cycle doc §11 step 9: a period can be restricted to an approved closing group, and "Review module and user group access as well as overall period status." A module can be closed for a period while the period itself stays open. |
| **Missing** | The per-module / per-user-group access records and their enforcement at posting. Today closing a period is all-or-nothing, so the documented close policy ("restrict access to the approved closing group") cannot be expressed. |
| **Affected** | `GeneralLedger/FiscalCalendar/*`, `Common/Posting/SubledgerPostingSupport.cs` (`LoadLedgerContextAsync`), `LedgerJournalSetupCheck`. |
| **Note** | The D365 table names for module/user-group period access were not verified against target metadata — **UNCERTAIN**. The five `"Open"` DTO properties should either be backed by real data or removed; leaving them invites callers to believe module access is enforced. |

### P2 — Charges (`MarkupTrans`) are never invoiced or posted

| | |
|---|---|
| **Existing** | Full charges setup exists: `MarkupTable`, `MarkupAutoTable`/`Line`, `MarkupTrans`, `MarkupChargeCalculator`, `MarkupTransService`, `SalesOrderChargeRefresh`; charges attach to sales orders. `MarkupTable` carries `CustomerLedgerDimension` / `VendorLedgerDimension` and a debit/credit routing type. **No posting service references `Markup`** — confirmed by grep across `AccountsReceivable/Invoice`, `AccountsPayable/Invoice`, `PackingSlip`, `ProductReceipt`. `CustInvoiceJour.SumMarkup` / `SumMarkupMst` exist and are never assigned; `SalesInvoiceService` sets only `SalesBalance`, `SumTax`, `InvoiceAmount`. `VendorInvoiceService`'s own summary says "Charges … are separate posting concerns". |
| **Expected (D365)** | `APTables.md` documents `SumMarkup` as "Charges total" on the invoice journal. At invoicing, order and line charges post to the accounts routed by `MarkupTable` and are included in the customer/vendor balance and the invoice totals. |
| **A field-naming trap, now documented** | The entity's own comments label `CustType`/`CustPosting`/`CustomerLedgerDimension` "Customer Side Mapping" and the `Vend*` trio "Vendor Side Mapping". **That is wrong and would lead straight to a wrong implementation.** CDM gives their display names as **"Debit type" / "Debit posting" / "Debit account"** and **"Credit type" / "Credit posting" / "Credit account"**: one charge code has a debit side and a credit side, and the `Cust`/`Vend` prefixes are historical. The type values are Ledger account `0`, Customer-or-vendor `1`, Item `2` — corroborated three ways: the entity comment, the seeder's own per-row comments, and the seeded configurations matching Microsoft's documented setups (sales charges `CustType=1`/`VendType=0` = "debit Customer, credit Ledger account"; purchase charges `CustType=0`/`VendType=1`). Sales does not allow the Item type; AP does, for landed cost. |
| **Sales side done 2026-10-31** | `SalesChargePosting` posts the order's and lines' charges at invoice: debit Customer (billed, folded into the single summary customer entry) or debit Ledger account (absorbed, posts its own balanced pair and never touches the customer balance); credit always a ledger account. Charge amount follows `MarkupCategory` (Fixed, Pcs, Percent) recomputed from the same basis `SalesOrderChargeRefresh` uses, so it does not depend on that refresh having run. Charge tax resolves through the same path the lines use and joins the voucher's tax. `SumMarkup`/`SumMarkupMst` are populated and billed charges are in `InvoiceAmount` and the customer balance. Each charge posts **once**, recorded in `MarkupTrans.Posted`, so a later partial invoice does not bill it again. 13 tests in `Tests/SalesChargePostingTests.cs`. |
| **Refused rather than approximated** | Item type on either side (sales forbids it; landed cost is not basic scope), a credit side set to customer-or-vendor (not a sales charge pattern), an unrecognised type value, and a ledger side with no account configured. |
| **Payable side done 2026-10-31** | `PurchaseChargePosting` mirrors it for the vendor invoice, reversed as the documented purchase pattern requires: debit a ledger account for the charge's cost, credit the vendor so the charge increases what is owed. A both-sides-ledger charge is absorbed and posts its own balanced pair. Input tax on a vendor-billed charge posts to the incoming tax account and writes a `TaxTrans` keyed by the invoice voucher — with no source line, as the receivable side's invoice tax already does, because a charge is not an invoice line and no table number identifies one. Refused: **Item type on either side (landed cost, which D365 *does* allow for purchases — so this refusal matters more here than on the sales side)**, a debit side set to customer-or-vendor, unrecognised types, and a ledger side with no account. 13 tests in `Tests/PurchaseChargePostingTests.cs`. |
| **`VendInvoiceJour.SumMarkup`** | Added with `SumMarkupMst` and a migration, after the build was made trustworthy. `APTables.md` documents them as the charges total; the entity had neither, so a payable charge sat in `InvoiceAmount` with no subtotal of its own. |
| **A latent bug fixed on the way** | `MarkupChargeCalculator` branched on `TransTableId` before module, and a purchase **header** charge carries `TransTableId = Unspecified`, which the sales branch also accepts. So a Percent or Pcs purchase charge was measured against *a sales order with the same record id* — matching the wrong order, or throwing on `SingleAsync`. It now branches on `ModuleType` first and measures against the purchase order's own lines. Nothing posted purchase charges before, so this never reached the ledger; it would have the moment the payable path started using it. |
| **Still outstanding** | • **Line-level purchase charges.** `MarkupTransService.ResolveTransTableId` maps `(Vendor, "line")` to null and `FinanceTableNumbers` has no `PurchTable`/`PurchLine` entry, so a purchase charge can only attach to the header. Adding line-level attach is its own change. <br>• **Proration across partial invoices** (`CalculatedProratedAmount`, `IsAdvancedLineProrated`), a separate D365 feature. A charge posts once in full on both sides. |
| **Affected** | `AccountsReceivable/Invoice/Services/SalesChargePosting.cs`, `AccountsPayable/Invoice/Services/PurchaseChargePosting.cs` (both new), `SalesInvoiceService.cs`, `VendorInvoiceService.cs`, `Foundation/Markup/Services/MarkupChargeCalculator.cs`. No schema change. |
| **Fixture cost** | Both invoice paths now read `MarkupTrans` and `MarkupTable`, so a test context that posts either invoice must model both. |

### Build-environment note — the intermittent compiler crash, and its cause (found and fixed 2026-10-31)

`csc.exe` was failing intermittently on `IAX.IXApi.Infrastructure` with `AccessViolationException`,
`Internal CLR error (0x80131506)` and exit code `-1073741819`, in varying stack traces
(`BoundTreeWalker`, `CSharpDiagnosticFilter`, `CompileMethods`). It blocked adding a migration, since
a schema change is not worth shipping when its build cannot be confirmed.

**Cause: an SDK mismatch, not the size of the migration history.** Every project targets `net9.0`
and references EF Core `9.0.9`, but the repository had **no `global.json`**, so `dotnet` floated to
the newest installed SDK — **10.0.103** — and compiled `net9.0` code with .NET 10's Roslyn. Measured,
with forced recompiles of the project that was failing:

| SDK | forced recompiles | crashes |
|---|---|---|
| 10.0.103 (unpinned) | 3 | 1 |
| 9.0.311 (pinned via `global.json`) | 9 | 0 |

At the observed ~33% failure rate, nine clean runs by chance is about 2.5%, and the nine include five
taken **after** adding another migration, so the pin holds against a larger compile unit than the one
that was failing. `dotnet ef migrations add` then succeeded first try, where it had crashed
repeatedly.

The size of the migration history — `Migrations` is **39 MB of the project's 40.3 MB**, 37
`.Designer.cs` files at ~1.1–1.2 MB each — is real but was **not** the cause. An initial read blamed
it, and that was wrong: the crash recurred after the suspect migration was removed, and disappeared
on pinning the SDK while the migration was back in place. It remains worth reducing eventually, and
the viable route is to move each migration's `[DbContext]`/`[Migration]` attributes into its main
file and exclude the `.Designer.cs` files from compilation — `BuildTargetModel` is `protected
override`, so the base is virtual and the bodies are droppable. That costs `TargetModel` on old
migrations, which `dotnet ef migrations remove` uses, so the newest Designer should be kept whole.
Not attempted; squashing the history is the other option and is riskier, because existing databases
carry `__EFMigrationsHistory` rows for all 78 migrations.

**Consequence of the pin:** a machine with only the .NET 10 SDK now fails the build with a clear
"global.json not satisfied" error instead of silently using it. For a `net9.0` repository that is the
intended trade; it means the 9.0 SDK is a prerequisite.

### P2 — No cash discount

| | |
|---|---|
| **Existing** | `SalesTable` carries `CashDisc`, `CashDiscPercent`, `CashDiscBaseDate`, written by `SalesOrderHeaderService` and `SalesOrderConfirmationService`. There is **no `CashDisc` master table** (`find -iname "*CashDisc*"` → nothing) and no discount logic in any posting or settlement path. |
| **Expected (D365)** | `CashDisc` defines the discount code, period and percentage. The discount is proposed on the invoice, and at payment settlement the taken discount reduces the settled amount and posts to the cash-discount account. |
| **Missing** | `CashDisc` table, due-date/discount-date calculation, and discount handling in `CustPaymentService` / `VendPaymentService` settlement. |
| **Affected** | New `Foundation` or AR/AP `CashDisc` feature; `CustPaymentService`, `VendPaymentService`, `PaymentDueDateCalculator`, `SalesInvoiceService`, `VendorInvoiceService`. |

### P2 — No settlement reversal or payment reversal in AR/AP

| | |
|---|---|
| **Existing** | `CustPaymentService` and `VendPaymentService` expose posting only. `CustSettlement` / `VendSettlement` rows are written and never unwound; `CustTransOpen` rows are **deleted** when fully settled (`Db.Set<CustTransOpen>().Remove(invoice.Open)`), so the open amount is not recoverable from that row. GL has `LedgerJournalReversalService` and `LedgerSettlementService.ReverseAsync`; the subledgers have no equivalent. |
| **Expected (D365)** | A posted customer/vendor payment can be reversed, and a settlement can be reversed independently of the payment, restoring the open transaction and posting the counter-voucher. |
| **Missing** | Payment reversal and settlement reversal for both subledgers. A mis-settled payment currently has no supported correction path. |
| **Affected** | `AccountsReceivable/Payment/Services/CustPaymentService.cs`, `AccountsPayable/Payment/Services/VendPaymentService.cs`, `CustTransOpen` lifecycle, `CustSettlement`/`VendSettlement`. |

### ~~P2 — No Validate / Simulate posting action~~ — FIXED 2026-10-31

| | |
|---|---|
| **Was** | Two read-only diagnostics, `setup-check` and `voucher-balance-check`, both reporting `PostingValidated = false`. No `Validate` or `Simulate` action existed. |
| **Expected (D365)** | GL cycle doc §5 step 7 ("Validate, then Simulate posting where available"), §6.6 (validate/simulate checks ledger, period access, structures, accounts and rates and creates **no** posted voucher) and acceptance test **T15**. |
| **Fixed** | `GET /LedgerJournalTable/{recId}/validate` combines the setup check and the voucher arithmetic, reads only, and says in its own `Scope` that it never authorizes posting. `POST /LedgerJournalTable/{recId}/simulate` **runs the real posting path and rolls it back**, reporting either the vouchers, totals and individual account entries it would create, or the refusal and why. 5 tests in `Tests/LedgerJournalValidateSimulateTests.cs`. |
| **Why Simulate runs the real path** | The alternative — a second implementation of the posting rules — would drift from the rules that actually post, and a validation that disagrees with posting is worse than none. Running `LedgerJournalPosting` inside a transaction and rolling back gives an answer that is correct by construction. It also means every rule added since (the parameter controls, the main account controls, the account structures) is covered without touching Validate or Simulate again. |
| **T15 is tested directly** | After simulating: no `GeneralJournalEntry`, no `GeneralJournalAccountEntry`, no `SubledgerVoucherGeneralJournalEntry`, the header still `Posted = No` with no `PostedDateTime`, and every line still editable. A separate test asserts the change tracker is **empty** afterwards and then calls `SaveChangesAsync` — a leaked in-memory mutation would otherwise post on the next save — and that the journal still posts for real after being simulated twice. |
| **Affected** | `GeneralLedger/LedgerJournalTable/` service, interface, controller and a new DTO file. No schema change. |

### P2 — No sales tax settlement ("Settle and post sales tax")

| | |
|---|---|
| **Existing** | `TaxPeriodHead` / `TaxReportPeriod` setup with `TaxPeriodService` (CRUD + interval read). `TaxTrans` is written by sales invoice, purchase invoice and both return paths. `TaxJournalTrans` exists as entity + configuration + DTO with **no service and no controller**. Nothing reads `TaxTrans` to settle a period; no `TaxTrans.TaxTransRecId`/settlement voucher link is set. |
| **Expected (D365)** | Settling a tax period nets outgoing and incoming tax per settlement period, posts the net to the tax settlement account, and stamps the settled `TaxTrans` rows so they cannot settle twice. |
| **Missing** | The settlement process and its posting; `TaxJournalTrans` is unused infrastructure. |
| **Affected** | `Foundation/Tax/*` (new settlement service), `TaxLedgerAccountGroup` (settlement account), `Common/Posting/SubledgerPostingSupport.cs`. |
| **Note** | Whether the tax-settlement voucher belongs to basic scope is a judgment call — the GL cycle doc §10 places tax under "Optional finance subledger integrations" while the AR/AP cycles post tax as a matter of course. **UNCERTAIN on priority**, not on absence. Confirm before scheduling. |

### P3 — Free text invoice entities exist with no process

| | |
|---|---|
| **Existing** | `CustInvoiceTable` + `CustInvoiceLine` entities, `CustInvoiceTableConfiguration`, `CustInvoiceTableDto`, `CustInvoiceTableDtoValidator` — and **no service, no controller, no posting**. The only non-definition references are an enum entry, DI registration and an unrelated address cleanup. |
| **Expected (D365)** | A free text invoice is a customer invoice with no sales order, posting revenue to a chosen ledger account plus tax and the customer balance. |
| **Missing** | Entry and posting. Either implement it or remove the half-built surface; leaving validated DTOs with no process invites callers to assume it works. |
| **Affected** | `AccountsReceivable/Invoice/*`. |
| **Note** | Free text invoice is not named in `ARTables.md` or `CycleSalesorder.md`. **UNCERTAIN whether it is in the agreed basic scope** — raised because the entities were already added. |

### P3 — `SpecTrans` (mark for settlement) is unused

| | |
|---|---|
| **Existing** | `SpecTrans` entity + configuration + DTO + validator under `AccountsReceivable/Settlement`. No service writes or reads it; `CustPaymentService` settles directly from `CustTransOpen` by `SettleInvoiceId` or oldest-due-first. |
| **Expected (D365)** | `SpecTrans` holds the transactions a user has marked for settlement before posting, so the selection survives between entry and posting and supports multi-invoice marking. |
| **Missing** | The marking flow. Current single `SettleInvoiceId` cannot express "settle these three invoices". |
| **Affected** | `AccountsReceivable/Settlement/*`, `CustPaymentService`, and the AP equivalent (no `SpecTrans` usage there either). |

### P3 — No inventory close cancellation

| | |
|---|---|
| **Existing** | `InventCloseService` states "Close reversal is not supported". `InventClosing` records and `InventSettlement` rows are written with no cancel path. |
| **Expected (D365)** | Cancel inventory close reverses the close's settlements and cost adjustments, restoring the prior state. |
| **Missing** | Cancellation. A close run on the wrong cutoff is unrecoverable through the API. |
| **Affected** | `Inventory/InventClosing/Services/InventCloseService.cs`, `InventSettlement`, `InventClosing`. |

### P3 — Two architectural deviations from the module's own posting rules

Not functional gaps, but they undermine the single-posting-path guarantee the `d365-parity` skill
is built on:

1. `InventCloseService` defines its **own** `LoadLedgerContextAsync` and `LedgerContext` instead of
   using `SubledgerPostingSupport.LoadLedgerContextAsync`. Period-status rules can drift between
   the two. — `Inventory/InventClosing/Services/InventCloseService.cs:345`
2. `VendSettlement` and `VendSettlementConfiguration` are both declared inside
   `AccountsPayable/Payment/Entities/VendTransOpen.cs`, breaking the enforced seven-folder layout
   (entity in `Entities`, configuration in `Configuration`, one type per file).

---

## 3. Priority summary

| # | Gap | Priority | Area |
|---|---|---|---|
| 1 | ~~Account structures not modelled or validated~~ | **FIXED** (phases A-C) | GL / Dimensions |
| 2 | Main account posting controls unenforced | ~~P1~~ **P3** (four controls applied; rest blocked on missing setup) | GL |
| 3 | `LedgerParameters` voucher/rounding controls unenforced | ~~P1~~ **P3, blocked** (silent skip fixed) | GL |
| 4 | ~~Closed fiscal period reopenable via generic CRUD~~ | **FIXED** | GL |
| 5 | No pending vendor invoice (`VendInvoiceInfoTable`) | P1 | AP |
| 5a | Period access by module and user group not modelled | P2 | GL |
| 6 | ~~Charges (`MarkupTrans`) never invoiced or posted~~ | **FIXED** (AR + AP) | AR / AP |
| 7 | No cash discount | P2 | AR / AP |
| 8 | No settlement or payment reversal | P2 | AR / AP |
| 9 | ~~No Validate / Simulate posting~~ | **FIXED** | GL |
| 10 | No sales tax settlement | P2 | Tax |
| 11 | Free text invoice entities with no process | P3 | AR |
| 12 | `SpecTrans` marking unused | P3 | AR / AP |
| 13 | No inventory close cancellation | P3 | Inventory |
| 14 | Duplicate ledger-context resolution; misplaced `VendSettlement` | P3 | Inventory / AP |

Suggested order: ~~4~~ → ~~3~~ → ~~2~~ → ~~1~~ → ~~9~~ → **6** (charges — the one most likely to
produce a visible accounting error, since charges are configured, attached to orders and then
dropped) and **5** (pending vendor invoice). All four P1 entries are addressed; what is left is P2
and below.

**Progress (2026-10-31, backend only — services, API, database and tests; no IXApp work).** Gaps 4,
3, 2 and 9 are schema-neutral; gap 1 added two migrations.

- Gap 4 **fixed**: ledger period status transitions are guarded.
- Gap 3 **partly addressed and reprioritized to P3/blocked**: switched-on posting controls are now
  refused rather than silently skipped, but applying them is blocked on setup that does not exist
  (automatic transaction accounts) and on unverified check semantics. The P1 rating was wrong.
- Gap 2 **mostly done, reprioritized to P3**. The enum questions this document raised are now
  **answered from public Microsoft metadata**, and the answers found **two wrong enums in this
  repository**, both corrected: `DebitCreditCheck` carries the required side itself (it shares
  `DebCredProposal` with `DebitCreditProposal`) rather than being a `None`/`Check` switch, and the
  three `Validate*` fields are `FieldControl`, not bare `int`. The debit/credit requirement,
  mandatory payment reference, and currency and posting-type validation in FillIn and Table modes
  are applied; the payment reference now reaches the posted entry instead of being dropped. What
  remains is blocked on setup that does not exist. Schema-neutral: no migration.
- Gap 1 **fixed across three phases**, and it was the largest by far because the dimension model had
  no database at all. **A** gave financial dimensions tables, a resolver and a seeded Department
  dimension; **B** added the four constraint tables, `LedgerStructure`, the criteria matcher and the
  define/activate/assign builder; **C** wired validation into `LoadDimensionAsync` and made the
  cycle doc's acceptance example pass. Two migrations, both verified free of model drift.
- Gap 9 **fixed**: Validate combines the static checks; Simulate runs the real posting path and
  rolls it back, so its answer cannot drift from the rules that actually post. T15 is tested
  directly.
- Next by this order is **gap 6**, posting order and invoice charges.

**Open questions for whoever picks this up:**

1. `DebitCreditBalanceDemand` uses its own enum `DebCredBalanceDemand`, whose members were not
   retrieved. Its basis (which balance, at what point) and whether it refuses or warns are still
   unestablished. The repo's `DebitCreditDemand` (`None`/`Demand`) may be the wrong shape for the
   same reason `DebitCreditCheck` was — **check it before using it**.
2. Whether to assign the seeded BS and PL structures to the ledger, which is what switches
   validation on for the demo company. See the gap 1 entry: PL's Department is mandatory and no
   seeded combination has one.
3. `LedgerStructure`'s name and field set are confirmed from the AX 2012 reference, not from target
   metadata. The relation set matches current D365 behaviour, but confirm before relying on it.

A note on method, since three of the four P1 entries turned out to be mis-rated: a field or a class
that exists and is unused is not evidence that the behaviour behind it is implementable, or even
that it has a table. Check what the control needs to *do*, and whether the setup it depends on
exists, before rating it. Gaps 3 and 2 were blocked rather than undone; gap 1 was far larger than
the entry said.

---

## 4. Deliberately out of scope

Confirmed absent and **correctly** absent for basic parity — not gaps. Listed so they are not
re-reported: fixed assets and depreciation, budgeting and budget control, year-end close,
foreign-currency revaluation and exchange adjustment, ledger allocation rules, accrual schemes,
intercompany accounting, interunit/balancing dimensions, consolidation and elimination, posting
layers other than `Current`, bank reconciliation and statement matching, withholding tax,
production and manufacturing, advanced warehousing, revenue recognition, workflow approval
engines, financial report designers, localization extensions.

Weighted-average costing is also absent; the `d365-parity` basic scope requires **one** costing
model, and FIFO is implemented end to end. Not a gap.

Each basic path that meets one of the above refuses with a message naming what is missing —
`LedgerJournalPosting.EnsureInScope`, the FX checks in `CustPaymentService`, and
`LedgerSettlementService`'s out-of-balance refusal. That is the behavior the parity rules ask for
and it should be preserved.

---

## 5. Evidence limits

- No target D365 version was supplied. Table and field shapes for things that do **not** exist yet
  (account-structure constraints, `CashDisc`, `VendInvoiceInfoTable`) are from Microsoft's published
  model and must be confirmed against target metadata before migrations are written.
- This analysis is static reading plus grep. It did not run the build or the test suites, so it
  makes no claim about current test status.
- Enum value semantics flagged `int` in the entities (`ValidateCurrency`, `ValidatePosting`,
  `ValidateUser`, `AllocationLevel`) were not resolved and are marked UNCERTAIN above.
- Bank (`BankAccountTable`, `BankAccountTrans`, `BankAccountStatement`, `BankGroup`,
  `BankTransType`) is generic CRUD only. `BankAccountTrans` is written correctly by both payment
  services, so there is no basic-scope gap there; statement handling is bank reconciliation, which
  is out of scope.

---

## 6. Revision 2026-11-01 — re-verification at `f4439405`

Every open entry in §2 was re-checked against the code at `f4439405` rather than carried forward on
this document's own "FIXED" annotations. Method and limits are unchanged: static reading plus grep,
no build and no test run, so this revision makes no claim about test status either.

### 6.1 Closed entries, confirmed in code

Verified present and wired, not inferred from the annotations above: the account-structure tables
with `AccountStructureValidator` reached from `SubledgerPostingSupport.LoadDimensionAsync`;
`FiscalPeriodStatusTransition` with generic update sealed; `SalesChargePosting` and
`PurchaseChargePosting`; Validate/Simulate; and the four main-account controls that have setup
behind them. Gaps 1, 4, 6 and 9 of the §3 summary are closed.

`FiscalPeriodStatusTransition` and the new year-end close are **consistent**: the transition machine
deliberately refuses both to set and to leave `YearClosed`, deferring it to the close process. The
apparent conflict with the gap-4 entry's "neither set nor left since year end close is out of scope"
is wording that predates the close, not a defect.

### 6.2 New entry — P1: year-end close exists, in a controller, and is out of basic scope

From `2862b60e`. Three problems in one place, and a scope decision that is not ours to take.

| | |
|---|---|
| **Existing** | `GeneralLedger/FiscalCalendar/Controllers/LedgerYearEndCloseController.cs` is 249 lines of posting logic in a controller, with no `Services/` or `Interfaces/` entry. It hand-builds `GeneralJournalEntry` (line 122) and `SubledgerVoucherGeneralJournalEntry` (line 134). It *does* take `SubledgerPostingSupport.LockAsync` (line 28) and use `SubledgerPostingSupport.AccountEntry` (lines 140, 179, 217), so this is not a full bypass of the engine — entry construction is what sits outside it. `GeneralLedger/JournalEntry/Controllers/LedgerFinancialStatementsController.cs` has the same shape: 80 lines of balance derivation in the controller, returning anonymous objects, with no service or interface. |
| **Expected** | The `d365-parity` rules list year-end close as out of basic scope; require the seven-folder layout; and require that `GeneralJournalEntry`/`GeneralJournalAccountEntry` are never hand-built outside `SubledgerPostingSupport`. |
| **Affected** | Both controllers; `Common/Posting/SubledgerPostingSupport.cs` would need a close-voucher path if the feature stays. |
| **Decision needed** | Either year-end close is in scope now — in which case it belongs in a service with an interface and tests, constructing its entries through the engine — or it comes out. Both files were left untouched pending that call. |

### 6.3 New entry — P2: no customer or vendor open-balance read

| | |
|---|---|
| **Existing** | `CustTransController` and `VendTransController` return paged transaction lists (`VendTransPageDto` is `Page, PageSize, TotalCount, Rows`). No endpoint aggregates open balances or aging, and `CustTransOpen` / `VendTransOpen` have no read surface at all. |
| **Expected** | Basic scope names "customer transactions **and open balances**" for both subledgers. The data is written correctly by the invoice, return and payment paths; it cannot be read as a balance. |
| **Affected** | `AccountsReceivable/CustTrans/*`, `AccountsPayable/VendTrans/*`. |

### 6.4 Still open, verified unchanged

| Entry | Evidence at `f4439405` |
|---|---|
| **P1** No pending vendor invoice | A `VendInvoiceInfo` search across `src` hits only `APTables.md` and `CyclePurchorder.md`. |
| **P2** No AR/AP payment or settlement reversal | One `Revers*` hit in both payment and settlement folders: `CustSettlement.cs:92` `CanBeReversed`, which nothing reads. `CustPaymentService.cs:176` still deletes the `CustTransOpen` row on full settlement. The most consequential P2 — a mis-settled payment has no correction path. |
| **P2** No cash discount | No file matches `*CashDisc*`. The `CashDisc*` fields on `SalesTable`/`PurchTable`/`CustTrans` are written by the order services and read by nothing. |
| **P2** Period access by module/user group | `LedgerFiscalCalendarPeriodDto.cs:14-18` still hard-codes five `"Open"` properties; `SubledgerPostingSupport.cs:47` still checks the single status only. |
| **P2** No sales tax settlement | `TaxJournalTrans` still has entity, configuration and DTO and no service or controller. Nothing reads `TaxTrans` to settle, and no settlement stamp is written. |
| **P3** `MarkupTrans.IsDeleted` shadow | `MarkupTrans.cs:131` unchanged. Now that charges post, a soft-deleted charge row is visible to the posting paths — worth doing sooner than its priority suggests. |
| **P3** Free text invoice | `AccountsReceivable/Invoice/Services` holds only `SalesInvoiceService` and `SalesChargePosting`; `Controllers` only `SalesInvoiceController`. The validator is still registered at `FinanceModule.cs:82`, and the only non-definition read is `SalesDeliveryAddressCleanup.cs:39`. |
| **P3** `SpecTrans` unused | No service reads or writes it; AP has none at all. |
| **P3** No inventory close cancellation | `InventCloseService.cs:20` still states close reversal is unsupported; no `Cancel` method. The backdated-close refusal at line 76 limits the exposure. |
| **P3** Main account / `LedgerParameters` remainders | Unchanged and still blocked on setup that does not exist, not on effort. Left refused rather than guessed, which is the correct state. |

### 6.5 Architectural deviations — one resolved, one open

- **Resolved.** `InventCloseService.LoadLedgerContextAsync` (line 347) now delegates to
  `Support.LoadLedgerContextAsync` and only wraps the result in a local record, so period rules can
  no longer drift between the two. §2's deviation 1 is closed.
- **Open.** `VendSettlement` (line 18) and `VendSettlementConfiguration` (line 41) are still both
  declared inside `AccountsPayable/Payment/Entities/VendTransOpen.cs`.
- **New.** The two controllers in §6.2 are a third instance of the same class of problem: process
  logic outside a service.

### 6.6 Revised priority summary

| # | Gap | Priority | Area |
|---|---|---|---|
| 5 | No pending vendor invoice | **P1** | AP |
| 15 | Year-end close in a controller, and out of basic scope | **P1 (decision)** | GL |
| 8 | No settlement or payment reversal | **P2** | AR / AP |
| 16 | No customer/vendor open-balance read | **P2** | AR / AP |
| 7 | No cash discount | P2 | AR / AP |
| 5a | Period access by module and user group | P2 | GL |
| 10 | No sales tax settlement | P2 (priority UNCERTAIN) | Tax |
| 2 | `MarkupTrans.IsDeleted` shadow | P3 (raised in practice) | Foundation |
| 11 | Free text invoice entities with no process | P3 (scope UNCERTAIN) | AR |
| 12 | `SpecTrans` marking unused | P3 | AR / AP |
| 13 | No inventory close cancellation | P3 | Inventory |
| 2, 3 | Main account / `LedgerParameters` remainders | P3, blocked | GL |
| 14 | Misplaced `VendSettlement`; controller-resident logic | P3 | AP / GL |

Suggested order: **15** first, because it is a decision rather than work and it governs whether the
year-end code is kept, moved or removed; then **5**, then **8** and **16** together since both touch
the same subledger read and correction surface.

### 6.7 Evidence limits carried forward

§5 stands in full. Unchanged: no target D365 version, so `VendInvoiceInfoTable`, `CashDisc` and the
module period-access tables are shaped from Microsoft's published model and must be confirmed before
a migration. `DebCredBalanceDemand`'s members and basis,
`AllowMultipleTransactionsWithinOneVoucher`'s grouping rule, and the exact `CheckVoucher` /
`CheckContinuous` / `CheckJournalizing` semantics remain **UNCERTAIN**. Whether sales tax settlement
and free text invoice belong to the agreed basic scope is still a judgment call, not an established
fact.

### 6.8 A note on what this pass did not find

No new accounting defect. Every gap above is something absent or unenforced, not something posting
wrongly — with one exception worth stating plainly: the `MarkupTrans` soft-delete shadow became
reachable the moment charge posting shipped, so it moved from latent to live without its priority
changing. The P1 that matters most is still a decision (§6.2), not a build.

---

## 7. Gap 5 implemented 2026-11-01 — pending vendor invoice

The year-end-close decision in section 6.2 was answered: **basic ERP only, no advanced features**,
so year-end close stays out of scope and its code is left in place untouched pending removal or a
move into a service. The next basic gap in the section 6.6 order, gap 5, is now done.

### What was built

`VendInvoiceInfoTable` and `VendInvoiceInfoLine` with the field lists from `APTables.md` sections
8.1 and 8.2, their configuration, a create/read/discard/post service, a controller, a number
sequence, and migration `20261101071102_AddPendingVendorInvoiceTables`.

| Decision | Why |
|---|---|
| **Posting is delegated, not reimplemented.** `VendInvoiceInfoService.PostAsync` maps the held invoice to `VendorInvoicePostDto` and calls `IVendorInvoiceService.PostAsync`. | The register's entry asked for exactly this: `VendorInvoiceService` posts *from* the pending invoice rather than from the PO. Quantity, period, profile, dimension, tax and charge rules stay in one place, so entry cannot drift from posting. |
| **Lines belong to the header through `ParmId`.** | `APTables.md` documents `ParmId` as the invoice processing group on the line and the processing reference on the header. |
| **The source link is `OrigPurchId` + `InventTransId`.** | Both are documented fields, and together they resolve the `PurchLine` that posting updates — so no convenience column was added to carry a `PurchLine` record id. The document's requirement to "preserve the association between the invoice line and the source PO/receipt lines" is met by the fields it names. |
| **Default quantity selection is the received, uninvoiced amount** (`RemainPurchFinancial` less `RemainPurchPhysical`), taken when the caller sends no lines. | D365's default selection for an invoice. An explicit selection is validated against the same figure. |
| **Duplicate invoice numbers are refused per vendor**, against both posted `VendInvoiceJour` and held pending invoices. | `APTables.md` section 8.1: check per the configured policy and **do not** impose uniqueness across all vendors and legal entities. |

### Flagged for verification

- **`TableRefId` is on both tables because the document lists it, and nothing depends on it.** What
  generates it in D365 is not established from this repository or the cycle docs, and inventing a
  generator would have been an invented rule. The column exists and stays zero. **Confirm against
  target metadata before anything reads it.**
- **A configurable duplicate-invoice policy has no setup to read.** `VendParameters` does not exist
  in this module (`find -name "VendParameters*"` → nothing), so the per-vendor rule is applied
  directly rather than from configuration. Adding the policy means adding `VendParameters` first.
- **One purchase order per pending invoice.** A pending invoice whose lines span several orders is
  refused by name rather than approximated, because the posting path takes one `PurchTable`.
- The field lists come from the project's own `APTables.md`, not from target D365 metadata. The
  section 5 caveat still applies.

### Not included, deliberately

Line-level purchase charges on the pending invoice, proration across partial invoices, and
`VendInvoicePackingSlipQuantityMatch` selection at entry time — the match is written by the posting
path, as it already was. Invoice-register and approval-journal flows are separate D365 features and
were not touched.

### Verification run

- `dotnet build IXApi/IXApi.csproj` → **0 errors** (194 warnings, all pre-existing and in unrelated
  seeding files).
- `dotnet ef migrations has-pending-model-changes` → **no model drift**; the migration creates only
  the two new tables.
- `dotnet test IXApi/Tests/IXApi.Tests.csproj` → **578 passed, 1 failed, 2 skipped**. The failure is
  `AccountsReceivableGenericArchitectureTests.Crud_controllers_use_registered_generic_services`, and
  it is **pre-existing**: it fails identically with this work stashed on an otherwise clean tree, and
  it scans only the `.AccountsReceivable` namespace, which this change does not touch.
- `dotnet test IXApi/Tests/InventoryCycle.Tests.csproj` → **18 passed, 0 failed**.
- 13 new tests in `Tests/VendInvoiceInfoTests.cs`, all passing: default selection, over-selection
  refusal, nothing-received refusal, the per-vendor duplicate rule in both directions, the
  already-posted refusal, a blocked vendor, the hand-off of line/quantity/price to the posting path
  with the hold released after it, a refused posting leaving the hold in place, discard, read-back,
  unknown `ParmId`, and a return order.

### A build-environment correction

`global.json` pinned `9.0.311` with `rollForward: latestFeature`, which requires SDK feature band
`9.0.3xx` **or higher**. A machine with `9.0.201`/`9.0.202` therefore could not build at all —
`dotnet build` failed with "a compatible .NET SDK was not found", not just the .NET 10 case the
section 2 note anticipated. The floor is now `9.0.202`, which still resolves to the newest installed
9.0 feature band (so `9.0.311` where it is installed) and still keeps the .NET 10 SDK out. The
intent of the pin is unchanged; only its floor moved.

### Next in the section 6.6 order

Gap 8 (no settlement or payment reversal) and gap 16 (no customer/vendor open-balance read), which
touch the same subledger correction and read surface.

---

## 8. Gap 8 part one implemented 2026-11-01 — settlement reversal in AR and AP

Gap 8 has two halves: **settlement reversal** and **payment reversal**. The settlement half is now
done for both subledgers, which closes the harm the register named — "a mis-settled payment
currently has no supported correction path". Payment reversal is **not** done; see the end of this
section.

### What was built

`CustSettlementReversalService` and `VendSettlementReversalService`, each with an interface and a
controller, plus migration `AddVendSettlementCanBeReversed`.

`POST /api/v1/customer-settlements/{settlementVoucher}/reverse` ·
`POST /api/v1/vendor-settlements/{voucher}/reverse`

### The record pattern, decided rather than assumed

The question was how the settlement rows should record a reversal: `CustSettlement` has
`CanBeReversed` but no `IsReversed` or reversal-link field. Two patterns were put to the owner —
mark in place, following this repository's own `LedgerSettlementService.ReverseAsync`, or keep the
originals and write offsetting rows. **The owner chose offsetting rows, as the closer match to
D365.** So:

- the original rows are kept untouched as history, apart from `CanBeReversed` going to `No`;
- offsetting rows are written with every amount negated, under a new settlement voucher, with
  `CanBeReversed = No` so a reversal cannot itself be reversed;
- the two vouchers net to zero, which is asserted by test.

**Flagged:** there is **no field linking a reversal back to the settlement it reverses** — unlike
`LedgerTransSettlement`, which has `OriginalSettleId`. The pair is discoverable only by its
`TransRecId`/`OffsetRecId` values. No link field was invented. Confirm against target metadata
whether standard `CustSettlement` carries one.

### Why there is no ledger voucher, and why that is correct

Settlement in this module **posts nothing to the general ledger**. The payment's voucher carries
Dr Bank / Cr Customer (or Dr Vendor / Cr Bank); settlement only moves the open receivable or
payable. Reversing it therefore has nothing to counter-post. This is an absence by correctness, not
an omission.

The things that *would* make a settlement post — cash discount, realised exchange adjustment, penny
difference — are not implemented, so on the receivable side a settlement carrying `UtilizedCashDisc`,
`ExchAdjustment` or `PennyDiff` is **refused by name** rather than reversed with its ledger effect
left stranded. (The payable side's reduced `VendSettlement` has no such fields to check.)

### How the restore is made exact

Both payment paths assert an invariant before they settle: the open row equals
`AmountCur` less `SettleAmountCur`. The reversal rebuilds the open row from that same invariant,
which makes the restore correct whether the row was decremented on a partial settlement or
**removed** on a full one — the deletion the register flagged as making the open amount
unrecoverable. It is recoverable, because `CustTrans` still carries both amounts. The payable side
reuses the existing `VendorOpenBalance.Add` helper, which already derives the row that way.

The two subledgers have **opposite sign conventions**, and both were checked against their payment
path rather than inferred: receivable settlement does `invoice += settle` / `payment -= settle`,
payable settlement does `invoice -= settled` / `payment += settled`. The first test run caught a
seed built on the wrong payable convention.

### Also fixed on the way

`VendSettlement` and `VendSettlementConfiguration` were declared inside
`AccountsPayable/Payment/Entities/VendTransOpen.cs`. Both are now in their own files under
`Entities/` and `Configuration/`, closing the layout half of gap 14.

Its unique index moved from `(DataAreaId, InvoiceTransRecId, PaymentTransRecId)` to include
`Voucher`: without that, an offsetting row for the same invoice and payment pair would collide with
the settlement it reverses.

### Flagged for verification

- **`VendSettlement` is not D365's shape.** Standard `VendSettlement` carries the same field set as
  `CustSettlement` — `SettlementVoucher`, `TransRecId`/`OffsetRecId` pairs, reporting-currency and
  adjustment amounts. This repository's version is a reduced custom shape keyed by the payment
  voucher, with one row per settled invoice instead of a pair. `CanBeReversed` was added because it
  is a standard field; **aligning the rest is its own change** and was not attempted here. It is
  noted on the entity.
- **Settlements written before this change default to `CanBeReversed = No`** and are therefore
  refused, with a message naming both possible reasons. Refusing is deliberate: nothing records what
  those rows did, so reversing them would rest on an assumption. Existing demo data will need
  re-settling to be reversible.

### Verification run

- `dotnet build IXApi/IXApi.csproj` → **0 errors**.
- `dotnet ef migrations has-pending-model-changes` → **no drift**; the migration touches only
  `VendSettlement`'s new column and its index.
- `dotnet test IXApi/Tests/IXApi.Tests.csproj` → **592 passed, 1 failed, 2 skipped**. The failure is
  the same pre-existing
  `AccountsReceivableGenericArchitectureTests.Crud_controllers_use_registered_generic_services` at
  the same line 49 assertion as before this change, already shown in section 7 to fail on a clean
  tree. The new `CustSettlementReversalController` is in the scanned namespace and satisfies every
  assertion that test makes.
- `dotnet test IXApi/Tests/InventoryCycle.Tests.csproj` → **18 passed, 0 failed**.
- 14 new tests in `Tests/SettlementReversalTests.cs`: full and partial reversal on both sides, the
  offsetting rows netting to zero, double-reversal refusal, the not-marked-reversible refusal, the
  three ledger-effect refusals, and unknown or empty vouchers.

### Still open: payment reversal

Reversing a **payment** — unwinding the cash, the bank movement and the GL voucher, not just the
match — is not built. It needs a counter-voucher through `SubledgerPostingSupport` and a decision
on whether D365 writes a reversing `CustTrans`/`VendTrans` or marks the original, which is the same
class of unverified question as the settlement row pattern was. Gap 8 stays **open at P2** for that
half; the register's stated harm is addressed, the feature is not complete.

### Next in the section 6.6 order

Gap 16, the customer/vendor open-balance read.

---

## 9. Gap 16 implemented 2026-11-01 — customer and vendor open-balance read

`GET /api/v1/CustTrans/open-balance?accountNum=&asOfDate=` ·
`GET /api/v1/VendTrans/open-balance?accountNum=&asOfDate=`

Both omit `accountNum` to read every customer or vendor in the company, and default `asOfDate` to
today. Each returns the balance, the open transaction count and the open transactions behind it.

### What it reads, and why that matters

The balance comes from **`CustTransOpen` / `VendTransOpen`** — the tables the payment paths settle
against — not from a sum recomputed over `CustTrans`/`VendTrans`. So the figure a caller sees is the
figure settlement will actually work from. An unsettled payment remainder is an open transaction
too and nets against the invoices, which is what makes the total the account's balance rather than
just its receivables.

`Reconciled` reports the condition the payment paths already enforce: both refuse to settle when an
open row disagrees with its transaction ("Customer open balances require reconciliation before
settlement"). The read surfaces that instead of presenting a balance settlement would reject. The
amount reported stays the open table's, because that is what settlement reads.

`DaysOverdue` is arithmetic on the due date and the as-of date, floored at zero. **No aging buckets
were invented** — basic scope names "customer transactions and open balances", not an aging report,
and the two are different features.

### Implementation notes

Extended the existing `ICustTransService` and `IVendTransReadService` rather than adding new
services, so nothing new had to be registered. The payable side's amounts are credits, so a vendor
balance owed reads negative; that is asserted by test rather than normalised away.

### Verification run

- `dotnet build IXApi/IXApi.csproj` → **0 errors**.
- `dotnet ef migrations has-pending-model-changes` → **no drift**. This change adds no schema.
- `dotnet test IXApi/Tests/IXApi.Tests.csproj` → **602 passed, 1 failed, 2 skipped** — the same
  pre-existing `Crud_controllers_use_registered_generic_services` failure as sections 7 and 8.
- `dotnet test IXApi/Tests/InventoryCycle.Tests.csproj` → **18 passed, 0 failed**.
- 10 new tests in `Tests/OpenBalanceReadTests.cs`: the balance as the sum of open rows on both
  sides, a payment remainder netting against invoices, due-date ordering with days overdue, the
  as-of cutoff, all-accounts versus one account, the unreconciled report on both sides, and an
  account with nothing open.

**A caveat on those suite numbers.** The tree these runs measured also contained an uncommitted,
in-progress **free text invoice** implementation (gap 11) written outside this work — `IdentitySeeder.cs`,
the four `CustInvoice*Configuration` files, two `FinanceModule` registrations, and five new
`AccountsReceivable/Invoice/FreeTextInvoice*` files. None of it is part of this change and none of it
was committed here. It compiled and the suite passed with it present, but **602/1 is therefore not a
clean measurement of this change alone**. The 10 new tests in this section exercise only the
open-balance read and pass on their own.

### Next in the section 6.6 order

Gap 7 (cash discount) and gap 5a (period access by module), with gap 8's **payment reversal** half
still outstanding from section 8. Gap 11 (free text invoice) appears to be under way outside this
work.

---

## 10. Gap 8 completed 2026-11-01 — payment reversal in AR and AP

The outstanding half of gap 8 from section 8 is now done, so **gap 8 is closed**.

`POST /api/v1/customer-payments/{journalNum}/reverse` ·
`POST /api/v1/vendor-payments/{journalNum}/reverse`

### The pattern, taken from the repository rather than guessed

Section 8 left this half open partly because it needed "a decision on whether D365 writes a
reversing CustTrans/VendTrans or marks the original". That question was **answered from the
repository**: `LedgerJournalReversalService` already establishes the house pattern for a reversal,
and current D365 behaviour agrees with it. A reversal is **itself a posted document**:

- a new `LedgerJournalTable` whose `OriginalJournalNum` points at the reversed journal, with its own
  journal number, voucher and accounting date;
- the date must fall in an **open period** — the original period is never reopened implicitly;
- nothing posted is edited or deleted, the original stays exactly as it was;
- a payment is reversed **once**, and a reversal **cannot itself be reversed**.

So no new pattern was invented and no second posting path exists: the counter-voucher goes through
`SubledgerPostingSupport` like everything else.

### What a reversal does

1. **Unwinds the payment's settlements first**, so the invoices go back to open. Without this the
   counter-voucher would leave the subledger disagreeing with the ledger.
2. **Posts the counter-voucher**: Dr Customer / Cr Bank for a receivable payment, Dr Bank /
   Cr Vendor for a payable one — the mirror of what the payment posted.
3. **Reverses the bank movement**, writing the opposite `BankAccountTrans` against the original
   bank account, found through the payment line rather than guessed.
4. **Settles the payment against its own reversal**, so neither is left sitting open. That closing
   settlement is marked not reversible: reversing the payment is what undoes it.

### The shared restore, extracted rather than duplicated

Payment reversal needs the settlement unwind that section 8 built, but inside its own transaction.
Rather than carry a second copy of those rules, the row work was extracted into
`CustSettlementRestore` and `VendSettlementRestore` — static helpers that manage no transaction and
call no save, so the caller owns atomicity. The settlement reversal services now delegate to the
same helpers. **There is one implementation of the restore, used by both features**, which is the
point: a payment reversal cannot drift from a settlement reversal.

A consequence worth having: a payment reversal is atomic. A refusal part-way through — a closed
period, for instance — rolls back the settlement unwind with it. That is asserted by test.

### A bug caught while extracting it

The settlement reversal services read their `restored` summary with `AsNoTracking` **before** the
save, so it reported pre-restore values from the database rather than the restored ones. The read
now happens after the save. The restore itself was always correct; only the response payload was
stale.

### Flagged for verification

- **Single-line payment journals only.** Both payment services write one line, so a journal with
  more is refused by name rather than reversed partially.
- The closing settlement between a payment and its reversal is written with `CanBeReversed = No`.
  Whether standard D365 records that link as a settlement at all was not established from the
  repository — it is how a payment and its reversal stop showing as open here.
- **Currency conversion is refused**, as the payment paths already refuse it.

### Verification run

- `dotnet build IXApi/IXApi.csproj` → **0 errors**.
- 12 new tests in `Tests/PaymentReversalTests.cs`, **all passing on the first run**: the
  counter-voucher and its signs on both sides, the reversal journal pointing at the one it reverses
  with the original untouched, the invoice reopened, payment and reversal left settled against each
  other, double-reversal refusal, a reversal journal refusing to be reversed, the closed-period
  refusal rolling the settlement unwind back with it, and unknown or wrong-type journals.
- `dotnet test IXApi/Tests/IXApi.Tests.csproj` → **615 passed, 1 failed, 2 skipped**, the same
  pre-existing `Crud_controllers_use_registered_generic_services` failure as sections 7 to 9.

**Two caveats on that run, both about concurrent work, not about this change.**

1. The tree also contained an in-progress **free text invoice** implementation written outside this
   work. It was subsequently committed together with this payment-reversal work as
   `27c26fa8 feat(finance): add free text invoices and payment reversals` — a commit made outside
   this work, bundling both. So **615/1 is not a clean measurement of payment reversal alone**,
   though the 12 tests above exercise only it.
2. **The build is currently broken by newer concurrent work, not by this change.**
   `GeneralLedger/LedgerJournalTable/Configuration/LedgerJournalIdentityConfiguration.cs`
   (uncommitted, written outside this work) fails with `CS0118: 'Ledger' is a namespace but is used
   like a type` at lines 7 and 9. The cause: the file's namespace is
   `...GeneralLedger.LedgerJournalTable`, which makes the bare identifier `Ledger` resolve to the
   sibling **namespace** `...GeneralLedger.Ledger` instead of the entity. The same file already
   works around this for `LedgerJournalTable` and `LedgerJournalTrans` by fully qualifying them;
   `Ledger` was missed. The fix is to write
   `IEntityTypeConfiguration<IAX.IXApi.Modules.Finance.Entities.Ledger>` and
   `EntityTypeBuilder<IAX.IXApi.Modules.Finance.Entities.Ledger>`. **It was left untouched**, being
   someone else's in-progress file. Until it is fixed, neither suite nor the drift check can run,
   and the `InventoryCycle` failures observed afterwards are that build failure, not test
   regressions.

### Gap 8 status

**Closed.** Settlement reversal (section 8) and payment reversal (this section) are both in, for
both subledgers, sharing one restore implementation.

### Next in the section 6.6 order

Gap 7 (cash discount) and gap 5a (period access by module). Gap 11 (free text invoice) was
implemented outside this work and should be reviewed and re-rated by whoever owns it.
