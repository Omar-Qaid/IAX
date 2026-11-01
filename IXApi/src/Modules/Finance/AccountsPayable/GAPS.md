# Accounts payable — gap analysis against basic D365 F&O

**Last updated:** 2026-11-01
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
| Purchase orders (`PurchTable`/`PurchLine`), confirmation | Implemented |
| Product receipt (physical update, accrual posting, `VendPackingSlipJour`/`Trans`) | Implemented |
| Vendor invoice (financial update, 3-way quantity match to the receipt, charges, tax) | Implemented |
| Pending vendor invoice (`VendInvoiceInfoTable`) | Implemented |
| Vendor payments, payment reversal, settlement reversal, cash discount | Implemented |
| Purchase returns / vendor credit notes | Implemented |
| Vendor transactions (`VendTrans`, `VendTransOpen`) | Implemented |
| Physical and financial inventory value through the receipt/invoice pair | Implemented |
| SQL Server behavior (atomicity, concurrency, idempotency) | **Unverified** — SQLite tests only |

## Open gaps

| Area | Existing behavior | Expected basic D365 F&O behavior | Missing | Affected components | Priority |
| --- | --- | --- | --- | --- | --- |
| Foreign currency | Vendor invoice, credit note and payment flows require the document currency to equal the ledger accounting currency, and the reporting currency to equal the accounting currency. Both are rejected with named errors. | A purchase order may be in any currency; the invoice posts at the exchange rate on the invoice date, and realized gain/loss is posted at settlement. | Exchange-rate conversion at posting, and exchange-rate gain/loss on settlement. | `VendorInvoiceService`, `PurchReturnService`, `VendPaymentService`, `Foundation/Currency` | **Critical** — this is core basic AP, and the restriction currently blocks any multi-currency vendor |
| Price variance on credit notes | A credit amount differing from the original receipt cost is rejected with a named error. | The difference posts as a purchase price variance per the posting profile. | Variance calculation and posting. | `PurchReturnService`, `InventPosting` | Medium |
| Pending invoice across several orders | A pending vendor invoice covering more than one purchase order is rejected with a named error. | One invoice may cover lines from several purchase orders. | Multi-order pending invoice composition. | `VendInvoiceInfoService` | Medium |
| Invoice matching tolerances | Quantity matching to the product receipt is implemented via `VendInvoicePackingSlipQuantityMatch`. | Basic AP also supports price and total matching with configurable tolerances, and matching policy per vendor or item. | Price/total matching and tolerance configuration; matching policy setup. | `VendorInvoiceService`, `AccountsPayableParameters` (absent) | **High** |
| Accounts payable parameters | `VendParameters` exists with the single-row-per-company shape of `CustParameters`, and holds the company's default posting profile, which vendor summary account resolution now reads. | `VendParameters` holds company-wide AP defaults (number sequences, matching policy, invoice defaults, posting controls). | The remaining defaults: matching policy and tolerances, invoice defaults, number sequence groups. Each is added with the flow that reads it — a setup field nothing reads is an invented one. Purchase order and trade agreement defaults are **not** missing from here: in D365 they live in `PurchParameters`. | `VendParameters/*`, `Common/Posting/SubledgerPostingSupport.cs` | ~~High~~ **Medium** — the table and the posting-profile default exist; the rest follows its consumers |
| Payment proposal | Payments are entered and posted per invoice. | A payment proposal selects open invoices by due date, discount date or vendor and creates the payment lines. | The proposal generator. | `VendPaymentService` | **High** — core basic AP function |
| Payment methods | No vendor method of payment setup. | `VendPaymModeTable` defines payment method, account, file format and posting behavior. | The entity and its setup. AR has `CustPaymMode`; AP has no counterpart. | `AccountsPayable/Payment` | **High** — asymmetric with AR |
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

## Flagged uncertainties

- **`VendParameters` field names.** The table now exists with `VendPostingProfile`, named after the
  receivable side's `CustParameters.CustPostingProfile`, which is the repository's convention. In
  D365 metadata the field is believed to be `PostingProfile` on both tables, named after the EDT
  rather than the EDT itself — **unverified against target metadata**. The remaining fields are
  still built only when an implemented flow reads them.
- **Purchase requisition and RFQ scope.** These are part of Procurement and sourcing rather than
  Accounts payable. Treated here as out of basic AP scope; confirm before building.
- **Payment file formats.** Method-of-payment setup implies electronic payment file generation,
  which is likely beyond basic scope. The setup entity is still basic.

## Changelog

| Date | Change |
| --- | --- |
| 2026-11-01 | `VendParameters` added (entity, configuration, DTO, validator, service, controller, migration `AddVendParameters`), holding the company's default vendor posting profile. `ResolveVendorSummaryAsync` now narrows `VendLedgerAccounts` to that profile before applying Table -> Group -> All, which is what the customer path has always done: without it, a company with two posting profiles resolved whichever rule matched first. A configured profile with no rules of its own still falls back to every rule, as on the receivable side. Covered by `Tests/VendParametersPostingProfileTests.cs`. |
| 2026-11-01 | Posting now passes the vendor scope to inventory posting resolution, so an `InventPosting` rule configured for one vendor or vendor group is applied instead of silently falling back to the All rule. Fix is in `Common`. |
| 2026-11-01 | File created from a source scan. Flagged the missing AP parameters and method-of-payment entities as asymmetries with AR, and foreign-currency support as the critical gap. |

## Sources

- [Accounts payable home page](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/accounts-payable)
- [Vendor invoices overview](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/vendor-invoices-overview)
- [Invoice matching](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/invoice-matching)
- [Vendor payments](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/vendor-payments-wn)
- [Physical and financial updates](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/physical-financial-updates)
