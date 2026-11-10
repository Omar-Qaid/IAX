# Pattern examined before implementation

The AP implementation was examined in both `Modules/AccountsPayable` and `CycleErpExamples/AccountsPayable`, including registration, dependency declarations, master insert-or-validate behavior, preparation validator, transaction options and flow, reconciliation snapshot/checks, reports, scenario documentation, and isolated-database test fixtures.

The important contracts are:

1. Module seeders own data. They resolve references using company and business keys through `SeedContext`; the example package does not become a second master-data writer.
2. `SeedRunner` resolves dependencies and executes masters, seed history, and validators inside a serializable transaction with rollback and tracker cleanup.
3. Preparation and transactions have separate explicit entry points. Neither runs on startup.
4. Transaction scenarios use business services and their transaction boundaries. Stable document keys allow retries after partial completion.
5. Reconciliation reads persisted inventory, documents, settlements, and ledger rows. Configuration validation is not posted-balance verification.
6. Repeated execution preserves posted documents, consumed numbering, and user modifications. Conflicts must fail without rewriting the existing row.

Inventory adapts this pattern using its own fiscal-year-scoped stock/service items and the existing owners for AP, AR, ledger, tax, dimensions, and number sequences. Its acceptance source remains `Read_only.md`. Historical AP documentation is not copied as evidence that Inventory cases passed.
