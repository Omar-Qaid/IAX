# Current master data and stable identifiers

The selected legal entity already exists. In company `dat`, the prepared records include:

| Record | Stable key | Details |
|---|---|---|
| Local supplier | APV2-dat-LOCAL | Riyadh Office Supplies and Services (Demo), SAR, APV2-N30 |
| USD supplier | APV2-dat-USD | Gulf Office Equipment Trading (Demo), USD, APV2-N30 |
| Paper ream | APV2-STOCK | Stocked/FIFO, EA, APV2 item/model group |
| Maintenance visit | APV2-SVC | Nonstocked service, EA, APV2-SVC model group |
| Site/warehouse | APV2-SITE / APV2-WH | Riyadh demo storage |
| Inventory dimension | APV2-DIM | The same site/warehouse |
| Company bank | APV2-BANK | SAR, valid configured bank ledger combination |
| Vendor bank | APV2 per supplier | Scoped by vendor account and company |
| Category/hierarchy | APV2-dat | Linked to the service product |

The company portion of supplier/product keys is resolved from the execution context. Other
companies are subject to the existing schema's global alternate-key limitations described in
[PHASE-ONE.md](../../../PHASE-ONE.md). No existing records are silently renamed or overwritten.

Names, premises, contacts and bank numbers are fictional. The manifests in `Data/` must match
module-owned example definitions. Reports contain actual saved RecIds and business keys; no
JSON fixture contains generated database IDs. This phase creates no on-hand balances, invoices
or vendor transactions.
