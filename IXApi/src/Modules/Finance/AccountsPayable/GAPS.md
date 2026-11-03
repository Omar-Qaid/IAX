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
| Purchase order cancellation (unreceived remainder, whole order) | Implemented |
| Purchase price / discount trade agreements | **Missing** — the line price is whatever the caller sends |
| Standalone *Settle open transactions* (vendor credit note against an invoice) | Implemented |
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
| Purchase order cancellation: line-level and reinstatement | **Order-level cancellation implemented** - see the *Changelog*. `POST api/.../purchase-orders/{recId}/cancel` releases the unreceived remainder of every open line. What remains: cancellation is **whole-order only**, there is no way to cancel **one line** or part of a quantity, and a cancelled line cannot be reinstated. | D365 cancels at the line as well as the order, and the quantity can be reduced rather than cancelled outright. | Per-line cancellation, and reducing an open quantity instead of zeroing it. The service method already works a line at a time, so a line-scoped entry point is a small addition; reinstatement is not, since the released ordered supply and its inventory transactions would have to be recreated. | `PurchOrderService.CancelRemainderAsync`, `PurchInventorySupplyService.CancelRemainingAsync`, `PurchTableController` | Medium |
| **No purchase price or discount trade agreements** | `PurchOrderService.AddLineCoreAsync` (lines 161-162) takes `PurchPrice` straight from `input.PurchPrice` and computes `LineAmount = PurchQty * PurchPrice / priceUnit`. There is **no `PriceDiscTable` reference anywhere under `AccountsPayable/`** (verified by grep), and no line, multiline or total discount. The sales side resolves all of this: `SalesOrderLineService` line 509 and `SalesOrderDiscountService` lines 55-96 apply agreements and line/multiline/end discount. | A purchase line defaults its price and discount from the vendor's purchase trade agreements, by vendor, vendor group or all, with quantity breaks and date validity. [Purchase price and discount agreements](https://learn.microsoft.com/en-us/dynamics365/supply-chain/procurement/purchase-prices-discounts) | Purchase-side agreement resolution. **The table needs no change**: `PriceDiscTable` is the shared D365 table and already carries `Module` (`ModuleInventCustVend`) and `Relation` (`PriceType`) to distinguish purchase from sales agreements. `PriceDiscJournalPostingService` (lines 126-135) handles only the `Cust` and `Invent` group axes, so the journal needs the `Vend` axis too. This also makes vendor cash discount and the invoice **price** match (row below) meaningful: with no agreed price, there is nothing to match an invoice price against. | `PurchOrderService.AddLineCoreAsync`, `PriceDiscJournalPostingService`, `PriceDiscTable` | **High** — a purchase order currently trusts whatever price the caller sends |
| Payment proposal | Payments are entered and posted per invoice. | A payment proposal selects open invoices by due date, discount date or vendor and creates the payment lines. | The proposal generator. | `VendPaymentService` | **High** — core basic AP function |
| Settlement marking, priority and cash discount on *Settle open transactions* | **Implemented 2026-11-03** - see the *Changelog*. `POST api/v1/vendor-settlements/settle-open-transactions` settles marked open vendor transactions against each other, so a vendor credit note now has an exit. What remains, mirroring the receivable side: no stored marking table, settlement order fixed to oldest due date first, and no cash discount on this path. AP additionally has **no `SpecTrans` equivalent at all**, and `VendSettlement` is still the reduced non-D365 shape its own remarks describe (`InvoiceTransRecId`/`PaymentTransRecId` rather than the Trans/Offset pair, and no reporting-currency columns). | D365 stores the marking, offers a settlement priority, and can take a cash discount when settling. | Stored marking, priority configuration, cash discount, and aligning `VendSettlement` with `CustSettlement`. | `VendSettleOpenTransactionsService`, `VendSettlement`, `Foundation/CashDiscount` | Medium |
| Prepayments / prepayment invoices | Not present. | A prepayment is posted against a purchase order and applied to the final invoice. | Prepayment flow and application. | Purchase order, vendor invoice | Medium |
| Invoice approval journal | Pending vendor invoices exist; no approval journal. | The invoice approval journal posts an approved pending invoice to the ledger. | The approval journal. Workflow-based approval is out of scope, but the journal itself is basic. | `VendInvoiceInfoService` | Medium |
| Vendor aging | `VendTrans`/`VendTransOpen` carry due dates. | Aging buckets and a vendor aging report/inquiry. | Aging period definitions and the report. | `VendTransReadService` | Medium |
| Purchase requisitions / RFQs | Not present. | Arguably beyond basic AP; requisitions and RFQs are separate procurement processes. | — | — | Low — confirm scope before building |
| SQL Server verification | Covered by SQLite in-memory tests. | — | Posting atomicity, concurrent invoice/payment posting, number-sequence contention. | Whole module | **Critical** |

## Deliberate restrictions (verified in code)

| Restriction | Message |
| --- | --- |
| Multi-order pending invoice | `A pending vendor invoice covering several purchase orders is not supported in the basic scope.` |
| Cancelling an order with nothing left | `The purchase order has no unreceived quantity left to cancel.` |
| Cancelling an already cancelled order | `The purchase order is already canceled.` |
| Cancellation with inconsistent ordered supply | `Ordered inventory for item {itemId} is {openQty}, which does not match the uninvoiced remainder {remaining}; the purchase line cannot be cancelled.` |
| Settling across currencies | `Marked transactions must share one currency; settling across currencies realizes an exchange difference, which this path does not post.` |
| Settling across exchange rates | `Marked transactions were posted at different exchange rates; settling them realizes an exchange difference, which this path does not post.` |
| Settling one side only | `Settling open transactions needs at least one invoice and one credit note or payment; marked transactions are all on the same side.` |
| Settling a transaction that is not open | `Transaction {recId} is not an open transaction of vendor '{account}'.` |
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
| 2026-11-03 | **Implemented *Settle open transactions*** on the payable side, so a vendor credit note can be applied to an invoice without a new payment. `VendSettleOpenTransactionsService` follows `VendPaymentService`'s own sign and record-id conventions exactly - `VendSettlement.AmountCur` as a positive magnitude, the invoice side decrementing `SettleAmountCur` and the offsetting side incrementing it - so **the existing `VendSettlementRestore` reverses these rows unchanged**, proven by a round-trip test rather than assumed. No ledger voucher is produced, for the same reason the receivable side produces none. Mixed currencies and mixed exchange rates are refused by name. See `AccountsReceivable/GAPS.md` for the shared design. |
| 2026-11-03 | **Implemented purchase order cancellation**, which did not exist in any form: `PurchStatus.Canceled` was read by nine call sites and written by none, so an unreceived remainder held `InventSum.Ordered` supply permanently. New `POST {recId}/cancel` on `PurchTableController` -> `PurchOrderService.CancelRemainderAsync`, with the inventory effect in `PurchInventorySupplyService.CancelRemainingAsync` as the exact reverse of the Ordered receipt `CreateAsync` writes - symmetric with the sales side, where the inventory effect lives in the demand service. **Received quantity stays invoiceable**: the physical remainder goes to zero and the financial remainder loses the same unreceived quantity, leaving the received-but-uninvoiced part behind. Order 100, receive 60, cancel -> remainders 0 and 60, and that 60 still invoices. A line that received nothing becomes `Canceled` and so drops out of the header derivations, which already excluded cancelled lines. **No ledger entry and no fiscal period check**: ordered supply is neither physical nor financial inventory, so releasing it has no accounting effect. A line whose ordered inventory transactions disagree with its remainder is **refused by name** and the whole cancellation rolls back, rather than releasing a quantity that might belong to a receipt. Covered by four tests in `Tests/InventoryCycleTests.cs`; the inventory cycle suite is 93/93. Line-level cancellation and reinstatement remain open at Medium. |
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
