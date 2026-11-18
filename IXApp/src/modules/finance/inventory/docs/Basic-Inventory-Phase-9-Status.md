# Phase 9 implementation status

Phase 9 is **not implemented for posting**. The finance module has customer-invoice entity classes but no customer-invoice posting service, no purchase-order execution path, and no verified original-invoice return entitlement or original-cost mapping. A customer return cannot safely be represented by an ordinary sales order issue or a positive adjustment journal.

## Safety changes in this pass

- Sales line create/change rejects `SalesType.ReturnItem` and return-lot input through the normal sales path. The earlier return-lot check accepted any same-item inventory origin, which did not prove an original invoiced sale or remaining return entitlement.
- Sales inventory demand rejects return orders and return-lot lines, avoiding an incorrect negative demand transaction for goods that should be received.
- Packing-slip preview/post rejects return orders and return lines, avoiding an incorrect physical issue when a customer return should be a receipt.
- A regression case was added to the existing packing-slip test for the return-order guard.

## Required implementation dependencies

1. Phase 8 invoice posting and verified invoice-line-to-inventory-transaction links for customer returns.
2. Purchase receipt and vendor invoice posting with their original receipt and cost links for vendor returns.
3. A source-owned, concurrent return entitlement allocation that accounts for open, received/shipped, credited, and canceled portions.
4. Verified return order types, status transitions, return-lot/marking fields, and original-cost allocation. The current `InventTransOrigin` existence check alone is insufficient.
5. Return-specific physical posting, customer/vendor credit services, balanced AR/AP and ledger entries, and database-backed partial/retry/rollback tests.

Return posting should remain unavailable until these relationships and acceptance cases are verified against the inventory database.
