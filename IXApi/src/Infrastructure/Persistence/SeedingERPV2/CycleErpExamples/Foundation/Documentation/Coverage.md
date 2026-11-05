# Foundation coverage and ownership

The checked-in `FoundationTableCatalog` lists 68 configuration/master entity types and three transaction types. `FoundationIntegrityValidator` requires a persisted row for each mapped master type and checks mapped foreign keys before the preparation transaction commits.

- Foundation base registration owns company, currency/rates, calendar, payment terms and cash discounts.
- Existing AP seeders own shared vendor parties, geography, postal/electronic addresses and charge-code fixtures. They are reused without duplicate inserts.
- GL builders own account structures, constraint trees, ledger combinations and their normalized groups/levels. Foundation adds a custom department example and default-value set with links to its organization unit.
- Tax seeders own tax codes, rates, groups, item groups and posting accounts. Foundation adds authority, settlement-period header and report-period configuration.
- Foundation extended seeders own delivery modes/terms, installment schedules, contacts/roles, personnel lookups, workers, showrooms, organization units, roles, positions, hierarchy nodes, reporting lines and worker assignments.

The contact fixture references a customer master and the AP supplier party; the tax authority references the AP supplier and location. No numeric RecId is assumed. Additional fixture identifiers use FNV2; existing shared fixtures retain APV2 identifiers.

`TaxTrans`, `TaxJournalTrans` and `MarkupTrans` are exercised through the opt-in Foundation transaction wrapper around AP scenarios 04 and 07. Vendor invoice journal posting retains calculated `TaxJournalTrans` detail with its company, source journal line, tax codes/groups, currency, accounting amounts and operation ledger dimension. The shared posting transaction saves it with the posted tax and voucher. Reruns reuse the posted journal without duplicate details. This does not introduce a general tax-journal editing or manual tax-adjustment API.

Preparation is atomic. Transaction examples use the existing document-service commit boundaries. A later transaction failure preserves earlier committed documents for rerun; it does not roll back the entire business cycle.
