# Output tax from a free text invoice

Implemented scenario `02`, run by `TaxTransactionExamples` through AR scenario `04`.

- **Procedure:** a free text invoice with one service line, `TaxGroup` `ARV2` and `TaxItemGroup` `APV2`.
- **Expected:** one `TaxTrans` with direction `Outgoing` and code `ARV2-15`. The amounts are negative (sales), and the tax is credited to `281004` (payable sales tax). No inventory moves.
- **Rerun:** the invoice is resolved by name, so a rerun verifies and does not repost.

The zero-rated (`APV2-0`) and exempt (`APV2-EX`) treatments are seeded and covered by `Tests/ZeroRatedTaxTests.cs`. See [VAT treatments](../../VAT-TREATMENTS.md). Running them as seeded documents is still open.
