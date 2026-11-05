# Foundation examples

Register `AddFoundationConfigurationAndMasterDataExamples()` and resolve
`FoundationCycleSeeder` in a fresh dependency injection scope with the intended
company context. Call `PrepareAsync(new FoundationExampleOptions(2026, actor), ct)`.
No startup hook executes these examples.

The preparation reuses AP's configuration and master prerequisites, including GL,
inventory, tax and bank references. Foundation adds delivery settings, installment
schedules, address roles and contacts, workers and organization structures,
default dimensions and tax reporting setup. `FoundationTableCatalog` inventories
the Foundation master and transaction entities separately.

All preparation runs through `SeedRunner`: dependency validation, company scope,
database lock, serializable transaction, history, and validation before commit.
Existing incompatible values cause rollback. RecIds are resolved at runtime from
business keys. The shared APV2 fixtures remain compatible with the AP entry point;
additional Foundation examples use FNV2 keys.

`FoundationIntegrityValidator` checks mapped foreign keys against parents in the
selected company, including other modules. AP's existing validators also check
its logical posting, tax, inventory, currency and vendor relationships.

`PrepareAsync` creates configuration and master data only. `TaxTrans`,
`TaxJournalTrans` and `MarkupTrans` belong to business transactions. They must be
created by their owning services; this package does not directly seed them.
The existing AP transaction examples exercise posted tax and purchase charges.
Register `AddFoundationTransactionExamples()` and call `FoundationTransactionExamples.RunAsync` to prepare Foundation and run AP scenarios 04 (taxed expense journal) and 07 (purchase charge) through their owning services. Earlier posted documents remain committed on a later failure, following the AP resume contract.

Use a disposable database with migrations already applied. This code does not
migrate or seed the application database automatically. Synthetic parties and
contacts are demonstration data.

Verification is in `FoundationSeedTests`: registration/dependency resolution,
mapped master coverage, FK-enabled persistence, reruns and conflict rollback.
Check current test output before claiming complete coverage or successful execution.
