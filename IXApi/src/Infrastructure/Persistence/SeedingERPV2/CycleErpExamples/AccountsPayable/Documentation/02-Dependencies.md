# Build prerequisites first, then reference them

## Required execution order

| Order | Owner | Output referenced by AP |
|---|---|---|
| 1 | Organization/reference | Tenant, legal entity, currency and units |
| 2 | General Ledger | Fiscal year, open periods, chart, accounts and ledger |
| 3 | Tax | Codes, rates, groups and posting setup |
| 4 | Inventory | Model/item groups, posting setup, site, warehouse and released items |
| 5 | Cash and Bank | Company bank and its ledger account |
| 6 | Accounts Payable | Parameters, vendor groups, profiles, terms, methods and vendors |
| 7 | Purchasing/pricing | Vendor prices and scenario orders |
| 8 | AP scenarios | Receipts, invoices, payments and settlements |
| 9 | Validation | Reconciliation evidence |

## Resolution rules

1. Inspect existing seeders and configuration before adding records.
2. Find a compatible existing record by company and stable business key.
3. Create missing prerequisite data through its owning module seeder.
4. Resolve database IDs from those keys at runtime.
5. Validate critical settings, including account type, currency, active status and company.
6. Fail with the missing key and owning seeder if resolution fails.

Do not duplicate GL or inventory records inside the AP folder. Do not hardcode generated RecIds or infer enum values from integer storage. Do not overwrite an existing production account merely because a demo expects another configuration. Create isolated demo setup or report incompatibility.

## Dependency declaration

Each seeder declares stable ID, version and prerequisite IDs. Validate unique IDs, missing references and dependency cycles before any writes. Topologically order dependencies with a stable ID tie-breaker. Include transitive dependencies when executing only the AP module.

An illustrative ID is `inventory.demo.released-products`; it is not a required repository class name. Keep business data in module seeders and dependency orchestration in the generic runner.
