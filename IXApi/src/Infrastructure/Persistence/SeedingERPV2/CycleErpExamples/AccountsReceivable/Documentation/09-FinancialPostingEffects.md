# Financial posting effects

All amounts are SAR unless stated. These are the net expectations the reconciliation checks assert;
resolve interim accounts and additional lines from the posting setup.

## Baseline: 100 units at 40, cost 25, output tax 15 percent

| Event | Debit | Credit |
|---|---|---|
| Delivery, cost posting enabled | Delivered-not-invoiced interim 2,500 | Inventory 2,500 |
| Invoice: reverse the delivery | Inventory 2,500 | Delivered-not-invoiced interim 2,500 |
| Invoice: customer, revenue, tax | Customer control 4,600 | Revenue 4,000; output tax 600 |
| Invoice: cost of goods sold | Cost of goods sold 2,500 | Inventory 2,500 |
| Collection | Bank 4,600 | Customer control 4,600 |

Final result: revenue 4,000 credit; output tax 600 credit; cost of goods sold 2,500 debit; inventory
2,500 credit; bank 4,600 debit; no remaining receivable and both delivery interim accounts at zero.

## Partial processing

| Stage | Issued units | Invoiced units | Open receivable |
|---|---:|---:|---:|
| Deliver 60 | 60 | 0 | 0 |
| Invoice 40 | 60 | 40 | 1,840 |
| Deliver another 40 | 100 | 40 | 1,840 |
| Invoice another 60 | 100 | 100 | 4,600 |

## Service sale with no inventory

Two maintenance visits at 500: customer control 1,150 debit; revenue 1,000 credit; output tax 150
credit. No inventory or cost entry at any step.

## Free text invoice

One advisory line at 350 plus tax 52.50: customer control 402.50 debit; revenue 350 credit; output
tax 52.50 credit. The revenue account comes from the invoice line, and there is no `InventTrans`.

## Cash discount

On a 4,600 invoice settled within 10 days at 2 percent, the customer pays 4,508: bank 4,508 debit;
sales cash discount 92 debit; customer control 4,600 credit. The output tax is not recalculated,
because no parameter in this codebase asks for that.

## Return of 10 units

A 10-unit return of the baseline invoice credits 400 net plus 60 tax: revenue 400 debit; output tax
60 debit; customer control 460 credit, with the cost and physical reversal lines the disposition
requires. The exact inventory cost can differ under the costing model; the original invoice line is
the link that decides it.

## Foreign currency

Sell 100 at USD 11 with the rate 3.75: customer control 4,125 debit at the invoice date rate. Collect
USD 1,100 at 3.80: bank 4,180 debit; clear the historical 4,125; recognize a realized exchange gain
of 55. Depending on the settlement path the difference may land in its own voucher, so assert the
aggregate effect and the actual voucher links.
