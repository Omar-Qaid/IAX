# Master data and stable identifiers

## Example records

| Key | Example | Owner |
|---|---|---|
| Company | APDEMO | Organization |
| Local vendor | AP-VEND-001, SAR, NET30 | AP |
| Foreign vendor | AP-VEND-USD, USD | AP |
| Stocked item | AP-ITEM-001, unit EA | Inventory |
| Service/category | AP-SERVICE-001 | Product/Purchasing |
| Site / warehouse | AP-SITE / AP-WH | Inventory |
| Company bank | AP-BANK-SAR | Bank |
| Vendor price | Vendor + item + unit + currency + date range | Pricing |

These identifiers are proposals. Preserve compatible existing project conventions.

## Vendor fields

Account, party/name, vendor group, currency, payment terms/method, tax group, invoice account if supported, blocked status, address and bank details. Use synthetic contact/bank data; do not invent a valid bank integration destination.

## Product fields

Product identity and released item, unit, item group, model group, item tax group, storage dimensions, purchasing defaults and supported inventory valuation method. Service lines must use the actual supported nonstocked/category path.

## Referential integrity

Resolve parents before children. Ensure warehouse belongs to the expected site; vendor belongs to the company; product is released to that company; profile accounts belong to the relevant chart; tax combinations are valid on the posting date.

## Reuse and isolation

Use distinct scenario document references with shared compatible master data. Avoid collisions with existing transactions. Assertions must filter by company, scenario and documents rather than relying on global balances being zero. Rerun validates existing values and reports incompatible changes instead of silently rewriting shared configuration.
