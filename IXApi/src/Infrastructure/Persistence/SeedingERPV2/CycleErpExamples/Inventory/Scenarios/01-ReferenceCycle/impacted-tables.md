# Scenario 01 — Impacted tables

| Table | Rows this scenario creates |
| --- | --- |
| `PurchTable` / `PurchLine` | 2 orders, 1 line each |
| `VendPurchOrderJour` | 2 confirmations |
| `VendPackingSlipJour` / `VendPackingSlipTrans` | 2 product receipts |
| `VendInvoiceJour` / `VendInvoiceTrans` | 2 vendor invoices |
| `VendTrans` / `VendTransOpen` | 2 open vendor invoices |
| `SalesTable` / `SalesLine` | 1 order, 2 lines (one non-stocked) |
| `CustPackingSlipJour` / `CustPackingSlipTrans` | 2 packing slips |
| `CustInvoiceJour` / `CustInvoiceTrans` | 2 sales invoices |
| `CustTrans` / `CustTransOpen` | 2 open customer invoices |
| `InventTransOrigin` | 1 per purchase line and sales line |
| `InventTrans` | 2 receipts, 2 issues — none for the service item |
| `InventSum` | 1 summary for the stocked item |
| `InventSettlement` | cost settlements from the close |
| `InventClosing` | 1 completed close |
| `GeneralJournalEntry` / `GeneralJournalAccountEntry` | one voucher per posting above |

Nothing is written to `InventTrans` for `IV2-{year}-SVC`. That absence is asserted, not assumed: a movement raised
for the service would be absorbed into the stocked item's period average and change the closing value.
