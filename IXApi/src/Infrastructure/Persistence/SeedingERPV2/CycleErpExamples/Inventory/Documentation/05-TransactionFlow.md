# 05 — Transaction flow

`InventoryTransactionFlow` sequences the owning services. It never writes a document, a stock movement or an
accounting entry itself.

| Step | Service | Effect |
| --- | --- | --- |
| 1 | `IPurchOrderService` | Purchase order and line, 100 at 10 |
| 2 | `IPurchConfirmationService` | Confirmation |
| 3 | `IProductReceiptService` | Product receipt: physical receipt |
| 4 | `IVendorInvoiceService` | Vendor invoice: financial receipt |
| 5 | `ISalesOrderQuickCreateService` / `ISalesOrderLineService` | Sales order, 60 stocked at 25 and 1 service at 300 |
| 6 | `IInventoryReservationService` | Reserve 60; on hand 100, reserved 60, available 40 |
| 7 | `IPackingSlipJournalService` | Deliver 40, then 20 |
| 8 | `ISalesInvoiceService` | Invoice 40, then 20 plus the service line |
| 9 | `IPurchOrderService` … `IVendorInvoiceService` | Second purchase, 50 at 16, received after both sales |
| 10 | — | Assert the pre-close value of 1,200 at the running cost |
| 11 | `IInventCloseService` | Periodic weighted-average close |

## Resumability

Every document number derives from `ExampleIdentifiers.ScenarioKey`. Each step first looks for its own document and
skips if it is already posted, so a run interrupted halfway resumes instead of posting a duplicate. A completed close
for the scenario's cutoff date is the terminal state: a rerun reconciles it rather than posting a second cycle,
because a second cycle would change the period average that every asserted figure depends on.

## Why delivery uses "All" rather than "Picked"

`PackingSlipJournalService` filters to registered picking lines under the `Picked` selection. This cycle does not run
a picking registration, so it posts `All` with the explicit quantity instead.
