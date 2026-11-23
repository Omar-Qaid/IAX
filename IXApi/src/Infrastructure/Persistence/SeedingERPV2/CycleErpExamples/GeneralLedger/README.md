# General Ledger transaction examples

The package now has separate configuration/master preparation and transaction registrations, a cycle seeder, typed options, execution reports, scenario data, and a transaction flow. Use `AddGeneralLedgerConfigurationAndMasterDataExamples` for preparation and `AddGeneralLedgerTransactionExamples` for service-driven transactions. The existing integer-year overload remains available.

Two transaction scenarios are implemented. `GeneralLedgerTransactionExamples.Scenarios` lists them, and omitting `ScenarioIds` runs both in ID order. The year-only `RunAsync` overload runs GL-01 alone.

[Scenarios/README.md](Scenarios/README.md) maps the catalog rows (GL-01 … GL-26 in `AllModules/Secnarios.md`, F1 … F6 in `AllModules/FullExample.md`) to the seeded scenarios or covering tests. The catalog numbering differs from the seeded IDs.

The fifteen-step §2 cycle of `Logic_Readonly.md` is seeded as GL-02 ([details](Scenarios/02-ReferenceCycle/README.md)). It asserts the 6,300 trial balance and the 1,200 profit over its own vouchers. The §5 year-end close is not seeded, because it would close the year for every example in the company. `General_ledger_reference_cycle_balances_at_six_thousand_three_hundred_and_carries_twelve_hundred_to_equity` in `Tests/GeneralLedgerCycleTests.cs` still proves it.

Register `AddGeneralLedgerTransactionExamples` in an explicit example host and resolve `GeneralLedgerTransactionExamples` from a fresh scope. The host supplies the application DbContext and company and current-user services. Setup uses the shared Foundation, AP, AR, Tax, Cash and Bank, Inventory and GL registrations, plus the example journal name and voucher sequence.

| Scenario | Business service calls | Accounting evidence |
| --- | --- | --- |
| GL-02 reference cycle | Purchase, receipt, vendor invoice, sales order, reservation, two deliveries and invoices, two customer receipts with settlement, vendor payment, inventory close, accrual journal | Over the scenario's vouchers: 6,300 debit and 6,300 credit, net profit 1,200, customer 0, vendor 3,000 open, inventory 3,500. |
| GL-01 expense reclassification | Create the daily journal and line, post, and reverse the posted journal the next day | 125 SAR debit to maintenance expense and credit to delivery expense. The reversal mirrors both amounts. Each voucher links to one posted entry and balances in accounting currency. |

The example uses existing demo expense dimensions and posts a net-zero reclassification across the two dates. AP and AR control accounts and the inventory valuation accounts are outside the example journal. The deterministic journal name and reversal link allow replay to reuse both documents. A conflicting source journal or line fails validation.

Use the opt-in SQL Server test in `SeedingErpV2SqlServerTests` to exercise setup across all modules and run the GL example twice. The test checks table counts before and after replay. Separate service suites cover GL posting, reversal, period controls, validation, settlement, and trial balance.
