# Exempt sale

Implemented scenario `04`, run by `TaxTransactionExamples` after scenario 03.

- **Procedure:** a free text invoice `TXV2-{year}-04` to `ARV2-{company}-LOCAL` (sales tax group `ARV2`, whose `APV2-EX` row is `ExemptTax = Yes` with exempt code `VATEX-SA29`), one line of 2 × 200 with item sales tax group `APV2-EXMPT`. Posted through `IFreeTextInvoiceService`.
- **Expected:** one `TaxTrans` with code `APV2-EX`, direction `TaxExemptSales`, base -400 and tax 0. Revenue 481003 is credited 400; payable sales tax 281004 is unchanged. No inventory moves.
- **Rerun:** the invoice is resolved by name, so a rerun verifies and does not repost.

See [VAT treatments](../../VAT-TREATMENTS.md).
