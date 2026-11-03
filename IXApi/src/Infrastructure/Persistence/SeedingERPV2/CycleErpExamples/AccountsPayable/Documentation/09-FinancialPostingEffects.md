# Financial posting examples

All amounts are SAR unless specified. These are simplified net accounting expectations. Resolve actual interim accounts and additional lines from posting setup. Positive payable amounts below mean liability outstanding; database signs may differ.

## Baseline: 100 units at 100; illustrative fully recoverable 15% tax

| Event | Debit | Credit |
|---|---|---|
| Receipt, accrual enabled | Receipt asset 10,000 | Receipt accrual 10,000 |
| Invoice: reverse receipt | Receipt accrual 10,000 | Receipt asset 10,000 |
| Invoice: recognize cost/tax/vendor | Inventory 10,000; input tax 1,500 | AP control 11,500 |
| First payment | AP control 6,000 | Bank 6,000 |
| Final payment | AP control 5,500 | Bank 5,500 |

Final result: inventory 10,000 debit; input tax 1,500 debit; bank 11,500 credit; no remaining AP or receipt accrual for this scenario.

## Partial processing

| Stage | Physical units | Invoiced units | Uninvoiced receipt net value | Gross invoiced liability before payment |
|---|---:|---:|---:|---:|
| Receive 60 | 60 | 0 | 6,000 | 0 |
| Invoice 40 | 60 | 40 | 2,000 | 4,600 |
| Receive another 40 | 100 | 40 | 6,000 | 4,600 |
| Invoice another 60 | 100 | 100 | 0 | 11,500 |

## Direct service invoice

For net 1,000 plus illustrative tax 150: debit expense 1,000 and input tax 150; credit AP 1,150. Payment debits AP and credits bank 1,150. No inventory entry.

## Return example

A supported return of 10 originally purchased units at 100 yields a 1,000 net credit plus 150 tax credit: debit AP 1,150; credit inventory 1,000 and input tax 150, with required physical/interim reversal lines. Exact inventory cost can differ under the actual costing model; use supported original-cost linkage and report adjustments.

## Foreign currency example

Invoice USD 1,000, no tax in this isolated FX example, at 3.75 SAR/USD: expense 3,750 debit, AP 3,750 credit. Pay USD 1,000 at 3.80: bank credit 3,800; clear AP historical 3,750; recognize realized FX loss 50. Depending on settlement path the loss may be posted in a separate voucher. Assert aggregate effect and actual voucher links.
