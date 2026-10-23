# Basic inventory — Phase 1 implementation status

Date: 2026-10-23. Phase 1 changes configure metadata only; no stock or ledger posting was performed. This is an implementation progress record, **not** a Phase 1 completion certificate. The copied SQL baseline remains unverified as noted in `Basic-Inventory-Phase-0-Assessment.md`.

## Implemented in this pass

- A later setup-only pass tightened unit conversion entry: the service rejects opposite-direction duplicate rules in the same product/global scope, cross-class conversions, offsets, and conflicting factor/fraction inputs. A positive fraction can now be used as the sole multiplier. Unit symbols are checked case-insensitively, and referenced units cannot change symbol, class, system, or precision. Deletion also checks barcode, inventory-parameter, and journal references. These changes read/write only unit setup tables; they create no stock or ledger effects.

The finance module build passed with zero errors. The focused unit/conversion and barcode tests passed (3 tests, 0 failures) using SQLite in-memory fixtures. No SQL Server result is claimed.

- Restricted new storage groups to site plus warehouse and blocked location; mapped existing field-setup flags so site is financial, warehouse is physical and nonfinancial, and both are required on receipt/issue.
- Restricted tracking groups to inactive batch and serial; saved group headers and field setup atomically; prevented ordinary changes after item assignment.
- Restricted model-group setup to stocked FIFO with negative inventory and physical-cost inclusion disabled and financial posting enabled. Assigned groups cannot be edited through basic setup. FIFO journal issues now use provisional same-site running-average cost; final settlement and close remain pending.
- Required a voucher sequence for journal names and disabled ordered reservation in inventory parameters.
- Blocked deletion of item groups used by posting profiles, and protected referenced sites/warehouses from deletion or warehouse site reassignment. Rejected advanced warehouse options on warehouse create/update.
- Rejected duplicate posting rules with the same type and relation in the service. This still needs a verified database constraint for concurrent requests.
- Added `GET /api/v1/InventParameters/readiness`, a read-only check for essential setup, ledger, numbering, journals, and core posting accounts. It does not create transactions or imply that later posting engines are implemented.

## Remaining before Phase 1 exit

1. Compare EF entities and enums with the authoritative copied SQL schema. Confirm dimension field flags, keys, `RecId` behavior, and actual number-sequence reference/scope tables before using this against production data.
2. Verify existing unit conversion behavior and account validation against real SQL Server, including product-specific conversions deferred to Phase 2.
3. Verify company ownership and all delete/update reference paths across the full schema, including posted documents and external modules.
4. Enforce posting-rule uniqueness in the verified database and test concurrent configuration writes. The current service check alone cannot guarantee uniqueness under concurrency.
5. Complete readiness coverage for fiscal-calendar assignment, valid posting accounts/ledger dimensions, exact journal defaults and voucher sequences, and all source-document posting types needed by later integration.
6. Run the Phase 1 acceptance cases on a disposable SQL Server database. Existing unit/in-memory tests do not establish database relationships, transaction rollback, or concurrent numbering guarantees.

Phase 2 item assignment and all inventory/ledger postings are outside this configuration pass.
