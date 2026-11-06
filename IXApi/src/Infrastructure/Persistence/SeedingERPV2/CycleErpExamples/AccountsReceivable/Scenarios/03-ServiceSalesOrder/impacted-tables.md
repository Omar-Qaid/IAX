# Impacted tables

SalesTable, SalesLine, CustConfirmJour, CustPackingSlipJour/Trans, CustInvoiceJour/Trans, CustTrans, CustTransOpen, CustSettlement, BankAccountTrans, TaxTrans, GL. No InventTrans.

Trace through the relationships in [Impacted tables](../../Documentation/07-ImpactedTables.md); do not join on InvoiceId or Voucher alone.
