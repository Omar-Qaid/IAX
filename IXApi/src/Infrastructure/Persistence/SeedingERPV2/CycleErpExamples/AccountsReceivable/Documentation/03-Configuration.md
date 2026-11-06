# Accounts Receivable configuration

Every row below is written by its owning module seeder and verified before commit.

| Setup | What it decides | Key |
|---|---|---|
| Sales account structure | Which main accounts a sales posting may use | `ARV2` |
| Customer control account | Where the receivable is carried | 181006 |
| Delivered-not-invoiced interim | Cost of delivered units before invoicing | 181007 |
| Customer prepayments | Where an unapplied collection sits | 281003 |
| Payable sales tax | Where output tax is carried | 281004 |
| Delivered not invoiced | Offset of the delivery cost entry | 281005 |
| Sales revenue | Where the invoice recognizes revenue | 481003 |
| Cost of goods sold | Where the invoice recognizes cost | 681004 |
| Sales cash discount | Where an early-settlement discount is taken | 681005 |
| Output tax code and group | 15 percent, fully payable | `ARV2-15` / `ARV2` |
| Customer tax group | Which codes apply to a customer | `ARV2` |
| Customer group | Customer defaults and the posting profile key | `ARV2` |
| Customer posting profile | Summary and clearing accounts per group | `ARV2` |
| AR parameters | The default posting profile | one row per company |
| Sales parameters | Default order type and pricing date | one row per company |
| Collection method | Bank account type and its journal | `ARV2` |
| AR journal names | Invoice journal and customer payment journal | `ARV2-INV` / `ARV2-PAY` |
| Sales cash discount | 2 percent in 10 days | `ARV2-2` |
| Number sequences | Customer, order, confirmation, packing slip, free text and invoice IDs | `ARV2-*` and the document tables |
| Sales inventory posting | Delivery, issue, cost, revenue and discount accounts | item group `APV2` |
| Return handling | One reason code and one crediting disposition | `ARV2-WRONG` / `ARV2-RETURN` |

## Posting rules this configuration implies

- The receivable comes from the customer posting profile, searched customer then group then all.
- Sales revenue and cost come from the sales inventory posting rules for the item group.
- A free text invoice takes its revenue account from its own line, not from inventory posting.
- The bank side comes from the collection method and the selected bank account.

## Pricing date

Sales parameters price on the delivery date rather than today, so a back-dated example order prices
from its own trade agreement window instead of from whenever the example happens to run.
