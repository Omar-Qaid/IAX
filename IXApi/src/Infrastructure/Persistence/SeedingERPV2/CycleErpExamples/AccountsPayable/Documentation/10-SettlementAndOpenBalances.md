# Payments, settlement and open balances

## Independent operations

Posting payment creates a vendor payment transaction. Settlement applies amounts between eligible transactions. Payment posting does not guarantee that a selected invoice is settled.

| Event | Invoice remaining | Payment remaining | Net vendor liability |
|---|---:|---:|---:|
| Invoice posted | 11,500 | 0 | 11,500 |
| Pay 6,000, no settlement | 11,500 | 6,000 unapplied credit | 5,500 |
| Apply 6,000 | 5,500 | 0 | 5,500 |
| Pay and apply 5,500 | 0 | 0 | 0 |

Historical transactions remain. Open-record representation may be zero rows or zero remaining amount depending on the application; validate business balance rather than imposing a physical-row convention.

## Required checks

Validate vendor, invoice account, company, currency, amount and settlement date. No allocation exceeds either available remaining amount. Partial applications preserve the balance. Discount and exchange adjustments use configured accounts and supported tax treatment. Support concurrent settlement protection.

## Advances

PO prepayment invoices and payment-journal advances are distinct paths. A simplified tax-free advance payment debits vendor advance 2,000 and credits bank 2,000. Applying it to a final 10,000 invoice debits AP 2,000 and credits vendor advance 2,000; remaining payable 8,000. Use actual tax/prepayment posting services for the taxable scenario and do not treat the tax-free illustration as statutory behavior.

## Cash discount example

For an isolated no-tax invoice 10,000 and eligible 2% discount, bank payment 9,800 plus discount 200 closes AP 10,000. Debit AP 10,000, credit bank 9,800 and discount account 200. Add a separate tax-sensitive case only with explicitly configured tax recalculation expectations.
