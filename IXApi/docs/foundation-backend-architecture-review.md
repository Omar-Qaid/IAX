# Foundation backend architecture review

Scope: `IXApi/src/Modules/Finance/Foundation`. The attachment names `GeneralLedger` only in copied examples; Foundation is the requested target.

## Shared architecture and baseline

Reviewed `BaseController`, `IBaseService`, `BaseService`, `IUnitOfWork`, generic repository, response and DTO contracts, Mapster registration, validation base, authorization and company context, Finance DI, and pagination/filtering. Baseline API build passed with 0 errors. No existing Foundation file was modified before this review.

## Folder-by-folder result

All 17 existing feature folders were processed in alphabetical order. Configuration and entity classes were retained to preserve EF mappings and schema. DTOs/interfaces/validators were retained where they already match existing contracts. Every controller delegates persistence to a service. Concrete service classes use `IUnitOfWork` directly or inherit it through `BaseService`. Custom operation controllers keep their existing routes and response behavior; converting them to generic CRUD would change contracts.

| Folder | C# files | Result |
| --- | ---: | --- |
| Currency | 29 | Reviewed; retained existing architecture and behavior. |
| DeliveryModes | 7 | Reviewed; retained existing architecture and behavior. |
| DeliveryTerms | 7 | Reviewed; retained existing architecture and behavior. |
| Departments | 7 | Reviewed; retained existing architecture and behavior. |
| Dimensions | 11 | Reviewed; retained existing architecture and behavior. |
| Genders | 7 | Reviewed; retained existing architecture and behavior. |
| HcmShowrooms | 8 | Reviewed; retained existing architecture and behavior. |
| HcmWorkers | 23 | Reviewed; retained existing architecture and behavior. |
| LegalEntities | 8 | Reviewed; retained existing architecture and behavior. |
| LogisticsAddresses | 44 | Reviewed; retained existing architecture and behavior. |
| Markup | 22 | Reviewed; retained existing architecture and behavior. |
| Nationalities | 7 | Reviewed; retained existing architecture and behavior. |
| Occupations | 7 | Reviewed; retained existing architecture and behavior. |
| OrganizationUnits | 2 | Reviewed; retained existing architecture and behavior. |
| PaymentSchedules | 13 | Reviewed; retained existing architecture and behavior. |
| PaymentTerms | 6 | Reviewed; retained existing architecture and behavior. |
| Tax | 77 | Reviewed; retained existing architecture and behavior. |

## Full source inventory

Every source file below was reviewed for class declarations, controller/service boundaries, mapping, validation, and schema ownership. Status for each existing file: retained. The list includes the empty `PaymSchedLineController.cs` placeholder, which declares no class.

### Currency

- `Currency/Configuration/CurrencyBulkMapping.cs` — ExchangeRateBulkMappingSource, CurrencyBulkMapping.
- `Currency/Configuration/CurrencyConfiguration.cs` — CurrencyConfiguration.
- `Currency/Configuration/ExchangeRateConfiguration.cs` — ExchangeRateConfiguration.
- `Currency/Configuration/ExchangeRateCurrencyPairConfiguration.cs` — ExchangeRateCurrencyPairConfiguration.
- `Currency/Configuration/ExchangeRateTypeConfiguration.cs` — ExchangeRateTypeConfiguration.
- `Currency/Controllers/CurrencyController.cs` — CurrencyController.
- `Currency/Controllers/ExchangeRateController.cs` — ExchangeRateController.
- `Currency/Controllers/ExchangeRateCurrencyPairController.cs` — ExchangeRateCurrencyPairController.
- `Currency/Controllers/ExchangeRateTypeController.cs` — ExchangeRateTypeController.
- `Currency/Dtos/BulkExchangeRatePairDto.cs` — BulkExchangeRatePairDto.
- `Currency/Dtos/CurrencyDto.cs` — CurrencyDto.
- `Currency/Dtos/ExchangeRateCurrencyPairDto.cs` — ExchangeRateCurrencyPairDto.
- `Currency/Dtos/ExchangeRateDto.cs` — ExchangeRateDto.
- `Currency/Dtos/ExchangeRateTypeDto.cs` — ExchangeRateTypeDto.
- `Currency/Entities/Currency.cs` — Currency.
- `Currency/Entities/ExchangeRate.cs` — ExchangeRate.
- `Currency/Entities/ExchangeRateCurrencyPair.cs` — ExchangeRateCurrencyPair.
- `Currency/Entities/ExchangeRateType.cs` — ExchangeRateType.
- `Currency/Interfaces/ICurrencyService.cs` — ICurrencyService.
- `Currency/Interfaces/IExchangeRateCurrencyPairService.cs` — IExchangeRateCurrencyPairService.
- `Currency/Interfaces/IExchangeRateService.cs` — IExchangeRateService.
- `Currency/Interfaces/IExchangeRateTypeService.cs` — IExchangeRateTypeService.
- `Currency/Services/CurrencyService.cs` — CurrencyService.
- `Currency/Services/ExchangeRateCurrencyPairService.cs` — ExchangeRateCurrencyPairService.
- `Currency/Services/ExchangeRateService.cs` — ExchangeRateService.
- `Currency/Services/ExchangeRateTypeService.cs` — ExchangeRateTypeService.
- `Currency/Validation/CurrencyDtoValidator.cs` — CurrencyDtoValidator.
- `Currency/Validation/ExchangeRateCurrencyPairDtoValidator.cs` — ExchangeRateCurrencyPairDtoValidator.
- `Currency/Validation/ExchangeRateTypeDtoValidator.cs` — ExchangeRateTypeDtoValidator.

### DeliveryModes

- `DeliveryModes/Configuration/DlvModeConfiguration.cs` — DlvModeConfiguration.
- `DeliveryModes/Controllers/DlvModeController.cs` — DlvModeController.
- `DeliveryModes/Dtos/DlvModeDto.cs` — DlvModeDto.
- `DeliveryModes/Entities/DlvMode.cs` — DlvMode.
- `DeliveryModes/Interfaces/IDlvModeService.cs` — IDlvModeService.
- `DeliveryModes/Services/DlvModeService.cs` — DlvModeService.
- `DeliveryModes/Validation/DlvModeValidators.cs` — DlvModeDtoValidator.

### DeliveryTerms

- `DeliveryTerms/Configuration/DlvTermConfiguration.cs` — DlvTermConfiguration.
- `DeliveryTerms/Controllers/DlvTermController.cs` — DlvTermController.
- `DeliveryTerms/Dtos/DlvTermDto.cs` — DlvTermDto.
- `DeliveryTerms/Entities/DlvTerm.cs` — DlvTerm.
- `DeliveryTerms/Interfaces/IDlvTermService.cs` — IDlvTermService.
- `DeliveryTerms/Services/DlvTermService.cs` — DlvTermService.
- `DeliveryTerms/Validation/DlvTermValidators.cs` — DlvTermDtoValidator.

### Departments

- `Departments/HcmDepartment.cs` — HcmDepartment.
- `Departments/HcmDepartmentConfiguration.cs` — HcmDepartmentConfiguration.
- `Departments/HcmDepartmentController.cs` — HcmDepartmentController, LegacyDepartmentController.
- `Departments/HcmDepartmentDto.cs` — HcmDepartmentDto.
- `Departments/HcmDepartmentDtoValidator.cs` — HcmDepartmentDtoValidator.
- `Departments/HcmDepartmentService.cs` — HcmDepartmentService.
- `Departments/IHcmDepartmentService.cs` — IHcmDepartmentService.

### Dimensions

- `Dimensions/DefaultDimension/Entities/DimensionAttributeSet.cs` — DimensionAttributeSet.
- `Dimensions/DefaultDimension/Entities/DimensionAttributeSetItem.cs` — DimensionAttributeSetItem.
- `Dimensions/DefaultDimension/Entities/DimensionAttributeValueSet.cs` — DimensionAttributeValueSet.
- `Dimensions/DefaultDimension/Entities/DimensionAttributeValueSetItem.cs` — DimensionAttributeValueSetItem.
- `Dimensions/DefaultDimension/Entities/Setup/DimensionAttribute.cs` — DimensionAttribute.
- `Dimensions/DefaultDimension/Entities/Setup/DimensionAttributeValue.cs` — DimensionAttributeValue.
- `Dimensions/LedgerDimension/Entities/DimensionAttributeLevelValue.cs` — DimensionAttributeLevelValue.
- `Dimensions/LedgerDimension/Entities/DimensionAttributeValueCombination.cs` — DimensionAttributeValueCombination.
- `Dimensions/LedgerDimension/Entities/DimensionAttributeValueGroup.cs` — DimensionAttributeValueGroup.
- `Dimensions/LedgerDimension/Entities/DimensionAttributeValueGroupCombination.cs` — DimensionAttributeValueGroupCombination.
- `Dimensions/LedgerDimension/Entities/DimensionHierarchy.cs` — DimensionHierarchy.

### Genders

- `Genders/Gender.cs` — Gender.
- `Genders/GenderConfiguration.cs` — GenderConfiguration.
- `Genders/GenderController.cs` — GenderController.
- `Genders/GenderDto.cs` — GenderDto.
- `Genders/GenderDtoValidator.cs` — GenderDtoValidator.
- `Genders/GenderService.cs` — GenderService.
- `Genders/IGenderService.cs` — IGenderService.

### HcmShowrooms

- `HcmShowrooms/HcmShowroom.cs` — HcmShowroom.
- `HcmShowrooms/HcmShowroomConfiguration.cs` — HcmShowroomConfiguration.
- `HcmShowrooms/HcmShowroomController.cs` — HcmShowroomController.
- `HcmShowrooms/HcmShowroomDto.cs` — HcmShowroomDto, HcmShowroomWorkerAssignmentDto, SaveHcmShowroomWorkerAssignmentRequest.
- `HcmShowrooms/HcmShowroomDtoValidator.cs` — HcmShowroomDtoValidator.
- `HcmShowrooms/HcmShowroomMapping.cs` — HcmShowroomMapping.
- `HcmShowrooms/HcmShowroomService.cs` — HcmShowroomService.
- `HcmShowrooms/IHcmShowroomService.cs` — IHcmShowroomService.

### HcmWorkers

- `HcmWorkers/HcmWorker/HcmWorker.cs` — HcmWorker.
- `HcmWorkers/HcmWorker/HcmWorkerConfiguration.cs` — HcmWorkerConfiguration.
- `HcmWorkers/HcmWorker/HcmWorkerController.cs` — HcmWorkerController.
- `HcmWorkers/HcmWorker/HcmWorkerDto.cs` — HcmWorkerDto, HcmWorkerLookupDto, HcmWorkerLookupPageDto, HcmWorkerAssignmentChainNodeDto, HcmWorkerOrganizationAssignmentV1Dto, HcmWorkerShowroomAssignmentDto, SaveHcmWorkerOrganizationAssignmentV1Request, SaveHcmWorkerShowroomAssignmentRequest.
- `HcmWorkers/HcmWorker/HcmWorkerDtoValidator.cs` — HcmWorkerDtoValidator.
- `HcmWorkers/HcmWorker/HcmWorkerMapping.cs` — HcmWorkerMapping.
- `HcmWorkers/HcmWorker/HcmWorkerQueryMapping.cs` — HcmWorkerListProjectionDto, HcmWorkerQueryMapping.
- `HcmWorkers/HcmWorker/HcmWorkerService.cs` — HcmWorkerService.
- `HcmWorkers/HcmWorker/IHcmWorkerService.cs` — IHcmWorkerService.
- `HcmWorkers/HcmWorkerOrganizationAssignmentV1/HcmWorkerOrganizationAssignmentV1.cs` — HcmWorkerOrganizationAssignmentV1.
- `HcmWorkers/HcmWorkerOrganizationAssignmentV1/HcmWorkerOrganizationAssignmentV1Configuration.cs` — HcmWorkerOrganizationAssignmentV1Configuration.
- `HcmWorkers/Structure/IOrganizationStructureService.cs` — IOrganizationStructureService.
- `HcmWorkers/Structure/OrganizationPeriod.cs` — OrganizationPeriod.
- `HcmWorkers/Structure/OrganizationStructureConfiguration.cs` — OrganizationRoleConfiguration, OrganizationHierarchyConfiguration, OrganizationHierarchyNodeConfiguration, HcmPositionConfiguration, HcmReportingHierarchyConfiguration, HcmPositionReportingLineConfiguration.
- `HcmWorkers/Structure/OrganizationStructureContracts.cs` — CreateOrganizationUnit, UpdateOrganizationUnit, CreateOrganizationRole, UpdateOrganizationRole, CreateOrganizationHierarchy, CreateOrganizationHierarchyWithRootNode, UpdateOrganizationHierarchy, CreateOrganizationNode, UpdateOrganizationNode, CreatePosition, UpdatePosition, AssignWorker, UpdateWorkerAssignment, TransferWorker, CloseOrganizationPeriod, OrganizationRoleInfo, OrganizationHierarchyInfo, OrganizationNodeInfo, PositionInfo, OrganizationUnitType.
- `HcmWorkers/Structure/OrganizationStructureController.cs` — OrganizationStructureController.
- `HcmWorkers/Structure/OrganizationStructureEntities.cs` — OrganizationRole, OrganizationHierarchy, OrganizationHierarchyNode, HcmPosition, HcmReportingHierarchy, HcmPositionReportingLine.
- `HcmWorkers/Structure/OrganizationStructureMapping.cs` — OrganizationStructureMapping.
- `HcmWorkers/Structure/OrganizationStructureService.cs` — OrganizationStructureService.
- `HcmWorkers/WorkerOrganizationAssignments/HcmWorkerOrganizationAssignment.cs` — HcmWorkerOrganizationAssignment.
- `HcmWorkers/WorkerOrganizationAssignments/HcmWorkerOrganizationAssignmentConfiguration.cs` — HcmWorkerOrganizationAssignmentConfiguration.
- `HcmWorkers/WorkerShowroomAssignments/HcmWorkerShowroomAssignment.cs` — HcmWorkerShowroomAssignment.
- `HcmWorkers/WorkerShowroomAssignments/HcmWorkerShowroomAssignmentConfiguration.cs` — HcmWorkerShowroomAssignmentConfiguration.

### LegalEntities

- `LegalEntities/Configuration/CompanyInfoConfiguration.cs` — CompanyInfoConfiguration.
- `LegalEntities/Controllers/CompanyInfoController.cs` — CompanyInfoController.
- `LegalEntities/Dtos/CompanyInfoDto.cs` — CompanyInfoDto.
- `LegalEntities/Entities/CompanyInfo.cs` — CompanyInfo.
- `LegalEntities/Interfaces/ICompanyInfoService.cs` — ICompanyInfoService.
- `LegalEntities/Mappings/CompanyInfoMapping.cs` — CompanyInfoMapping.
- `LegalEntities/Mappings/CompanyOperationMapping.cs` — CompanyWriteSource, CompanyPostalMappingSource, CompanyContactMappingSource, CompanyOperationMapping.
- `LegalEntities/Services/CompanyInfoService.cs` — CompanyInfoService.

### LogisticsAddresses

- `LogisticsAddresses/Configuration/AddressBookMapping.cs` — PostalAddressReadSource, ElectronicAddressReadSource, PostalAddressWriteSource, ElectronicAddressWriteSource, AddressBookMapping.
- `LogisticsAddresses/Configuration/ContactPersonConfiguration.cs` — ContactPersonConfiguration.
- `LogisticsAddresses/Configuration/DirPartyLocationRoleConfiguration.cs` — DirPartyLocationRoleConfiguration.
- `LogisticsAddresses/Configuration/DirPartyTableConfiguration.cs` — DirPartyTableConfiguration.
- `LogisticsAddresses/Configuration/LogisticsAddressCityConfiguration.cs` — LogisticsAddressCityConfiguration.
- `LogisticsAddresses/Configuration/LogisticsAddressCountryRegionConfiguration.cs` — LogisticsAddressCountryRegionConfiguration.
- `LogisticsAddresses/Configuration/LogisticsAddressCountyConfiguration.cs` — LogisticsAddressCountyConfiguration.
- `LogisticsAddresses/Configuration/LogisticsAddressDistrictConfiguration.cs` — LogisticsAddressDistrictConfiguration.
- `LogisticsAddresses/Configuration/LogisticsAddressStateConfiguration.cs` — LogisticsAddressStateConfiguration.
- `LogisticsAddresses/Configuration/LogisticsElectronicAddressConfiguration.cs` — LogisticsElectronicAddressConfiguration.
- `LogisticsAddresses/Configuration/LogisticsLocationConfiguration.cs` — LogisticsLocationConfiguration.
- `LogisticsAddresses/Configuration/LogisticsLocationRoleConfiguration.cs` — LogisticsLocationRoleConfiguration.
- `LogisticsAddresses/Configuration/LogisticsPostalAddressConfiguration.cs` — LogisticsPostalAddressConfiguration.
- `LogisticsAddresses/Controllers/LogisticsElectronicAddressController.cs` — LogisticsElectronicAddressController.
- `LogisticsAddresses/Controllers/LogisticsPostalAddressController.cs` — LogisticsPostalAddressController.
- `LogisticsAddresses/Dtos/ContactPersonDto.cs` — ContactPersonDto.
- `LogisticsAddresses/Entities/ContactPerson.cs` — ContactPerson.
- `LogisticsAddresses/Entities/DirPartyLocation.cs` — DirPartyLocation.
- `LogisticsAddresses/Entities/DirPartyLocationRole.cs` — DirPartyLocationRole.
- `LogisticsAddresses/Entities/DirPartyTable.cs` — DirPartyTable.
- `LogisticsAddresses/Entities/LogisticsAddressCity.cs` — LogisticsAddressCity.
- `LogisticsAddresses/Entities/LogisticsAddressCountryRegion.cs` — LogisticsAddressCountryRegion.
- `LogisticsAddresses/Entities/LogisticsAddressCounty.cs` — LogisticsAddressCounty.
- `LogisticsAddresses/Entities/LogisticsAddressDistrict.cs` — LogisticsAddressDistrict.
- `LogisticsAddresses/Entities/LogisticsAddressState.cs` — LogisticsAddressState.
- `LogisticsAddresses/Entities/LogisticsAddressZipCode.cs` — LogisticsAddressZipCode.
- `LogisticsAddresses/Entities/LogisticsElectronicAddress.cs` — LogisticsElectronicAddress.
- `LogisticsAddresses/Entities/LogisticsLocation.cs` — LogisticsLocation.
- `LogisticsAddresses/Entities/LogisticsLocationRole.cs` — LogisticsLocationRole.
- `LogisticsAddresses/Entities/LogisticsPostalAddress.cs` — LogisticsPostalAddress.
- `LogisticsAddresses/Interfaces/IAddressBookQueryService.cs` — IAddressBookQueryService.
- `LogisticsAddresses/Interfaces/IElectronicAddressService.cs` — IElectronicAddressService.
- `LogisticsAddresses/Interfaces/IGlobalAddressBookService.cs` — IGlobalAddressBookService.
- `LogisticsAddresses/Interfaces/ILocationService.cs` — ILocationService.
- `LogisticsAddresses/Interfaces/IPartyLocationService.cs` — IPartyLocationService.
- `LogisticsAddresses/Interfaces/IPartyService.cs` — IPartyService.
- `LogisticsAddresses/Interfaces/IPostalAddressService.cs` — IPostalAddressService.
- `LogisticsAddresses/Services/AddressBookQueryService.cs` — AddressBookQueryService.
- `LogisticsAddresses/Services/ElectronicAddressService.cs` — ElectronicAddressService.
- `LogisticsAddresses/Services/GlobalAddressBookService.cs` — GlobalAddressBookService.
- `LogisticsAddresses/Services/LocationService.cs` — LocationService.
- `LogisticsAddresses/Services/PartyLocationService.cs` — PartyLocationService.
- `LogisticsAddresses/Services/PartyService.cs` — PartyService.
- `LogisticsAddresses/Services/PostalAddressService.cs` — PostalAddressService.

### Markup

- `Markup/Configuration/MarkupAutoConfiguration.cs` — MarkupAutoTableConfiguration, MarkupAutoLineConfiguration.
- `Markup/Configuration/MarkupConfiguration.cs` — MarkupTableConfiguration.
- `Markup/Configuration/MarkupMapping.cs` — MarkupMapping.
- `Markup/Controllers/MarkupTableController.cs` — MarkupTableController.
- `Markup/Controllers/MarkupTransController.cs` — MarkupTransController.
- `Markup/Dtos/MarkupCodeDto.cs` — MarkupCodeDto.
- `Markup/Dtos/MarkupResult.cs` — MarkupServiceError, MarkupResult.
- `Markup/Dtos/MarkupTableDto.cs` — MarkupTableDto.
- `Markup/Dtos/MarkupTransDto.cs` — MarkupTransDto.
- `Markup/Entities/MarkupAutoLine.cs` — MarkupAutoLine.
- `Markup/Entities/MarkupAutoTable.cs` — MarkupAutoTable.
- `Markup/Entities/MarkupTable.cs` — MarkupTable.
- `Markup/Entities/MarkupTrans.cs` — MarkupTrans.
- `Markup/Interfaces/IMarkupChargeCalculator.cs` — IMarkupChargeCalculator.
- `Markup/Interfaces/IMarkupTableService.cs` — IMarkupTableService.
- `Markup/Interfaces/IMarkupTransCommandService.cs` — IMarkupTransCommandService.
- `Markup/Interfaces/IMarkupTransService.cs` — IMarkupTransService.
- `Markup/Services/MarkupChargeCalculator.cs` — MarkupChargeCalculator.
- `Markup/Services/MarkupTableService.cs` — MarkupTableService.
- `Markup/Services/MarkupTransCommandService.cs` — MarkupTransCommandService.
- `Markup/Services/MarkupTransService.cs` — MarkupTransService.
- `Markup/Validation/MarkupTransValidator.cs` — MarkupTransValidator.

### Nationalities

- `Nationalities/HcmNationality.cs` — HcmNationality.
- `Nationalities/HcmNationalityConfiguration.cs` — HcmNationalityConfiguration.
- `Nationalities/HcmNationalityController.cs` — HcmNationalityController, LegacyNationalityController.
- `Nationalities/HcmNationalityDto.cs` — HcmNationalityDto.
- `Nationalities/HcmNationalityDtoValidator.cs` — HcmNationalityDtoValidator.
- `Nationalities/HcmNationalityService.cs` — HcmNationalityService.
- `Nationalities/IHcmNationalityService.cs` — IHcmNationalityService.

### Occupations

- `Occupations/HcmOccupation.cs` — HcmOccupation.
- `Occupations/HcmOccupationConfiguration.cs` — HcmOccupationConfiguration.
- `Occupations/HcmOccupationController.cs` — HcmOccupationController, LegacyOccupationController.
- `Occupations/HcmOccupationDto.cs` — HcmOccupationDto.
- `Occupations/HcmOccupationDtoValidator.cs` — HcmOccupationDtoValidator.
- `Occupations/HcmOccupationService.cs` — HcmOccupationService.
- `Occupations/IHcmOccupationService.cs` — IHcmOccupationService.

### OrganizationUnits

- `OrganizationUnits/OrganizationUnit.cs` — OrganizationUnit.
- `OrganizationUnits/OrganizationUnitConfiguration.cs` — OrganizationUnitConfiguration.

### PaymentSchedules

- `PaymentSchedules/Configuration/PaymSchedConfiguration.cs` — PaymSchedConfiguration.
- `PaymentSchedules/Configuration/PaymSchedLineConfiguration.cs` — PaymSchedLineConfiguration.
- `PaymentSchedules/Controllers/PaymSchedController.cs` — PaymSchedController.
- `PaymentSchedules/Controllers/PaymSchedLineController.cs` — no type declaration.
- `PaymentSchedules/Dtos/PaymSchedDto.cs` — PaymSchedDto.
- `PaymentSchedules/Dtos/PaymSchedLineDto.cs` — PaymSchedLineDto.
- `PaymentSchedules/Entities/PaymSched.cs` — PaymSched.
- `PaymentSchedules/Entities/PaymSchedLine.cs` — PaymSchedLine.
- `PaymentSchedules/Interfaces/IPaymSchedLineService.cs` — IPaymSchedLineService.
- `PaymentSchedules/Interfaces/IPaymSchedService.cs` — IPaymSchedService.
- `PaymentSchedules/Services/PaymSchedLineService.cs` — PaymSchedLineService.
- `PaymentSchedules/Services/PaymSchedService.cs` — PaymSchedService.
- `PaymentSchedules/Validation/PaymSchedLineDtoValidator.cs` — PaymSchedLineDtoValidator.

### PaymentTerms

- `PaymentTerms/Controllers/PaymTermController.cs` — PaymTermController.
- `PaymentTerms/Dtos/PaymTermDto.cs` — PaymTermDto.
- `PaymentTerms/Entities/PaymTerm.cs` — PaymTerm.
- `PaymentTerms/Interfaces/IPaymTermService.cs` — IPaymTermService.
- `PaymentTerms/Services/PaymTermService.cs` — PaymTermService.
- `PaymentTerms/Validation/PaymTermValidators.cs` — PaymTermDtoValidator.

### Tax

- `Tax/Configuration/TaxAuthorityAddressConfiguration.cs` — TaxAuthorityAddressConfiguration.
- `Tax/Configuration/TaxDataConfiguration.cs` — TaxDataConfiguration.
- `Tax/Configuration/TaxExemptCodeTableConfiguration.cs` — TaxExemptCodeTableConfiguration.
- `Tax/Configuration/TaxGroupDataConfiguration.cs` — TaxGroupDataConfiguration.
- `Tax/Configuration/TaxGroupHeadingConfiguration.cs` — TaxGroupHeadingConfiguration.
- `Tax/Configuration/TaxItemGroupHeadingConfiguration.cs` — TaxItemGroupHeadingConfiguration.
- `Tax/Configuration/TaxJournalTransConfiguration.cs` — TaxJournalTransConfiguration.
- `Tax/Configuration/TaxLedgerAccountGroupConfiguration.cs` — TaxLedgerAccountGroupConfiguration.
- `Tax/Configuration/TaxOnItemConfiguration.cs` — TaxOnItemConfiguration.
- `Tax/Configuration/TaxOperationMapping.cs` — TaxGroupReadSource, TaxGroupLineReadSource, TaxItemGroupReadSource, TaxItemGroupLineReadSource, TaxGroupLineWriteSource, TaxItemGroupLineWriteSource, TaxGroupLineCreateSource, TaxItemGroupLineCreateSource, TaxOperationMapping.
- `Tax/Configuration/TaxPeriodHeadConfiguration.cs` — TaxPeriodHeadConfiguration.
- `Tax/Configuration/TaxReportPeriodConfiguration.cs` — TaxReportPeriodConfiguration.
- `Tax/Configuration/TaxTableConfiguration.cs` — TaxTableConfiguration.
- `Tax/Configuration/TaxTransConfiguration.cs` — TaxTransConfiguration.
- `Tax/Controllers/TaxAuthorityAddressController.cs` — TaxAuthorityAddressController.
- `Tax/Controllers/TaxExemptCodeController.cs` — TaxExemptCodeController.
- `Tax/Controllers/TaxGroupController.cs` — TaxGroupController.
- `Tax/Controllers/TaxItemGroupController.cs` — TaxItemGroupController.
- `Tax/Controllers/TaxLedgerAccountGroupController.cs` — TaxLedgerAccountGroupController.
- `Tax/Controllers/TaxPeriodHeadController.cs` — TaxPeriodHeadController.
- `Tax/Controllers/TaxTableController.cs` — TaxTableController.
- `Tax/Dtos/TaxAuthorityAddressDto.cs` — TaxAuthorityAddressDto.
- `Tax/Dtos/TaxDataDto.cs` — TaxDataDto.
- `Tax/Dtos/TaxExemptCodeDto.cs` — TaxExemptCodeDto.
- `Tax/Dtos/TaxGroupDataDto.cs` — TaxGroupDataDto.
- `Tax/Dtos/TaxGroupDto.cs` — TaxGroupDto.
- `Tax/Dtos/TaxItemGroupDto.cs` — TaxItemGroupDto.
- `Tax/Dtos/TaxJournalTransDto.cs` — TaxJournalTransDto.
- `Tax/Dtos/TaxLedgerAccountGroupDto.cs` — TaxLedgerAccountGroupDto.
- `Tax/Dtos/TaxOnItemDto.cs` — TaxOnItemDto.
- `Tax/Dtos/TaxPeriodHeadDto.cs` — TaxPeriodHeadDto.
- `Tax/Dtos/TaxReportPeriodDto.cs` — TaxReportPeriodDto.
- `Tax/Dtos/TaxTableDto.cs` — TaxTableDto.
- `Tax/Dtos/TaxTransDto.cs` — TaxTransDto.
- `Tax/Entities/TaxAuthorityAddress.cs` — TaxAuthorityAddress.
- `Tax/Entities/TaxData.cs` — TaxData.
- `Tax/Entities/TaxExemptCodeTable.cs` — TaxExemptCodeTable.
- `Tax/Entities/TaxGroupData.cs` — TaxGroupData.
- `Tax/Entities/TaxGroupHeading.cs` — TaxGroupHeading.
- `Tax/Entities/TaxItemGroupHeading.cs` — TaxItemGroupHeading.
- `Tax/Entities/TaxJournalTrans.cs` — TaxJournalTrans.
- `Tax/Entities/TaxLedgerAccountGroup.cs` — TaxLedgerAccountGroup.
- `Tax/Entities/TaxOnItem.cs` — TaxOnItem.
- `Tax/Entities/TaxPeriodHead.cs` — TaxPeriodHead.
- `Tax/Entities/TaxReportPeriod.cs` — TaxReportPeriod.
- `Tax/Entities/TaxTable.cs` — TaxTable.
- `Tax/Entities/TaxTrans.cs` — TaxTrans.
- `Tax/Interfaces/ITaxAuthorityAddressService.cs` — ITaxAuthorityAddressService.
- `Tax/Interfaces/ITaxCodeRateService.cs` — ITaxCodeRateService.
- `Tax/Interfaces/ITaxExemptCodeService.cs` — ITaxExemptCodeService.
- `Tax/Interfaces/ITaxGroupCommandService.cs` — ITaxGroupCommandService.
- `Tax/Interfaces/ITaxGroupLineService.cs` — ITaxGroupLineService.
- `Tax/Interfaces/ITaxGroupQueryService.cs` — ITaxGroupQueryService.
- `Tax/Interfaces/ITaxGroupService.cs` — ITaxGroupService.
- `Tax/Interfaces/ITaxItemGroupCommandService.cs` — ITaxItemGroupCommandService.
- `Tax/Interfaces/ITaxItemGroupLineService.cs` — ITaxItemGroupLineService.
- `Tax/Interfaces/ITaxItemGroupQueryService.cs` — ITaxItemGroupQueryService.
- `Tax/Interfaces/ITaxItemGroupService.cs` — ITaxItemGroupService.
- `Tax/Interfaces/ITaxLedgerAccountGroupService.cs` — ITaxLedgerAccountGroupService.
- `Tax/Interfaces/ITaxPeriodHeadCrudService.cs` — ITaxPeriodHeadCrudService.
- `Tax/Interfaces/ITaxPeriodService.cs` — ITaxPeriodService.
- `Tax/Interfaces/ITaxTableService.cs` — ITaxTableService.
- `Tax/Services/TaxAuthorityAddressService.cs` — TaxAuthorityAddressService.
- `Tax/Services/TaxCodeRateService.cs` — TaxCodeRateService.
- `Tax/Services/TaxExemptCodeService.cs` — TaxExemptCodeService.
- `Tax/Services/TaxGroupCommandService.cs` — TaxGroupCommandService.
- `Tax/Services/TaxGroupLineService.cs` — TaxGroupLineService.
- `Tax/Services/TaxGroupQueryService.cs` — TaxGroupQueryService.
- `Tax/Services/TaxGroupService.cs` — TaxGroupService.
- `Tax/Services/TaxItemGroupCommandService.cs` — TaxItemGroupCommandService.
- `Tax/Services/TaxItemGroupLineService.cs` — TaxItemGroupLineService.
- `Tax/Services/TaxItemGroupQueryService.cs` — TaxItemGroupQueryService.
- `Tax/Services/TaxItemGroupService.cs` — TaxItemGroupService.
- `Tax/Services/TaxLedgerAccountGroupService.cs` — TaxLedgerAccountGroupService.
- `Tax/Services/TaxPeriodHeadCrudService.cs` — TaxPeriodHeadCrudService.
- `Tax/Services/TaxPeriodService.cs` — TaxPeriodService.
- `Tax/Services/TaxTableService.cs` — TaxTableService.

## Architecture findings

- Existing ordinary CRUD controllers inherit `BaseController`; custom Markup, LogisticsAddresses, and OrganizationStructure endpoints delegate to dedicated services. No controller uses `DbContext`, `IUnitOfWork`, `Set<T>`, or `SaveChangesAsync` directly.
- All concrete service classes use `IUnitOfWork` directly or through `BaseService`. Where services expose `DbContext`, it is `unitOfWork.Context`, not a separately injected context.
- Finance assembly is included in Mapster scanning. Existing `IRegister` mappings cover nontrivial operations; no new mapping was needed.
- Finance DI already registers Foundation services and validators. No DI changes were needed.
- No validation, controller, service, DTO, entity, EF configuration, route, permission, migration, or database schema code was changed.
- `PaymentSchedules/Controllers/PaymSchedLineController.cs` is an empty placeholder, not a skipped controller class.

## Verification and limits

- API build: 0 errors; the incremental baseline emitted 9 project warnings.
- Full backend tests: 235 passed, 2 skipped, 0 failed.
- API compatibility: source routes, verbs, request/response types, and permission attributes are unchanged; no live HTTP request was run.
- Database/schema: no Foundation or migration source changed; no live database comparison was run.
- No classes modified, created, or removed. This report is the only new artifact from the Foundation review.
- Remaining issue: the empty `PaymSchedLineController.cs` placeholder has no implementation. Its removal or an added API would be a separate behavior decision.
