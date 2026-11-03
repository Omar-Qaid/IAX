# ERP cycle examples

This directory follows the idea used by `Processes/Process590`, `Process603`, and `Process651`: a readable entry service coordinates dedicated example definitions. Required setup, reference data, and legacy imports remain under `Modules/` and are not duplicated here.

```text
CycleErpExamples/
  ErpCycleExampleService.cs       Entry point for the new connected example
  ErpCycleExampleData.cs          Stable keys, quantities and prices
  ErpCycleExampleContext.cs       Shared actor and service dependencies
  Foundation/                    Prerequisites, sample counterparties and setup
  AccountsPayable/               Purchase, receipt, invoice and vendor payment
  AccountsReceivable/            Sales, delivery, invoice and customer payment
  Inventory/                     Inventory movements, journals and worked cycles
  GeneralLedger/                 Ledger setup and general journal example
  HumanResources/                Worker and organization examples
  Administration/                Batch setup examples
  Organization/                  Collaboration and document examples
  Workflow/                      Workflow request and variable examples
```

Each module keeps its service/data classes in its own folder. Existing example classes, IDs, versions, policies, relationships and completion history are preserved when moved. The workflow process definition folders remain in `Processes/`; the Workflow folder here owns supporting demo request data.

## New connected trading dataset

`ErpCycleExampleService` is registered as `ERP.TradingCycleExample`, version 1, Demo, RunOnce, company `dat`. It runs after the existing inventory full-cycle and draft-finance examples, using their supported setup. The five new module service classes are registered through DI in `ErpCycleExamplesRegistration`.

| Module service | New example data |
| --- | --- |
| `FoundationCycleExampleService` | Validates existing DAT/SAR masters and adds vendor bank-payment method `ERP-BANK` if missing. Existing values are preserved. |
| `PurchaseCycleExampleService` | `ERP-CYCLE-PO-001`: purchases 12 `ITEM-001` chairs at SAR 100 from `VEND-100`; confirms, receives, invoices and fully pays/settles the selected invoice using `ERP-CYCLE-VPAY-001`. |
| `SalesCycleExampleService` | `ERP-CYCLE-SO-001`: sells four of the chairs to `CUST-100` at SAR 150; confirms, delivers, invoices and fully pays/settles that invoice using `ERP-CYCLE-PAY-001`. |
| `InventoryCycleExampleService` | Purchase and sale services generate their inventory movements. Adds `ERP-CYCLE-INV-001`, an unposted +2-chair adjustment for review, without changing the on-hand balance through that draft. |
| `LedgerCycleExampleService` | `ERP-CYCLE-GL-001`: posts SAR 250 from the existing cash main account to `DEMO-SAR` through the general-journal services. The Bank account type generates both ledger entries and the matching bank subledger movement. |

The setup uses `JED` / `JED-1`, `PCS`, SAR, and the existing `DEMO-SAR` bank account. Taxes, invoice totals, balances, inventory costing, vouchers and settlements are calculated by the existing business services. The eight-chair net receipt from this purchase/sale is a scenario movement, not a claim about total company on-hand, which includes other examples and user transactions.

## Execution and reruns

Enable the existing `DatabaseInitialization:Enabled` / `Seeding:Enabled` startup controls and `Seeding:EnableDemoData` in a development or isolated test environment. This change does not turn on initialization, demo data, legacy imports, or embedded fixtures in configuration. Production initialization remains prohibited.

The entry class reuses the existing seeder graph, distributed startup lock and execution tracking. It does not invoke the older seeders a second time. Each new document has a stable business/request key. A retry reuses completed stages and resumes missing stages. Conflicting, soft-deleted, or customized example documents are rejected rather than rewritten; existing payment requests are replayed with their original dates. Uniqueness is checked in the correct company scope.

Posting examples use a separate company-scoped context because several existing services begin transactions directly. Individual service operations retain their transaction boundaries and concurrency locks. The whole multi-module dataset and its outer success-history record are not one atomic transaction; stable document keys allow resumption if a later step fails. Do not call the new entry service directly in parallel outside the normal locked seeding orchestrator.

Existing Administration, Human Resources, Organization and Workflow datasets were grouped here without creating duplicate copies or changing their data. The new trading dataset covers the five core ERP module services above; it does not add advanced modules, change the database schema, or populate runtime logs and authentication tables.

## Verification

The focused seed suite verifies DI registration, dependency ordering, the opt-in/run-once contract, company isolation, preservation of custom setup, and repeatable setup behavior using isolated SQLite databases. The API build verifies service/DTO compatibility. A complete SQL Server posting run of the new trading dataset must be verified in an isolated development database before claiming that it has executed successfully; these checks do not run it against an existing application database.
