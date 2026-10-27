# Seed integrity verification - 2026-10-27

## Result

The current `ERM` database has data in all 231 in-scope business entity tables. The EF model contains 242 mapped entity types: 231 business tables, the generated seeder log, and ten intentionally empty runtime/Identity/archive tables. The database also contains EF migration history. See [the complete entity inventory](seed-entity-coverage.csv) for every entity, row count, and declared parent reference.

This is verified coverage of the existing development database, not a claim that every possible business case has been exercised or that a clean-database installation was tested.

## Connected examples

- Administration: batch settings, groups and active windows, preserving configured jobs.
- Organization/HCM: bilingual workers, seller/manager assignments, organization hierarchy, legal-entity addresses and contact responsibilities.
- Collaboration/documents: announcements, in-app notifications and recipients, chat, preferences, and customer document notes. The demonstration reminder is cancelled; no external notification is sent.
- Workflow: representative requests, details/tracking, print templates and request variables derived from existing control-to-variable mappings.
- Finance setup: ledger/chart/calendar links, account dimensions, bank/payment setup, product categories/lifecycles, count groups, packaging, barcodes, planned costs, batch and serial masters.
- Purchasing: confirmed open order, partial receipt, and fully received/invoiced order, using the existing posting services.
- Sales: confirmed open order, partially delivered order, fully delivered/invoiced order, and an explicit partial payment against that example invoice.
- Commercial: published quantity-tier price agreement, draft free-text invoice, draft/sent quotations, automatic freight rule, draft charge/tax records, and an unposted payment with a valid settlement selection.
- Inventory: posted receipt/issue journals, FIFO close and settlement, and an unposted counting journal. The historical close refuses to process pre-existing financial inventory history through its cutoff. Batch/serial masters are setup examples; they do not claim batch/serial transaction coverage.

## Confirmed repairs

- Added missing dependency ordering for vendor/payment/tax/unit/product setup.
- Scoped currency, customer/vendor, posting, delivery, main-account and fiscal lookups correctly to their company. State lookups now include country.
- Repaired blank seeded item-group codes, missing shared product/translation/group/module relationships and incorrect product/unit conversion references while retaining customized values.
- Filled missing inventory statuses, voucher sequence links, inventory posting accounts, tax account dimensions, and inventory closing/settlement sequences.
- Corrected required audit fields and relationship keys in document/contact examples, and shortened the draft settlement code to its declared column length.
- The example ledger now defaults to SAR for both accounting and reporting currencies, matching supported inventory posting. The exact ledger created by this task was corrected once in ERM; customized existing ledgers are not rewritten by the seeder. The newly introduced draft payment header was likewise corrected to include its bank offset.

Three authorized migrations were generated and applied to ERM:

1. `20261027082322_AddSubledgerVoucherLink`: maps and creates the voucher-to-general-journal link with its foreign key and indexes.
2. `20261027082529_FixWorkflowRequestVariableKey`: uses the existing identity RECID as the primary key and enforces uniqueness of request/variable pairs, permitting multiple variables per request.
3. `20261027084117_AllowUnpostedPriceAgreementLine`: makes the published-price-rule reference optional until posting. The existing API zero sentinel maps to null; real published references and the foreign key are preserved.

## Verification

- Build succeeded; all 36 focused seeding tests passed. Tests include model/snapshot consistency, dependency ordering, company isolation, preservation of customized values, partial-seed recovery and repeatability.
- All 41 eligible seeders completed successfully. Four final live runs completed; counts across all 242 database tables other than the seeder log were unchanged over the last two runs.
- `DBCC CHECKCONSTRAINTS WITH ALL_CONSTRAINTS, ALL_ERRORMSGS` returned no violations. All 184 foreign keys are enabled and trusted.
- All 57 explicit semantic checks returned zero violations: 21 core, eight setup/collaboration/document, 14 accounting/scenario checks, and 14 additional cross-module checks. These include balanced vouchers, invoice totals, receivable balances, inventory quantities, polymorphic references and workflow process ownership.
- Existing uncommitted changes were retained, including the unrelated project-package edit. No commit or push was made.

Verification used the registered development seeding service against ERM with demo data enabled and external legacy imports/embedded fixtures disabled. Transactional examples use RunOnce execution and stable business/request keys; repeatable setup fills missing records. Existing sales/pricing service transaction boundaries are retained. Row counts are snapshots and are weaker than full-content equality; focused preservation tests and semantic checks supplement them. The normal hosted API/background-worker lifecycle and a new empty database were not tested in this checkpoint. Existing compiler/package warnings remain.

## Deliberate exclusions

No synthetic rows were added to `SysAuditLogs`, `SysExceptionLogs`, `BatchJobHistory`, `BatchJobTaskHistory`, `SysBackgroundJobRecurrenceCounts`, or `UnitOfMeasureConversionCache`. External-login identities and authentication credentials (`AspNetUserLogins`, `AspNetUserTokens`) remain empty. `WFProcessScheduled` remains an empty retired archive. `AspNetRoleClaims` is also empty: the current application uses `AspNetRolePermissions` for authorization; no arbitrary optional Identity claims were invented solely to fill that table. Seeder logs and migration history are generated normally.

## Repeat the checks

Run [core checks](seed-data-integrity.sql), [example checks](seed-example-integrity.sql), [scenario checks](seed-scenario-integrity.sql), and [cross-module checks](seed-cross-module-integrity.sql) against the intended development database after initialization. Use the application's existing secret configuration; do not place credentials in scripts.

```powershell
dotnet test IXApi/Tests/IXApi.Tests.csproj --no-restore --filter "FullyQualifiedName~Seed"
```

Local verification logs are under `.artifacts/`: `seed-sequence-build.log`, `seed-complete-tests.log`, `seed-complete-live.log`, `seed-final-repeat.log`, `seed-final-before.log`, `seed-final-after.log`, `seed-complete-integrity.log`, `seed-complete-examples.log`, and `seed-final-scenarios.log`.

## Follow-up cross-module review

The follow-up found one live semantic defect missed by foreign-key validation: the HBMC Riyadh postal address referred to a Los Angeles district. The logistics seeder now creates a distinct Riyadh example district and repairs only the exact original malformed address, retaining its record ID. Customized addresses and valid districts are preserved. Geography is validated before reusing postal codes or districts; incompatible existing records cause a descriptive failure rather than a silent wrong-city link. Postal codes and district names are alternate keys in the current model, so duplicate keys are not introduced to bypass a collision.

Three regression cases exercise the original repair, preservation of a customized address, and rejection/preservation of an existing postal code belonging to another country. The 14 additional SQL checks cover city/district/postal geography, organization hierarchy parent membership, worker assignment/position unit and role alignment, assignment dates, self-references, request assignment units, workflow variable mapping process ownership, and batch-window ranges.

The declared-FK company comparison found two cross-company links to shared logistics locations/roles; these are valid shared references and were preserved. All other declared foreign keys with company columns had no nonblank company mismatch. This is a diagnostic of declared relationships, not proof that all unmodeled references or every possible scenario are correct.

Follow-up logs: `.artifacts/seed-review-build.log`, `seed-review-tests.log`, `seed-review-live.log`, `seed-review-repeat.log`, `seed-review-cross-module-before.log`, `seed-review-cross-module-after.log`, and `seed-review-counts-before.log` / `seed-review-counts-after.log`.

## Saudi address examples

The logistics seeder now uses Saudi Arabia for both example company addresses: DAT in Jeddah (Makkah region, Jeddah governorate, Al Rawdah district, 23431) and HBMC in Riyadh (Riyadh region/governorate, Al Amal district, 12643). Each address connects country, region, governorate, city, district and postal-code records by their existing keys. Streets, four-digit sample building numbers, unit and additional numbers complete the formatted address. Building/unit/additional numbers are illustrative, not verified company premises. The existing BuildingCompliment field holds unit/additional details because the entity has no dedicated columns.

Only the exact original example addresses are converted, preserving their IDs and company/party links. Customized addresses and pre-existing geography remain intact. Legacy US reference rows are retained for existing references; fresh seeding no longer creates US examples. No schema migration is needed.

Geography references: [SPL National Address components](https://splonline.com.sa/en/door-step/), [SPL Riyadh/Al Amal sample](https://api.address.gov.sa/fixedsearch), and [published Jeddah/Al Rawdah address](https://www.saudiexchange.sa/Resources/fsPdf/7367_442_2024-02-12_16-15-43_en.pdf). These provide geography/format references, not validation of the illustrative building numbers.

Build and all 38 focused seeding tests passed. The KSA cases cover complete related geography, conversion of both original company examples, customization preservation, postal-code collisions and repeated execution. See [KSA address checks](seed-ksa-address-integrity.sql).

KSA live verification completed: both company addresses are now SA and retain address RECIDs 1 and 2. All three KSA-specific checks and all 57 existing semantic checks return zero violations; all 184 foreign keys remain enabled/trusted and DBCC reports no violations. All 41 seeders succeed. Two additional reruns leave table counts unchanged, excluding execution logs. Evidence: `.artifacts/seed-ksa-before.log`, `seed-ksa-after.log`, `seed-ksa-live.log`, `seed-ksa-repeat.log`, `seed-ksa-counts-before.log`, `seed-ksa-counts-after.log`, and `seed-ksa-all-checks.log`.
