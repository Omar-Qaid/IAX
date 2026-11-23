# Zero-rated sale

Implemented scenario `03`, run by `TaxTransactionExamples` after scenarios 01 and 02.

- **Procedure:** a free text invoice `TXV2-{year}-03` to `ARV2-{company}-LOCAL` (sales tax group `ARV2`), one line of 2 × 200 with item sales tax group `APV2-ZERO`, which holds only `APV2-0`. Posted through `IFreeTextInvoiceService`.
- **Expected:** one `TaxTrans` with code `APV2-0`, direction `Outgoing`, base -400 and tax 0. Revenue 481003 is credited 400; payable sales tax 281004 is unchanged. No inventory moves.
- **Rerun:** the invoice is resolved by name, so a rerun verifies and does not repost.

See [VAT treatments](../../VAT-TREATMENTS.md).
