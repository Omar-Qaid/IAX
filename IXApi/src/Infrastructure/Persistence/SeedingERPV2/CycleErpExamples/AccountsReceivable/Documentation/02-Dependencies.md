# Configuration and master-data dependencies

Use the existing seed dependency resolver; do not impose a second module ordering here.

- The purchase example is the prerequisite phase, not a sibling: the demo items must be bought and
  received before a sale has anything to deliver.
- Currency, chart, accounts, fiscal calendar and ledger support ledger combinations.
- The sales main accounts are seeded into the shared chart, then a second `ARV2` account structure
  is defined, activated and assigned to the same ledger before any sales combination resolves.
- The output tax code, its posting group and the customer tax group follow those combinations and
  the shared item tax group.
- Site, warehouse and the company bank support customer defaults and the collection method.
- Customer configuration, parties, locations, addresses and contacts precede the customer records.
- Sales trade agreements follow both the customers and the released-product module records.
- Return handling codes stand alone and are required before a return order can be opened.

The actual completed seeder IDs and their order are returned in `CompletedSeeders`.

Resolve records by the current company and stable keys. Preserve incompatible existing records and
fail atomically. Do not hardcode RecIds, duplicate shared setup in the AR example folder, or insert
transaction rows during preparation. The JSON manifests verify the same module-owned fixtures.

## Why a second account structure

An activated account structure cannot be redefined, and the builder refuses a duplicate name. The
sales accounts therefore live in their own structure, in main-account ranges disjoint from the
purchase ones, and both structures are assigned to the same ledger. Widening the purchase structure
would work on an empty database and silently fail on one that already ran the purchase example.
