# Purchasing documentation versus seed logic - 2026-10-27

## Scope and conclusion

Reviewed `src/Modules/Finance/AccountsPayable/CyclePurchorder.md`, `APTables.md`, the historical `accountspayable-backend-refactor-report.md`, `PurchaseScenarioSeeder`, vendor/setup seeders, and current purchase creation, receipt and invoice service code. This is a source comparison; no seed data, services or schema were modified and no new live-database validation was run for this review.

The examples follow the basic purchase document lifecycle, but do not implement or demonstrate the complete documented procure-to-pay logic. Existing referential-integrity results do not establish full purchasing acceptance.

## Comparison

| Documented requirement | Current seed/service behavior | Assessment |
| --- | --- | --- |
| Vendor/product/site/warehouse/unit/posting prerequisites | Dependencies run setup before PurchaseScenarioSeeder; uses VEND-100, ITEM-001, PCS, JED/JED-1, SAR | Aligned basic setup; different identifiers are illustrative differences |
| Separate creation, confirmation, physical receipt and financial invoice | Seeder invokes PurchOrderService, PurchConfirmationService, ProductReceiptService and VendorInvoiceService in order | Aligned; no manually fabricated stock or invoice voucher |
| Open, received and invoiced states | Three confirmed POs: open 10, partial 20 with 8 received and no invoice, full 30 received/invoiced; price SAR 100 | Valid examples, different from blueprint |
| One PO for 100 at SAR 50; receive/invoice 60 then 40 | No example partially invoices the received portion or completes the same PO in a second receipt/invoice | Missing scenario; current invoice code supports splitting received inventory portions but seed does not exercise it |
| 15% VAT; total SAR 5,750 | VendorInvoiceService explicitly excludes tax/charges; invoice amount equals net total | Not aligned with full VAT example; cannot solve by inserting fabricated tax rows |
| Physical accrual and invoice accrual reversal | ProductReceiptService explicitly excludes receipt-accrual GL posting; invoice service excludes reversal | Implementation scope gap despite physical-posting model-group setup |
| Vendor defaults include address, tax, delivery and dimensions | PO creation copies invoice account, currency and payment terms; the inspected initializer does not copy all documented defaults | Partial defaulting |
| Payment terms determine due date | Example vendor uses Monthly; VendorInvoiceService sets VendTrans.DueDate to invoice date | Confirmed service/defaulting discrepancy requiring separate behavior repair, not a direct seed overwrite |
| Purchase prices/discounts | Seeder supplies 100 directly; CommercialScenarioSeeder publishes PriceSales rules | No purchase trade-agreement/discount example; sales pricing is not purchase pricing |
| Receipt/invoice matching | Service limits invoice quantity to received-not-invoiced and selects Received inventory portions | Basic quantity control, not proof of full three-way matching/tolerances or explicit receipt-line match records |
| Vendor payment and settlement | No vendor payment/settlement step in PurchaseScenarioSeeder; the separate payment example is a customer payment | Full procure-to-pay cycle incomplete |
| Purchase returns | PurchReturnService exists, but no vendor-return example is invoked by the seeders | Missing seed scenario; do not describe the service itself as absent |
| Reconfirmation, cancellations, corrections/reversals, over/underdelivery | No such purchase scenarios seeded | Acceptance coverage missing |
| Unit conversion, alternate invoice vendor, multiple lines/currencies | One line, same PCS inventory/purchase unit, same invoice vendor fallback, SAR | Edge-case coverage missing |
| Inventory/vendor/GL reconciliation | Existing seeded invoice total and balanced-voucher checks | Does not prove full VAT/accrual/open-liability/settlement reconciliation |
| Repeatability | RunOnce policy; stable company/marker lookup skips existing orders; normal services own lifecycle effects | Normal reruns protected; an existing partial marker is skipped, not automatically completed |

## Important source pointers

- `PurchaseScenarioSeeder.cs:39-57`: three example quantities and service sequencing.
- `VendorInvoiceService.cs:14-18`: explicit tax/charges/accrual-reversal limits; line 192 assigns invoice-date due date.
- `ProductReceiptService.cs:18-20`: physical update versus excluded receipt-accrual ledger posting.
- `PurchOrderService.cs:81-98`: actual supported header defaults.
- `CyclePurchorder.md:1038-1059`: 100-unit, 60/40, VAT/payment/return acceptance scenario.
- `APTables.md:1546-1676`: vendor settlement, payment and receipt-match structures in the target dictionary.

The older architecture report covers the vendor-maintenance scope at the time it was written; it is not the current purchase lifecycle feature inventory. APTables.md is a functional dictionary, not authority to invent unmapped standard tables or columns.

## Recommended order

1. Add a dedicated supported 60/40 receipt-and-invoice example through current services, preserving posted existing examples.
2. Repair due-date defaulting with the existing payment-term semantics and regression tests.
3. Treat VAT, charges, receipt accrual/reversal, vendor payment/settlement and complete matching as separately scoped implementation work where missing. Add seed scenarios only through the supported business services.
4. Add returns/corrections and negative-path tests, then reconcile the entire document chain against the blueprint amounts.
