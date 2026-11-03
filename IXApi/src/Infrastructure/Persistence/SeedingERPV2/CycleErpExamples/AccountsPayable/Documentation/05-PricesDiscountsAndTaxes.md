# Prices, discounts, charges and taxes

## Input contract

Every line specifies quantity, unit, currency, price unit, selected unit price, discount inputs, charge treatment, tax groups and posting date. Vendor-price records additionally specify vendor/item scope, validity dates and quantity breaks. Use decimal arithmetic and explicit currency rounding.

## Calculation example

| Calculation | Amount SAR |
|---|---:|
| 100 units × 100 / price unit 1 | 10,000.00 |
| 10% line discount | -1,000.00 |
| Net goods | 9,000.00 |
| Capitalized taxable purchase charge | 200.00 |
| Tax base for this example | 9,200.00 |
| Illustrative tax at 15% | 1,380.00 |
| Total payable | 10,580.00 |

For this example, inventory cost is 9,200 and recoverable tax is separate. Charge taxation and capitalization are explicit assumptions, not universal defaults.

## Implementation rules

Resolve trade prices through the pricing service; apply a manual override only through its supported operation. Preserve the selected price on the document. Define whether percentage discounts apply before fixed discounts and header allocations. Prevent applying the same charge twice at receipt and invoice. Store charge allocations with source links.

Tax rates are illustrative configurable test data, not tax advice. Use tax-service results and date-effective setup. Explicitly document tax-inclusive/exclusive mode, recoverability, per-line versus document rounding and any rounding residual account. Reject missing rates or currencies before posting.

## Assertions

Recalculate expected net, discount, charges, tax and gross independently from the posted totals. Verify partial invoices consume only their allocated amounts and the final invoice handles the supported rounding remainder. Changing a future price record must not modify historical posted amounts.
