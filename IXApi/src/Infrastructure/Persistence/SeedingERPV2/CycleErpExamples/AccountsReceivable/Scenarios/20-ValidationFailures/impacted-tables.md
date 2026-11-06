# Impacted tables

The rejected call must leave CustTrans, GeneralJournalEntry, GeneralJournalAccountEntry, AccountingDistribution, SubledgerJournalEntry, SourceDocumentHeader and SourceDocumentLine at their prior counts.

Trace through the relationships in [Impacted tables](../../Documentation/07-ImpactedTables.md); do not join on InvoiceId or Voucher alone.
