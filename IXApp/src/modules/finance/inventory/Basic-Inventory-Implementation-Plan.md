# Basic inventory module — phased implementation plan

Prepared for an existing ASP.NET Core ERP API. Scope: a basic trading-company inventory cycle aligned with AX 2012 / Dynamics 365 Finance & Operations concepts.

This is a proposed implementation blueprint, not a review of the project's current code or database. Table names below identify the core functional footprint and integration dependencies. Phase 0 must establish the exact tables, fields, enums, keys, and posting relationships present in the copied schema. AX 2012 and D365 are related products, not interchangeable database versions. Microsoft CDM references describe published D365 metadata; they do not prove the schema or business logic in your project.

## 1. Scope and release decisions

### Included in the basic release

- Stocked, purchased and sold items; item groups and item model groups.
- Inventory unit, basic purchase/sales unit conversion, and item barcode lookup.
- Site and warehouse dimensions; one company per transaction.
- Opening balances, movement journals, inventory adjustment journals, transfer journals, and counting journals.
- Inventory transactions, physical on-hand, physical reservation, and available physical quantity.
- PO receipt/invoice and SO delivery/invoice inventory integration, including partial quantities.
- Customer returns and vendor returns linked to their original transactions.
- One costing method, inventory value, ledger integration, period close, and reconciliation.
- Auditability, reversals, company isolation, concurrency, and safe posting retries.

### Explicitly outside this release

Advanced WHS work, waves, loads, license plates, mobile warehouse processes, advanced inventory-status workflows, manufacturing, production, BOM journals, routes, formulas, MRP, quality/quarantine workflows, landed cost, consignment, intercompany inventory, projects, retail/POS, catch weight, and advanced replenishment.

Product variants, batch and serial tracking, expiry/FEFO, bin/location tracking, and inventory blocking are deferred. A later phase may activate these using existing dimensions; no dummy values should be inserted now. A basic warehouse is still required even though advanced warehouse management is excluded.

Transfer orders with transit are an optional extension after the basic release; immediate transfer journals satisfy the initial transfer requirement.

### Recommended first-release configuration

| Setting | Initial decision | Reason |
| --- | --- | --- |
| Costing | FIFO only | One supported periodic model is enough for a basic release; it still requires close and settlement |
| Inventory dimensions | Site + warehouse | Minimal practical stock segregation |
| Financial inventory dimensions | Site only; warehouse not financial | Value pooled by site, stock tracked by warehouse |
| Physical negative inventory | Disabled | Prevent quantities falling below allowed available stock |
| Financial negative inventory | Disabled initially | Require financially supported issues; invoice receipts before dependent financial issues when necessary |
| Include physical value in estimated cost | Disabled initially | Simpler estimate basis; received-not-invoiced quantities remain visible physically |
| Reservation | Manual physical reservation only | No ordered reservation, automatic reservation hierarchy, or soft allocation engine |
| Currency | Accounting currency in inventory engine | Shared AP/AR services perform currency conversion before financial inventory posting |
| Returns | Original-document reference required | Avoid unsupported guesses about cost reversal |
| Posting | All-or-nothing per posting request | No partially committed journal |

These are scope decisions for this implementation, not universal Microsoft defaults. If an issue lacks a supported cost basis or financial receipt coverage, block it with an actionable validation message. Do not silently enable negative inventory.

## 2. Core data model and ownership

Preserve copied AX/D365 scalar columns. Use existing fields and enums; add navigation properties, EF configuration, relationships, indexes or alternate keys only where compatible with the project's schema policy. Do not invent an Item.StockQty or use a replacement stock-balance table.

### Core table catalog

| Area | Principal tables | Purpose and expected owner |
| --- | --- | --- |
| Company/setup | DataArea, CompanyInfo, InventParameters | Existing company context and inventory configuration |
| Number sequences | NumberSequenceTable, NumberSequenceReference, NumberSequenceScope; applicable sequence state/list tables in target version | Shared numbering service for items, journals, vouchers and inventory transaction IDs |
| Sites/warehouses | InventSite, InventLocation | Inventory-owned basic storage masters |
| Dimension values | InventDim | Reusable dimension combinations; validate company and warehouse/site relationship |
| Dimension setup | EcoResStorageDimensionGroup, EcoResStorageDimensionGroupFldSetup, EcoResStorageDimensionGroupItem; EcoResTrackingDimensionGroup, EcoResTrackingDimensionGroupFldSetup, EcoResTrackingDimensionGroupItem | Active physical/financial dimensions and released-item assignments; verify target-version mappings |
| Shared products | EcoResProduct, EcoResProductTranslation | Product identity and translated names |
| Released items | InventTable, InventTableModule | Company-specific item and inventory/purchase/sales unit and module settings |
| Item groups | InventItemGroup, InventItemGroupItem | Group definition and item assignment |
| Model groups | InventModelGroup, InventModelGroupItem | Stocked/model/negative-inventory policies and item assignment |
| Units | UnitOfMeasure, UnitOfMeasureConversion; applicable conversion support tables | Shared unit identity and conversions, including product-specific conversion where supported |
| Barcode | InventItemBarcode | Simple barcode lookup; not a warehouse scanning workflow |
| Default ordering | InventItemInventSetup, InventItemPurchSetup, InventItemSalesSetup | Existing default site/warehouse, quantity and stopped-order controls as supported |
| Posting setup | InventPosting, MainAccount, DimensionAttributeValueCombination and relevant shared dimension tables | Inventory posting account resolution and ledger dimensions |
| Journal definitions | InventJournalName | Journal type, voucher and account defaults |
| Journal documents | InventJournalTable, InventJournalTrans | Journal header and lines; counting and transfer use type-specific fields on these tables |
| Source tracking | InventTransOrigin; applicable origin link tables such as InventTransOriginPurchLine and InventTransOriginSalesLine | Source identity and traceability |
| Transaction detail | InventTrans | Quantity, receipt/issue state, dimensions, posting dates and inventory cost |
| Current stock summary | InventSum | Derived current totals by item/dimensions, maintained by the posting/reservation engine |
| Inventory-to-ledger trace | InventTransPosting | Physical/financial posting references; use target-version relationships |
| Cost settlement | InventSettlement, InventClosing | Cost adjustments/settlement trace and close/recalculation run information |
| General ledger | GeneralJournalEntry, GeneralJournalAccountEntry, SubledgerVoucherGeneralJournalEntry | Owned by shared ledger posting service; additional subledger staging tables depend on existing architecture |

Do not assume each table has DataAreaId. Shared products, unit tables, and cross-company assignment tables have different ownership patterns. Do not apply a blind company filter to every entity. Some assignment tables use ItemDataAreaId or other company columns instead.

### Purchase and sales integration tables

| Flow | Read/update source documents | Posted evidence owned by source module | Other downstream dependencies |
| --- | --- | --- | --- |
| PO receipt | PurchTable, PurchLine | VendPackingSlipJour, VendPackingSlipTrans | Inventory engine and optional receipt accrual ledger |
| PO invoice | PurchTable, PurchLine; posted receipt matching | VendInvoiceJour, VendInvoiceTrans | VendTrans and relevant tax/subledger tables owned by AP |
| SO delivery | SalesTable, SalesLine | CustPackingSlipJour, CustPackingSlipTrans | Inventory engine and optional physical COGS ledger |
| SO invoice | SalesTable, SalesLine; posted delivery matching | CustInvoiceJour, CustInvoiceTrans | CustTrans and relevant tax/subledger tables owned by AR |
| Customer/vendor return | Corresponding Sales/Purch documents and original posted references | Corresponding credit/delivery/receipt records | Existing return-reference/origin relationships and cost-settlement logic |

This inventory project consumes existing purchasing, sales, AP, AR and GL contracts. Build only the inventory integration needed if those modules are incomplete; do not expand into full procurement, sales pricing, tax, payments or collections.

### Essential relationships

- InventTable.Product references shared EcoResProduct identity.
- InventTrans.InventTransOrigin references InventTransOrigin.RecId.
- Source-line InventTransId resolves to the appropriate origin, using company and target-version link metadata.
- One origin can have multiple InventTrans rows. Partial receipt, reservation and delivery require splits.
- InventTrans.InventDimId references InventDim; InventSum summarizes item/dimension balances.
- Journal lines reference their header, item, from-dimensions and, for transfers, destination dimensions and transaction identity.
- InventSettlement and InventTransPosting references must be mapped from actual foreign keys. Do not join every voucher table on voucher text alone.

## 3. Non-negotiable engine behavior

1. Draft document editing changes demand/supply where applicable, but does not change physical inventory.
2. Every posted stock movement has a source, item, inventory-unit quantity, dimensions, date and audit trail.
3. The source line is not the stock ledger. InventTrans is the transaction detail; InventSum is its maintained current summary.
4. Reservation changes availability, not physical stock or inventory value.
5. Financial updates of already received/delivered quantities must not move the physical stock a second time.
6. Physical and financial dates and costs remain separate.
7. Posted documents are immutable. Corrections use supported cancellation/reversal or compensating documents with traceable references.
8. Stock checks and stock consumption occur under concurrency protection in the same transaction.
9. Identical posting retries return the existing result, not a second stock movement or voucher.
10. Closed periods and incompatible configuration changes are rejected.

### Simplified state model for the supported scope

| Event | Receipt state | Issue state | Physical quantity effect |
| --- | --- | --- | --- |
| Create PO line | Ordered | None | 0 |
| Post PO product receipt | Received | None | +received quantity |
| Invoice received PO quantity | Purchased | None | 0 additional |
| Create SO line | None | OnOrder | 0 |
| Reserve physical stock | None | ReservPhysical on reserved split | 0; available quantity decreases |
| Deliver SO quantity | None | Deducted | -delivered quantity |
| Invoice delivered SO quantity | None | Sold | 0 additional |
| Positive posted quantity journal | Purchased | None | +quantity; physical and financial update together |
| Negative posted quantity journal | None | Sold | -quantity; physical and financial update together |

Registered/Arrived/ReservOrdered/Picked and other states must not be exposed as working features unless their processes are implemented. Read enum values from the copied version; do not hard-code guessed integer values. Returns use the opposite receipt/issue direction of the original flow and require their own transitions.

### Availability contract

For this restricted scope with no advanced blocking, picking or ordered reservations:

Available physical = physical on-hand - physically reserved quantity.

Ordered receipts and outstanding demand are shown separately and never counted as physically available. Never calculate physical on-hand by summing all InventTrans.Qty regardless of status. Ordered transactions are not stock. A financially updated receipt/issue remains part of physical stock history; it is not added a second time.

Historical on-hand and value must be reconstructed using dated movements, financial adjustments and reversals. Today's InventSum is not an as-of-date report.

## 4. Phased delivery

Each phase ends with working API behavior, documented table reads/writes and an observable acceptance result. Later screens should not be used to conceal incomplete posting logic.
Build and verify the inventory core, FIFO costing, reports, and migration preparation first. Complete purchase, sales, and returns integration in the final three phases; run end-to-end validation and go-live after those integrations are complete.

### Phase 0 — Inspect the project and freeze the baseline

**Goal:** establish one exact target schema and a bounded implementation contract.

**Work:** inspect Shared classes, DbContext mappings, base controller/service patterns, unit of work, validators, Mapster configuration, company context, authorization, numbering, GL posting, Purch/Sales integration and existing inventory features. Identify implemented, partial and missing behavior separately. Map actual scalar fields, decimal precision, date rules, primary/alternate keys and enums. Record whether the database is copied AX 2012 or a specific D365 version; do not mix both layouts.

**Tables:** inspect all catalog tables above. No operational stock writes.

**Deliverables:** schema map, current-feature matrix, dependency map, supported settings, and phased backlog. Include every missing table or posting service as a blocker rather than silently adding scalar schema changes.

**Example:** InventModelGroupItem exists: use its assignment. Do not also add a ModelGroupId column to InventTable. If the copied version stores a relation differently, use that verified relation.

**Exit:** every planned table is identified as present, mapped differently, or missing; source/GL integration owners are explicit.

### Phase 1 — Basic configuration

**Forms/APIs:** inventory parameters, item groups, item model groups, sites, warehouses, storage/tracking dimension group setup, inventory journal names, posting profiles, number-sequence references, and shared unit lookup/conversion.

**Logic:** enforce site/warehouse ownership, allowed dimensions, one costing method, mandatory posting accounts and sequence scopes. Turn off advanced WHS behavior. Use the shared ledger-period calendar. Prevent changing valuation or financially tracked dimensions on an item with transactions through ordinary setup editing.

**Table impact:** setup catalog tables R/W; MainAccount and shared financial dimensions R; no InventTrans/InventSum operational quantities.

**Example setup:** company DEMO; currency SAR; sites JED and RUH; warehouses JED-MAIN and JED-SHOP in JED, RUH-MAIN in RUH. Item group TRADE; model FIFO-BASIC; inventory unit EA; journal names OPEN/MOVE/ADJ/TRANS/COUNT. Accounts: inventory 130100, opening clearing 390100, gain 490100, loss 590100, consumption 610100, receipt accrual 210200, COGS 510100.

**Exit cases:** reject warehouse JED-MAIN under RUH; reject missing posting account; issue unique journal/voucher numbers concurrently; closed ledger period blocks posting later.

### Phase 2 — Products and released items

**Forms/APIs:** released-item list/details, unit settings, barcode maintenance, group assignment, model assignment, and basic order defaults/stopped flags.

**Logic:** create/reuse shared product identity, release an item to a company, maintain inventory/purchase/sales settings, validate required groups and dimensions. Use existing supported stopped controls, not an invented lifecycle table. A barcode resolves item and unit; barcode lookup does not itself move inventory.

**Table impact:** EcoResProduct, EcoResProductTranslation, InventTable, InventTableModule, item/model/dimension assignment tables, InventItemBarcode, default order setup tables R/W; group/unit definitions R. No posted stock.

**Example:** ITEM-001, Trading item, EA inventory/sales unit; BOX purchase unit; 1 BOX = 10 EA. Purchase 3 BOX produces 30 EA in inventory. Line amount SAR 300 gives cost SAR 10/EA. ITEM-002 has EA only. ITEM-003 is stopped for sales but may still support allowed adjustment transactions according to configured controls.

**Exit cases:** duplicate item in same company rejected; shared product can be released to another company without sharing stock; unknown conversion rejected; unsupported batch/serial group rejected; non-stocked service cannot use quantity journals. Full service-order behavior is outside this release.

### Phase 3 — Dimensions, inventory engine and inquiries

**APIs:** resolve dimensions, transaction inquiry, on-hand inquiry, source trace and internal posting contracts.

**Logic:** canonical dimension find-or-create, company-scoped origins, split transactions by quantity/state/dimension, maintain InventSum within the same transaction, and implement stock/reservation locking. No independent public endpoint may edit InventSum quantities.

**Table impact:** InventDim, InventTransOrigin, applicable origin link tables, InventTrans, InventSum R/W; source/configuration tables R. Introduce ledger integration contract here; complete monetary posting in Phase 4.

**Example:** existing 100 EA, physical reservation 20 => physical 100, reserved 20, available 80. An open PO for 50 shows ordered receipts 50 but available remains 80. After delivery of the reserved 20: physical 80, reserved 0, available 80.

**Concurrency example:** 10 EA available; two independent requests each try to issue 8. Exactly one succeeds; final stock 2. No successful request may be followed by a negative-balance repair.

**Exit:** transitions maintain detail/summary agreement; dimension combinations are reused; no cross-company source resolution; meaningful concurrency test against SQL Server passes.

### Phase 4 — Opening, movement and adjustment journals

**APIs:** create/edit/delete draft, validate, post, posted-document inquiry and supported reversal.

**Logic:** OPEN is a movement journal configuration with an opening offset; it is not a new AX journal type. Movement allows explicit approved offset account. Adjustment resolves gain/loss accounts from posting setup. Positive quantity needs explicit supported receipt value. Negative quantity is valued by the engine, not arbitrary client input. Physical and financial update occur together. Validate every line before committing the entire journal.

**Table impact:** InventJournalTable, InventJournalTrans, InventDim, InventTransOrigin, InventTrans, InventSum, InventTransPosting R/W; InventPosting R; shared ledger/subledger tables W through ledger service. Origin/link details depend on actual version.

**Examples:** opening +100 EA at SAR 10 => inventory +1,000; Dr Inventory 1,000 / Cr Opening clearing 1,000. Movement -5 at cost 10 => stock 95, Dr Consumption 50 / Cr Inventory 50. Adjustment +3 at cost 10 => stock 98, Dr Inventory 30 / Cr Gain 30. Adjustment -2 => stock 96, Dr Loss 20 / Cr Inventory 20.

**Exit cases:** zero quantity rejected for these journal types; missing offset rejected; duplicate post returns existing voucher; a failing line rolls back the entire journal and ledger; posted quantity cannot be edited.

### Phase 5 — Immediate warehouse transfer journals

**Logic:** one line produces a linked source issue and destination receipt. Validate different dimensions and available source stock. Preserve company-level quantity and transferred cost. For same-site warehouses that are not financially tracked separately, there is no inventory gain/loss and no net ledger revaluation. Cross-site transfers need appropriate cost-pool and financial-dimension accounting; enable only after verified.

**Table impact:** journal header/lines including from/to dimension and transaction fields; core origin/InventTrans/InventSum tables R/W; posting trace/ledger only where the configured financial-dimension change requires it. InventTransferTable is not needed for transfer journals.

**Example:** JED-MAIN 100 EA and JED-SHOP 0. Transfer 30 => 70 and 30. At SAR 10 cost, transferred value is 300. Company quantity remains 100 and company inventory value remains 1,000.

**Exit cases:** source=destination rejected; transfer 101 rejected; paired transactions committed atomically; destination failure leaves source unchanged; source reservations cannot be consumed by an unrelated transfer.

### Phase 6 — Physical counting

**Logic:** create count lines by item/dimensions, record expected quantity and count, validate, then post only the difference. Basic implementation locks affected stock from count start until completion/cancellation; enforce this in the central posting/reservation engine. Define abandoned count release behavior. Do not post a stale count against moved inventory.

**Table impact:** InventJournalName, InventJournalTable, InventJournalTrans including counting fields; InventItemGroup/InventModelGroup and posting setup R; core inventory/posting/ledger tables W only for nonzero differences. Counting groups can be deferred if manual count selection satisfies the basic scope.

**Examples:** expected 50, counted 47 => adjustment -3, loss SAR 30 at cost 10. Expected 50, counted 52 => +2 and gain SAR 20, using supported receipt-cost policy. Expected 50, counted 50 => posted count evidence, no stock or value movement.

**Exit cases:** negative count rejected; duplicate item/dimension count lines rejected; stock movement during locked count rejected; cancel releases locks; concurrent counts on the same stock are blocked.

### Phase 7 — FIFO close and reconciliation

**Logic:** implement running estimated issue cost, financially eligible receipt matching, deterministic FIFO settlement by financial sequence within active financial dimensions, cost adjustment propagation, close history and closed-date enforcement. Do not select an arbitrary warehouse FIFO pool when warehouse is not financially tracked. Recalculation and close are distinct operations; close establishes the inventory posting boundary. Handle cancellation/reversal of close in dependency order if exposed; otherwise block that operation explicitly in the first release.

**Table impact:** InventTrans costs/settlement state, InventSettlement, InventClosing, InventSum financial summaries, posting trace and shared ledger adjustment entries. Read source documents, model-group and financial-dimension configuration.

**Worked FIFO case:** receipt A 100 EA at 10 = 1,000; receipt B 100 EA at 12 = 1,200; both financially posted before issue. Sell 150 EA. Running average estimated issue cost = 2,200 / 200 = 11 => provisional COGS 1,650, remaining value 550. FIFO close matches 100 at 10 plus 50 at 12 => final COGS 1,600. Reduce COGS 50 and increase remaining inventory value 50. Closing stock is 50 EA at 12 = 600. Close changes value, not physical quantity.

**Late invoice case:** receive 100 at provisional 10, invoice at 12, and sell 40 before final invoice cost is known. When all financial prerequisites are complete, final receipt cost 1,200 must be allocated to 40 issued units (480 COGS) and 60 remaining (720 inventory). A receipt invoice adjustment of 200 cannot be assigned entirely to stock when some quantity was issued. If initial financial-negative policy blocks the sales invoice, deliver physically and defer its financial posting until receipt invoicing is complete.

**Close controls:** report received-not-invoiced and delivered-not-invoiced quantities; unresolved eligible settlement; negative/unsupported financial balances; missing accounts; transaction/summary mismatch; and GL differences. Diagnose or block unresolved issues according to documented policy. No fabricated settlement against an unposted invoice.

**Exit:** journal-driven FIFO case produces exactly 600 inventory/1,600 COGS; rerun does not duplicate adjustments; closed-date posting blocked; valuation agrees with inventory ledger control accounts after explicitly identified timing differences. Recheck invoice-cost propagation to dependent issues, returns, and transfers during the final integration phases.

### Phase 8 — Reporting and migration preparation

**Reports:** current on-hand, available/reserved, transaction history, movement by period, stock card, inventory value as of date, journal history, count differences, open receipt/delivery accruals, and inventory-to-GL reconciliation. Reports filter by company, item, site, warehouse and applicable dates. Quantity by warehouse is always available; warehouse value allocation must not be presented as an independent financial cost pool when only site is financial.

**Table impact:** reporting reads inventory/source/GL tables; exports and report DTOs do not become another stock source of truth. Migration writes through validated opening journals and shared master-data services.

**Migration:** clean masters; create required dimension/group assignments; map accounts; establish cutover date; freeze legacy movement; import opening quantity/value by item/active financial dimensions; reconcile totals; avoid double-loading stock both as opening balance and historical PO receipt. Tie opening clearing to migration GL balances exactly once.

**Exit:** journal-based reports, reconciliation, opening-balance migration, rollback, concurrent posting, company isolation, and close pass against real SQL Server. Roles distinguish setup, draft entry, posting and close. No direct API edits of posted stock/cost summaries. Complete source-document reports and final reconciliation after purchase, sales, and returns integration.

### Phase 9 — Purchase inventory integration

**Logic:** creation creates ordered receipt quantities. Product receipt moves only the selected quantity to physical received stock. Invoice changes the received quantity's financial state/value, without adding stock again. Split remaining ordered, received-not-invoiced and purchased quantities. Initially require receipt before invoice; direct invoice posting is an explicit unsupported route until implemented.

**Table impact:** PurchTable/PurchLine R/W by purchasing service; VendPackingSlipJour/Trans W on receipt; VendInvoiceJour/Trans and VendTrans W on invoice by AP; core inventory tables R/W, InventTransPosting and ledger tables W as required.

**Example:** PO 100 EA at SAR 10. Order => physical 0, ordered 100. Receive 60 => physical 60, open order 40, received-not-invoiced 60. Invoice 60 at SAR 11 => physical still 60, financial receipt value 660. Receive remaining 40 => physical 100, open order 0. Invoice 40 at SAR 11 => financial receipt value 1,100 and physical still 100.

**Simplified accounting example:** if receipt accrual enabled, receive 60 at 10: Dr Inventory received-not-invoiced 600 / Cr Receipt accrual 600. Invoice at 11 clears accrual/provisional entries and produces final inventory 660 against AP 660. Resolve actual accounts and voucher lines from posting setup; do not hard-code this illustration as Microsoft's exact voucher structure. Without receipt accrual, receipt still changes physical stock and records provisional cost; invoice creates financial GL recognition.

**Exit cases:** invoice 61 against only 60 received blocked under initial policy; overreceipt blocked; receipt cancellation blocked if quantity has been invoiced/consumed unless supported dependencies are reversed; cancel unreceived remainder does not remove received stock; unit conversion preserves inventory quantity and line value.

### Phase 10 — Sales demand, reservation, delivery and invoice

**Logic:** source order creates outstanding issue demand. Reserve/release physical stock manually; partial reservation is supported explicitly. Deliver selected quantities against own reservations plus eligible free stock. Invoice only delivered quantities initially. Financial invoice updates inventory cost and source-module revenue/AR; it does not deduct stock again. Cancellation releases only remaining reservations/demand.

**Table impact:** SalesTable/SalesLine R/W by sales service; CustPackingSlipJour/Trans W on delivery; CustInvoiceJour/Trans and CustTrans W on invoice by AR; core inventory tables R/W, posting trace and ledger W as required.

**Example:** start 100 EA at cost 10. SO 30 at sales price 15 => physical 100. Reserve 30 => available 70. Deliver 20 => physical 80, reserved remainder 10, available 70. Invoice 20 => physical remains 80; revenue 300, AR 300, provisional COGS 200 subject to FIFO settlement. Deliver/invoice remaining 10 => physical 70, reservation 0, total revenue 450, provisional COGS 300.

**Simplified financial entries:** Dr AR 300 / Cr Revenue 300; Dr COGS 200 / Cr Inventory 200. Where physical COGS posting is enabled, invoice clears physical provisional entries and posts financial entries rather than recognizing inventory loss twice. Tax is handled by the existing tax service and is excluded from numeric examples.

**Exit cases:** deliver beyond available rejected; unrelated line cannot consume another line's reservation; deliver/invoice partial quantities independently; cancel remainder releases its reservation; sales-stopped item cannot create new sales demand; delivered quantities require supported return/cancellation, not deletion.

### Phase 11 — Customer and vendor returns, final integration and go-live

**Logic:** linked returns only; validate original company, item, dimensions where required, invoiced/received quantities, and remaining returnable quantity. Derive return cost from the original supported transaction and carry later cost corrections consistently. Require goods receipt for inventory-affecting customer credits and goods issue for inventory-affecting vendor credits. Pure financial credits are separate source-module behavior.

**Table impact:** existing Sales/Purch source and return-reference fields/tables; corresponding posted receipt/delivery/invoice tables and AR/AP tables; inventory origin/detail/summary/posting tables; applicable settlement and ledger entries. Verify specific return linkage in copied schema.

**Customer example:** original sale 20 EA at price 15 and cost 10; return 5 => physical +5, revenue reversal 75, AR credit 75, inventory restored 50, COGS reversal 50. This assumes the original final cost is 10. If close later adjusts the original issue, linked return cost must follow the defined settlement policy.

**Vendor example:** original purchase 100 EA invoiced at 11; return 10 => physical -10, AP credit 110 and inventory value reduction 110 when linked return valuation is supported. Do not allow an unlinked return to force an arbitrary cost.

**Exit cases:** return more than original unreturned quantity rejected; duplicate return posting rejected; customer credit does not double-receive stock; vendor credit does not double-issue stock; insufficient vendor-return available quantity rejected; return from a closed original period uses an allowed current date and preserves original references.

**Final integration exit:** run all purchase, sales, and returns acceptance cases against real SQL Server, including partial quantities, rollback, concurrent posting, company isolation, invoice-cost propagation through FIFO close, source-document reports, and inventory-to-GL reconciliation. Complete cutover and go-live only after those checks pass; document supported operations and deferred features.

## 5. Complete basic-cycle example

All amounts SAR, no tax, site JED, cost 10/EA for every movement. Main and Shop warehouses share one site financial pool. All inbound quantities are financially posted before dependent financial issues. Returns are linked; figures are final inventory cost, not revenue.

| Step | Event | Main EA | Shop EA | Company EA | Inventory value | Main physical reservation | Main available |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| 0 | Initial | 0 | 0 | 0 | 0 | 0 | 0 |
| 1 | Opening +100 Main | 100 | 0 | 100 | 1,000 | 0 | 100 |
| 2 | Purchase receive and invoice +50 Main | 150 | 0 | 150 | 1,500 | 0 | 150 |
| 3 | Reserve 40 Main | 150 | 0 | 150 | 1,500 | 40 | 110 |
| 4 | Deliver/invoice reserved 40 | 110 | 0 | 110 | 1,100 | 0 | 110 |
| 5 | Transfer 30 Main to Shop | 80 | 30 | 110 | 1,100 | 0 | 80 |
| 6 | Count Shop at 28; difference -2 | 80 | 28 | 108 | 1,080 | 0 | 80 |
| 7 | Customer return +5 to Main | 85 | 28 | 113 | 1,130 | 0 | 85 |
| 8 | Vendor return -10 from Main | 75 | 28 | 103 | 1,030 | 0 | 75 |
| 9 | Internal consumption -3 Main | 72 | 28 | 100 | 1,000 | 0 | 72 |
| 10 | Adjustment gain +2 Shop | 72 | 30 | 102 | 1,020 | 0 | 72 |

Proof: 100 opening +50 purchase -40 sale -2 count +5 customer return -10 vendor return -3 consumption +2 gain = 102 EA. Transfer has zero company-level quantity effect. Value = 102 × 10 = 1,020. FIFO close has no adjustment in this equal-cost example.

## 6. Basic-scope acceptance case matrix

Each case is independent unless it refers to the full-cycle example. Use real SQL Server for concurrency/transaction behavior, not EF's in-memory provider.

| ID | Case / input | Expected result |
| --- | --- | --- |
| B01 | Create item without model group | Validation failure; no operational stock |
| B02 | Same item ID in different companies | Allowed if independently released; balances isolated |
| B03 | Warehouse/site mismatch | Reject dimension resolution/posting |
| B04 | Resolve same active dimension combination twice | Reuse same dimension identity |
| B05 | Purchase 3 BOX, 10 EA/BOX, amount 300 | Inventory 30 EA; cost 10/EA |
| B06 | Missing BOX-to-EA conversion | Reject before posting |
| B07 | Barcode lookup | Resolves correct item/unit; no quantity movement |
| B08 | Nonstocked service in inventory journal | Reject; no InventSum stock |
| B09 | Opening +100 at 10 | Physical/financial quantity 100; value 1,000 |
| B10 | Adjustment -4 from 100 at 10 | Stock 96; loss 40 |
| B11 | Movement -3 to consumption from 10 at 10 | Stock 7; expense 30 |
| B12 | Multi-line journal includes invalid line | Entire request rolled back, including ledger |
| B13 | Same journal posted twice | One stock effect and one logical voucher result |
| B14 | Edit posted journal quantity | Reject; correction requires supported reversing document |
| B15 | Two simultaneous 8-unit issues against 10 | One succeeds; other fails/revalidates; final 2 |
| B16 | Reserve 30 of 100 | Physical 100; reserved 30; available 70 |
| B17 | Reserve 80 when only 70 available | Reject or explicitly reserve only 70 through partial-reserve command |
| B18 | Release 10 from reservation 30 | Physical 100; reserved 20; available 80 |
| B19 | Another order consumes reserved quantity | Reject unavailable portion |
| B20 | Deliver 20 of own reservation 30 | Physical -20; reserved -20; no double consumption |
| B21 | Transfer 30 Main to Shop | Paired -30/+30; company quantity/value unchanged |
| B22 | Transfer same dimensions | Reject no-op |
| B23 | Transfer exceeds free source stock | Reject, neither side changes |
| B24 | Fail destination write after source attempt | Roll back source, destination and ledger |
| B25 | Count expected 50, actual 47 | Post -3 only |
| B26 | Count expected 50, actual 52 | Post +2 only using supported receipt cost |
| B27 | Count expected 50, actual 50 | No inventory movement; retain count evidence |
| B28 | Try movement during active locked count | Reject affected movement; no stale count posting |
| B29 | PO ordered 100, receipt 60 | Physical +60; outstanding ordered 40 |
| B30 | Invoice received 60 | No extra physical +60; financial quantity/value updated |
| B31 | Invoice 61 with only 60 received | Reject under receipt-first initial policy |
| B32 | Receive above remaining PO quantity | Reject under no-overreceipt policy |
| B33 | Cancel remaining PO 40 after receipt 60 | Remove only outstanding 40; physical remains 60 |
| B34 | Sales order before delivery | Demand only; no stock decrement |
| B35 | Deliver 20 then invoice 20 | One physical -20; final financial cost once |
| B36 | Invoice quantity above delivered | Reject under delivery-first initial policy |
| B37 | Customer return 5 of original sold 20 | +5 physical once; credit and cost reversal supported |
| B38 | Customer return total exceeds original 20 | Reject excess, accounting for prior returns |
| B39 | Vendor return 10 with sufficient linked stock | -10 once; original-linked value/AP credit |
| B40 | Vendor return without available physical stock | Reject |
| B41 | Pure price credit with no goods returned | No stock movement; source financial module handles credit |
| B42 | Backdate to closed inventory period | Reject even if document was drafted earlier |
| B43 | Backdate to closed GL period | Reject even if inventory period is open |
| B44 | Change item's FIFO model after transactions | Block ordinary edit; require separately designed conversion |
| B45 | FIFO receipts 100@10 and 100@12, issue 150 | After close: COGS 1,600; remaining 50 worth 600 |
| B46 | Receipt invoice cost increase after issue | Propagate cost to issue and remainder; no quantity change |
| B47 | Repeat completed close/post request | No duplicate settlements or GL adjustments |
| B48 | Lookup same voucher text in another company | Never join to wrong company/source |
| B49 | Historical report before later movement | Excludes later quantity/cost events according to dates; not today's InventSum |
| B50 | Rebuild/check stock summary from transaction detail | Same current quantities; differences diagnosed before repair |
| B51 | Negative quantity journal asks arbitrary issue cost | Reject supplied override or ignore by documented contract; engine calculates cost |
| B52 | Ledger write fails during inventory posting | Entire posting rolls back |
| B53 | Return original issue later adjusted by close | Linked return value follows tested settlement policy |
| B54 | Cross-site transfer | Reject until financial pool/account propagation is implemented and tested |
| B55 | Inactive dimensions supplied in request | Reject; do not silently create hidden stock partitions |
| B56 | As-of inventory value vs GL | Reconcile posted inventory accounts plus explicitly listed timing differences |

This matrix covers the declared basic scope. It is not a claim to cover every AX/D365 feature or configuration combination.

## 7. Suggested ASP.NET Core service structure

Use existing project conventions after inspecting Shared. Proposed contracts below are application design suggestions, not Microsoft class names.

Request -> Controller -> Application service -> validators/domain posting engine -> existing unit of work/EF mappings and shared ledger/numbering services -> SQL Server.

- Controllers use service interfaces; no direct DbContext access.
- DTOs and Mapster handle API mappings. Posting calculations remain explicit domain logic; do not map client amounts directly into protected cost fields.
- IInventorySetupService: parameters, groups, dimension rules, journal names.
- IReleasedItemService: product release, assignments, units and barcodes.
- IInventoryDimensionService: canonical validation/find-or-create.
- IInventoryTransactionService: origin and quantity/state splits.
- IInventoryAvailabilityService: inquiry, physical reservation, release.
- IInventoryPostingService: single shared stock/cost posting gateway.
- IInventoryJournalService: journal lifecycle and type-specific validation.
- IInventoryCountingService: count selection, locks, difference posting.
- IPurchaseInventoryService / ISalesInventoryService: source inventory integration.
- IInventoryReturnService: linked return validation and cost linkage.
- IInventoryCostingService: estimate, FIFO settlement and adjustments.
- IInventoryCloseService: precheck, close, history and supported reversal.
- IInventoryInquiryService: stock card, quantity/value and reconciliation reports.

Use existing IBaseService/IUnitOfWork patterns where compatible, but generic CRUD must not bypass posting rules. Changes to transactions, summaries and ledger evidence must participate in one durable atomic boundary. If ledger is remote, a proper durable integration design is a separate prerequisite; do not pretend two independent commits are atomic.

Posting sequence: load and lock current document/affected balances; detect prior post; validate permissions/company/period/configuration/quantities; convert units; resolve dimensions and origin; determine inventory transitions and costs; create ledger entries through shared posting service; persist transaction detail and summary; mark posted with references; commit; then publish any optional notifications. A preview is useful but cannot replace revalidation inside the posting transaction.

Use existing source document posting state and locking for idempotency when sufficient. If durable request identity/audit storage is missing, report it as an infrastructure dependency and obtain the appropriate schema design decision; do not hide it in memory or add unauthorized scalar columns. Likewise, count locks must be durable and enforced by all stock posting paths.

## 8. Optional Phase 12 — Transfer orders with transit

Implement only if the business needs separate shipment and receipt times. This remains possible without advanced WHS but is additional scope.

**Tables:** InventTransferTable, InventTransferLine, InventTransferJour, InventTransferJourLine; applicable InventTransferParmTable/Line posting records and transfer origin-link tables; InventLocation transit setup, InventDim, core inventory and cost/ledger tables. Verify exact table usage in the copied version.

**Example:** Main starts 100. Ship 20 => Main 80, transit 20, Shop 0. Receive 12 => Main 80, transit 8, Shop 12. Receive remaining 8 => Main 80, transit 0, Shop 20. Company total remains 100 throughout, including transit. Handle partial shipment/receipt and reject overreceipt. This is not implemented by placing a delay between the two halves of an ordinary transfer journal.

## 9. Completion definition

The basic module is complete when a user can configure and release a stocked item, load opening inventory, receive and invoice a purchase, reserve and deliver/invoice a sale, transfer, count, adjust, return goods, close FIFO inventory and reconcile value to GL, with source trace and correct concurrent behavior.

Do not call the module complete when only master CRUD exists, when on-hand is editable, when FIFO close is missing, or when financial posting is simulated. Unsupported features must be explicitly disabled and documented.

## 10. Microsoft references

These sources support the inventory concepts and selected table identities. Implementation phases, simplified account examples, service names and acceptance cases above are the proposed design for this project.

- [Physical and financial updates](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/physical-financial-updates)
- [Inventory journals](https://learn.microsoft.com/en-us/dynamics365/supply-chain/inventory/inventory-journals)
- [Inventory transaction details](https://learn.microsoft.com/en-us/dynamics365/supply-chain/inventory/inventory-transactions-details)
- [FIFO with physical value and marking](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/fifo-physical-value-marking)
- [Inventory costing FAQ](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/inventory-costing-faq)
- [InventTransOrigin metadata](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/inventory/transaction/inventtransorigin)
- [InventModelGroupItem metadata](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/inventory/main/inventmodelgroupitem)
- [InventJournalTrans metadata](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/inventory/worksheetline/inventjournaltrans)
- [InventSettlement metadata](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/inventory/transactionline/inventsettlement)
- [InventClosing metadata](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/inventory/transactionheader/inventclosing)
