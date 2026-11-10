# Scenario 01 — Accounting entries

Amounts are in the accounting currency. Tax is zero-rated throughout, so no tax account appears.

## Purchase receipt (physical)

| Account | Debit | Credit |
| --- | --- | --- |
| 181003 received not invoiced, interim | 1,000 | |
| 281002 received not invoiced | | 1,000 |

## Vendor invoice (financial)

| Account | Debit | Credit |
| --- | --- | --- |
| 281002 received not invoiced | 1,000 | |
| 181003 received not invoiced, interim | | 1,000 |
| 181002 inventory | 1,000 | |
| 281001 vendor control | | 1,000 |

The second purchase repeats both vouchers at 800.

## Sales packing slip (physical issue)

| Account | Debit | Credit |
| --- | --- | --- |
| 181007 delivered not invoiced, interim | 400 | |
| 281005 delivered not invoiced | | 400 |

## Sales invoice (financial issue)

For the 40-piece invoice, at the running cost of 10:

| Account | Debit | Credit |
| --- | --- | --- |
| 181006 customer control | 1,000 | |
| 481003 sales revenue | | 1,000 |
| 681004 cost of goods sold | 400 | |
| 181002 inventory | | 400 |

The second invoice adds the 20 pieces (500 revenue, 200 cost) and the 300 service line, which has no cost side
because it is non-stocked.

## Inventory close

The close moves the difference between the running cost and the period average onto the issues:

| Account | Debit | Credit |
| --- | --- | --- |
| 681004 cost of goods sold | 120 | |
| 181002 inventory | | 120 |

`LedgerBalanceValidator` requires every voucher above to net to zero in the accounting and reporting currency, and
`InventoryValueValidator` and `CostOfGoodsSoldValidator` tie 181002 and 681004 back to the movements.
