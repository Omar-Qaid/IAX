# Sales transaction flow

Each step is executed by the service that owns the document. The flow resolves what already exists
before posting again, so a rerun resumes.

| Step | Service | Document |
|---|---|---|
| Create order | `ISalesOrderQuickCreateService` | `SalesTable` |
| Add line | `ISalesOrderLineService` | `SalesLine`, `InventTransOrigin`, `InventTrans` |
| Confirm | `ISalesOrderConfirmationService` | `CustConfirmJour` |
| Deliver | `IPackingSlipJournalService` | `CustPackingSlipJour`, `CustPackingSlipTrans` |
| Invoice | `ISalesInvoiceService` | `CustInvoiceJour`, `CustInvoiceTrans`, `CustTrans`, `CustTransOpen` |
| Free text invoice | `IFreeTextInvoiceService` | `CustInvoiceTable`, `CustInvoiceLine`, then the posted journal |
| Collect | `ICustPaymentService` | `LedgerJournalTable`, `LedgerJournalTrans`, `CustTrans`, `BankAccountTrans` |
| Settle | `ICustSettleOpenTransactionsService` | `CustSettlement`, `CustTransOpen` |
| Return | `ICustReturnService` | return `SalesTable`, receipt, credit note |
| Cancel remainder | `ISalesOrderCancellationService` | `SalesLine` remaining quantities |
| Reverse settlement | `ICustSettlementReversalService` | `CustSettlement`, `CustTransOpen` |
| Reverse payment | `ICustPaymentReversalService` | reversing journal, `CustTrans`, `CustTransOpen` |

## What each step does and does not do

- Creating and confirming an order raises no receivable and issues no stock.
- Delivery issues the stock physically and may post its cost, but raises no receivable.
- The invoice raises the receivable, recognizes revenue and tax, updates inventory financially and
  reverses what the delivery posted.
- A collection reduces the open receivable and moves the bank; it creates no revenue.
- Settlement allocates and closes open amounts; it collects nothing new.
- A return receipt brings stock back; only the credit note reduces the customer debt.
