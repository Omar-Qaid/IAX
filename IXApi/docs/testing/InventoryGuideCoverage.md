# Inventory service coverage against the supplied D365 F&O guide

The table maps the guide's case IDs to IXApi service paths and existing assertions. Coverage describes what those assertions exercise. The inventory package's [acceptance evidence](../../src/Infrastructure/Persistence/SeedingERPV2/CycleErpExamples/Inventory/Data/acceptance-status.json) and the separate [SQL Server integrity results](../../src/Infrastructure/Persistence/SeedingERPV2/INTEGRITY-REFERENCE.md) record provider-specific verification.

| Case | Service path and evidence | Remaining coverage |
| --- | --- | --- |
| CFG-01 | Site, warehouse, WMS location and dimension policy. Setup and architecture tests check valid dimensions, warehouse/site mismatches and company scope. | |
| CFG-02, CST-02 | Model group and running cost. Acceptance and cycle tests exercise weighted average and the include-physical flag. | Full combination of physical/financial posting and negative-inventory policies in one matrix. |
| CFG-03, CFG-04 | Posting profiles, journal names, number sequences and fiscal periods. Setup and architecture tests cover missing accounts, closed periods and rollback. | Voucher account and financial-dimension assertions for every posting type. |
| MD-01 | Product creation, release, module defaults and categories. Architecture tests cover company release, three module rows and category assignment separately. | Combined category-to-order-default workflow. |
| MD-02, ISS-03 | Sales order and invoice with a stock item and a service. Inventory scenario 01 checks combined revenue and absence of service inventory movements. | |
| MD-03 | Unit conversion and barcode lookup. Setup tests check 2 BOX to 20 PCS through purchase and on-hand. | |
| MD-04 | Batch/serial, reservation and dimension policy. Batch and setup tests check receipt, issue, on-hand and invalid dimensions. | Tracked return to the original batch or serial and the complete insufficient-batch reservation case. |
| MD-05 | Product lifecycle and inventory status. Architecture tests check blocked transactions and sales issue revalidation. | End-to-end matrix for each operation and warehouse-level stop rule in the guide. |
| RCV-01 to RCV-05 | Purchase confirmation, receipt, vendor invoicing and receipt correction. Setup and cycle tests cover 70+30 receipt, partial financial update, price tolerance and receipt history. | |
| RES-01, RES-02 | Inventory reservation and availability. Setup and scenario tests check release, excessive demand, insufficient stock and partial reservation. | Two simultaneous requests for the last available stock on SQL Server. |
| ISS-01, ISS-02 | Packing slips and sales invoice. Cycle and replay tests cover partial delivery, Sold status, revenue and issue cost. | |
| ISS-04 | Packing-slip correction. The order-to-cash attachment test exercises corrections in the cross-module service path. | Inventory guide-specific before/after quantity and physical voucher assertions. |
| MOV-01, ADJ-01, CNT-01, CNT-02 | Journal posting. Inventory scenarios 02–05 and 08 cover offsets, shortage, overage and matching count. | |
| TRF-01, TRF-02 | Transfer journal and transfer order. Scenarios 06–07 assert warehouse quantities, transit and value conservation. | |
| TRF-03 | Inventory cost pooling and transfer services. Architecture tests cover costing by financial site. | End-to-end transfer across two financially tracked sites with both cost dimensions and voucher assertions. |
| RET-01, RET-02, RET-03 | Customer return, credit-only and vendor return. Cycle tests cover original invoice attribution, inventory and credit effects. | Exact guide example of five units returned after the original sale was adjusted to 12 per unit, with credit of 125. |
| CTL-01 | Inventory replay preserves movement and voucher counts on SQLite and SQL Server. Separate SQL Server tests verify the shared posting lock. | Concurrent attempts through the inventory posting service on the same document. |
| CST-01, CST-03, CST-04 | Running cost, weighted-average recalculation and close. Scenarios 01 and 09 assert initial COGS of 600, final COGS of 720, value of 1,080 and unchanged quantity of 90. | |
| CST-05 | Passed. The weighted-average close test verifies that the uninvoiced receipt stays outside financial settlement, physical quantity stays at 150, and the issue settles at 500 with a 50 adjustment. | Warning output for an insufficient financially updated quantity. |
| CST-06 | Inventory close, cancellation and financial date boundaries. Cycle tests exercise reversal, reclose, unique run numbers and backdated posting controls. | |
| REP-01 | On-hand and transaction inquiries. Architecture tests check dimensions and company scope. Replay reconciles on-hand against movements. | |
| REP-02 | Inventory value and physical-as-of services. Tests cover physical/financial splits, summary drift and dated movements. | One same-cutoff, account-range reconciliation against GL with separate uninvoiced receipts and deliveries. |
| REP-03 | AP/AR document linkage and transfer reconciliation. Service tests cover individual workflows. | One reconciliation proving all document pairings and explaining open quantities or timing differences across the cycle. |

Primary test sources are [InventorySetupCaseTests](../../Tests/InventorySetupCaseTests.cs), [InventoryArchitectureTests](../../Tests/InventoryArchitectureTests.cs), [InventoryCycleTests](../../Tests/InventoryCycleTests.cs), [InventoryBatchTrackingTests](../../Tests/InventoryBatchTrackingTests.cs), [InventorySeedIntegrityTests](../../Tests/InventorySeedIntegrityTests.cs) and [OrderToCashAcceptanceTests](../../Tests/OrderToCashAcceptanceTests.cs).

The dedicated inventory cycle and acceptance suite passed 147 tests, including the added CST-05 test.
