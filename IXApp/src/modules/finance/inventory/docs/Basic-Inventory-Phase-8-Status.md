# Phase 8 implementation status

Phase 8 is **in progress**. Sales order creation already creates `InventTransOrigin`, sales-line origin links, `InventTrans` demand in `StatusIssue.Ordered`, and `InventSum.OnOrder` without changing physical inventory. Packing-slip posting already creates posted delivery evidence, splits partial demand, moves the processed portion to `StatusIssue.Deducted`, and decreases physical inventory in a transaction.

The partial packing-slip split now calculates catch-weight quantity from the original open transaction before reducing that transaction. The previous order of operations applied the partial ratio twice. A focused catch-weight posting integration fixture remains to be added; this correction does not complete sales invoicing or reservation.

## Implemented in this pass

- SQL Server packing-slip posting now takes a transaction-scoped order lock and the same item/dimension stock locks used by inventory journals, in a stable order. This protects the supported journal and delivery paths from overselling each other's unreserved stock.
- Delivery checks `PhysicalInvent - ReservPhysical` for all selected lines grouped by item/dimension before allocating a packing-slip ID, then checks again while issuing. Because there is no verified source-owned physical reservation state yet, the path cannot consume another source's reserved stock and currently rejects delivery against any reservation, including one that might belong to this order.
- `AvailPhysical` is recalculated from physical inventory and physical reservations after the issue.
- A focused test case was added for a delivery when all physical stock is reserved by other demand. The test command did not finish in this pass; the finance module build succeeded.

## Required before Phase 8 can be enabled as specified

- The checked-in `StatusIssue` enum has no verified `ReservPhysical` value. Phase 3's source-owned reservation/release service and partial transaction splits are absent. Do not map reservation to `Picked` or another guessed value.
- The finance module contains `CustInvoiceJour` and `CustInvoiceTrans` entities but no customer-invoice posting service or matching workflow. Delivery-first partial invoicing, financial issue updates, `CustTrans`, COGS/revenue/receivable ledger posting, financial coverage, and invoice retry behavior remain unimplemented.
- Phase 6's durable counting freeze is not implemented, so delivery cannot yet honor an active count lock.
- Packing-slip physical cost posting and the related invoice-time reversal are not implemented. The existing delivery path changes physical stock without a verified physical ledger posting policy.
- The copied inventory schema, invoice-to-packing-slip matching relationship, posting profiles, and live database remain unverified. Cross-path concurrency and rollback need database-backed tests.

Phase 8 is complete only after reservation, delivery, invoice, accounting, and reconciliation acceptance cases pass together.
