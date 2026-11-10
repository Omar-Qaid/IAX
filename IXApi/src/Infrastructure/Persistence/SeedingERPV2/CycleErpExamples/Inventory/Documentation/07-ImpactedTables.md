# 07 — Impacted tables

| Table | Written by | Read by the checks |
| --- | --- | --- |
| `InventTrans` | receipt, invoice, delivery, close | quantity, value, cost of goods sold, adjustment, origin |
| `InventTransOrigin` | the document services | `TransactionOriginValidator` |
| `InventSum` | `InventoryMovement` | `InventSumConsistencyValidator`, `ReservationValidator` |
| `InventSettlement` | `InventCloseService` | `InventSettlementValidator` |
| `InventClosing` | `InventCloseService` | `CloseAdjustmentValidator`, rerun detection |
| `VendPackingSlipJour` / `Trans` | `IProductReceiptService` | rerun detection |
| `VendInvoiceJour` / `Trans` | `IVendorInvoiceService` | rerun detection |
| `CustPackingSlipJour` | `IPackingSlipJournalService` | rerun detection |
| `CustInvoiceJour` | `ISalesInvoiceService` | invoice total |
| `GeneralJournalEntry` / `GeneralJournalAccountEntry` | the subledger posting engine | ledger reconciliation and balance |

`InventSum` is a maintained running total, not a view. A service that updated a movement but not the summary would
leave the two silently apart and every later availability check reading the wrong number, which is why
`InventSumConsistencyValidator` compares them rather than trusting either alone.
