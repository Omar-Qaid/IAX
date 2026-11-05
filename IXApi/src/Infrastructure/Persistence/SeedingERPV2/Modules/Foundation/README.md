# Foundation module seeders

Use `Core.Registration.AddFoundationPhaseOneSeeding()` for the complete dependency graph. `AddFoundationSeeders()` registers the eight shared base seeders; `AddFoundationExtendedSeeders()` registers 28 Foundation-specific seeders. These low-level registrations are composed with the existing GL, Tax, Inventory, Cash/Bank and AP prerequisites by the phase-one entry point. They are not independent runners.

Base seeders cover legal entity, currencies/rates, fiscal calendar, payment terms and cash discount setup. Their original ERPV2 seed IDs are retained after moving ownership into Foundation. Existing AP registration remains idempotent and retains its original phase-one graph.

Extended seeders cover delivery and payment schedules, address roles/contacts, personnel lookups, workers/showrooms, organizational/reporting structures and assignments, dimensions and tax reporting setup. Shared AP party/address/geography and posting prerequisites are reused rather than copied.

FoundationTableCatalog inventories all 68 master/configuration and three transaction entity types. FoundationIntegrityValidator verifies mapped company-scoped parents and logical customer, schedule, dimension and tax references before commit. FoundationLookup handles the existing byte/short-key lookup masters without assuming numeric identities. FoundationSeedExample describes stable business references used by the embedded example catalog.

The complete example package is in `../../CycleErpExamples/Foundation`. It provides validated JSON manifests, preparation and transaction reports, scenario documentation, and an explicit transaction entry point. TaxTrans, TaxJournalTrans and MarkupTrans are generated through owning business services. No registration runs application seeding or migration automatically.
