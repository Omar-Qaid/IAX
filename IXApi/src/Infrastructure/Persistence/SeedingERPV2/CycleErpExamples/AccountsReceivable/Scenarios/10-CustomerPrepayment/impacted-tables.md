# Impacted tables

As scenario 01, plus the prepayment's `LedgerJournalTrans` (`Prepayment = Yes`), its `CustTrans` posted with posting profile `ARV2-PRE`, and the `CustSettlement` pair that applies it to the invoice.

Trace through the relationships in [Impacted tables](../../Documentation/07-ImpactedTables.md); do not join on InvoiceId or Voucher alone.
