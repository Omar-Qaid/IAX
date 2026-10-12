# AccountsReceivable backend architecture review

Scope: `IXApi/src/Modules/Finance/AccountsReceivable`. The attachment contains copied `GeneralLedger` examples; AccountsReceivable is the requested module.

## Shared architecture and baseline

Reviewed `BaseController`, `IBaseService`, `BaseService`, `IUnitOfWork`, generic repository, API response and DTO contracts, Mapster scanning, validation base, authorization, company context, pagination, and Finance dependency injection. Baseline API build passed with 0 errors. No AccountsReceivable source file had pre-existing working-tree changes.

## Folder-by-folder review

All 13 existing feature folders were checked in alphabetical order, including folders containing only entities, DTOs and EF configuration. Each existing source file is inventoried below. Existing files were retained because no behavior-preserving architecture change was required.

| Folder | C# files | Result |
| --- | ---: | --- |
| Confirm | 10 | Reviewed; retained existing contracts and persistence boundaries. |
| CustParameters | 6 | Reviewed; retained existing contracts and persistence boundaries. |
| CustTrans | 6 | Reviewed; retained existing contracts and persistence boundaries. |
| Customer | 17 | Reviewed; retained existing contracts and persistence boundaries. |
| Invoice | 12 | Reviewed; retained existing contracts and persistence boundaries. |
| PackingSlip | 6 | Reviewed; retained existing contracts and persistence boundaries. |
| PaymMode | 6 | Reviewed; retained existing contracts and persistence boundaries. |
| PostingProfile | 14 | Reviewed; retained existing contracts and persistence boundaries. |
| PriceDisc | 32 | Reviewed; retained existing contracts and persistence boundaries. |
| SalesOrder | 43 | Reviewed; retained existing contracts and persistence boundaries. |
| SalesPool | 6 | Reviewed; retained existing contracts and persistence boundaries. |
| SalesQuotation | 11 | Reviewed; retained existing contracts and persistence boundaries. |
| Settlement | 6 | Reviewed; retained existing contracts and persistence boundaries. |

## Full source inventory

Status for each existing file: retained. File declarations are listed to make empty or type-only folders visible.

### Confirm

- `Confirm/Configuration/SalesOrderConfirmationMapping.cs` — SalesOrderConfirmationMapping.
- `Confirm/Controllers/SalesOrderConfirmationController.cs` — SalesOrderConfirmationController.
- `Confirm/Dtos/CustConfirmJourDto.cs` — CustConfirmJourDto.
- `Confirm/Dtos/CustConfirmTransDto.cs` — CustConfirmTransDto.
- `Confirm/Dtos/SalesOrderConfirmationDtos.cs` — PostConfirmationRequest, ConfirmationResult, ConfirmationListItemDto, ConfirmationHeaderDto, ConfirmationLineDto.
- `Confirm/Entities/CustConfirmJour.cs` — CustConfirmJour.
- `Confirm/Entities/CustConfirmTrans.cs` — CustConfirmTrans.
- `Confirm/Interfaces/ISalesOrderConfirmationService.cs` — ISalesOrderConfirmationService.
- `Confirm/Services/SalesOrderConfirmationService.cs` — SalesOrderConfirmationService, Calculation.
- `Confirm/Validation/SalesOrderConfirmationValidator.cs` — SalesOrderConfirmationValidator.

### CustParameters

- `CustParameters/Configuration/CustParametersConfiguration.cs` — CustParametersConfiguration.
- `CustParameters/Controllers/CustParametersController.cs` — CustParametersController.
- `CustParameters/Dtos/CustParametersDto.cs` — CustParametersDto.
- `CustParameters/Entities/CustParameters.cs` — CustParameters.
- `CustParameters/Services/CustParametersService.cs` — CustParametersService.
- `CustParameters/Services/ICustParametersService.cs` — ICustParametersService.

### CustTrans

- `CustTrans/Configuration/CustTransOpenConfiguration.cs` — CustTransOpenConfiguration.
- `CustTrans/Dtos/CustTransDto.cs` — CustTransDto.
- `CustTrans/Dtos/CustTransDtoValidator.cs` — CustTransDtoValidator.
- `CustTrans/Dtos/CustTransOpenDto.cs` — CustTransOpenDto.
- `CustTrans/Entities/CustTrans.cs` — CustTrans.
- `CustTrans/Entities/CustTransOpen.cs` — CustTransOpen.

### Customer

- `Customer/Configuration/CustGroupConfiguration.cs` — CustGroupConfiguration.
- `Customer/Configuration/CustTableConfiguration.cs` — CustTableConfiguration.
- `Customer/Configuration/CustomerMapping.cs` — CustomerMappingSource, CustomerSalesOrderAddressSource, CustomerSalesOrderContactSource, CustomerSalesOrderDefaultsSource, CustomerMapping.
- `Customer/Configuration/CustomerQuickMapping.cs` — CustomerQuickUpdateSource, CustomerQuickMapping.
- `Customer/Controllers/CustomerController.cs` — CustomerController.
- `Customer/Controllers/CustomerGroupController.cs` — CustomerGroupController.
- `Customer/Dtos/CustGroupDto.cs` — CustGroupDto.
- `Customer/Dtos/CustomerDto.cs` — CustomerDto.
- `Customer/Dtos/CustomerQuickCreateDto.cs` — CustomerQuickCreateDto, CustomerListDto, CustomerSalesOrderDefaultsDto, CustomerSalesOrderAddressDto, CustomerSalesOrderContactDto.
- `Customer/Entities/CustGroup.cs` — CustGroup.
- `Customer/Entities/CustTable.cs` — CustTable.
- `Customer/Interfaces/ICustomerGroupService.cs` — ICustomerGroupService.
- `Customer/Interfaces/ICustomerQuickService.cs` — ICustomerQuickService.
- `Customer/Interfaces/ICustomerService.cs` — ICustomerService.
- `Customer/Services/CustomerGroupService.cs` — CustomerGroupService.
- `Customer/Services/CustomerQuickService.cs` — CustomerQuickUpdateStatus, CustomerQuickUpdateResult, CustomerQuickService.
- `Customer/Services/CustomerService.cs` — CustomerService.

### Invoice

- `Invoice/Configuration/CustInvoiceJourConfiguration.cs` — CustInvoiceJourConfiguration.
- `Invoice/Configuration/CustInvoiceLineConfiguration.cs` — CustInvoiceLineConfiguration.
- `Invoice/Configuration/CustInvoiceTableConfiguration.cs` — CustInvoiceTableConfiguration.
- `Invoice/Configuration/CustInvoiceTransConfiguration.cs` — CustInvoiceTransConfiguration.
- `Invoice/Dtos/CustInvoiceJourDto.cs` — CustInvoiceJourDto.
- `Invoice/Dtos/CustInvoiceJourDtoValidator.cs` — CustInvoiceJourDtoValidator.
- `Invoice/Dtos/CustInvoiceTableDto.cs` — CustInvoiceTableDto.
- `Invoice/Dtos/CustInvoiceTransDto.cs` — CustInvoiceTransDto.
- `Invoice/Entities/CustInvoiceJour.cs` — CustInvoiceJour.
- `Invoice/Entities/CustInvoiceLine.cs` — CustInvoiceLine.
- `Invoice/Entities/CustInvoiceTable.cs` — CustInvoiceTable.
- `Invoice/Entities/CustInvoiceTrans.cs` — CustInvoiceTrans.

### PackingSlip

- `PackingSlip/Configuration/CustPackingSlipJourConfiguration.cs` — CustPackingSlipJourConfiguration.
- `PackingSlip/Configuration/CustPackingSlipTransConfiguration.cs` — CustPackingSlipTransConfiguration.
- `PackingSlip/Dtos/CustPackingSlipJourDto.cs` — CustPackingSlipJourDto.
- `PackingSlip/Dtos/CustPackingSlipTransDto.cs` — CustPackingSlipTransDto.
- `PackingSlip/Entities/CustPackingSlipJour.cs` — CustPackingSlipJour.
- `PackingSlip/Entities/CustPackingSlipTrans.cs` — CustPackingSlipTrans.

### PaymMode

- `PaymMode/Configuration/CustPaymModeConfiguration.cs` — CustPaymModeConfiguration.
- `PaymMode/Controllers/CustPaymModeController.cs` — CustPaymModeController.
- `PaymMode/Dtos/CustPaymModeDto.cs` — CustPaymModeDto.
- `PaymMode/Entities/CustPaymModeTable.cs` — CustPaymModeTable.
- `PaymMode/Interfaces/ICustPaymModeService.cs` — ICustPaymModeService.
- `PaymMode/Services/CustPaymModeService.cs` — CustPaymModeService.

### PostingProfile

- `PostingProfile/Configuration/CustLedgerAccountsConfiguration.cs` — CustLedgerAccountsConfiguration.
- `PostingProfile/Configuration/CustLedgerConfiguration.cs` — CustLedgerConfiguration.
- `PostingProfile/Controllers/CustLedgerAccountsController.cs` — CustLedgerAccountsController.
- `PostingProfile/Controllers/CustLedgerController.cs` — CustLedgerController.
- `PostingProfile/Dtos/CustLedgerAccountsDto.cs` — CustLedgerAccountsDto.
- `PostingProfile/Dtos/CustLedgerDto.cs` — CustLedgerDto.
- `PostingProfile/Entities/CustLedger.cs` — CustLedger.
- `PostingProfile/Entities/CustLedgerAccounts.cs` — CustLedgerAccounts.
- `PostingProfile/Interfaces/ICustPostingProfileService.cs` — ICustPostingProfileService.
- `PostingProfile/Services/CustLedgerAccountsService.cs` — CustLedgerAccountsService.
- `PostingProfile/Services/CustLedgerService.cs` — CustLedgerService.
- `PostingProfile/Services/CustPostingProfileService.cs` — CustPostingProfileService.
- `PostingProfile/Services/ICustLedgerAccountsService.cs` — ICustLedgerAccountsService.
- `PostingProfile/Services/ICustLedgerService.cs` — ICustLedgerService.

### PriceDisc

- `PriceDisc/Configuration/PriceDiscAdmNameConfiguration.cs` — PriceDiscAdmNameConfiguration.
- `PriceDisc/Configuration/PriceDiscAdmTableConfiguration.cs` — PriceDiscAdmTableConfiguration.
- `PriceDisc/Configuration/PriceDiscAdmTransConfiguration.cs` — PriceDiscAdmTransConfiguration.
- `PriceDisc/Configuration/PriceDiscGroupConfiguration.cs` — PriceDiscGroupConfiguration.
- `PriceDisc/Configuration/PriceDiscTableConfiguration.cs` — PriceDiscTableConfiguration.
- `PriceDisc/Controllers/PriceDiscAdmNameController.cs` — PriceDiscAdmNameController.
- `PriceDisc/Controllers/PriceDiscAdmTableController.cs` — PriceDiscAdmTableController.
- `PriceDisc/Controllers/PriceDiscAdmTransController.cs` — PriceDiscAdmTransController.
- `PriceDisc/Controllers/PriceDiscGroupController.cs` — PriceDiscGroupController.
- `PriceDisc/Controllers/PriceDiscTableController.cs` — PriceDiscTableController.
- `PriceDisc/Dtos/PriceDiscAdmNameDto.cs` — PriceDiscAdmNameDto.
- `PriceDisc/Dtos/PriceDiscAdmTableDto.cs` — PriceDiscAdmTableDto.
- `PriceDisc/Dtos/PriceDiscAdmTransDto.cs` — PriceDiscAdmTransDto.
- `PriceDisc/Dtos/PriceDiscGroupDto.cs` — PriceDiscGroupDto.
- `PriceDisc/Dtos/PriceDiscTableDto.cs` — PriceDiscTableDto.
- `PriceDisc/Entities/PriceDiscAdmName.cs` — PriceDiscAdmName.
- `PriceDisc/Entities/PriceDiscAdmTable.cs` — PriceDiscAdmTable.
- `PriceDisc/Entities/PriceDiscAdmTrans.cs` — PriceDiscAdmTrans.
- `PriceDisc/Entities/PriceDiscGroup.cs` — PriceDiscGroup.
- `PriceDisc/Entities/PriceDiscTable.cs` — PriceDiscTable.
- `PriceDisc/Services/IPriceDiscAdmNameService.cs` — IPriceDiscAdmNameService.
- `PriceDisc/Services/IPriceDiscAdmTableService.cs` — IPriceDiscAdmTableService.
- `PriceDisc/Services/IPriceDiscAdmTransService.cs` — IPriceDiscAdmTransService.
- `PriceDisc/Services/IPriceDiscGroupService.cs` — IPriceDiscGroupService.
- `PriceDisc/Services/IPriceDiscJournalPostingService.cs` — IPriceDiscJournalPostingService.
- `PriceDisc/Services/IPriceDiscTableService.cs` — IPriceDiscTableService.
- `PriceDisc/Services/PriceDiscAdmNameService.cs` — PriceDiscAdmNameService.
- `PriceDisc/Services/PriceDiscAdmTableService.cs` — PriceDiscAdmTableService.
- `PriceDisc/Services/PriceDiscAdmTransService.cs` — PriceDiscAdmTransService.
- `PriceDisc/Services/PriceDiscGroupService.cs` — PriceDiscGroupService.
- `PriceDisc/Services/PriceDiscJournalPostingService.cs` — PriceDiscJournalPostStatus, PriceDiscJournalPostResult, PriceDiscJournalPostingService.
- `PriceDisc/Services/PriceDiscTableService.cs` — PriceDiscTableService.

### SalesOrder

- `SalesOrder/Configuration/SalesLineConfiguration.cs` — SalesLineConfiguration.
- `SalesOrder/Configuration/SalesOrderListMapping.cs` — SalesOrderListMappingSource, SalesOrderListMapping.
- `SalesOrder/Configuration/SalesOrderQuickCreateMapping.cs` — SalesOrderQuickCreateMappingSource, SalesOrderQuickCreateMapping.
- `SalesOrder/Configuration/SalesTableConfiguration.cs` — SalesTableConfiguration.
- `SalesOrder/Controllers/SalesOrderCopyController.cs` — SalesOrderCopyController, CopyRequest.
- `SalesOrder/Controllers/SalesTableController.cs` — SalesTableController, AddSalesLineInput, CreateSalesDeliveryAddressInput, UpdateSalesHeaderInput.
- `SalesOrder/Dtos/SalesLineDto.cs` — SalesLineDto.
- `SalesOrder/Dtos/SalesLineInputDto.cs` — SalesLineInputDto.
- `SalesOrder/Dtos/SalesOrderListDto.cs` — SalesOrderListDto.
- `SalesOrder/Dtos/SalesOrderQuickCreateDto.cs` — SalesOrderQuickCreateDto.
- `SalesOrder/Dtos/SalesTableDto.cs` — SalesTableDto.
- `SalesOrder/Dtos/SalesTableDtoValidator.cs` — SalesTableDtoValidator.
- `SalesOrder/Dtos/UpdateSalesHeaderInputDto.cs` — UpdateSalesHeaderInputDto.
- `SalesOrder/Entities/SalesLine.cs` — SalesLine.
- `SalesOrder/Entities/SalesTable.cs` — SalesTable.
- `SalesOrder/Interfaces/IInventDimensionResolver.cs` — IInventDimensionResolver.
- `SalesOrder/Interfaces/ISalesInventoryDemandService.cs` — ISalesInventoryDemandService.
- `SalesOrder/Interfaces/ISalesInventoryNumberService.cs` — ISalesInventoryNumberService.
- `SalesOrder/Interfaces/ISalesOrderCancellationService.cs` — ISalesOrderCancellationService.
- `SalesOrder/Interfaces/ISalesOrderCopyService.cs` — ISalesOrderCopyService, SalesOrderCopyLookupResult.
- `SalesOrder/Interfaces/ISalesOrderDeliveryAddressService.cs` — ISalesOrderDeliveryAddressService.
- `SalesOrder/Interfaces/ISalesOrderDiscountService.cs` — ISalesOrderDiscountService.
- `SalesOrder/Interfaces/ISalesOrderHeaderService.cs` — ISalesOrderHeaderService.
- `SalesOrder/Interfaces/ISalesOrderLineService.cs` — ISalesOrderLineService.
- `SalesOrder/Interfaces/ISalesOrderLookupService.cs` — ISalesOrderLookupService.
- `SalesOrder/Interfaces/ISalesOrderQuickCreateService.cs` — ISalesOrderQuickCreateService.
- `SalesOrder/Interfaces/ISalesOrderReadService.cs` — ISalesOrderReadService.
- `SalesOrder/Services/InventDimensionResolver.cs` — InventDimensionResolver.
- `SalesOrder/Services/SalesInventoryDemandService.cs` — SalesInventoryDemandService.
- `SalesOrder/Services/SalesInventoryNumberService.cs` — SalesInventoryNumberService.
- `SalesOrder/Services/SalesOrderCancellationService.cs` — SalesOrderCancelStatus, SalesOrderCancellationService.
- `SalesOrder/Services/SalesOrderCopyService.cs` — SalesOrderCopyRequest, SalesOrderCopyStatus, SalesOrderCopyResult, SalesOrderCopyService, CopySourceLine.
- `SalesOrder/Services/SalesOrderDeliveryAddressService.cs` — SalesDeliveryAddressStatus, SalesDeliveryAddressResult, SalesOrderDeliveryAddressService.
- `SalesOrder/Services/SalesOrderDiscountService.cs` — SalesDiscountRecalculationStatus, SalesDiscountRecalculationResult, SalesOrderDiscountService.
- `SalesOrder/Services/SalesOrderHeaderService.cs` — SalesHeaderUpdateStatus, SalesHeaderUpdateResult, SalesOrderHeaderService.
- `SalesOrder/Services/SalesOrderLinePresenter.cs` — SalesOrderLinePresenter.
- `SalesOrder/Services/SalesOrderLineService.cs` — SalesLineWriteStatus, SalesLineWriteResult, SalesOrderLineService.
- `SalesOrder/Services/SalesOrderLookupService.cs` — SalesOrderLookupService.
- `SalesOrder/Services/SalesOrderQuickCreateService.cs` — SalesOrderQuickCreateResult, SalesOrderQuickCreateService.
- `SalesOrder/Services/SalesOrderReadService.cs` — SalesOrderReadService.
- `SalesOrder/Services/SalesOrderValidationService.cs` — SalesOrderValidationService.
- `SalesOrder/Services/SalesUnitConversionService.cs` — SalesUnitConversionService.
- `SalesOrder/Validation/SalesOrderCopyValidator.cs` — SalesOrderCopyValidator.

### SalesPool

- `SalesPool/Configuration/SalesPoolConfiguration.cs` — SalesPoolConfiguration.
- `SalesPool/Controllers/SalesPoolController.cs` — SalesPoolController.
- `SalesPool/Dtos/SalesPoolDto.cs` — SalesPoolDto.
- `SalesPool/Entities/SalesPool.cs` — SalesPool.
- `SalesPool/Services/ISalesPoolService.cs` — ISalesPoolService.
- `SalesPool/Services/SalesPoolService.cs` — SalesPoolService.

### SalesQuotation

- `SalesQuotation/Configuration/CustQuotationJourConfiguration.cs` — CustQuotationJourConfiguration.
- `SalesQuotation/Configuration/CustQuotationTransConfiguration.cs` — CustQuotationTransConfiguration.
- `SalesQuotation/Configuration/SalesQuotationConfiguration.cs` — SalesQuotationConfiguration.
- `SalesQuotation/Configuration/SalesQuotationLineConfiguration.cs` — SalesQuotationLineConfiguration.
- `SalesQuotation/Dtos/CustQuotationJourDto.cs` — CustQuotationJourDto.
- `SalesQuotation/Dtos/SalesQuotationDto.cs` — SalesQuotationTableDto.
- `SalesQuotation/Dtos/SalesQuotationDtoValidator.cs` — SalesQuotationDtoValidator.
- `SalesQuotation/Entities/CustQuotationJour.cs` — CustQuotationJour.
- `SalesQuotation/Entities/CustQuotationTrans.cs` — CustQuotationTrans.
- `SalesQuotation/Entities/SalesQuotationLine.cs` — SalesQuotationLine.
- `SalesQuotation/Entities/SalesQuotationTable.cs` — SalesQuotationTable.

### Settlement

- `Settlement/Configuration/SpecTransConfiguration.cs` — SpecTransConfiguration.
- `Settlement/Dtos/CustSettlementDto.cs` — CustSettlementDto.
- `Settlement/Dtos/CustSettlementDtoValidator.cs` — CustSettlementDtoValidator.
- `Settlement/Dtos/SpecTransDto.cs` — SpecTransDto.
- `Settlement/Entities/CustSettlement.cs` — CustSettlement.
- `Settlement/Entities/SpecTrans.cs` — SpecTrans.

## Architecture and compatibility findings

- Ordinary CRUD controllers inherit `BaseController` with service interfaces extending `IBaseService`. Custom sales-order, copy, confirmation and customer endpoints delegate business operations to dedicated services. No controller accesses `DbContext`, `IUnitOfWork`, `Set<T>`, or `SaveChangesAsync` directly.
- Concrete persistence services use `IUnitOfWork` directly or via `BaseService`. `SalesInventoryNumberService` delegates exclusively to the shared number-sequence service and has no direct persistence.
- Entity/DTO transformations use Mapster where needed. `CustomerQuickService` constructs `AddressInfoDto` as an input command for address creation; it is not an entity-to-DTO projection.
- Existing configuration, entities, DTO contracts, interfaces, validators, services, controllers, DI and Mapster registrations were retained. No new class, service, mapping, validation or dependency registration was needed.
- No entity, EF configuration, migration, index, relationship, route, verb, permission or schema source was changed.

## Verification and remaining limits

- API build: 0 errors, 171 project warnings in the baseline build.
- Full backend tests: 235 passed, 2 skipped, 0 failed.
- API compatibility: source route, verb, request/response and permission declarations are unchanged. No live HTTP request was run.
- Database/schema: no AccountsReceivable or migration source changed. No live database comparison was run.
- The sales-order discount recalculation POST currently inherits the controller View permission. It is a mutating endpoint; changing its permission would alter authorization behavior and was outside this behavior-preserving refactor.
- No AccountsReceivable class was modified, created or removed. This review report is the only new artifact for this module.
