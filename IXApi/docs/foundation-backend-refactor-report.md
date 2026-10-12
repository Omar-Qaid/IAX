# Foundation backend refactor report

## Scope and baseline

All 17 existing Foundation feature folders and their nested folders were reviewed sequentially. The inventory below covers every C# file and its declared classes, interfaces, records and enums: 268 starting files, 285 final files. Existing conforming classes were retained. This report compares against the workspace at the start of this request, including its pre-existing uncommitted work.

- 46 existing Foundation files modified, including two interfaces that were also moved.
- 17 Foundation files created.
- 9 files moved: 7 Tax interfaces and 2 PaymentSchedules EF configurations.
- No classes removed. Old paths of moved files were removed.
- Integration changes: `src/Modules/Finance/FinanceModule.cs`, `Tests/OrganizationStructureTests.cs`, and new `Tests/FoundationRefactorTests.cs`.
- Backend only; no frontend, entity, table, migration or schema changes.

## Shared architecture applied

Reviewed BaseController, IBaseService, BaseService, IUnitOfWork/UnitOfWork, GenericRepository, APIResponse, DTO/entity bases, Mapster registration, BaseValidator/FluentValidation, DomainPermission, company context, exception handling, logging, DI, filtering and pagination before feature changes.

Generic CRUD follows `BaseController -> feature interface : IBaseService<T> -> feature service : BaseService<T> -> IUnitOfWork -> repository/persistence`. Composite operations retain dedicated service contracts rather than exposing unsupported generic CRUD endpoints. Persistence-only entities remain persistence-only. Stateless calculators have no artificial database dependency.

Controllers handle HTTP inputs, responses and service delegation. No Foundation controller directly accesses DbContext, IFinanceDataContext, repositories or transactions. Services use the existing UOW's repositories or Context and CompleteAsync. No duplicate base service, UOW, repository framework, mediator, events or mapping framework was introduced.

## Changes by concern

### Controllers and services

Worker/showroom create/update sequences, tax-code operations, tax-ledger normalization and address-book queries now belong to services. Existing read/reload ordering, primary-assignment decisions, default navigation includes, number-sequence calls and response messages were preserved. Custom Tax dependencies and organization structure now expose interfaces.

### Mapster

Added CurrencyBulkMapping, HcmWorkerQueryMapping, OrganizationStructureMapping, CompanyOperationMapping, AddressBookMapping and TaxOperationMapping; extended HcmShowroomMapping. Operation-specific mapping sources preserve narrower writable/readable field sets without changing generic CRUD mapping. Keys, parent relationships and company fields are protected where the original operation protected them. Resolved identities, relationship choices, rate enrichment and business decisions remain in services.

### Validation

Eight existing validators now inherit Shared BaseValidator, keeping their rules: CurrencyDtoValidator, ExchangeRateCurrencyPairDtoValidator, ExchangeRateTypeDtoValidator, DlvModeDtoValidator, DlvTermDtoValidator, HcmWorkerDtoValidator, PaymSchedLineDtoValidator and PaymTermDtoValidator. Existing annotations, validation exceptions and domain validation remain. Validators previously inactive in DI were not newly activated; doing so could alter accepted requests.

### Transactions

Saves use CompleteAsync. Company, worker and showroom operations use the existing UOW transaction methods and preserve ownership. Nested address-book operations no longer commit their caller's transaction. OrganizationStructure preserves Serializable isolation through UOW.Context because IUnitOfWork has no isolation-level overload. Existing Tax execution strategies and scoped EF transactions run through UOW.Context, preserving their commit/disposal behavior. Tax-rate best-effort writes and list fallbacks remain as before.

### Configuration and DI

FinanceModule registers IAddressBookQueryService, IOrganizationStructureService and eight Tax operation interfaces with their implementations. Existing CRUD registrations remain. Mapster's existing Finance assembly scan discovers the new IRegister classes. Seven Tax service interfaces now reside in Interfaces with unchanged namespaces. PaymentSchedules configurations now reside in Configuration; EF mapping contents are preserved.

## Verification

- Each changed feature passed a Finance compilation gate before proceeding; already-conforming/persistence-only folders were inspected and retained.
- Full API build: `dotnet build IXApi/IXApi.csproj --no-restore -v:q -clp:ErrorsOnly` passed, **0 errors, 172 warnings**.
- Final focused tests: **18 passed, 0 failed**: FoundationRefactorTests (6), OrganizationStructureTests (10), MarkupTests (2).
- Tests cover protected mapping fields, operation-specific tax exemption defaults, worker list/detail mapping separation, nested address transaction ownership, organization structure behavior and existing markup behavior. Transaction/organization tests use SQLite; projection parity test uses LINQ in memory.
- Controller Route, Http* and DomainPermission/Authorize attributes compared with the starting snapshot: **no differences**.
- Entity definitions and existing EF mapping content compared with the starting snapshot, accounting for moved files and encoding: **no differences**.
- Final scans found no direct controller database access and no Foundation IFinanceDataContext or direct SaveChangesAsync usage.
- `git diff --check` passed for touched Foundation, DI and test paths.

## Verification limits and retained behavior

No authenticated live API requests, SQL Server database tests or production migrations were run. Route/contract preservation is supported by source comparison and focused regression tests, not an end-to-end acceptance claim. Existing build warnings remain. Placeholder TaxDataDto, TaxJournalTransDto and TaxTransDto, the empty PaymSchedLineController file, and persistence-only features remain unchanged; this refactor does not implement new ERP functions. Existing tax rate failure fallbacks and other domain policies were deliberately preserved.

## Folder results

| Folder | Result |
|---|---|
| Currency | Bulk pair/rate writes use explicit Mapster whitelists. Three validators now inherit Shared BaseValidator. Existing CRUD and transaction/save order retained. |
| DeliveryModes | Validator aligned with Shared BaseValidator; controller, interface, service and schema retained. |
| DeliveryTerms | Validator aligned with Shared BaseValidator; both legacy routes retained. |
| Departments | Existing BaseController, IBaseService, BaseService, UOW and Mapster structure reviewed and retained, including legacy route. |
| Dimensions | DefaultDimension, DefaultDimension/Setup and LedgerDimension persistence classes reviewed; no new endpoints or schema changes. |
| Genders | Existing CRUD, service, mapping and validation reviewed and retained. |
| HcmShowrooms | Service owns create/update orchestration and UOW persistence. Assignment query/write mappings added to existing Mapster configuration. |
| HcmWorkers | HcmWorker, HcmWorkerOrganizationAssignmentV1, Structure, WorkerOrganizationAssignments and WorkerShowroomAssignments reviewed. Worker orchestration moved into service; Mapster query/write mappings added. Structure now has a service interface, UOW data access and explicit Mapster mappings. Serializable transaction isolation retained. |
| LegalEntities | Company operations use UOW and explicit write/read mappings. Company keys, party association and legacy logo persistence preserved. |
| LogisticsAddresses | Geography/address queries moved behind IAddressBookQueryService. Explicit address mappings and CompleteAsync used. Nested address-book operations respect the caller-owned transaction. |
| Markup | Existing refactor reviewed and retained: thin controllers, interfaces, UOW, Mapster, validator and calculator; regression tests passed. |
| Nationalities | Existing CRUD and legacy routes reviewed and retained. |
| Occupations | Existing CRUD and legacy routes reviewed and retained. |
| OrganizationUnits | Persistence-only entity/configuration reviewed and retained. No new API added. |
| PaymentSchedules | EF configurations moved from Entities to Configuration without semantic changes. Existing empty controller file retained; no endpoint invented. Line validator aligned with BaseValidator. |
| PaymentTerms | Validator aligned with Shared BaseValidator; existing CRUD retained. |
| Tax | Controllers use interfaces; TaxTable queries/save orchestration and ledger normalization moved into services. Custom services use UOW. Group/line read and write mapping uses Mapster with operation-specific defaults. Seven existing interfaces moved to Interfaces; eight operation interfaces added. Existing rate fallbacks and transaction/save behavior retained. |

## Files created

- `Currency/Configuration/CurrencyBulkMapping.cs`
- `HcmWorkers/HcmWorker/HcmWorkerQueryMapping.cs`
- `HcmWorkers/Structure/IOrganizationStructureService.cs`
- `HcmWorkers/Structure/OrganizationStructureMapping.cs`
- `LegalEntities/Mappings/CompanyOperationMapping.cs`
- `LogisticsAddresses/Configuration/AddressBookMapping.cs`
- `LogisticsAddresses/Interfaces/IAddressBookQueryService.cs`
- `LogisticsAddresses/Services/AddressBookQueryService.cs`
- `Tax/Configuration/TaxOperationMapping.cs`
- `Tax/Interfaces/ITaxCodeRateService.cs`
- `Tax/Interfaces/ITaxGroupCommandService.cs`
- `Tax/Interfaces/ITaxGroupLineService.cs`
- `Tax/Interfaces/ITaxGroupQueryService.cs`
- `Tax/Interfaces/ITaxItemGroupCommandService.cs`
- `Tax/Interfaces/ITaxItemGroupLineService.cs`
- `Tax/Interfaces/ITaxItemGroupQueryService.cs`
- `Tax/Interfaces/ITaxPeriodService.cs`

## Files moved

- `PaymentSchedules/Entities/PaymSchedConfiguration.cs` -> `PaymentSchedules/Configuration/PaymSchedConfiguration.cs`
- `PaymentSchedules/Entities/PaymSchedLineConfiguration.cs` -> `PaymentSchedules/Configuration/PaymSchedLineConfiguration.cs`
- `Tax/Services/ITaxAuthorityAddressService.cs` -> `Tax/Interfaces/ITaxAuthorityAddressService.cs`
- `Tax/Services/ITaxExemptCodeService.cs` -> `Tax/Interfaces/ITaxExemptCodeService.cs`
- `Tax/Services/ITaxGroupService.cs` -> `Tax/Interfaces/ITaxGroupService.cs`
- `Tax/Services/ITaxItemGroupService.cs` -> `Tax/Interfaces/ITaxItemGroupService.cs`
- `Tax/Services/ITaxLedgerAccountGroupService.cs` -> `Tax/Interfaces/ITaxLedgerAccountGroupService.cs`
- `Tax/Services/ITaxPeriodHeadCrudService.cs` -> `Tax/Interfaces/ITaxPeriodHeadCrudService.cs`
- `Tax/Services/ITaxTableService.cs` -> `Tax/Interfaces/ITaxTableService.cs`

## Complete class and file inventory

Status is relative to the starting workspace. Declarations sharing a file are listed together. Every nested folder is included.

### Currency

| File | Declarations | Result |
|---|---|---|
| `Currency/Configuration/CurrencyBulkMapping.cs` | `ExchangeRateBulkMappingSource`, `CurrencyBulkMapping` | Created |
| `Currency/Configuration/CurrencyConfiguration.cs` | `CurrencyConfiguration` | Reviewed; retained |
| `Currency/Configuration/ExchangeRateConfiguration.cs` | `ExchangeRateConfiguration` | Reviewed; retained |
| `Currency/Configuration/ExchangeRateCurrencyPairConfiguration.cs` | `ExchangeRateCurrencyPairConfiguration` | Reviewed; retained |
| `Currency/Configuration/ExchangeRateTypeConfiguration.cs` | `ExchangeRateTypeConfiguration` | Reviewed; retained |
| `Currency/Controllers/CurrencyController.cs` | `CurrencyController` | Reviewed; retained |
| `Currency/Controllers/ExchangeRateController.cs` | `ExchangeRateController` | Reviewed; retained |
| `Currency/Controllers/ExchangeRateCurrencyPairController.cs` | `ExchangeRateCurrencyPairController` | Reviewed; retained |
| `Currency/Controllers/ExchangeRateTypeController.cs` | `ExchangeRateTypeController` | Reviewed; retained |
| `Currency/Dtos/BulkExchangeRatePairDto.cs` | `BulkExchangeRatePairDto` | Reviewed; retained |
| `Currency/Dtos/CurrencyDto.cs` | `CurrencyDto` | Reviewed; retained |
| `Currency/Dtos/ExchangeRateCurrencyPairDto.cs` | `ExchangeRateCurrencyPairDto` | Reviewed; retained |
| `Currency/Dtos/ExchangeRateDto.cs` | `ExchangeRateDto` | Reviewed; retained |
| `Currency/Dtos/ExchangeRateTypeDto.cs` | `ExchangeRateTypeDto` | Reviewed; retained |
| `Currency/Entities/Currency.cs` | `Currency` | Reviewed; retained |
| `Currency/Entities/ExchangeRate.cs` | `ExchangeRate` | Reviewed; retained |
| `Currency/Entities/ExchangeRateCurrencyPair.cs` | `ExchangeRateCurrencyPair` | Reviewed; retained |
| `Currency/Entities/ExchangeRateType.cs` | `ExchangeRateType` | Reviewed; retained |
| `Currency/Interfaces/ICurrencyService.cs` | `ICurrencyService` | Reviewed; retained |
| `Currency/Interfaces/IExchangeRateCurrencyPairService.cs` | `IExchangeRateCurrencyPairService` | Reviewed; retained |
| `Currency/Interfaces/IExchangeRateService.cs` | `IExchangeRateService` | Reviewed; retained |
| `Currency/Interfaces/IExchangeRateTypeService.cs` | `IExchangeRateTypeService` | Reviewed; retained |
| `Currency/Services/CurrencyService.cs` | `CurrencyService` | Reviewed; retained |
| `Currency/Services/ExchangeRateCurrencyPairService.cs` | `ExchangeRateCurrencyPairService` | Modified |
| `Currency/Services/ExchangeRateService.cs` | `ExchangeRateService` | Reviewed; retained |
| `Currency/Services/ExchangeRateTypeService.cs` | `ExchangeRateTypeService` | Reviewed; retained |
| `Currency/Validation/CurrencyDtoValidator.cs` | `CurrencyDtoValidator` | Modified |
| `Currency/Validation/ExchangeRateCurrencyPairDtoValidator.cs` | `ExchangeRateCurrencyPairDtoValidator` | Modified |
| `Currency/Validation/ExchangeRateTypeDtoValidator.cs` | `ExchangeRateTypeDtoValidator` | Modified |

### DeliveryModes

| File | Declarations | Result |
|---|---|---|
| `DeliveryModes/Configuration/DlvModeConfiguration.cs` | `DlvModeConfiguration` | Reviewed; retained |
| `DeliveryModes/Controllers/DlvModeController.cs` | `DlvModeController` | Reviewed; retained |
| `DeliveryModes/Dtos/DlvModeDto.cs` | `DlvModeDto` | Reviewed; retained |
| `DeliveryModes/Entities/DlvMode.cs` | `DlvMode` | Reviewed; retained |
| `DeliveryModes/Interfaces/IDlvModeService.cs` | `IDlvModeService` | Reviewed; retained |
| `DeliveryModes/Services/DlvModeService.cs` | `DlvModeService` | Reviewed; retained |
| `DeliveryModes/Validation/DlvModeValidators.cs` | `DlvModeDtoValidator` | Modified |

### DeliveryTerms

| File | Declarations | Result |
|---|---|---|
| `DeliveryTerms/Configuration/DlvTermConfiguration.cs` | `DlvTermConfiguration` | Reviewed; retained |
| `DeliveryTerms/Controllers/DlvTermController.cs` | `DlvTermController` | Reviewed; retained |
| `DeliveryTerms/Dtos/DlvTermDto.cs` | `DlvTermDto` | Reviewed; retained |
| `DeliveryTerms/Entities/DlvTerm.cs` | `DlvTerm` | Reviewed; retained |
| `DeliveryTerms/Interfaces/IDlvTermService.cs` | `IDlvTermService` | Reviewed; retained |
| `DeliveryTerms/Services/DlvTermService.cs` | `DlvTermService` | Reviewed; retained |
| `DeliveryTerms/Validation/DlvTermValidators.cs` | `DlvTermDtoValidator` | Modified |

### Departments

| File | Declarations | Result |
|---|---|---|
| `Departments/HcmDepartment.cs` | `HcmDepartment` | Reviewed; retained |
| `Departments/HcmDepartmentConfiguration.cs` | `HcmDepartmentConfiguration` | Reviewed; retained |
| `Departments/HcmDepartmentController.cs` | `HcmDepartmentController`, `LegacyDepartmentController` | Reviewed; retained |
| `Departments/HcmDepartmentDto.cs` | `HcmDepartmentDto` | Reviewed; retained |
| `Departments/HcmDepartmentDtoValidator.cs` | `HcmDepartmentDtoValidator` | Reviewed; retained |
| `Departments/HcmDepartmentService.cs` | `HcmDepartmentService` | Reviewed; retained |
| `Departments/IHcmDepartmentService.cs` | `IHcmDepartmentService` | Reviewed; retained |

### Dimensions

| File | Declarations | Result |
|---|---|---|
| `Dimensions/DefaultDimension/Entities/DimensionAttributeSet.cs` | `DimensionAttributeSet` | Reviewed; retained |
| `Dimensions/DefaultDimension/Entities/DimensionAttributeSetItem.cs` | `DimensionAttributeSetItem` | Reviewed; retained |
| `Dimensions/DefaultDimension/Entities/DimensionAttributeValueSet.cs` | `DimensionAttributeValueSet` | Reviewed; retained |
| `Dimensions/DefaultDimension/Entities/DimensionAttributeValueSetItem.cs` | `DimensionAttributeValueSetItem` | Reviewed; retained |
| `Dimensions/DefaultDimension/Entities/Setup/DimensionAttribute.cs` | `DimensionAttribute` | Reviewed; retained |
| `Dimensions/DefaultDimension/Entities/Setup/DimensionAttributeValue.cs` | `DimensionAttributeValue` | Reviewed; retained |
| `Dimensions/LedgerDimension/Entities/DimensionAttributeLevelValue.cs` | `DimensionAttributeLevelValue` | Reviewed; retained |
| `Dimensions/LedgerDimension/Entities/DimensionAttributeValueCombination.cs` | `DimensionAttributeValueCombination` | Reviewed; retained |
| `Dimensions/LedgerDimension/Entities/DimensionAttributeValueGroup.cs` | `DimensionAttributeValueGroup` | Reviewed; retained |
| `Dimensions/LedgerDimension/Entities/DimensionAttributeValueGroupCombination.cs` | `DimensionAttributeValueGroupCombination` | Reviewed; retained |
| `Dimensions/LedgerDimension/Entities/DimensionHierarchy.cs` | `DimensionHierarchy` | Reviewed; retained |

### Genders

| File | Declarations | Result |
|---|---|---|
| `Genders/Gender.cs` | `Gender` | Reviewed; retained |
| `Genders/GenderConfiguration.cs` | `GenderConfiguration` | Reviewed; retained |
| `Genders/GenderController.cs` | `GenderController` | Reviewed; retained |
| `Genders/GenderDto.cs` | `GenderDto` | Reviewed; retained |
| `Genders/GenderDtoValidator.cs` | `GenderDtoValidator` | Reviewed; retained |
| `Genders/GenderService.cs` | `GenderService` | Reviewed; retained |
| `Genders/IGenderService.cs` | `IGenderService` | Reviewed; retained |

### HcmShowrooms

| File | Declarations | Result |
|---|---|---|
| `HcmShowrooms/HcmShowroom.cs` | `HcmShowroom` | Reviewed; retained |
| `HcmShowrooms/HcmShowroomConfiguration.cs` | `HcmShowroomConfiguration` | Reviewed; retained |
| `HcmShowrooms/HcmShowroomController.cs` | `HcmShowroomController` | Modified |
| `HcmShowrooms/HcmShowroomDto.cs` | `HcmShowroomDto`, `HcmShowroomWorkerAssignmentDto`, `SaveHcmShowroomWorkerAssignmentRequest` | Reviewed; retained |
| `HcmShowrooms/HcmShowroomDtoValidator.cs` | `HcmShowroomDtoValidator` | Reviewed; retained |
| `HcmShowrooms/HcmShowroomMapping.cs` | `HcmShowroomMapping` | Modified |
| `HcmShowrooms/HcmShowroomService.cs` | `HcmShowroomService` | Modified |
| `HcmShowrooms/IHcmShowroomService.cs` | `IHcmShowroomService` | Modified |

### HcmWorkers

| File | Declarations | Result |
|---|---|---|
| `HcmWorkers/HcmWorker/HcmWorker.cs` | `HcmWorker` | Reviewed; retained |
| `HcmWorkers/HcmWorker/HcmWorkerConfiguration.cs` | `HcmWorkerConfiguration` | Reviewed; retained |
| `HcmWorkers/HcmWorker/HcmWorkerController.cs` | `HcmWorkerController` | Modified |
| `HcmWorkers/HcmWorker/HcmWorkerDto.cs` | `HcmWorkerDto`, `HcmWorkerLookupDto`, `HcmWorkerLookupPageDto`, `HcmWorkerAssignmentChainNodeDto`, `HcmWorkerOrganizationAssignmentV1Dto`, `HcmWorkerShowroomAssignmentDto`, `SaveHcmWorkerOrganizationAssignmentV1Request`, `SaveHcmWorkerShowroomAssignmentRequest` | Reviewed; retained |
| `HcmWorkers/HcmWorker/HcmWorkerDtoValidator.cs` | `HcmWorkerDtoValidator` | Modified |
| `HcmWorkers/HcmWorker/HcmWorkerMapping.cs` | `HcmWorkerMapping` | Reviewed; retained |
| `HcmWorkers/HcmWorker/HcmWorkerQueryMapping.cs` | `HcmWorkerListProjectionDto`, `HcmWorkerQueryMapping` | Created |
| `HcmWorkers/HcmWorker/HcmWorkerService.cs` | `HcmWorkerService` | Modified |
| `HcmWorkers/HcmWorker/IHcmWorkerService.cs` | `IHcmWorkerService` | Modified |
| `HcmWorkers/HcmWorkerOrganizationAssignmentV1/HcmWorkerOrganizationAssignmentV1.cs` | `HcmWorkerOrganizationAssignmentV1` | Reviewed; retained |
| `HcmWorkers/HcmWorkerOrganizationAssignmentV1/HcmWorkerOrganizationAssignmentV1Configuration.cs` | `HcmWorkerOrganizationAssignmentV1Configuration` | Reviewed; retained |
| `HcmWorkers/Structure/IOrganizationStructureService.cs` | `IOrganizationStructureService` | Created |
| `HcmWorkers/Structure/OrganizationPeriod.cs` | `OrganizationPeriod` | Reviewed; retained |
| `HcmWorkers/Structure/OrganizationStructureConfiguration.cs` | `OrganizationRoleConfiguration`, `OrganizationHierarchyConfiguration`, `OrganizationHierarchyNodeConfiguration`, `HcmPositionConfiguration`, `HcmReportingHierarchyConfiguration`, `HcmPositionReportingLineConfiguration` | Reviewed; retained |
| `HcmWorkers/Structure/OrganizationStructureContracts.cs` | `CreateOrganizationUnit`, `UpdateOrganizationUnit`, `CreateOrganizationRole`, `UpdateOrganizationRole`, `CreateOrganizationHierarchy`, `CreateOrganizationHierarchyWithRootNode`, `UpdateOrganizationHierarchy`, `CreateOrganizationNode`, `UpdateOrganizationNode`, `CreatePosition`, `UpdatePosition`, `AssignWorker`, `UpdateWorkerAssignment`, `TransferWorker`, `CloseOrganizationPeriod`, `OrganizationRoleInfo`, `OrganizationHierarchyInfo`, `OrganizationNodeInfo`, `PositionInfo`, `OrganizationUnitType` | Reviewed; retained |
| `HcmWorkers/Structure/OrganizationStructureController.cs` | `OrganizationStructureController` | Modified |
| `HcmWorkers/Structure/OrganizationStructureEntities.cs` | `OrganizationRole`, `OrganizationHierarchy`, `OrganizationHierarchyNode`, `HcmPosition`, `HcmReportingHierarchy`, `HcmPositionReportingLine` | Reviewed; retained |
| `HcmWorkers/Structure/OrganizationStructureMapping.cs` | `OrganizationStructureMapping` | Created |
| `HcmWorkers/Structure/OrganizationStructureService.cs` | `OrganizationStructureService` | Modified |
| `HcmWorkers/WorkerOrganizationAssignments/HcmWorkerOrganizationAssignment.cs` | `HcmWorkerOrganizationAssignment` | Reviewed; retained |
| `HcmWorkers/WorkerOrganizationAssignments/HcmWorkerOrganizationAssignmentConfiguration.cs` | `HcmWorkerOrganizationAssignmentConfiguration` | Reviewed; retained |
| `HcmWorkers/WorkerShowroomAssignments/HcmWorkerShowroomAssignment.cs` | `HcmWorkerShowroomAssignment` | Reviewed; retained |
| `HcmWorkers/WorkerShowroomAssignments/HcmWorkerShowroomAssignmentConfiguration.cs` | `HcmWorkerShowroomAssignmentConfiguration` | Reviewed; retained |

### LegalEntities

| File | Declarations | Result |
|---|---|---|
| `LegalEntities/Configuration/CompanyInfoConfiguration.cs` | `CompanyInfoConfiguration` | Reviewed; retained |
| `LegalEntities/Controllers/CompanyInfoController.cs` | `CompanyInfoController`, `with`, `via` | Reviewed; retained |
| `LegalEntities/Dtos/CompanyInfoDto.cs` | `CompanyInfoDto` | Reviewed; retained |
| `LegalEntities/Entities/CompanyInfo.cs` | `CompanyInfo` | Reviewed; retained |
| `LegalEntities/Interfaces/ICompanyInfoService.cs` | `ICompanyInfoService` | Reviewed; retained |
| `LegalEntities/Mappings/CompanyInfoMapping.cs` | `CompanyInfoMapping` | Reviewed; retained |
| `LegalEntities/Mappings/CompanyOperationMapping.cs` | `CompanyWriteSource`, `CompanyPostalMappingSource`, `CompanyContactMappingSource`, `CompanyOperationMapping` | Created |
| `LegalEntities/Services/CompanyInfoService.cs` | `CompanyInfoService` | Modified |

### LogisticsAddresses

| File | Declarations | Result |
|---|---|---|
| `LogisticsAddresses/Configuration/AddressBookMapping.cs` | `PostalAddressReadSource`, `ElectronicAddressReadSource`, `PostalAddressWriteSource`, `ElectronicAddressWriteSource`, `AddressBookMapping` | Created |
| `LogisticsAddresses/Configuration/ContactPersonConfiguration.cs` | `ContactPersonConfiguration` | Reviewed; retained |
| `LogisticsAddresses/Configuration/DirPartyLocationRoleConfiguration.cs` | `DirPartyLocationRoleConfiguration` | Reviewed; retained |
| `LogisticsAddresses/Configuration/DirPartyTableConfiguration.cs` | `DirPartyTableConfiguration` | Reviewed; retained |
| `LogisticsAddresses/Configuration/LogisticsAddressCityConfiguration.cs` | `LogisticsAddressCityConfiguration` | Reviewed; retained |
| `LogisticsAddresses/Configuration/LogisticsAddressCountryRegionConfiguration.cs` | `LogisticsAddressCountryRegionConfiguration` | Reviewed; retained |
| `LogisticsAddresses/Configuration/LogisticsAddressCountyConfiguration.cs` | `LogisticsAddressCountyConfiguration` | Reviewed; retained |
| `LogisticsAddresses/Configuration/LogisticsAddressDistrictConfiguration.cs` | `LogisticsAddressDistrictConfiguration` | Reviewed; retained |
| `LogisticsAddresses/Configuration/LogisticsAddressStateConfiguration.cs` | `LogisticsAddressStateConfiguration` | Reviewed; retained |
| `LogisticsAddresses/Configuration/LogisticsElectronicAddressConfiguration.cs` | `LogisticsElectronicAddressConfiguration` | Reviewed; retained |
| `LogisticsAddresses/Configuration/LogisticsLocationConfiguration.cs` | `LogisticsLocationConfiguration` | Reviewed; retained |
| `LogisticsAddresses/Configuration/LogisticsLocationRoleConfiguration.cs` | `LogisticsLocationRoleConfiguration` | Reviewed; retained |
| `LogisticsAddresses/Configuration/LogisticsPostalAddressConfiguration.cs` | `LogisticsPostalAddressConfiguration` | Reviewed; retained |
| `LogisticsAddresses/Controllers/LogisticsElectronicAddressController.cs` | `LogisticsElectronicAddressController` | Modified |
| `LogisticsAddresses/Controllers/LogisticsPostalAddressController.cs` | `LogisticsPostalAddressController` | Modified |
| `LogisticsAddresses/Dtos/ContactPersonDto.cs` | `ContactPersonDto` | Reviewed; retained |
| `LogisticsAddresses/Entities/ContactPerson.cs` | `ContactPerson` | Reviewed; retained |
| `LogisticsAddresses/Entities/DirPartyLocation.cs` | `DirPartyLocation` | Reviewed; retained |
| `LogisticsAddresses/Entities/DirPartyLocationRole.cs` | `DirPartyLocationRole` | Reviewed; retained |
| `LogisticsAddresses/Entities/DirPartyTable.cs` | `DirPartyTable` | Reviewed; retained |
| `LogisticsAddresses/Entities/LogisticsAddressCity.cs` | `LogisticsAddressCity` | Reviewed; retained |
| `LogisticsAddresses/Entities/LogisticsAddressCountryRegion.cs` | `LogisticsAddressCountryRegion` | Reviewed; retained |
| `LogisticsAddresses/Entities/LogisticsAddressCounty.cs` | `LogisticsAddressCounty` | Reviewed; retained |
| `LogisticsAddresses/Entities/LogisticsAddressDistrict.cs` | `LogisticsAddressDistrict` | Reviewed; retained |
| `LogisticsAddresses/Entities/LogisticsAddressState.cs` | `LogisticsAddressState` | Reviewed; retained |
| `LogisticsAddresses/Entities/LogisticsAddressZipCode.cs` | `LogisticsAddressZipCode` | Reviewed; retained |
| `LogisticsAddresses/Entities/LogisticsElectronicAddress.cs` | `LogisticsElectronicAddress` | Reviewed; retained |
| `LogisticsAddresses/Entities/LogisticsLocation.cs` | `LogisticsLocation` | Reviewed; retained |
| `LogisticsAddresses/Entities/LogisticsLocationRole.cs` | `LogisticsLocationRole` | Reviewed; retained |
| `LogisticsAddresses/Entities/LogisticsPostalAddress.cs` | `LogisticsPostalAddress` | Reviewed; retained |
| `LogisticsAddresses/Interfaces/IAddressBookQueryService.cs` | `IAddressBookQueryService` | Created |
| `LogisticsAddresses/Interfaces/IElectronicAddressService.cs` | `IElectronicAddressService` | Reviewed; retained |
| `LogisticsAddresses/Interfaces/IGlobalAddressBookService.cs` | `IGlobalAddressBookService` | Reviewed; retained |
| `LogisticsAddresses/Interfaces/ILocationService.cs` | `ILocationService` | Reviewed; retained |
| `LogisticsAddresses/Interfaces/IPartyLocationService.cs` | `IPartyLocationService` | Reviewed; retained |
| `LogisticsAddresses/Interfaces/IPartyService.cs` | `IPartyService` | Reviewed; retained |
| `LogisticsAddresses/Interfaces/IPostalAddressService.cs` | `IPostalAddressService` | Reviewed; retained |
| `LogisticsAddresses/Services/AddressBookQueryService.cs` | `AddressBookQueryService` | Created |
| `LogisticsAddresses/Services/ElectronicAddressService.cs` | `ElectronicAddressService` | Modified |
| `LogisticsAddresses/Services/GlobalAddressBookService.cs` | `GlobalAddressBookService` | Modified |
| `LogisticsAddresses/Services/LocationService.cs` | `LocationService` | Modified |
| `LogisticsAddresses/Services/PartyLocationService.cs` | `PartyLocationService` | Modified |
| `LogisticsAddresses/Services/PartyService.cs` | `PartyService` | Modified |
| `LogisticsAddresses/Services/PostalAddressService.cs` | `PostalAddressService` | Modified |

### Markup

| File | Declarations | Result |
|---|---|---|
| `Markup/Configuration/MarkupAutoConfiguration.cs` | `MarkupAutoTableConfiguration`, `MarkupAutoLineConfiguration` | Reviewed; retained |
| `Markup/Configuration/MarkupConfiguration.cs` | `MarkupTableConfiguration` | Reviewed; retained |
| `Markup/Configuration/MarkupMapping.cs` | `MarkupMapping` | Reviewed; retained |
| `Markup/Controllers/MarkupTableController.cs` | `MarkupTableController` | Reviewed; retained |
| `Markup/Controllers/MarkupTransController.cs` | `MarkupTransController` | Reviewed; retained |
| `Markup/Dtos/MarkupCodeDto.cs` | `MarkupCodeDto` | Reviewed; retained |
| `Markup/Dtos/MarkupResult.cs` | `MarkupServiceError`, `MarkupResult` | Reviewed; retained |
| `Markup/Dtos/MarkupTableDto.cs` | `MarkupTableDto` | Reviewed; retained |
| `Markup/Dtos/MarkupTransDto.cs` | `MarkupTransDto` | Reviewed; retained |
| `Markup/Entities/MarkupAutoLine.cs` | `MarkupAutoLine` | Reviewed; retained |
| `Markup/Entities/MarkupAutoTable.cs` | `MarkupAutoTable` | Reviewed; retained |
| `Markup/Entities/MarkupTable.cs` | `MarkupTable` | Reviewed; retained |
| `Markup/Entities/MarkupTrans.cs` | `MarkupTrans` | Reviewed; retained |
| `Markup/Interfaces/IMarkupChargeCalculator.cs` | `IMarkupChargeCalculator` | Reviewed; retained |
| `Markup/Interfaces/IMarkupTableService.cs` | `IMarkupTableService` | Reviewed; retained |
| `Markup/Interfaces/IMarkupTransCommandService.cs` | `IMarkupTransCommandService` | Reviewed; retained |
| `Markup/Interfaces/IMarkupTransService.cs` | `IMarkupTransService` | Reviewed; retained |
| `Markup/Services/MarkupChargeCalculator.cs` | `MarkupChargeCalculator` | Reviewed; retained |
| `Markup/Services/MarkupTableService.cs` | `MarkupTableService` | Reviewed; retained |
| `Markup/Services/MarkupTransCommandService.cs` | `MarkupTransCommandService` | Reviewed; retained |
| `Markup/Services/MarkupTransService.cs` | `MarkupTransService` | Reviewed; retained |
| `Markup/Validation/MarkupTransValidator.cs` | `MarkupTransValidator` | Reviewed; retained |

### Nationalities

| File | Declarations | Result |
|---|---|---|
| `Nationalities/HcmNationality.cs` | `HcmNationality` | Reviewed; retained |
| `Nationalities/HcmNationalityConfiguration.cs` | `HcmNationalityConfiguration` | Reviewed; retained |
| `Nationalities/HcmNationalityController.cs` | `HcmNationalityController`, `LegacyNationalityController` | Reviewed; retained |
| `Nationalities/HcmNationalityDto.cs` | `HcmNationalityDto` | Reviewed; retained |
| `Nationalities/HcmNationalityDtoValidator.cs` | `HcmNationalityDtoValidator` | Reviewed; retained |
| `Nationalities/HcmNationalityService.cs` | `HcmNationalityService` | Reviewed; retained |
| `Nationalities/IHcmNationalityService.cs` | `IHcmNationalityService` | Reviewed; retained |

### Occupations

| File | Declarations | Result |
|---|---|---|
| `Occupations/HcmOccupation.cs` | `HcmOccupation` | Reviewed; retained |
| `Occupations/HcmOccupationConfiguration.cs` | `HcmOccupationConfiguration` | Reviewed; retained |
| `Occupations/HcmOccupationController.cs` | `HcmOccupationController`, `LegacyOccupationController` | Reviewed; retained |
| `Occupations/HcmOccupationDto.cs` | `HcmOccupationDto` | Reviewed; retained |
| `Occupations/HcmOccupationDtoValidator.cs` | `HcmOccupationDtoValidator` | Reviewed; retained |
| `Occupations/HcmOccupationService.cs` | `HcmOccupationService` | Reviewed; retained |
| `Occupations/IHcmOccupationService.cs` | `IHcmOccupationService` | Reviewed; retained |

### OrganizationUnits

| File | Declarations | Result |
|---|---|---|
| `OrganizationUnits/OrganizationUnit.cs` | `OrganizationUnit` | Reviewed; retained |
| `OrganizationUnits/OrganizationUnitConfiguration.cs` | `OrganizationUnitConfiguration` | Reviewed; retained |

### PaymentSchedules

| File | Declarations | Result |
|---|---|---|
| `PaymentSchedules/Configuration/PaymSchedConfiguration.cs` | `PaymSchedConfiguration` | Moved; contents retained |
| `PaymentSchedules/Configuration/PaymSchedLineConfiguration.cs` | `PaymSchedLineConfiguration` | Moved; contents retained |
| `PaymentSchedules/Controllers/PaymSchedController.cs` | `PaymSchedController` | Reviewed; retained |
| `PaymentSchedules/Controllers/PaymSchedLineController.cs` | No declarations (existing empty file) | Reviewed; retained |
| `PaymentSchedules/Dtos/PaymSchedDto.cs` | `PaymSchedDto` | Reviewed; retained |
| `PaymentSchedules/Dtos/PaymSchedLineDto.cs` | `PaymSchedLineDto` | Reviewed; retained |
| `PaymentSchedules/Entities/PaymSched.cs` | `PaymSched` | Reviewed; retained |
| `PaymentSchedules/Entities/PaymSchedLine.cs` | `PaymSchedLine` | Reviewed; retained |
| `PaymentSchedules/Interfaces/IPaymSchedLineService.cs` | `IPaymSchedLineService` | Reviewed; retained |
| `PaymentSchedules/Interfaces/IPaymSchedService.cs` | `IPaymSchedService` | Reviewed; retained |
| `PaymentSchedules/Services/PaymSchedLineService.cs` | `PaymSchedLineService` | Reviewed; retained |
| `PaymentSchedules/Services/PaymSchedService.cs` | `PaymSchedService` | Reviewed; retained |
| `PaymentSchedules/Validation/PaymSchedLineDtoValidator.cs` | `PaymSchedLineDtoValidator` | Modified |

### PaymentTerms

| File | Declarations | Result |
|---|---|---|
| `PaymentTerms/Controllers/PaymTermController.cs` | `PaymTermController` | Reviewed; retained |
| `PaymentTerms/Dtos/PaymTermDto.cs` | `PaymTermDto` | Reviewed; retained |
| `PaymentTerms/Entities/PaymTerm.cs` | `PaymTerm` | Reviewed; retained |
| `PaymentTerms/Interfaces/IPaymTermService.cs` | `IPaymTermService` | Reviewed; retained |
| `PaymentTerms/Services/PaymTermService.cs` | `PaymTermService` | Reviewed; retained |
| `PaymentTerms/Validation/PaymTermValidators.cs` | `PaymTermDtoValidator` | Modified |

### Tax

| File | Declarations | Result |
|---|---|---|
| `Tax/Configuration/TaxAuthorityAddressConfiguration.cs` | `TaxAuthorityAddressConfiguration` | Reviewed; retained |
| `Tax/Configuration/TaxDataConfiguration.cs` | `TaxDataConfiguration` | Reviewed; retained |
| `Tax/Configuration/TaxExemptCodeTableConfiguration.cs` | `TaxExemptCodeTableConfiguration` | Reviewed; retained |
| `Tax/Configuration/TaxGroupDataConfiguration.cs` | `TaxGroupDataConfiguration` | Reviewed; retained |
| `Tax/Configuration/TaxGroupHeadingConfiguration.cs` | `TaxGroupHeadingConfiguration` | Reviewed; retained |
| `Tax/Configuration/TaxItemGroupHeadingConfiguration.cs` | `TaxItemGroupHeadingConfiguration` | Reviewed; retained |
| `Tax/Configuration/TaxJournalTransConfiguration.cs` | `TaxJournalTransConfiguration` | Reviewed; retained |
| `Tax/Configuration/TaxLedgerAccountGroupConfiguration.cs` | `TaxLedgerAccountGroupConfiguration` | Reviewed; retained |
| `Tax/Configuration/TaxOnItemConfiguration.cs` | `TaxOnItemConfiguration` | Reviewed; retained |
| `Tax/Configuration/TaxOperationMapping.cs` | `TaxGroupReadSource`, `TaxGroupLineReadSource`, `TaxItemGroupReadSource`, `TaxItemGroupLineReadSource`, `TaxGroupLineWriteSource`, `TaxItemGroupLineWriteSource`, `TaxGroupLineCreateSource`, `TaxItemGroupLineCreateSource`, `TaxOperationMapping` | Created |
| `Tax/Configuration/TaxPeriodHeadConfiguration.cs` | `TaxPeriodHeadConfiguration` | Reviewed; retained |
| `Tax/Configuration/TaxReportPeriodConfiguration.cs` | `TaxReportPeriodConfiguration` | Reviewed; retained |
| `Tax/Configuration/TaxTableConfiguration.cs` | `TaxTableConfiguration` | Reviewed; retained |
| `Tax/Configuration/TaxTransConfiguration.cs` | `TaxTransConfiguration` | Reviewed; retained |
| `Tax/Controllers/TaxAuthorityAddressController.cs` | `TaxAuthorityAddressController` | Modified |
| `Tax/Controllers/TaxExemptCodeController.cs` | `TaxExemptCodeController` | Modified |
| `Tax/Controllers/TaxGroupController.cs` | `TaxGroupController` | Modified |
| `Tax/Controllers/TaxItemGroupController.cs` | `TaxItemGroupController` | Modified |
| `Tax/Controllers/TaxLedgerAccountGroupController.cs` | `TaxLedgerAccountGroupController` | Modified |
| `Tax/Controllers/TaxPeriodHeadController.cs` | `TaxPeriodHeadController` | Modified |
| `Tax/Controllers/TaxTableController.cs` | `TaxTableController` | Modified |
| `Tax/Dtos/TaxAuthorityAddressDto.cs` | `TaxAuthorityAddressDto` | Reviewed; retained |
| `Tax/Dtos/TaxDataDto.cs` | `TaxDataDto` | Reviewed; retained |
| `Tax/Dtos/TaxExemptCodeDto.cs` | `TaxExemptCodeDto` | Reviewed; retained |
| `Tax/Dtos/TaxGroupDataDto.cs` | `TaxGroupDataDto` | Reviewed; retained |
| `Tax/Dtos/TaxGroupDto.cs` | `TaxGroupDto` | Reviewed; retained |
| `Tax/Dtos/TaxItemGroupDto.cs` | `TaxItemGroupDto` | Reviewed; retained |
| `Tax/Dtos/TaxJournalTransDto.cs` | `TaxJournalTransDto` | Reviewed; retained |
| `Tax/Dtos/TaxLedgerAccountGroupDto.cs` | `TaxLedgerAccountGroupDto` | Reviewed; retained |
| `Tax/Dtos/TaxOnItemDto.cs` | `TaxOnItemDto` | Reviewed; retained |
| `Tax/Dtos/TaxPeriodHeadDto.cs` | `TaxPeriodHeadDto` | Reviewed; retained |
| `Tax/Dtos/TaxReportPeriodDto.cs` | `TaxReportPeriodDto` | Reviewed; retained |
| `Tax/Dtos/TaxTableDto.cs` | `TaxTableDto` | Reviewed; retained |
| `Tax/Dtos/TaxTransDto.cs` | `TaxTransDto` | Reviewed; retained |
| `Tax/Entities/TaxAuthorityAddress.cs` | `TaxAuthorityAddress` | Reviewed; retained |
| `Tax/Entities/TaxData.cs` | `TaxData` | Reviewed; retained |
| `Tax/Entities/TaxExemptCodeTable.cs` | `TaxExemptCodeTable` | Reviewed; retained |
| `Tax/Entities/TaxGroupData.cs` | `TaxGroupData` | Reviewed; retained |
| `Tax/Entities/TaxGroupHeading.cs` | `TaxGroupHeading` | Reviewed; retained |
| `Tax/Entities/TaxItemGroupHeading.cs` | `TaxItemGroupHeading` | Reviewed; retained |
| `Tax/Entities/TaxJournalTrans.cs` | `TaxJournalTrans` | Reviewed; retained |
| `Tax/Entities/TaxLedgerAccountGroup.cs` | `TaxLedgerAccountGroup` | Reviewed; retained |
| `Tax/Entities/TaxOnItem.cs` | `TaxOnItem` | Reviewed; retained |
| `Tax/Entities/TaxPeriodHead.cs` | `TaxPeriodHead` | Reviewed; retained |
| `Tax/Entities/TaxReportPeriod.cs` | `TaxReportPeriod` | Reviewed; retained |
| `Tax/Entities/TaxTable.cs` | `TaxTable` | Reviewed; retained |
| `Tax/Entities/TaxTrans.cs` | `TaxTrans` | Reviewed; retained |
| `Tax/Interfaces/ITaxAuthorityAddressService.cs` | `ITaxAuthorityAddressService` | Moved; contents retained |
| `Tax/Interfaces/ITaxCodeRateService.cs` | `ITaxCodeRateService` | Created |
| `Tax/Interfaces/ITaxExemptCodeService.cs` | `ITaxExemptCodeService` | Moved; contents retained |
| `Tax/Interfaces/ITaxGroupCommandService.cs` | `ITaxGroupCommandService` | Created |
| `Tax/Interfaces/ITaxGroupLineService.cs` | `ITaxGroupLineService` | Created |
| `Tax/Interfaces/ITaxGroupQueryService.cs` | `ITaxGroupQueryService` | Created |
| `Tax/Interfaces/ITaxGroupService.cs` | `ITaxGroupService` | Moved; contents retained |
| `Tax/Interfaces/ITaxItemGroupCommandService.cs` | `ITaxItemGroupCommandService` | Created |
| `Tax/Interfaces/ITaxItemGroupLineService.cs` | `ITaxItemGroupLineService` | Created |
| `Tax/Interfaces/ITaxItemGroupQueryService.cs` | `ITaxItemGroupQueryService` | Created |
| `Tax/Interfaces/ITaxItemGroupService.cs` | `ITaxItemGroupService` | Moved; contents retained |
| `Tax/Interfaces/ITaxLedgerAccountGroupService.cs` | `ITaxLedgerAccountGroupService` | Moved and modified |
| `Tax/Interfaces/ITaxPeriodHeadCrudService.cs` | `ITaxPeriodHeadCrudService` | Moved; contents retained |
| `Tax/Interfaces/ITaxPeriodService.cs` | `ITaxPeriodService` | Created |
| `Tax/Interfaces/ITaxTableService.cs` | `ITaxTableService` | Moved and modified |
| `Tax/Services/TaxAuthorityAddressService.cs` | `TaxAuthorityAddressService` | Reviewed; retained |
| `Tax/Services/TaxCodeRateService.cs` | `TaxCodeRateService` | Modified |
| `Tax/Services/TaxExemptCodeService.cs` | `TaxExemptCodeService` | Reviewed; retained |
| `Tax/Services/TaxGroupCommandService.cs` | `TaxGroupCommandService` | Modified |
| `Tax/Services/TaxGroupLineService.cs` | `TaxGroupLineService` | Modified |
| `Tax/Services/TaxGroupQueryService.cs` | `TaxGroupQueryService` | Modified |
| `Tax/Services/TaxGroupService.cs` | `TaxGroupService` | Reviewed; retained |
| `Tax/Services/TaxItemGroupCommandService.cs` | `TaxItemGroupCommandService` | Modified |
| `Tax/Services/TaxItemGroupLineService.cs` | `TaxItemGroupLineService` | Modified |
| `Tax/Services/TaxItemGroupQueryService.cs` | `TaxItemGroupQueryService` | Modified |
| `Tax/Services/TaxItemGroupService.cs` | `TaxItemGroupService` | Reviewed; retained |
| `Tax/Services/TaxLedgerAccountGroupService.cs` | `TaxLedgerAccountGroupService` | Modified |
| `Tax/Services/TaxPeriodHeadCrudService.cs` | `TaxPeriodHeadCrudService` | Reviewed; retained |
| `Tax/Services/TaxPeriodService.cs` | `TaxPeriodService` | Modified |
| `Tax/Services/TaxTableService.cs` | `TaxTableService` | Modified |
