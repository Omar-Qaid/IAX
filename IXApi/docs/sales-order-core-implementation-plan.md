# Sales Order Core Implementation Plan

## Scope and constraints

Implement the 16 Sales Order core functions from the supplied requirements using only entities and tables already present in this repository. If a function depends on a table that is not implemented, record it as skipped and do not add a replacement table or migration. The warehouse reservation point (WHS) is excluded, including WHS-specific reservation, availability, and cancellation behavior.

This plan separates existing behavior from remaining work. It does not authorize changes outside the Sales Order scope or changes to unrelated working-tree edits.

## Current implementation baseline

| # | Function | Current repository support | Planned treatment |
|---|---|---|---|
| 1 | Create order | `SalesTableController` quick-create allocates a sequence, validates the customer and defaults header values. | Audit and close any gaps in customer/invoice account and legal-entity validation. |
| 2 | Customer defaults | Quick-create and header update already read customer commercial, tax, and delivery defaults. | Verify override and customer-change propagation rules; preserve existing orders when master data changes. |
| 3 | Order lines | Add/update/delete endpoints and inventory-demand service exist. | Complete item/service and unit-conversion behavior using existing unit tables; skip unsupported missing conversion structures. |
| 4 | Inventory dimensions | `InventDim`, site/warehouse, batch, serial, and resolver support exist. | Verify dimension validation and demand reconciliation; exclude WHS-only dimension/reservation logic. |
| 5 | Pricing | Item sales-module fallback exists; direct customer/item agreement selection was recently added to line create/update. | Characterize agreement precedence, date, quantity, currency, unit, and price-unit behavior; extend only where existing fields support it. |
| 6 | Discounts | Line and multiline values are persisted; totals consume them. | Apply supported `PriceDiscTable` discount agreements and order discount only where existing fields encode the behavior; otherwise skip unsupported group/configuration rules. |
| 7 | Charges | `MarkupTrans` is read for totals; charge master and transaction entities exist elsewhere in Finance. | Audit existing charge CRUD/API wiring, ownership references, taxability, and allocation; add only missing safe endpoints using existing tables. |
| 8 | Tax calculation | Tax-group intersection and totals calculation exist in the Sales Order controller. | Verify rate validity, inclusive-tax, exemptions/rounding and taxable charges against current schema; no `TaxTrans` posting. |
| 9 | Totals | Read-only totals endpoint includes line values, charges, and tax. | Validate whole-order totals and avoid duplicate line/header charge inclusion. |
| 10 | Delivery details | Header/line addresses, dates, terms, modes, and customer-address validation exist. | Verify propagation policy and address ownership for header and lines. |
| 11 | Availability | `InventSum`, `InventTrans`, and standard dimensions exist. | Use existing standard inventory data only; document unsupported filters and do not use WHS tables. |
| 12 | Reservation | Standard inventory transactions exist; no `WHSInventReserve` or `WHSInventReserveDelta` entities found. | Skip reservation workflow because required reservation framework tables are absent and WHS point is excluded. Do not create tables. |
| 13 | Confirmation | `CustConfirmJour` and `CustConfirmTrans` entities exist; no Sales Order confirmation action or confirmation-number sequence was found. | Audit available numbering and entity configuration. Implement snapshots only if an existing sequence and all required persistence fields are present; otherwise skip without inventing numbering. |
| 14 | Changes | Open line/header updates exist and inventory demand is reconciled for supported cases. | Expand safe quantity, dimensions, and commercial edits; preserve confirmed history and block unsafe processed-line edits. |
| 15 | Cancellation | Open-order cancellation updates demand and status. | Add partial remainder cancellation only if existing quantity/status fields can preserve delivered and invoiced history; otherwise retain current restriction and document the skipped scenario. No WHS release behavior. |
| 16 | Tracking | Packing-slip, invoice, confirmation, line, and inventory entities exist; copy UI reads some confirmation history. | Add a read-only per-line history view/API from existing entities and calculate delivered/invoiced quantities from journals, never from ordered minus remaining alone. Skip missing sources. |

## Delivery checkpoints

### C0 — Contract and table inventory

- Confirm each required table/entity, mapping, and existing endpoint before implementation.
- Record missing tables and dependent operations as skipped; do not scaffold entities, migrations, seed records, or substitute tables.
- Preserve existing uncommitted work outside files explicitly needed for a checkpoint.

**Gate:** A table-to-function matrix identifies present, absent, and unsupported dependencies. WHS work is excluded.

### C1 — Characterize current order and line lifecycle

- Review create, customer defaults, line add/update/delete, dimensions, delivery details, and order cancellation.
- Define safe behavior for order and invoice account validation, company context, price-unit calculations, discount fields, and header-to-line propagation.
- Correct only confirmed gaps in existing tables and API contracts.

**Gate:** API build and focused regression coverage for create/defaults, line CRUD, dimensions, and unchanged inventory demand on unrelated edits.

### C2 — Commercial calculations

- Verify pricing precedence and fallback among supported customer/item agreements and `InventTableModule`.
- Apply line and multiline discounts only when existing agreement fields and group links express the requested rules.
- Verify charge ownership, totals inclusion, tax-group intersection, effective tax rates, inclusive tax, and rounding.
- Keep totals read-only; do not create posting records.

**Gate:** Calculation examples reconcile gross, discounts, charges, taxable base, tax, and final amount. Unsupported pricing or tax rules are explicitly listed as skipped.

### C3 — Standard inventory views and supported changes

- Validate site/warehouse/dimension combinations and item batch/serial requirements using present standard tables.
- Implement standard availability from existing inventory summaries/transactions with clearly defined filters.
- Reconcile demand when changing quantity or dimensions.
- Keep reservations and any WHS-dependent paths skipped.

**Gate:** Inventory demand and summary quantities remain consistent through line create, update, delete, cancellation, and dimension changes.

### C4 — Confirmation and tracking

- Confirm whether a usable existing confirmation number sequence is configured.
- If available, implement confirmation snapshots transactionally in existing confirmation journal entities; otherwise skip confirmation creation.
- Build read-only line history from existing confirmation, packing-slip, invoice, and inventory records.
- Derive delivered and invoiced values from journal history; preserve cancelled remainder distinctions.

**Gate:** History is immutable and read-only, and quantities reconcile for open, partially delivered, invoiced, and cancelled examples. Missing data sources remain skipped.

### C5 — Acceptance and handoff

- Run focused regression coverage for each implemented checkpoint and build the API and affected frontend modules.
- Review permission checks, company scoping, transactional boundaries, and the final table-to-function matrix.
- Report implemented, already supported, and skipped functions separately; list reasons for skips and any live database/runtime checks not performed.

**Gate:** No new table or migration was introduced for an absent dependency; no WHS-specific behavior was added.

## Implementation rules

- Use the existing Finance data context, entities, company context, permissions, API envelopes, and UI patterns.
- Keep `DataAreaId` in all company-scoped queries and writes.
- Keep `RecId` values lossless across API/frontend boundaries.
- Preserve existing business rules and uncommitted work; make narrow changes per checkpoint.
- Do not treat a successful build as evidence of live database schema or runtime behavior.
- Do not implement a skipped function by introducing new tables, placeholder foreign keys, or speculative number sequences.

## Acceptance checklist

- [ ] All 16 functions have a recorded status: existing, implemented, or skipped with reason.
- [ ] Missing-table dependencies remain uncreated and unused.
- [ ] WHS reservation point and WHS-specific behavior remain excluded.
- [ ] Sales order creation, pricing, discounts, charges, tax, and totals reconcile using supported configurations.
- [ ] Inventory demand remains consistent for supported line changes and cancellation.
- [ ] Confirmation/tracking history uses existing records and preserves quantity distinctions.
- [ ] Focused checks and build results are recorded without claiming live database acceptance unless verified.
