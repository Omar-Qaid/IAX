# Accounting and physical effects

## Expected net result

Prepayment invoice: supplier advance 181005 debit 1,000, vendor 281001 credit 1,000; paid from the bank. Final invoice 2,500 (no sales tax). Application moves the 1,000 from 181005 to the vendor. Net: inventory 2,500; bank -2,500 (1,000 + 1,500); supplier advance 0; vendor 0; nothing open.

## Posting specification

Apply [Financial posting effects](../../Documentation/09-FinancialPostingEffects.md) and [Physical effects](../../Documentation/08-PhysicalInventoryEffects.md) for the relevant path. Resolve actual accounts from configuration; do not substitute a hardcoded account to balance a failing voucher.

For every posted step capture accounting date, voucher, account role and actual main account, debit/credit in transaction and accounting currency, source document and any reversing reference. Include intermediate lines and tax/FX/discount effects used by the real implementation. Pending or rejected operations must not create posted financial effects.

Distinguish physical quantity, financial inventory value, vendor net liability and gross open documents. A net-zero vendor balance does not prove settlement. Verify sum of debits equals credits and reconcile all effects to the source input.
