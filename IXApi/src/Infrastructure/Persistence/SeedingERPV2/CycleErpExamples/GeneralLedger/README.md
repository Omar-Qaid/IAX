# General Ledger transaction examples

The package now has separate configuration/master preparation and transaction registrations, a cycle seeder, typed options, execution reports, scenario data, and a transaction flow. Use `AddGeneralLedgerConfigurationAndMasterDataExamples` for preparation and `AddGeneralLedgerTransactionExamples` for service-driven transactions. The existing integer-year overload remains available.

GL-01 is the only implemented transaction scenario. The package still requires the remaining GL scenario definitions, preparation manifests, and comprehensive reconciliation before it matches the AP package's coverage.

The fifteen-step §2 cycle of `Logic_Readonly.md` is **not** a scenario here. It is executed instead by `General_ledger_reference_cycle_balances_at_six_thousand_three_hundred_and_carries_twelve_hundred_to_equity` in `Tests/GeneralLedgerCycleTests.cs`, which drives all fifteen steps through the real purchase, sales, payment, settlement, inventory-close and journal services and asserts the reference's 6,300 trial balance, its 1,200 result and the 4,500 year-end opening. Promoting that cycle into a seeded scenario here is still open work; until then the test, not this package, is what proves those figures.

Register `AddGeneralLedgerTransactionExamples` in an explicit example host and resolve `GeneralLedgerTransactionExamples` from a fresh scope. The host supplies the application DbContext and company and current-user services. Setup uses the shared Foundation, AP, AR, Tax, Cash and Bank, Inventory and GL registrations, plus the example journal name and voucher sequence.

| Scenario | Business service calls | Accounting evidence |
| --- | --- | --- |
| GL-01 expense reclassification | Create the daily journal and line, post, and reverse the posted journal the next day | 125 SAR debit to maintenance expense and credit to delivery expense. The reversal mirrors both amounts. Each voucher links to one posted entry and balances in accounting currency. |

The example uses existing demo expense dimensions and posts a net-zero reclassification across the two dates. AP and AR control accounts and the inventory valuation accounts are outside the example journal. The deterministic journal name and reversal link allow replay to reuse both documents. A conflicting source journal or line fails validation.

Use the opt-in SQL Server test in `SeedingErpV2SqlServerTests` to exercise setup across all modules and run the GL example twice. The test checks table counts before and after replay. Separate service suites cover GL posting, reversal, period controls, validation, settlement, and trial balance.
