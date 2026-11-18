# Tax module ownership

Tax is a peer of AccountsPayable, AccountsReceivable, GeneralLedger and Inventory under IXApi/src/Modules/Finance/Tax. Its seven existing feature folders are retained. This is an ownership move, not a new assembly, schema or posting implementation.

## Compatibility

- Services, interfaces, DTOs, controllers, validators and mappings use IAX.IXApi.Modules.Finance.Tax (with their existing child namespaces).
- Entity CLR identities stay in IAX.IXApi.Modules.Finance.Entities, matching the shared Finance convention and preserving EF snapshot identity. Tables, columns, keys, precision, indexes, relationships and filters are unchanged. No migration or database write is part of this move.
- FinanceModule retains the existing service and validator registrations under the new namespace. Controllers retain their route templates, verbs, payloads, response handling and permissions. BaseController, BaseService, IUnitOfWork, Mapster and validation implementations are retained.
- Frontend pages, APIs and panels live in IXApp/src/modules/finance/tax. Tax has its own navigation module and /tax/authorities, /tax/settlement-periods, /tax/codes, /tax/groups, /tax/item-groups pages. Existing /foundation/tax-* links redirect with query, hash and navigation state preserved. Existing permissions apply to canonical and legacy routes.
- Three Tax master seeders now live in SeedingERPV2/Modules/Tax/MasterData; historical seed IDs and dependency IDs remain unchanged to preserve execution history. Foundation preparation still composes these seeders with its other dependencies.
- TaxTransGeneralJournalAccountEntrySeeder now belongs to CycleErpExamples/Tax. The existing Foundation transaction example still invokes it. No extra seed run or financial data change is triggered by moving files.

## Verification (2026-11-18)

- Backend build passed through the API test-project dependency build. The final focused run passed **83 tests**, failed **0**, and skipped **2 opt-in SQL Server integration tests**. It covers Tax, Foundation integration, authorization and EF model compatibility, including no pending model changes.
- The first completed focused run had **82 passed, 1 failed, 2 skipped**. Its integration fixture discovered CRUD services only under Foundation and therefore omitted the moved Tax services. Discovery now includes Tax, with explicit assertions for its group, item-group and period CRUD services. The complete focused rerun produced the final result above. Production open-generic registration required no behavior change.
- Inventory regression suite: **147 passed, 0 failed, 0 skipped**.
- Frontend production build passed with full TypeScript project checking and Vite bundling. The moved Tax pages, APIs, route definitions and legacy redirect are present in the production assets. Initial compilation exposed remaining caller imports/master routes in customers, sales orders, charges and record previews; these now reference the Tax module.
- Final frontend navigation, lookup and saved-link checks: **18 passed** across three test files. All five legacy Tax URLs preserve query strings and hashes and retain the corresponding canonical-route permission.
- Implementation audit: all **98** moved backend files match their original content after normalizing only the namespace prefix. No production business-rule change was needed.
- API compatibility evidence is the source audit, authorization tests and service regressions. This move did not restart the running API, execute a live database migration or rerun seed data against ERM. Model compatibility is verified by the EF model tests; existing entity identities and seed-history IDs are preserved.

## Confirmation rerun (2026-11-18)

- Fresh backend run: **83 passed, 0 failed, 2 skipped** (the same opt-in SQL Server integration suites). Fresh inventory run: **147 passed, 0 failed, 0 skipped**.
- Fresh frontend production build and full TypeScript project checking passed. All **18** navigation, lookup and legacy-route tests passed again.
- Added e2e/tax-module.pw.ts: **2 browser scenarios passed**, one desktop and one mobile. Each opens all five canonical Tax pages and all five saved Foundation URLs, checks visible page titles and query/hash-preserving redirects, and asserts no uncaught browser errors or Tax write requests. Backend HTTP calls are mocked; this is UI smoke evidence, not a live ERP/SQL integration run.
- Initial browser attempts failed because the test mock matched frontend source directories named api. Restricting interception to the configured backend address fixed the test harness; production code required no change. The final browser run passed both projects.
- Repeated the implementation audit: **98 files, zero implementation drift**, and the old backend Foundation/Tax folder remains absent. No ERP database writes were performed during confirmation.

## Source inventory

Backend/frontend confirmation: both builds passed; the backend focused suite passed **83 tests**, failed **0**, and skipped the same **2 opt-in SQL tests**. The expanded frontend run passed **41 tests** across Tax navigation, lookup navigation, record previews and sales-order customer defaults. Both desktop/mobile browser scenarios passed with mocked APIs. No production changes were required. Results are recorded under TestResults/TaxBackendFrontendConfirmation, TestResults/TaxBackendFrontendBrowserConfirmation and .validation/tax-module/backend-frontend-*.log.

Latest focused reconfirmation: **31 backend tests passed**, **18 frontend tests passed**, and **2 desktop/mobile browser scenarios passed**. The 98-file source audit again found zero implementation drift. This selected rerun did not include the two opt-in SQL integration suites; their previously reported skipped status is unchanged. Reports: TestResults/TaxModuleReconfirmation and TestResults/TaxModuleReconfirmationBrowser.

All 98 existing backend source files were compared with the pre-move snapshot. After normalizing only the Tax namespace prefix, every implementation matches exactly. No classes or API endpoints were added or removed.

| Folder | Source files |
| --- | ---: |
| Configuration | 15 |
| Controllers | 9 |
| Dtos | 15 |
| Entities | 14 |
| Interfaces | 17 |
| Services | 18 |
| Validation | 10 |

| Source file | Declarations | Status |
| --- | --- | --- |
| Configuration/TaxAuthorityAddressConfiguration.cs | TaxAuthorityAddressConfiguration | Moved; namespace rebased; implementation retained |
| Configuration/TaxDataConfiguration.cs | TaxDataConfiguration | Moved; namespace rebased; implementation retained |
| Configuration/TaxExemptCodeTableConfiguration.cs | TaxExemptCodeTableConfiguration | Moved; namespace rebased; implementation retained |
| Configuration/TaxGroupDataConfiguration.cs | TaxGroupDataConfiguration | Moved; namespace rebased; implementation retained |
| Configuration/TaxGroupHeadingConfiguration.cs | TaxGroupHeadingConfiguration | Moved; namespace rebased; implementation retained |
| Configuration/TaxItemGroupHeadingConfiguration.cs | TaxItemGroupHeadingConfiguration | Moved; namespace rebased; implementation retained |
| Configuration/TaxJournalTransConfiguration.cs | TaxJournalTransConfiguration | Moved; namespace rebased; implementation retained |
| Configuration/TaxLedgerAccountGroupConfiguration.cs | TaxLedgerAccountGroupConfiguration | Moved; namespace rebased; implementation retained |
| Configuration/TaxOnItemConfiguration.cs | TaxOnItemConfiguration | Moved; namespace rebased; implementation retained |
| Configuration/TaxOperationMapping.cs | TaxGroupReadSource, TaxGroupLineReadSource, TaxItemGroupReadSource, TaxItemGroupLineReadSource, TaxGroupLineWriteSource, TaxItemGroupLineWriteSource, TaxGroupLineCreateSource, TaxItemGroupLineCreateSource, TaxOperationMapping | Moved; namespace rebased; implementation retained |
| Configuration/TaxPeriodHeadConfiguration.cs | TaxPeriodHeadConfiguration | Moved; namespace rebased; implementation retained |
| Configuration/TaxReportPeriodConfiguration.cs | TaxReportPeriodConfiguration | Moved; namespace rebased; implementation retained |
| Configuration/TaxTableConfiguration.cs | TaxTableConfiguration | Moved; namespace rebased; implementation retained |
| Configuration/TaxTransConfiguration.cs | TaxTransConfiguration | Moved; namespace rebased; implementation retained |
| Configuration/TaxTransGeneralJournalAccountEntryConfiguration.cs | TaxTransGeneralJournalAccountEntryConfiguration | Moved; namespace rebased; implementation retained |
| Controllers/TaxAuthorityAddressController.cs | TaxAuthorityAddressController | Moved; namespace rebased; implementation retained |
| Controllers/TaxExemptCodeController.cs | TaxExemptCodeController | Moved; namespace rebased; implementation retained |
| Controllers/TaxGroupController.cs | TaxGroupController | Moved; namespace rebased; implementation retained |
| Controllers/TaxItemGroupController.cs | TaxItemGroupController | Moved; namespace rebased; implementation retained |
| Controllers/TaxLedgerAccountGroupController.cs | TaxLedgerAccountGroupController | Moved; namespace rebased; implementation retained |
| Controllers/TaxPeriodHeadController.cs | TaxPeriodHeadController | Moved; namespace rebased; implementation retained |
| Controllers/TaxReportController.cs | TaxReportController | Moved; namespace rebased; implementation retained |
| Controllers/TaxSettlementController.cs | TaxSettlementController | Moved; namespace rebased; implementation retained |
| Controllers/TaxTableController.cs | TaxTableController | Moved; namespace rebased; implementation retained |
| Dtos/TaxAuthorityAddressDto.cs | TaxAuthorityAddressDto | Moved; namespace rebased; implementation retained |
| Dtos/TaxDataDto.cs | TaxDataDto | Moved; namespace rebased; implementation retained |
| Dtos/TaxExemptCodeDto.cs | TaxExemptCodeDto | Moved; namespace rebased; implementation retained |
| Dtos/TaxGroupDataDto.cs | TaxGroupDataDto | Moved; namespace rebased; implementation retained |
| Dtos/TaxGroupDto.cs | TaxGroupDto | Moved; namespace rebased; implementation retained |
| Dtos/TaxItemGroupDto.cs | TaxItemGroupDto | Moved; namespace rebased; implementation retained |
| Dtos/TaxJournalTransDto.cs | TaxJournalTransDto | Moved; namespace rebased; implementation retained |
| Dtos/TaxLedgerAccountGroupDto.cs | TaxLedgerAccountGroupDto | Moved; namespace rebased; implementation retained |
| Dtos/TaxOnItemDto.cs | TaxOnItemDto | Moved; namespace rebased; implementation retained |
| Dtos/TaxPeriodHeadDto.cs | TaxPeriodHeadDto | Moved; namespace rebased; implementation retained |
| Dtos/TaxReportDtos.cs | TaxReportLineDto, TaxReportDto | Moved; namespace rebased; implementation retained |
| Dtos/TaxReportPeriodDto.cs | TaxReportPeriodDto | Moved; namespace rebased; implementation retained |
| Dtos/TaxSettlementDtos.cs | TaxSettlementInputDto, TaxSettlementAccountDto, TaxSettlementResultDto | Moved; namespace rebased; implementation retained |
| Dtos/TaxTableDto.cs | TaxTableDto | Moved; namespace rebased; implementation retained |
| Dtos/TaxTransDto.cs | TaxTransDto | Moved; namespace rebased; implementation retained |
| Entities/TaxAuthorityAddress.cs | TaxAuthorityAddress | Moved; implementation and namespace retained |
| Entities/TaxData.cs | TaxData | Moved; implementation and namespace retained |
| Entities/TaxExemptCodeTable.cs | TaxExemptCodeTable | Moved; implementation and namespace retained |
| Entities/TaxGroupData.cs | TaxGroupData | Moved; implementation and namespace retained |
| Entities/TaxGroupHeading.cs | TaxGroupHeading | Moved; implementation and namespace retained |
| Entities/TaxItemGroupHeading.cs | TaxItemGroupHeading | Moved; implementation and namespace retained |
| Entities/TaxJournalTrans.cs | TaxJournalTrans | Moved; implementation and namespace retained |
| Entities/TaxLedgerAccountGroup.cs | TaxLedgerAccountGroup | Moved; implementation and namespace retained |
| Entities/TaxOnItem.cs | TaxOnItem | Moved; implementation and namespace retained |
| Entities/TaxPeriodHead.cs | TaxPeriodHead | Moved; implementation and namespace retained |
| Entities/TaxReportPeriod.cs | TaxReportPeriod | Moved; implementation and namespace retained |
| Entities/TaxTable.cs | TaxTable | Moved; implementation and namespace retained |
| Entities/TaxTrans.cs | TaxTrans | Moved; implementation and namespace retained |
| Entities/TaxTransGeneralJournalAccountEntry.cs | TaxTransGeneralJournalAccountEntry | Moved; implementation and namespace retained |
| Interfaces/ITaxAuthorityAddressService.cs | ITaxAuthorityAddressService | Moved; namespace rebased; implementation retained |
| Interfaces/ITaxCodeRateService.cs | ITaxCodeRateService | Moved; namespace rebased; implementation retained |
| Interfaces/ITaxExemptCodeService.cs | ITaxExemptCodeService | Moved; namespace rebased; implementation retained |
| Interfaces/ITaxGroupCommandService.cs | ITaxGroupCommandService | Moved; namespace rebased; implementation retained |
| Interfaces/ITaxGroupLineService.cs | ITaxGroupLineService | Moved; namespace rebased; implementation retained |
| Interfaces/ITaxGroupQueryService.cs | ITaxGroupQueryService | Moved; namespace rebased; implementation retained |
| Interfaces/ITaxGroupService.cs | ITaxGroupService | Moved; namespace rebased; implementation retained |
| Interfaces/ITaxItemGroupCommandService.cs | ITaxItemGroupCommandService | Moved; namespace rebased; implementation retained |
| Interfaces/ITaxItemGroupLineService.cs | ITaxItemGroupLineService | Moved; namespace rebased; implementation retained |
| Interfaces/ITaxItemGroupQueryService.cs | ITaxItemGroupQueryService | Moved; namespace rebased; implementation retained |
| Interfaces/ITaxItemGroupService.cs | ITaxItemGroupService | Moved; namespace rebased; implementation retained |
| Interfaces/ITaxLedgerAccountGroupService.cs | ITaxLedgerAccountGroupService | Moved; namespace rebased; implementation retained |
| Interfaces/ITaxPeriodHeadCrudService.cs | ITaxPeriodHeadCrudService | Moved; namespace rebased; implementation retained |
| Interfaces/ITaxPeriodService.cs | ITaxPeriodService | Moved; namespace rebased; implementation retained |
| Interfaces/ITaxReportService.cs | ITaxReportService | Moved; namespace rebased; implementation retained |
| Interfaces/ITaxSettlementService.cs | ITaxSettlementService | Moved; namespace rebased; implementation retained |
| Interfaces/ITaxTableService.cs | ITaxTableService | Moved; namespace rebased; implementation retained |
| Services/TaxAuthorityAddressService.cs | TaxAuthorityAddressService | Moved; namespace rebased; implementation retained |
| Services/TaxCodeRateService.cs | TaxCodeRateService | Moved; namespace rebased; implementation retained |
| Services/TaxCodeSaveTransaction.cs | TaxCodeSaveTransaction | Moved; namespace rebased; implementation retained |
| Services/TaxExemptCodeService.cs | TaxExemptCodeService | Moved; namespace rebased; implementation retained |
| Services/TaxGroupCommandService.cs | TaxGroupCommandService | Moved; namespace rebased; implementation retained |
| Services/TaxGroupLineService.cs | TaxGroupLineService | Moved; namespace rebased; implementation retained |
| Services/TaxGroupQueryService.cs | TaxGroupQueryService | Moved; namespace rebased; implementation retained |
| Services/TaxGroupService.cs | TaxGroupService | Moved; namespace rebased; implementation retained |
| Services/TaxItemGroupCommandService.cs | TaxItemGroupCommandService | Moved; namespace rebased; implementation retained |
| Services/TaxItemGroupLineService.cs | TaxItemGroupLineService | Moved; namespace rebased; implementation retained |
| Services/TaxItemGroupQueryService.cs | TaxItemGroupQueryService | Moved; namespace rebased; implementation retained |
| Services/TaxItemGroupService.cs | TaxItemGroupService | Moved; namespace rebased; implementation retained |
| Services/TaxLedgerAccountGroupService.cs | TaxLedgerAccountGroupService | Moved; namespace rebased; implementation retained |
| Services/TaxPeriodHeadCrudService.cs | TaxPeriodHeadCrudService | Moved; namespace rebased; implementation retained |
| Services/TaxPeriodService.cs | TaxPeriodService | Moved; namespace rebased; implementation retained |
| Services/TaxReportService.cs | TaxReportService | Moved; namespace rebased; implementation retained |
| Services/TaxSettlementService.cs | TaxSettlementService | Moved; namespace rebased; implementation retained |
| Services/TaxTableService.cs | TaxTableService | Moved; namespace rebased; implementation retained |
| Validation/TaxAuthorityAddressDtoValidator.cs | TaxAuthorityAddressDtoValidator | Moved; namespace rebased; implementation retained |
| Validation/TaxDataDtoValidator.cs | TaxDataDtoValidator | Moved; namespace rebased; implementation retained |
| Validation/TaxExemptCodeDtoValidator.cs | TaxExemptCodeDtoValidator | Moved; namespace rebased; implementation retained |
| Validation/TaxGroupDataDtoValidator.cs | TaxGroupDataDtoValidator | Moved; namespace rebased; implementation retained |
| Validation/TaxGroupDtoValidator.cs | TaxGroupDtoValidator | Moved; namespace rebased; implementation retained |
| Validation/TaxItemGroupDtoValidator.cs | TaxItemGroupDtoValidator | Moved; namespace rebased; implementation retained |
| Validation/TaxLedgerAccountGroupDtoValidator.cs | TaxLedgerAccountGroupDtoValidator | Moved; namespace rebased; implementation retained |
| Validation/TaxOnItemDtoValidator.cs | TaxOnItemDtoValidator | Moved; namespace rebased; implementation retained |
| Validation/TaxPeriodHeadDtoValidator.cs | TaxPeriodHeadDtoValidator | Moved; namespace rebased; implementation retained |
| Validation/TaxTableDtoValidator.cs | TaxTableDtoValidator | Moved; namespace rebased; implementation retained |


## Follow-up bug verification (2026-11-18)

Two production defects were reproduced and fixed after the structural move:

- Legacy Tax URLs matched by React Router with a trailing slash or different letter casing previously rendered an empty redirect component. Normalize the pathname before matching, preserving query, hash, and return state. All ten new route regression cases failed before the fix and passed after it.
- Tax authority DTO validation previously accepted a missing Name and values exceeding the entity's existing StringLength limits. Apply those existing required/length constraints before persistence. The new tests reproduced 17 failures (5 existing valid cases passed); the focused backend run after the fix passed 105 tests with 2 SQL integration tests skipped.

The original namespace-only implementation comparison describes the initial move. TaxAuthorityAddressDtoValidator now intentionally differs because of this validation fix. A fresh comparison of all 98 moved C# files confirms that only this validator differs beyond namespace normalization. No schema migration or seed-data changes are part of these bug fixes.

Broader frontend testing also exposed obsolete representative-page fixtures after those pages moved from local mock records to API-backed data, and hardcoded grid-height expectations. Test fixtures are being aligned with the current API/shared-component contracts; this does not establish live SQL persistence or production browser write behavior.
