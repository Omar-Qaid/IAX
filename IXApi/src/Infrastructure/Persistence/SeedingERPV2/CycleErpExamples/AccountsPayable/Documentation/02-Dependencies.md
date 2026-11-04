# Configuration and master-data dependencies

Use the existing seed dependency resolver; do not impose a second module ordering here.

- The selected legal entity is an existing prerequisite, not created by the example package.
- Currency, chart, accounts, fiscal calendar and ledger support ledger combinations.
- Tax and inventory setup reference those combinations and their own groups/units.
- Site/warehouse and company bank setup support supplier defaults and payment methods.
- Vendor configuration, parties, addresses and bank masters precede supplier pricing.
- Released products reference the primary vendor and their item/model groups.
- Supplier quotes follow both the vendor and released-product/module records.

The actual 46 completed seeder IDs and their order are returned in `CompletedSeeders`.
Inventory storage setup precedes vendors, while released products with a primary vendor follow
vendor setup; the graph therefore interleaves some module-owned work deliberately.

Resolve records by the current company and stable keys. Preserve incompatible existing records
and fail atomically. Do not hardcode RecIds, duplicate shared setup in the AP example folder, or
insert transaction/posting rows. The JSON manifests verify the same module-owned fixtures.
