# Finance modules — D365 F&O basic parity gap analysis

Date: 2026-10-31. Branch `main` at `36c018bc`.

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

### P1 — Account structures are not modelled or validated

| | |
|---|---|
| **Existing** | `DimensionHierarchy` exists as a *header only* (`Name`, `StructureType`, draft fields, `IsDraft`). There are **no constraint tables** — no `DimensionHierarchyLevel`, `DimensionConstraintTree`, `DimensionConstraintNode`, `DimensionConstraintNodeCriteria`. `Ledger` has `ChartOfAccounts` and `FiscalCalendar` but **no account-structure assignment field**. At posting, `SubledgerPostingSupport.LoadDimensionAsync` validates only that the combination's main account exists; `LedgerJournalSetupCheck` additionally checks chart-of-accounts membership and explicitly reports "Account structures, dimension defaults and legal-entity account overrides" as *unverified*. |
| **Expected (D365)** | GL cycle doc §4 Step 4: account structures define which main-account ranges allow or require which dimensions; both structures are activated and **assigned to the ledger**. Its stated acceptance example: `611000 ADMIN` is valid, and a **blank Department on 611000 must fail**; a blank Department on `130100` is valid. |
| **Missing** | Account-structure definition tables, ledger assignment, and the posting-time validation of a `DimensionAttributeValueCombination` against the active structure. Today any dimension combination posts. |
| **Affected** | `Foundation/Dimensions/LedgerDimension/*`, `GeneralLedger/Ledger/Entities/Ledger.cs`, `Common/Posting/SubledgerPostingSupport.cs` (`LoadDimensionAsync`), `LedgerJournalSetupCheck`, every posting service (all resolve accounts through `LoadDimensionAsync`). |
| **Note** | Exact D365 constraint-table shape should be confirmed against target metadata before adding tables. The *gap* is certain; the *table set* above is from Microsoft's published model and is **UNCERTAIN** in detail. |

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
| **Missing** | Charge posting at sales invoice and vendor invoice; `SumMarkup` population; charge tax. A freight charge entered on an order is silently dropped from the invoice and the ledger. |
| **Affected** | `AccountsReceivable/Invoice/Services/SalesInvoiceService.cs`, `AccountsPayable/Invoice/Services/VendorInvoiceService.cs`, `Foundation/Markup/*`, `Common/Posting/SubledgerPostingSupport.cs` (needs a charge-account resolver). |

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

### P2 — No Validate / Simulate posting action

| | |
|---|---|
| **Existing** | Two read-only diagnostics: `GET /LedgerJournalTable/{recId}/setup-check` and `/voucher-balance-check`. Both deliberately report `PostingValidated = false`. No `Validate` or `Simulate` action exists (`grep Simulate` → 0 hits). |
| **Expected (D365)** | GL cycle doc §5 step 7 ("Validate, then Simulate posting where available"), §6.6 (validate/simulate checks ledger, period access, structures, accounts and rates and creates **no** posted voucher) and acceptance test **T15**. |
| **Missing** | A single Validate action running the full posting rule set, and a Simulate action that runs the posting path and rolls back. Both are now feasible since `LedgerJournalPosting` already runs inside a caller-controlled transaction. |
| **Affected** | `GeneralLedger/LedgerJournalTable/*` (service, interface, controller), reusing `LedgerJournalPosting`. |

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
| 1 | Account structures not modelled or validated | P1 | GL / Dimensions |
| 2 | Main account posting controls unenforced | ~~P1~~ **P3** (four controls applied; rest blocked on missing setup) | GL |
| 3 | `LedgerParameters` voucher/rounding controls unenforced | ~~P1~~ **P3, blocked** (silent skip fixed) | GL |
| 4 | ~~Closed fiscal period reopenable via generic CRUD~~ | **FIXED** | GL |
| 5 | No pending vendor invoice (`VendInvoiceInfoTable`) | P1 | AP |
| 5a | Period access by module and user group not modelled | P2 | GL |
| 6 | Charges (`MarkupTrans`) never invoiced or posted | P2 | AR / AP |
| 7 | No cash discount | P2 | AR / AP |
| 8 | No settlement or payment reversal | P2 | AR / AP |
| 9 | No Validate / Simulate posting | P2 | GL |
| 10 | No sales tax settlement | P2 | Tax |
| 11 | Free text invoice entities with no process | P3 | AR |
| 12 | `SpecTrans` marking unused | P3 | AR / AP |
| 13 | No inventory close cancellation | P3 | Inventory |
| 14 | Duplicate ledger-context resolution; misplaced `VendSettlement` | P3 | Inventory / AP |

Suggested order: ~~4~~ → ~~3~~ → ~~2~~ → **1** (largest, and a prerequisite for trustworthy dimension
posting), then 9, then 5 and 6.

**Progress (2026-10-31, backend only — services, API and tests; no schema change, no IXApp work).**

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
- Next by this order is **gap 1**, account structures. It is the largest and needs its constraint
  tables confirmed against target metadata before any migration.

**One open question remains** (the two raised on 2026-10-31 are resolved above):

1. `DebitCreditBalanceDemand` uses its own enum `DebCredBalanceDemand`, whose members were not
   retrieved. Its basis (which balance, at what point) and whether it refuses or warns are still
   unestablished. The repo's `DebitCreditDemand` (`None`/`Demand`) may be the wrong shape for the
   same reason `DebitCreditCheck` was — **check it before using it**.

A note on method, since two of the first three P1s turned out to be blocked rather than merely
undone: a field that exists and is unread is not by itself evidence that the behavior behind it is
implementable. Check what the control needs to *do* — and whether the setup it posts through
exists — before rating it.

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
