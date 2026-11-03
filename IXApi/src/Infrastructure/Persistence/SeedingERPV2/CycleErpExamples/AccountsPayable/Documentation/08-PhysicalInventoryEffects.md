# Physical inventory effects

## Stocked item baseline

| Event | Physical on-hand delta | Financial update |
|---|---:|---|
| Create/confirm PO | 0 | None |
| Receive 60 | +60 | Physical receipt cost/accrual if enabled |
| Invoice 40 | 0 | Financial cost for 40 |
| Receive 40 | +40 | Physical receipt cost/accrual if enabled |
| Invoice remaining 60 | 0 | Financial cost for remaining 60 |
| Return 10 after invoicing | -10 through supported return issue | Credit/cost adjustment through return invoice |

Before returns the cumulative receipt and invoice quantities are both 100. Service and direct expense invoices have no stocked movement.

## Validation

Compare scenario-scoped inventory transactions and the corresponding on-hand dimension delta from a captured baseline. Check site, warehouse, item and company. Validate expected ordered quantity separately from physical on-hand. Ensure receipt/invoice splits remain linked to the originating PO line.

Physical receipt value remaining uninvoiced is based on the remaining eligible receipt quantities and their valuation. Do not derive it from total PO quantity less invoiced quantity when some goods are unreceived or cancelled.

For corrections verify linked reversing movements, resulting on-hand and the allowed remaining invoice quantity. Reject correction of already consumed/invoiced quantities when the business operation does not support it. Do not manually delete historical movements.
