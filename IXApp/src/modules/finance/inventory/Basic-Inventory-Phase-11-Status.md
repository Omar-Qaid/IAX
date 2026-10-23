# Phase 11 implementation status

Phase 11 is **in progress**. The current schema and earlier inventory flows do not yet support a trustworthy historical valuation, inventory-to-GL reconciliation, migration acceptance, or go-live sign-off.

## Implemented in this pass

- The existing company-scoped on-hand inquiry now has a paged endpoint at `GET /api/v1/InventOnHand/paged`. It accepts `itemId`, `itemGroupId`, `siteId`, `warehouseId`, `includeZero`, `page`, and `pageSize` (maximum 200). Its response includes `TotalCount`, so an empty result is distinct from a zero-balance row. Rows include the inventory unit from `InventTableModule`. The existing `GET /api/v1/InventOnHand` remains available.
- The transaction inquiry exposes physical, posted, and adjustment cost amounts separately and includes the physical voucher. Its existing total cost field remains for compatibility.
- `GET /api/v1/InventTrans/paged` now filters transactions before paging by item, source inventory transaction ID, site, warehouse, physical date, and financial date. It returns a stable date/RecId order and total count. The existing list route remains available.
- The read-only `GET /api/v1/inventory-reconciliation/physical` compares current `InventTrans` physical quantities with `InventSum.PhysicalInvent` by company, item, and inventory dimension. It accepts optional item/site/warehouse filters and returns difference rows, missing-dimension keys, and a count of transactions whose physical state cannot be classified from the current entity conventions. It does not change stock. The supported transaction states are `Received`/`Purchased` and `Deducted`/`Sold`; other physical candidates make `NoReportedIssues` false.
- The on-hand page now calls the paged endpoint with applied item/group/site/warehouse/zero-balance filters and shows the server's matching-row count and page controls. It displays the inventory unit. Transaction details display the separate cost fields and physical voucher; English and Arabic labels were added.

## Tables and contracts used

- Reads: `InventDim`, `InventTrans`, and `InventSum` for the physical reconciliation; existing on-hand and transaction inquiries also read item, module, group-assignment, origin, and status tables.
- Writes: none from the reconciliation endpoint. No operational stock, ledger, or migration data was written.
- The new endpoint follows Controller → service interface → service → `IUnitOfWork`, uses company context and Inventory Transactions View permission, and returns a shared `APIResponse` DTO.
- Finance module build passed with zero errors; frontend typecheck passed. Focused SQLite reconciliation and paging tests were added. A test build first exposed a controller call compatibility error, which was fixed by making optional query filters optional in both inquiry controllers. Subsequent test-project builds did not reach the test runner before interruption, so the tests must not be treated as passing evidence. SQL Server was not tested.
- A date/source/paging assertion was added to the existing transaction inquiry test. The finance module still builds, but its focused `dotnet test` invocation again stalled before producing a test result and was interrupted. No transaction paging test pass is claimed.

## Remaining acceptance gates

1. Verify the copied SQL Server schema, enum values, relationships, and posting links with `IXApi/scripts/audit-basic-inventory-schema.sql` against the intended database. Local source models alone are insufficient evidence.
2. Complete purchase receipt/invoice, sales invoice, reservation, counting protection, return, and FIFO close workflows recorded in the earlier phase status files. Do not infer historical physical balances from current transaction status: it does not prove when a quantity became physical. Historical value also needs financial and adjustment event dates.
3. Establish source-line, receipt-to-invoice, `InventTransPosting`, settlement, and ledger links before implementing source-document and inventory-to-GL reconciliation. A read-only mismatch report must preserve the exact company, account, financial dimension, currency, and cutoff scope; it must not repair balances automatically.
4. Rehearse opening migration through posted opening journals on a fresh SQL Server database. Reconcile quantities by item/site/warehouse and value by the configured financial scope. Never seed operational `InventSum` balances directly.
5. Run the full physical/financial cycle, failure and retry scenarios, concurrent posting checks, period-close restrictions, and user acceptance on SQL Server. Record results and obtain business approval before go-live.

The current inquiry endpoints and physical comparison are diagnostics, not proof that the full Phase 11 release or historical reports are complete. A zero difference is not a validated financial reconciliation. The missing purchase, posting-link, and reservation-enum definitions have been deferred by the user until after inventory core and existing sales integration work; they remain release gates.
