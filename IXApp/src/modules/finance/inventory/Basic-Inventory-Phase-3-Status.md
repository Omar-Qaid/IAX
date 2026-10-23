# Phase 3 implementation status

Phase 3 is **in progress**. The inventory engine cannot yet be declared complete against the Phase 3 acceptance cases.

## Implemented in this pass

- `InventDim` resolution now validates the company, site, and warehouse relationship and rejects dimensions outside the basic site/warehouse scope.
- SQL Server resolution uses a transaction-scoped application lock for a canonical dimension combination. Concurrent callers using this resolver recheck the combination after taking the lock and reuse the existing row.
- On-hand inquiry accepts item, site, and warehouse filters. Available physical is calculated as physical inventory less physical reservations.
- Inventory transaction inquiry accepts item, source inventory ID, site, and warehouse filters. It remains company scoped and read only.

## Required before reservation and transaction-state commands

The checked-in `StatusIssue` enum in `IXApi/src/Modules/Finance/Common/Enums/Enumeration.cs` contains `None`, `Quotation`, `Ordered`, `Picked`, `Deducted`, `Sold`, and `Released`. It has no verified `ReservPhysical` or `ReservOrdered` value. The Phase 3 specification requires the copied schema's actual enum mapping. Using `Picked` or an invented integer for physical reservation would corrupt the meaning of existing transactions.

The available connected `ERMOld` database contains workflow tables but no inventory tables. It cannot establish the inventory schema baseline or verify the enum mapping, origin links, transaction keys, and summary bucket behavior. Phase 0's schema baseline remains open.

Until an authoritative inventory schema and enum mapping are available, the following Phase 3 acceptance cases are **not implemented or verified**: physical reservation/release, partial transaction splits, purchase supply synchronization, delivery of owned reservations, summary reconciliation, stock-level concurrency, and retry-safe source commands. Existing sales and journal posting paths still maintain inventory through their own services; the shared engine has not replaced them.

## Verification

`dotnet build IXApi/src/Modules/Finance/IAX.IXApi.Modules.Finance.csproj --no-restore -v:q -clp:ErrorsOnly` succeeded with zero errors. Database-backed Phase 3 acceptance tests require the verified inventory schema and test database.
