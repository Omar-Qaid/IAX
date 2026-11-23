# GL-02 fifteen-step reference cycle

Implemented scenario `02`, executed by `GeneralLedgerReferenceCycleFlow` through the services that own each document. It is §2 of `Logic_Readonly.md`.

## Data

Stocked item `IV2-{year}-GLREF` (its own item, so the inventory close covers only this cycle), the shared non-stocked service `IV2-{year}-SVC`, vendor `APV2-{company}-LOCAL`, customer `ARV2-{company}-LOCAL` and bank `APV2-BANK`. Every document name derives from `GLV2-{year}-02`. Dates run from February 1 to February 28 of the fiscal year.

## Steps

| Step | Service | Result |
| --- | --- | --- |
| 1-3 | Purchase order, confirmation, product receipt, vendor invoice | Buy 100 at 50. Inventory 5,000 against the vendor 5,000. |
| 4 | Sales order and lines | 30 of the stocked item at 90, plus the 300 service. |
| 5 | Reservation | Reserve 30. On hand 100, available 70. |
| 6-7 | Packing slip, sales invoice | Deliver and invoice 20. Customer 1,800, cost of sales 1,000. |
| 8-9 | Packing slip, sales invoice | Deliver and invoice 10 with the service. Customer 1,200, cost of sales 500. The service posts no inventory transaction. |
| 10-12 | Customer payment, then settlement | Collect 2,000 (settles 1,800 and 200), then 1,000. The two invoices close at zero. |
| 13 | Vendor payment | Pay 2,000 against the invoice. 3,000 stays open. |
| 14 | Inventory close | One purchase price, so no adjustment. 70 remain at 50 = 3,500. |
| 15 | General journal | Debit maintenance expense `681001` 300, credit the accrual liability `281002` 300. |

Customer receipts are posted on account and then settled explicitly against this scenario's two invoices. The demo customer is shared with the AR examples, so automatic settlement would otherwise pick up their open invoices.

The seeded chart has no separate accrued-expenses account. The accrual uses `281002`, the purchase accrual liability. The receipt and the vendor invoice net to zero on that account, so its balance at the end is exactly the 300 accrual.

## Expected result (§49 trial balance)

The balances are computed over this scenario's vouchers only, because the seeded company also holds the other examples' postings.

| Account | Balance |
| --- | --- |
| Bank `181001` | 1,000 debit |
| Inventory `181002` | 3,500 debit |
| Cost of goods sold `681004` | 1,500 debit |
| Maintenance expense `681001` | 300 debit |
| Vendor control `281001` | 3,000 credit |
| Sales revenue `481003` | 3,000 credit |
| Accrual liability `281002` | 300 credit |
| Customer control `181006` | 0 |

The totals are 6,300 debit and 6,300 credit. Net profit is 1,200: revenue 3,000, less cost of sales 1,500 and expense 300.

## Not covered here

The §5 year-end close, which carries the 1,200 to retained earnings and opens the next year at 4,500, closes the year for the whole company. It would also close the year for every other example, so it is not seeded. `General_ledger_reference_cycle_balances_at_six_thousand_three_hundred_and_carries_twelve_hundred_to_equity` in `Tests/GeneralLedgerCycleTests.cs` still proves it.

## Rerun

The scenario key is `ERPV2.GL.{year}.02`. Each step first looks for its document by name (purchase order, sales order, packing slips, invoices, payment request IDs, the close and the accrual journal) and posts only what is missing. A completed run is verified again and nothing is reposted.
