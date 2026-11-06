# Physical inventory effects

| Operation | Physical quantity | Financial inventory value | Customer balance |
|---|---|---|---|
| Create and confirm the order | unchanged | unchanged | unchanged |
| Reserve | unchanged; available to reserve falls | unchanged | unchanged |
| Deliver | falls | physically updated, may post cost | unchanged |
| Invoice the delivered units | no second issue | financially updated | rises |
| Free text invoice | unchanged | unchanged | rises |
| Collect | unchanged | unchanged | falls |
| Settle | unchanged | unchanged | allocated and closed |
| Receive a return | rises | physically updated | unchanged by the receipt alone |
| Credit note | depends on the disposition | depends on the disposition | falls |

Sales issues are stored as negative quantities, so the scenario reports read negative for a
delivered order. A scenario that delivers 100 units and takes 10 back reports a net issue of 90.

The non-stocked service item never moves inventory, in either direction, at any step.

Delivering does not invoice and invoicing does not deliver a second time. If a scenario delivers 60
of 100 and then invoices 60, the physical quantity is 60 and the remaining 40 stays open on the
order until it is delivered or cancelled.
