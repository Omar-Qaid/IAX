# Accounts Payable phase-one seeding

Phase one contains configuration and master data only. It does not create purchase orders,
receipts, invoices, vendor balances, inventory movements, bank movements, settlements or posted
ledger entries. Those remain phase two and must use the owning business services.

See [the realistic office-supplies example](REAL-EXAMPLE.md) for the named suppliers, product
quotations, reference relationships and a SAR 3,680 purchasing exercise prepared for phase two.

## Invocation

Register `AddAccountsPayablePhaseOneSeeding()` from `Core.Registration`. In a fresh dependency
injection scope, resolve `Core.Execution.SeedRunner` and call
`await runner.RunAsync(fiscalYear: 2026, actor: "your-audit-user-id", cancellationToken)`.
The runner uses the `ApplicationDbContext` company execution context. No startup hook runs it
automatically. Apply migrations first. An existing selected legal entity is preserved; when absent, the explicit demo run creates it and its party.

For an opt-in configuration/master-data preparation report, use
`AddAccountsPayableConfigurationAndMasterDataExamples()` and
`AccountsPayableCycleSeeder.PrepareAsync(...)` from
[CycleErpExamples](CycleErpExamples/AccountsPayable/README.md). This reuses the same module seeders
and adds manifest/reference validation before commit; it executes no transaction scenarios.

This is an explicit illustrative APV2 data set, with SAR accounting currency, USD reference rates,
two synthetic vendors, stocked and service products, and a 15-percent example input tax code.
It is intended for a clean demo legal entity. Existing incompatible ledger/parameter setup causes
an error and rollback; it is never silently replaced. Synthetic bank numbers and `.invalid`
contacts are not payment-integration destinations. The USD/SAR rate is illustrative seed data,
not a live market quote.

## Execution and reruns

The dependency graph is checked before writes. The existing database seeder lock serializes
execution. A serializable transaction covers the entire phase and its `SysDataSeedLogs` history.
Execution strategies can retry after clearing tracked state. Reruns insert missing records and
validate existing business fields. Sequence counters are preserved, including after voucher use.
Deleted, inactive, duplicate or conflicting seed keys cause failure. Closed existing periods are
not reopened. Company predicates are explicit, including where query filters are bypassed.
Registered integrity validators run after the seeders and before commit. They check storage
lengths and the configured example's cross-table references; seed history rolls back with any
validation failure.

Some existing repository alternate keys (including currency, vendor-group and payment-term codes)
are globally unique even though those entities carry company fields. This implementation does not
change their schema or reuse another company's rows. Such collisions fail atomically and require
resolving the underlying shared/company schema before using the same codes in another company.

## Requested table coverage

| Owner | Requested configuration/master tables |
|---|---|
| AP | VendParameters, VendGroup, PaymTerm, VendPaymModeTable, CashDisc, VendTable, VendBankAccount, DirPartyTable, DirPartyLocation, LogisticsLocation, LogisticsPostalAddress, LogisticsElectronicAddress, PriceDiscTable |
| AP posting profile | The requested `VendPosting` concept maps to the existing `VendLedgerAccounts` entity/table. No duplicate `VendPosting` table is introduced. |
| GL | LedgerJournalName, Ledger, Currency, ExchangeRateType, ExchangeRateCurrencyPair, ExchangeRate, MainAccount, DimensionAttributeValueCombination |
| Tax | TaxTable, TaxGroupHeading, TaxGroupData, TaxItemGroupHeading, TaxOnItem |
| Inventory | InventPosting, InventModelGroup, InventParameters, EcoResProduct, InventTable, InventTableModule, InventSite, InventLocation, InventDim, EcoResCategory |
| Cash and bank | BankAccountTable |

Existing supporting tables are populated where required: LedgerChartOfAccounts; fiscal calendar,
year, periods and ledger period status; account structure and combination links through the
existing builders; SysNumberSequences; UnitOfMeasure; InventItemGroup; item/model group
assignments; product category hierarchy/assignment; tax rates, exemption code; and address
geography. Matching policy is configured on the existing vendor/item fields.

`VendBankAccount` is a new basic mapped entity because the repository had no vendor-bank master.
Its field names follow [Microsoft's VendBankAccount metadata](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/vendor/main/vendbankaccount).
It carries identity, vendor, account number, name and currency; payment-file generation is outside
this phase. `VendPostingSeeder` deliberately targets the existing posting-profile representation.

## Validation

Validation refreshed November 4, 2026:

- API build: 0 errors; existing package warnings remain.
- Full API suite: 801 passed, 2 skipped, including the enabled SQL Server integrity test.
- Inventory cycle suite: 97 passed.
- Phase-one integrity/preparation cases: 11 passed within the full API suite.

The focused tests populate the 49 covered mappings, rerun without changing table counts,
preserve sequence progress, validate the named supplier quotations and SAR 3,680 example,
check company-scoped reads and reject excessive mapped string lengths. They demonstrate
rollback both when next-year configuration has already been inserted and when a final seeder
introduces a wrong quotation currency or cross-company ledger combination. Transaction tables
remain empty in phase one.

SQLite runs with foreign keys enabled; only storage types and Unicode-literal syntax are
normalized for that provider. The SQL Server test uses the actual production EF model and
storage types on `(localdb)\MSSQLLocalDB`, creates a generated `IXApi_APV2_Integrity_<guid>`
database, performs the first run and rerun, compares supplier IDs and all mapped table counts,
and deletes that database in `finally`. This verifies current-model SQL Server persistence,
not replay of the historical migration chain or purchase-to-settlement posting.

To opt into that disposable SQL Server test:

```powershell
$env:IXAPI_AP_PHASE_ONE_SQLSERVER = '(localdb)\MSSQLLocalDB'
dotnet test IXApi/Tests/IXApi.Tests.csproj --filter FullyQualifiedName~AccountsPayablePhaseOneSeedTests
```

The SQL Server case is explicitly skipped when this environment variable is absent. It uses a
generated database name, never an application connection string or application database. No
application database has been migrated or seeded as part of this implementation. Cross-process
concurrent seeding and phase-two transaction posting remain unverified by these checks.

## Additional basic AP references

The phase contains 57 registered seeders. Eleven additional table mappings extend the original
38 concepts: CompanyInfo; EcoResStorageDimensionGroup, EcoResStorageDimensionGroupFldSetup,
EcoResStorageDimensionGroupItem; EcoResTrackingDimensionGroup,
EcoResTrackingDimensionGroupFldSetup, EcoResTrackingDimensionGroupItem;
InventItemPurchSetup; InventItemInventSetup; PurchParameters; MarkupTable.

The stocked paper item uses a company-named shared storage group with mandatory site and
warehouse, no location, financial inventory at site, and a no-batch/no-serial tracking group.
Disabled field-setup records are intentional configuration, not inactive item masters.
Purchase and inventory default order settings select APV2-DIM, quantity 100 EA, minimum/multiple
1 EA, and illustrative two-calendar-day lead time. The service item has no stocked-item groups.

APV2-FRT is a purchase charge code: debit delivery expense account 681003, credit vendor,
with the APV2 item tax group. No MarkupTrans or monetary charge is created. The original
SAR 3,680 illustration is unchanged because charges have not been applied.

PurchParameters is a new minimal mapped D365 configuration subset with PurchaseType=Purch
and one row per company. Its migration must be applied before application use. This phase
persists the default only; purchase-order initialization consuming it belongs to phase two.
It does not claim implementation of all D365 procurement parameters.
Field provenance: https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/procurementandsourcing/parameter/purchparameters

The CycleErpExamples report includes AdditionalMasters with the persisted company, parameter,
dimension-group, order-default and charge-code references. All additions remain inside the
same atomic seed transaction and are validated before commit.
