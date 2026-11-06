# Accounts Receivable master data

Two fictional customers, one domestic in SAR and one foreign in USD, both in the customer group
`ARV2` and both collected through the company bank.

| Data | LOCAL | USD |
|---|---|---|
| Account | `ARV2-{company}-LOCAL` | `ARV2-{company}-USD` |
| Name | Al Noor Trading Company (Demo) | Gulf Paper Distribution (Demo) |
| Currency | SAR | USD |
| Payment terms | `APV2-N30` | `APV2-N30` |
| Collection method | `ARV2` | `ARV2` |
| Tax group | `ARV2` | `ARV2` |
| Premises | 7100 Prince Mohammed Bin Abdulaziz Road | 7200 Takhassusi Street |

Each customer has a party record, a location, a primary postal address in the Riyadh geography the
purchase example already seeded, a primary email, and a shipping site and warehouse.

The invoice account equals the order account for both customers. The two can differ in D365, and
the sales services honour the difference, but this example does not need a third party to carry
another customer debt and does not invent one.

The released items are the shared demo items, not new ones: the stocked paper ream and the
non-stocked maintenance visit, both already configured by the inventory seeders.

There is no customer bank account master, because the repository has no `CustBankAccount` entity;
collections land on the company bank account instead.
