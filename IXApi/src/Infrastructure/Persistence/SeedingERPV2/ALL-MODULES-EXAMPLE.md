# All-modules seeding example

One run seeds the configuration and master data of all eleven modules for one company. The scope is basic D365 F&O only: standard tables and fields, nothing from advanced modules. No transaction is created here. Transactions run afterwards through each module's transaction examples, which call the business services.

## Entry point

```csharp
services.AddAllModulesPhaseOneSeeding();   // Core.Registration

// In a fresh DI scope, after applying the migrations:
var result = await scope.ServiceProvider.GetRequiredService<SeedRunner>()
    .RunAsync(fiscalYear: 2026, actor: "<audit-user-id>", ct);
```

The run:

1. Resolves every registered seeder into one order based on its declared `Dependencies`. A cycle or a missing dependency fails before anything is written.
2. Runs every seeder inside one serializable transaction, together with the `SysDataSeedLogs` history.
3. Runs every module's integrity validator over the combined result before it commits. Any failure rolls back the whole run.

A rerun adds only rows that are missing, so table counts do not change. Number-sequence counters and the operator's own settings are kept.

## What each module contributes

The modules are listed in the order the data depends on them: setup first, then masters, then the modules that read them.

| # | Module | Seeders | D365 tables seeded (basic scope) | Integrity validator |
|---|---|---|---|---|
| 1 | Administration | 3 | SysNumberSequence (one per D365 document reference, all non-cyclic), application settings, SysBackgroundJob | AdministrationIntegrityValidator |
| 2 | Foundation | 36 | CompanyInfo, Currency, ExchangeRateType, ExchangeRateCurrencyPair and ExchangeRate, fiscal calendar, PaymTerm, PaymSched and PaymSchedLine, CashDisc, DlvMode and DlvTerm, organization units and hierarchy, HcmPosition and HcmWorker, contact persons, financial dimensions, TaxPeriodHead and TaxReportPeriod | FoundationIntegrityValidator |
| 3 | General ledger | 7 | LedgerChartOfAccounts, MainAccount, Ledger, DimensionAttributeValueCombination, LedgerJournalName, journal number sequences, currency gain and loss accounts | covered by the AP and AR validators |
| 4 | Tax | 6 | Tax ledger posting group, TaxTable and its rates, TaxGroupHeading and TaxGroupData, TaxItemGroupHeading and TaxOnItem | TaxIntegrityValidator |
| 5 | Cash and bank | 2 | BankGroup, BankAccountTable | CashAndBankIntegrityValidator |
| 6 | Inventory | 25 | UnitOfMeasure, InventModelGroup, InventItemGroup and InventPosting, InventSite, InventLocation, InventDim, storage and tracking dimension groups, EcoResProduct, InventTable and InventTableModule, InventJournalName | covered by the AP and AR validators |
| 7 | Accounts payable | 16 | VendParameters, PurchParameters, VendGroup, VendLedgerAccounts (posting profile), VendPaymModeTable, DirPartyTable with its locations and addresses, VendTable, invoice matching policy, VendBankAccount, PriceDiscTable, MarkupTable | AccountsPayablePhaseOneValidator |
| 8 | Accounts receivable | 19 | CustParameters, SalesParameters, CustGroup, customer posting profile, CustPaymModeTable, customer party with its locations and addresses, CustTable, sales PriceDiscTable, sales tax and inventory posting, cash discount, return disposition codes, sales journal names | AccountsReceivablePhaseOneValidator |
| 9 | Communication | 1 | SysNotificationTemplate (placeholders checked against the declared variables) | CommunicationIntegrityValidator |
| 10 | Workflow | 9 | Workflow reference catalog: category, priority, process type, activity type, operator, data type, control, performer type, performer | WorkflowIntegrityValidator |
| 11 | Organization | 1 | DocuType (document management types) | OrganizationIntegrityValidator |

`SeedModelIntegrityValidator` also runs over every tracked row and checks string lengths against the mapped column sizes.

## Integrity rules every module follows

- **Company scope.** Every row carries the run's `DataAreaId`. Five tables have a unique key without the company: DocuType.TypeId, SysNumberSequence.NumberSequence, SysBackgroundJob.Caption, WfCategory.Code and SysNotificationTemplate.Code. For those, the first company to seed owns the row and later companies reuse it.
- **References.** A module reuses the records its owner module seeded, such as main accounts, units, sites, payment terms and currencies, and never creates its own copy. A reference must resolve inside the same company. It also cannot point at a soft-deleted or deactivated parent: the Foundation validator now rejects that.
- **Validators agree with services.** A seeded row must be one the business service would accept. For example, a cyclic number sequence whose format carries a year would re-issue codes, so it is converged to non-cyclic and the validator rejects it.
- **No transactions.** Purchase orders, sales orders, invoices, payments, inventory movements and ledger postings are created only through the services, in the cycle examples.

## Full cycle after setup (transaction examples)

Run these after the setup, each in its own scope. Every one prepares the setup itself, so it can also run on its own.

| Cycle | Registration | Basic D365 flow it exercises |
|---|---|---|
| Procure to pay | `AddAccountsPayableTransactionExamples` | PO, confirmation, product receipt, vendor invoice, payment journal, settlement, return and credit note |
| Order to cash | `AddAccountsReceivableTransactionExamples` | SO, confirmation, packing slip, customer invoice, free-text invoice, payment journal, settlement, return |
| Inventory | `AddInventoryTransactionExamples` | Movement, adjustment, transfer and counting journals, reservation, weighted-average inventory close |
| General ledger | `AddGeneralLedgerTransactionExamples` | General journal posting and reversal of a posted journal |
| Tax | `AddTaxTransactionExamples` | Tax calculated on journal and invoice lines, posted to TaxTrans and reconciled to the voucher |
| Cash and bank | `AddCashAndBankTransactionExamples` | Payments posted to the bank account and reconciled to the ledger |
| Foundation | `AddFoundationTransactionExamples` | Taxed journal and charges, reconciled through TaxJournalTrans |

## Out of scope

The following are basic D365 exclusions and are not seeded: posting layers other than Current, foreign-currency revaluation, intercompany, consolidation, budgeting, fixed assets, production, warehouse management (WMS), bank reconciliation, year-end close and workflow approval engines. Workflow seeds only its reference catalog. Process definitions, forms and performer users stay with the legacy seeder.

## Verification

`Tests/AllModulesSeedIntegrityTests.cs` covers two things:

- every module contributes seeders, and every module that owns a validator has it registered;
- all eleven modules seed together on SQLite with foreign keys on, pass every validator, and a rerun leaves every table's row count unchanged (the seed history table is excluded).
