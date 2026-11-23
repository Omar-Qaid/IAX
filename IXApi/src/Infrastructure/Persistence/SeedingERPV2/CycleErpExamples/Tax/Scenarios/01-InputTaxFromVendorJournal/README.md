# Input tax from a vendor journal

Implemented scenario `01`, run by `TaxTransactionExamples` through the Foundation transaction examples.

- **Procedure:** the AP vendor journal posts an inclusive-tax line, `TaxGroup` `APV2` and `TaxItemGroup` `APV2`, through `VendorJournalTaxPosting`.
- **Expected:** one `TaxTrans` with direction `Incoming` and code `APV2-15`, reconciled to its `TaxJournalTrans` detail and its source `LedgerJournalTrans`. The input tax posts to `181004` (recoverable purchase tax), and `TaxTransGeneralJournalAccountEntry` links the row to its ledger line.
- **Rerun:** the vendor journal is resolved by key, so a rerun verifies and does not repost.
