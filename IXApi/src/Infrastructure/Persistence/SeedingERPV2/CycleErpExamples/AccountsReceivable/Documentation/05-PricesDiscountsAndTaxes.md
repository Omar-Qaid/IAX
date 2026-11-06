# Sales prices, discounts and taxes

| Customer | Item | Currency | Price | Purchase cost | Margin |
|---|---|---|---:|---:|---:|
| LOCAL | `APV2-STOCK` | SAR | 40 | 25 | 15 |
| LOCAL | `APV2-SVC` | SAR | 500 | 350 | 150 |
| USD | `APV2-STOCK` | USD | 11 | 7 | 4 |

Each quote is valid for the whole example fiscal year, from quantity 1, in the price unit 1, for the
example inventory dimension.

Output tax is 15 percent, fully payable, on the net line amount. The shared item tax group holds
both the purchase and the sales code; the customer tax group selects only the sales code, so the
group intersection resolves exactly one rate per sales line.

The cash discount is 2 percent within 10 days, posted to its own sales discount account. Whether a
cash discount also adjusts the output tax is a parameter this codebase does not have, so the example
takes the discount without recalculating tax and says so rather than implying a choice was made.

Each SAR quote is validated to sit above the purchase price of the same item. That is not a D365
rule; it is what makes the revenue and cost accounts distinguishable in the reconciliation.
