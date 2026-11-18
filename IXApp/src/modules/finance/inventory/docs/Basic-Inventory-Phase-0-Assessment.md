# Basic inventory — Phase 0 assessment

## Schema verification follow-up (2026-10-23)

The repository and its EF migration snapshot are **implementation evidence, not verification of the copied database**. The checked-in `StatusIssue : byte` is `None=0`, `Quotation=1`, `Ordered=2`, `Picked=3`, `Deducted=4`, `Sold=5`, `Released=6`; it contains no `ReservPhysical` or `ReservOrdered`. `StatusReceipt : byte` is `None=0`, `Ordered=1`, `Arrived=2`, `Registered=3`, `Received=4`, `Purchased=5`. `InventSum` does map `ReservPhysical` and `ReservOrdered` quantities, so adding reservations by guessing a missing transaction status would make detail and summary inconsistent.

The initial EF migration contains `InventTrans`, `InventSum`, `InventSettlement`, `InventClosing`, and `InventTransOrigin` tables, but that proves only the schema the application proposes. The available BlazeSQL connection points to `ERMOld`, whose inspected tables are workflow tables rather than inventory. A read-only attempt to connect to a local SQL Server using Windows authentication failed before any database could be inspected. No live inventory table, enum value, key, or posting relationship was established.

The read-only script [audit-basic-inventory-schema.sql](../../../../../IXApi/scripts/audit-basic-inventory-schema.sql) records actual table presence, columns, indexes, foreign keys, observed inventory status values, and applied EF migrations when run against the authoritative copied database. Its output must be compared to the EF model and migration snapshot before changing enum mappings or enabling physical reservation, settlement, and close. Index/FK absence in an AX-style copy does not itself prove a logical relationship is absent; document relationships still need source-key verification.

Date: 2026-10-23. Scope: read-only review of the repository against the Phase 0 inspection specification. No database connection, live SQL schema comparison, stock posting, or ledger write was performed. The copied schema's AX/D365 version is **not verified**, so code entity names and enum values below are not treated as proof of the database layout.

## Scope decision record

The proposed release remains stocked purchased/sold items; site and warehouse; opening, movement, adjustment, immediate transfer, and counting journals; physical sales reservation; purchase/sales receipts and invoices; linked returns; FIFO only; negative physical and financial inventory disabled. Batch, serial, bin, license plate, advanced WHS, production, BOM, and MRP are excluded. These are **proposed**, not confirmed configuration. Current code includes batch/serial validation and warehouse status paths, and journal posting accepts non-FIFO models; those differences require explicit configuration and enforcement before release.

## Architecture and ownership found

| Area | Repository evidence | Assessment |
| --- | --- | --- |
| Module boundary | `IXApi/src/Modules/Finance/Inventory`, `FinanceModule.cs`, `ApplicationDbContext.cs` | Inventory has a Finance module and EF registrations. API controllers use service interfaces in the inspected journal/transaction paths. |
| Shared contracts | `Shared/Application/Services/BaseService.cs`, `Shared/Application/Controllers/BaseController.cs`, `Shared/Application/Persistence/IUnitOfWork.cs`, `Infrastructure/Persistence/Repositories/UnitOfWork.cs` | CRUD conventions and a transaction-capable unit of work exist. Posting must use business services, not generic CRUD. |
| DTO, mapping, validation | `InventJournal` DTO/service files, `InventTransMapping.cs`, inventory validators, Mapster/FluentValidation registrations in `FinanceModule.cs` | Established patterns exist; coverage of every inventory command remains to be checked. |
| Company context | `Infrastructure/Identity/CompanyExecutionContext.cs` | Resolves allowed company claims and requested company header. Posting queries inspected filter `DataAreaId`; cross-company tests are still needed. |
| Authorization | `InventJournalController.cs`, `InventTransController.cs` | Domain permissions exist; separate draft, post, and close privileges need review. |
| Posting ownership | `InventJournalPostingService.cs`, `AccountsReceivable/PackingSlip/Services/PackingSlipJournalService.cs`, `AccountsReceivable/SalesOrder/Services/SalesInventoryDemandService.cs` | Multiple services directly maintain `InventTrans`/`InventSum`. Define a shared inventory posting contract and a single invariant/concurrency policy before extending integration. |

## Candidate schema map — code evidence only

All entries below are **present in EF code, live database not verified** unless stated otherwise. For each table, Phase 0 still needs SQL schema/table, all columns and types, `RecId` generation, keys, indexes, defaults, company scope, relationships, concurrency token, and enum storage compared to one authoritative copied database.

| Group | Evidence and status |
| --- | --- |
| Company/setup | `InventParameters`, `InventSite`, `InventLocation`, `InventItemGroup`, `InventModelGroup`, `InventModelGroupItem`, storage/tracking dimension groups, `InventJournalName`, `InventPosting` have `DbSet` entries in `ApplicationDbContext.cs`. `DataArea`/`CompanyInfo` ownership and physical schema need comparison. |
| Products/items/units | `EcoResProduct`, translation, `InventTable`, `InventTableModule`, item group assignment, item order setups, barcodes, and unit services/entities are present in code. Confirm shared versus company keys and conversions in SQL. |
| Movement | `InventDim`, `InventTransOrigin`, `InventTrans`, `InventSum`, journal header/lines, `InventTransOriginSalesLine` are mapped or represented in code. Verify keys, unique dimension identity, links, and summary constraints. |
| Cost/ledger | `InventSettlement` and `InventClosing` are EF entities/DbSets, but no close service was found in the inventory folder. `InventJournalPostingService` creates general journal entries directly. Verify `InventTransPosting` in the copied schema and code before treating the posting trace as complete. |
| Purchase source | No `PurchTable`, `PurchLine`, `VendPackingSlipJour/Trans`, or `VendInvoiceJour/Trans` entity/service was found under the reviewed Finance module or DbContext. Classify purchase integration as **missing in this repository search**, pending wider solution/database confirmation. |
| Sales source | `SalesTable`, `SalesLine`, `CustPackingSlipJour/Trans`, and `CustInvoiceJour/Trans` entities are present. Sales delivery inventory updates are implemented; invoice and return stock/cost behavior remain unverified. |

The C# enums currently assign `StatusIssue` 0–6, `StatusReceipt` 0–5, and `InventModel.FIFO` 0 (`Finance/Common/Enums/Enumeration.cs`). These must be compared with stored SQL values before posting logic depends on them. `InventModelGroupItem` is the existing model assignment path; do not add a second `InventTable.ModelGroupId` without a verified schema reason.

## Feature-gap matrix

“Partial” means code exists but does not establish the full proposed business result. “Not verified” means the specific behavior was not demonstrated by this read-only review.

| Feature | Status | Evidence / gap | Target phase |
| --- | --- | --- | --- |
| Released items, groups, warehouses, dimensions | Partial | Entities, services and UI pages exist; live schema and complete scope controls unverified. | 1–2 |
| Transaction inquiry | Partial | `InventTransService.cs` exposes a company-filtered read-only list. Historical valuation/source trace not established. | 3, 8 |
| On-hand and reservation | Partial | `SalesInventoryDemandService.cs` writes ordered demand and summaries; physical reservation command and concurrency policy need verification. | 3, 10 |
| Inventory journals | Partial | `InventJournalPostingService.cs` writes transactions, summary, and GL in a unit-of-work transaction. FIFO items are explicitly rejected by `ResolveUnitCostAsync`; this blocks the proposed FIFO-only release. | 4, 7 |
| Immediate transfer | Partial | Journal posting creates paired issue/receipt. Cross-site financial dimension and cost-pool rules need verification. | 5 |
| Counting | Partial | Journal posting computes difference from current summary. Count-start locking and stale-count protection were not found in the inspected posting path. | 6 |
| FIFO settlement/close | Missing | Settlement/closing entities exist, but no close service or posting path was found. | 7 |
| Reporting/reconciliation/migration | Not verified | Transaction inquiry exists; full historical value, GL reconciliation, and cutover process not established. | 8 |
| Purchase receipt/invoice/return | Missing in reviewed code | Purchase document entities/services not found in Finance search. | 9, 11 |
| Sales delivery | Partial | `PackingSlipJournalService.cs` uses a serializable transaction, splits open issues, and updates `InventSum`; costing and ledger completion not established. | 10 |
| Sales invoice/customer return | Not verified | Invoice entities exist; end-to-end financial update and linked return behavior not demonstrated. | 10–11 |

## Posting contract and priority blockers

1. **P0 — Decide and verify the copied SQL baseline.** Compare EF model/migrations with the actual database, including every candidate table, scalar field, relationship, precision, index, and enum. Do not change copied scalar fields during discovery.
2. **P0 — Resolve FIFO versus current journal engine.** `InventJournalPostingService.ResolveUnitCostAsync` rejects FIFO/weighted-average models and permits moving average/standard cost. Implement estimated issue cost plus FIFO settlement/close before enabling FIFO-only stock posting. Do not merely remove the rejection.
3. **P0 — Establish one posting invariant across journals and sales.** Journal and packing-slip services both mutate `InventTrans`/`InventSum`; define shared availability, dimension, status-transition, financial-date, and summary rules. Purchase integration must use the same contract.
4. **P0 — Prove stock concurrency and retry behavior.** The journal path starts a default database transaction and reads/modifies summary rows; the reviewed code does not demonstrate serializable locking or a row-version check for last-unit contention. A posted journal retry currently returns an error rather than its existing voucher result. Verify with real SQL Server tests.
5. **P0 — Complete counting controls.** Current count difference uses on-hand at posting time; the required count-start lock, cancellation release, and stale-count rejection need design and evidence.
6. **P1 — Confirm ledger and period integration.** Journal posting checks an open fiscal period and writes balanced general journal entries. Verify account resolution, `InventTransPosting` evidence, inventory period close boundary, and atomic behavior against live SQL.
7. **P1 — Define source integration owners.** Sales order/delivery code exists; purchase source is absent from reviewed code. Specify receipt/invoice/return contracts and which service owns each physical versus financial transition before Phases 9–11.

## Phase 0 completion gate

This assessment is **incomplete as a database baseline**. The next read-only step is to obtain the authoritative copied database/schema and produce a verified table-by-table SQL/EF comparison. Then run focused SQL Server evidence tests for journal rollback, concurrent last-unit issue, duplicate post, company isolation, and summary/detail reconciliation. Phase 1 can begin after the schema and shared-service owners are confirmed and the P0 blockers have assigned resolutions. No operational `InventTrans`, `InventSum`, or ledger writes were made in this assessment.
