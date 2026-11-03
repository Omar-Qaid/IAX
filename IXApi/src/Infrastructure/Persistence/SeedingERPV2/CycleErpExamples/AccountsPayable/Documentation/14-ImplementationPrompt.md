# Implementation prompt

Implement the Accounts Payable cycle described by this documentation in the existing ERP.

Inspect the repository architecture, entities, services and seed registration first. Identify prerequisites owned by General Ledger, Inventory, Tax, Cash and Bank and Organization. Build missing prerequisite configuration/master seed data in its owning module, reuse compatible existing records, and reference those records from AP through stable keys resolved at runtime.

Register dependencies explicitly and execute prerequisites first. Do not copy shared setup into AP or hardcode generated database IDs. Keep the seeding framework generic and module business rules in their owning modules.

Implement the documented scenarios through business services, including realistic quantities, prices, discounts, taxes, partial receipts/invoices, payments, settlements, advances, foreign currency, returns and supported reversals. Implement missing basic business operations in the appropriate module. Never seed generated posting tables directly.

Complete actual table/field mapping, physical and financial assertions, reconciliation, idempotency, company isolation and crash-safe rerun behavior. Follow existing conventions and avoid unnecessary abstractions. Scope is basic ERP, without WHS, production, manufacturing or dimension derivation.

Continue through implementation, appropriate tests and reconciliation until all enabled scenarios pass. Report concrete blockers with affected scenarios; do not silently skip them or claim completion based only on documentation.
