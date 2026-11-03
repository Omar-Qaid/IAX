# Accounts payable — gap analysis against basic D365 F&O

**Last updated:** 2026-11-03
**Scope:** basic (core) D365 F&O Accounts payable and the purchase order cycle only. Vendor
collaboration, invoice automation/OCR, workflow approval hierarchies, promissory notes, centralized
payments, and vendor rebates are out of scope by request and are not listed as gaps.

## How this file is maintained

Update this file **in the same change** that alters behavior. Closing a gap moves the row out of
*Open gaps* and adds a *Changelog* line. A new deliberate restriction is recorded with the **exact**
message the code raises, so the table stays verifiable by grep. A gap whose standard D365 behavior
cannot be verified goes under *Flagged uncertainties* rather than being guessed at.

## Status summary

| Area | Status |
| --- | --- |
| Vendors, vendor groups, vendor posting profiles (`VendLedgerAccounts`) | Implemented |
| Purchase orders (`PurchTable`/`PurchLine`), confirmation, partial receipt and invoice remainders | Implemented |
| Purchase order cancellation | **Missing** — `PurchStatus.Canceled` is read in nine places and written nowhere |
| Purchase price / discount trade agreements | **Missing** — the line price is whatever the caller sends |
| Standalone *Settle open transactions* (vendor credit note against an invoice) | **Missing** |
| Product receipt (physical update, accrual posting, `VendPackingSlipJour`/`Trans`) | Implemented |
| Vendor invoice (financial update, 3-way quantity match to the receipt, charges, tax) | Implemented |
| Pending vendor invoice (`VendInvoiceInfoTable`) | Implemented |
| Vendor payments, payment reversal, settlement reversal, cash discount | Implemented |
| Accounts payable parameters (`VendParameters`) with the default posting profile | Implemented |
| Vendor methods of payment (`VendPaymModeTable`), validated at payment | Implemented |
| Purchase returns / vendor credit notes | Implemented |
| Vendor transactions (`VendTrans`, `VendTransOpen`) | Implemented |
| Physical and financial inventory value through the receipt/invoice pair | Implemented |
| SQL Server behavior (atomicity, concurrency, idempotency) | **Unverified** — SQLite tests only |

## Open gaps

| Area | Existing behavior | Expected basic D365 F&O behavior | Missing | Affected components | Priority |
| --- | --- | --- | --- | --- | --- |
| Accounting-currency columns on AP documents | `VendTrans` has `AmountMst` but no reporting-currency amount, and `VendInvoiceTrans` has no accounting-currency line amount at all. | The AR counterparts have both: `CustTrans.ReportingCurrencyAmount` and `CustInvoiceTrans.LineAmountMst`. | The columns. Found while wiring currency translation: the translated figures exist but AP has nowhere to store the per-line and reporting ones, so only `VendTrans.AmountMst` is written. **Not invented** - adding them is a schema change. | `VendTrans`, `VendInvoiceTrans` | Medium |
| Foreign currency — vendor payment | Re-verified 2026-11-03: `VendPaymentService` lines 63-67 still refuse any payment whose currency differs from the accounting currency, the reporting currency **or the bank account's currency**, with `Payment currency conversion requires configured exchange-rate posting; a 1:1 conversion cannot be assumed.` Every amount is then written at 1:1 (`AmountCur = AmountMst = input.Amount`). The customer side was translated on 2026-11-02; the vendor side was not started and still is not. | A payment in any currency posts at its date's rate, and realized gain/loss is posted at settlement. | Payment translation and realized gain/loss on the vendor side. `SubledgerPostingSupport.ResolveRealizedGainLossAsync` and `CurrencyLedgerGainLossAccount` already exist from the AR work, so the accounts and the lookup are in place — but copying `CustPaymentService` would copy an **untested** calculation (see the AR file), so the AR test should come first. | `VendPaymentService`, `VendSettlementReversalService` | **High** |
| Foreign currency | **The vendor invoice and the vendor credit note translate** a foreign-currency document at the document date's rate. Each line's cost is translated **before** it is written, so `InventTrans.CostAmountPosted` and `InventSum.PostedValue` stay in the accounting currency. The vendor line is derived from the translated lines, so the voucher balances exactly. The vendor **payment** still has no translation. | A document in any currency posts at its date's rate, and realized gain/loss is posted at settlement. | Vendor payment translation, and **exchange gain/loss at settlement**, which has not been started. | `VendPaymentService`, `VendSettlementReversalService` | **High** |
| Price variance on credit notes | A credit amount differing from the original receipt cost is rejected with a named error. | The difference posts as a purchase price variance per the posting profile. | Variance calculation and posting. | `PurchReturnService`, `InventPosting` | Medium |
| Pending invoice across several orders | A pending vendor invoice covering more than one purchase order is rejected with a named error. | One invoice may cover lines from several purchase orders. | Multi-order pending invoice composition. | `VendInvoiceInfoService` | Medium |
| Invoice matching tolerances | Quantity matching to the product receipt is implemented via `VendInvoicePackingSlipQuantityMatch`. | Basic AP also supports price and total matching with configurable tolerances, and matching policy per vendor or item. | Price/total matching and tolerance configuration; matching policy setup. | `VendorInvoiceService`, `AccountsPayableParameters` (absent) | **High** |
| **A purchase order can never be cancelled** | There is **no cancellation path at all**. `PurchTableController` exposes create, read, add line, confirm, receive and invoice; generic update and delete are disabled; `IPurchOrderService` declares `CreateAsync`, `AddLineAsync` and `GetAsync` only. `PurchStatus.Canceled` is **read by nine call sites** across confirmation, charges, pending invoice, invoicing, receipt and returns — and **written by none** (verified by grep for `PurchStatus = PurchStatus.Canceled`: no match in `src/`). | Cancelling a purchase order removes the quantity not yet received or invoiced; the received part stays invoiceable. [Purchase order cancellation](https://learn.microsoft.com/en-us/dynamics365/supply-chain/procurement/purchase-order-approval-confirmation) | The whole action. Every consumer is already written to respect the status, so the status plumbing exists and nothing sets it: an order placed in error, or one whose remainder a vendor will never ship, stays open with live `InventSum.Ordered` supply forever. Worse than the AR side, which at least cancels an untouched order. The change must reduce only open supply and the matching `InventSum.Ordered`, preserving received-but-uninvoiced quantity as invoiceable. | `IPurchOrderService`, `PurchOrderService`, `PurchTableController`, `PurchInventorySupplyService`, `InventSum` | **High** — procure-to-pay has no ending for an unreceived remainder |
| **No purchase price or discount trade agreements** | `PurchOrderService.AddLineCoreAsync` (lines 161-162) takes `PurchPrice` straight from `input.PurchPrice` and computes `LineAmount = PurchQty * PurchPrice / priceUnit`. There is **no `PriceDiscTable` reference anywhere under `AccountsPayable/`** (verified by grep), and no line, multiline or total discount. The sales side resolves all of this: `SalesOrderLineService` line 509 and `SalesOrderDiscountService` lines 55-96 apply agreements and line/multiline/end discount. | A purchase line defaults its price and discount from the vendor's purchase trade agreements, by vendor, vendor group or all, with quantity breaks and date validity. [Purchase price and discount agreements](https://learn.microsoft.com/en-us/dynamics365/supply-chain/procurement/purchase-prices-discounts) | Purchase-side agreement resolution. **The table needs no change**: `PriceDiscTable` is the shared D365 table and already carries `Module` (`ModuleInventCustVend`) and `Relation` (`PriceType`) to distinguish purchase from sales agreements. `PriceDiscJournalPostingService` (lines 126-135) handles only the `Cust` and `Invent` group axes, so the journal needs the `Vend` axis too. This also makes vendor cash discount and the invoice **price** match (row below) meaningful: with no agreed price, there is nothing to match an invoice price against. | `PurchOrderService.AddLineCoreAsync`, `PriceDiscJournalPostingService`, `PriceDiscTable` | **High** — a purchase order currently trusts whatever price the caller sends |
| Payment proposal | Payments are entered and posted per invoice. | A payment proposal selects open invoices by due date, discount date or vendor and creates the payment lines. | The proposal generator. | `VendPaymentService` | **High** — core basic AP function |
| **No "Settle open transactions" process** | The mirror of the AR finding, with the same shape. `VendPaymentService` selects and settles open invoices itself as it posts; `Payment/` holds `VendSettlementReversalService` and `VendSettlementRestore` for reversal only. A vendor credit note writes an open credit row (`PurchReturnService` line 503, via `VendorOpenBalance.Add`) that no process can apply to an invoice. | Marking a vendor credit note against an open invoice, or applying an on-account payment to a later invoice, without a new payment. | The forward settlement action. AP has no `SpecTrans` equivalent at all. | `AccountsPayable/Payment/`, `VendPaymentService`, `PurchReturnService` | **High** — the only exit for a vendor credit note |
| Prepayments / prepayment invoices | Not present. | A prepayment is posted against a purchase order and applied to the final invoice. | Prepayment flow and application. | Purchase order, vendor invoice | Medium |
| Invoice approval journal | Pending vendor invoices exist; no approval journal. | The invoice approval journal posts an approved pending invoice to the ledger. | The approval journal. Workflow-based approval is out of scope, but the journal itself is basic. | `VendInvoiceInfoService` | Medium |
| Vendor aging | `VendTrans`/`VendTransOpen` carry due dates. | Aging buckets and a vendor aging report/inquiry. | Aging period definitions and the report. | `VendTransReadService` | Medium |
| Purchase requisitions / RFQs | Not present. | Arguably beyond basic AP; requisitions and RFQs are separate procurement processes. | — | — | Low — confirm scope before building |
| SQL Server verification | Covered by SQLite in-memory tests. | — | Posting atomicity, concurrent invoice/payment posting, number-sequence contention. | Whole module | **Critical** |

## Deliberate restrictions (verified in code)

| Restriction | Message |
| --- | --- |
| Multi-order pending invoice | `A pending vendor invoice covering several purchase orders is not supported in the basic scope.` |
| Credit note price variance | `The credit amount for item {itemId} differs from its original receipt cost; price variance posting is outside the basic scope.` |
| Credit note document currency | `Vendor credit note posting requires the return order currency to equal the ledger accounting currency; foreign-currency conversion is not supported.` |
| Credit note reporting currency | `Vendor credit note posting requires the reporting currency to equal the accounting currency; reporting-currency conversion is not supported.` |
| Unknown method of payment | `The method of payment was not found in the selected company.` |

## Flagged uncertainties

- **`VendParameters` field names.** The table now exists with `VendPostingProfile`, named after the
  receivable side's `CustParameters.CustPostingProfile`, which is the repository's convention. In
  D365 metadata the field is believed to be `PostingProfile` on both tables, named after the EDT
  rather than the EDT itself — **unverified against target metadata**. The remaining fields are
  still built only when an implemented flow reads them.
- **Purchase requisition and RFQ scope.** These are part of Procurement and sourcing rather than
  Accounts payable. Treated here as out of basic AP scope; confirm before building.
- **Payment file formats.** Method-of-payment setup implies electronic payment file generation,
  which is likely beyond basic scope. The setup entity is still basic, and now exists.
- **Whether the method of payment is mandatory on a vendor payment.** It is optional here: a payment
  naming no method, for a vendor with no default, still posts, because requiring one would refuse
  payments that post today. Whether D365 requires it is **unverified**.
- **`VendPaymModeTable.PaymentLedgerDimension`.** The field name mirrors the repository's
  `CustPaymModeTable`. Whether D365 names the method's payment account this way, and how it relates
  to `AccountType`, is **unverified**; nothing reads it yet, so a payment still names its own bank
  account.

## Changelog

| Date | Change |
| --- | --- |
| 2026-11-03 | Zero-rated tax codes now post their taxable base on the purchase side. `PurchaseTaxPosting` and `PurchaseChargePosting` write the `TaxTrans` row for a zero-rated code but no longer post a zero-amount ledger line, and no longer require an input tax account for it — previously a zero-rated code either never arrived or would have been refused for a missing account. Both already stamped `TaxValue`. Shared change in `Common`; gap recorded in `Foundation/GAPS.md`. |
| 2026-11-03 | Source scan. **No behavior changed.** Three new High rows, and the status table corrected where it overstated the purchase order as complete. **A purchase order cannot be cancelled** — the status is read in nine places and written in none, so an unreceived remainder holds `InventSum.Ordered` supply permanently. **There are no purchase trade agreements** — the line price is taken verbatim from the request while the sales side resolves agreements plus line, multiline and total discount; `PriceDiscTable` already supports the purchase axis, so the gap is resolution, not schema. **No forward settlement process**, mirroring the AR finding: a vendor credit note's open row can never be applied. Split the foreign-currency row so the untranslated vendor payment is visible on its own, and recorded that copying the customer payment would copy an untested calculation. |
| 2026-11-02 | The vendor invoice and vendor credit note now translate a foreign-currency document instead of rejecting it. The inventory cost is translated before it reaches `InventTrans`/`InventSum`, so a foreign-currency purchase cannot store foreign-currency amounts as inventory value. The credit note's price-variance check now compares the translated credit against the original cost, so it still compares like with like. |
| 2026-11-01 | `VendParameters` and `VendPaymModeTable` now exist, closing the two asymmetries with AR recorded earlier today. Recorded why wiring currency translation into the vendor invoice is not a mechanical repeat of the AR work. |
| 2026-11-01 | `VendParameters` added (entity, configuration, DTO, validator, service, controller, migration `AddVendParameters`), holding the company's default vendor posting profile. `ResolveVendorSummaryAsync` now narrows `VendLedgerAccounts` to that profile before applying Table -> Group -> All, which is what the customer path has always done: without it, a company with two posting profiles resolved whichever rule matched first. A configured profile with no rules of its own still falls back to every rule, as on the receivable side. Covered by `Tests/VendParametersPostingProfileTests.cs`. |
| 2026-11-01 | Posting now passes the vendor scope to inventory posting resolution, so an `InventPosting` rule configured for one vendor or vendor group is applied instead of silently falling back to the All rule. Fix is in `Common`. |
| 2026-11-01 | File created from a source scan. Flagged the missing AP parameters and method-of-payment entities as asymmetries with AR, and foreign-currency support as the critical gap. |

## Sources

- [Accounts payable home page](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/accounts-payable)
- [Vendor invoices overview](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/vendor-invoices-overview)
- [Invoice matching](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/invoice-matching)
- [Vendor payments](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/vendor-payments-wn)
- [Physical and financial updates](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/physical-financial-updates)
