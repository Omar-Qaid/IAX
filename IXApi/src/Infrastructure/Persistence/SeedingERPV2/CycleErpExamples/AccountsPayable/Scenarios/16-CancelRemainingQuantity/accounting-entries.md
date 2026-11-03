# Accounting and physical effects

## Expected net result

Received/invoiced remain 60; receipt obligation 0; cancelled quantity 40; invoice total 6,900; no reversal of posted 60.

## Posting specification

Apply [Financial posting effects](../../Documentation/09-FinancialPostingEffects.md) and [Physical effects](../../Documentation/08-PhysicalInventoryEffects.md) for the relevant path. Resolve actual accounts from configuration; do not substitute a hardcoded account to balance a failing voucher.

For every posted step capture accounting date, voucher, account role and actual main account, debit/credit in transaction and accounting currency, source document and any reversing reference. Include intermediate lines and tax/FX/discount effects used by the real implementation. Pending or rejected operations must not create posted financial effects.

Distinguish physical quantity, financial inventory value, vendor net liability and gross open documents. A net-zero vendor balance does not prove settlement. Verify sum of debits equals credits and reconcile all effects to the source input.
