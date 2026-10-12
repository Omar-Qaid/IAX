# AccountsReceivable backend architecture refactor

Reviewed 175 current C# files across 13 feature folders. The starting module had 160 files; 15 new architecture files were added.

## Shared architecture checked

Inspected BaseController, IBaseService, BaseService, IUnitOfWork and UnitOfWork, generic repository, APIResponse, DTO and entity conventions, Mapster scanning, FluentValidation registration, DomainPermission, CompanyExecutionContext, exception handling, logging, dependency injection, and paging/filtering. Existing Shared components remain the source of truth.

## Folder results

| Folder | Files | Result |
|---|---:|---|
| Confirm | 10 | Reviewed; existing classes retained |
| CustParameters | 6 | Reviewed; existing classes retained |
| CustTrans | 6 | Reviewed; existing classes retained |
| Customer | 17 | Refactored confirmed gaps; reviewed remaining classes |
| Invoice | 12 | Reviewed; existing classes retained |
| PackingSlip | 6 | Reviewed; existing classes retained |
| PaymMode | 6 | Reviewed; existing classes retained |
| PostingProfile | 14 | Reviewed; existing classes retained |
| PriceDisc | 32 | Refactored confirmed gaps; reviewed remaining classes |
| SalesOrder | 43 | Refactored confirmed gaps; reviewed remaining classes |
| SalesPool | 6 | Reviewed; existing classes retained |
| SalesQuotation | 11 | Reviewed; existing classes retained |
| Settlement | 6 | Reviewed; existing classes retained |

## Requested final checklist

1. **Folders analyzed:** Confirm, CustParameters, CustTrans, Customer, Invoice, PackingSlip, PaymMode, PostingProfile, PriceDisc, SalesOrder, SalesPool, SalesQuotation and Settlement. All nested source folders are included in the inventory below.
2. **Every class analyzed:** See the complete file and declaration inventory below. Entity-only areas were retained; no new endpoint was invented.
3. **Classes modified:** 17 existing AccountsReceivable files. The changes are concentrated in Customer mapping, PriceDisc posting, and SalesOrder controller/service boundaries.
4. **Classes created:** 15 AccountsReceivable files: 9 SalesOrder service interfaces, 2 read services, 2 Mapster configurations, 1 PriceDisc posting interface, and 1 SalesOrder copy validator. Three focused test files were also created.
5. **Classes removed:** None.
6. **Controller changes:** SalesTableController delegates all reads, lookups, calculations and commands to services. SalesOrderCopyController delegates reads and copy to ISalesOrderCopyService. PriceDiscAdmTableController uses a posting service interface. All 15 AccountsReceivable controllers have zero direct EF query or DbContext references.
7. **Service changes:** SalesOrder read, lookup, copy, validation, commands, inventory demand, unit conversion and dimension resolution use IUnitOfWork.Context. PriceDisc journal posting uses IUnitOfWork. Customer defaults now map through Mapster.
8. **IBaseService usage:** Existing generic CRUD interfaces still extend IBaseService and their services still extend BaseService. Transactional SalesOrder and confirmation operations use dedicated service interfaces because generic CRUD would not preserve their workflow.
9. **IUnitOfWork usage:** All AccountsReceivable persistence services use IUnitOfWork or BaseService, which holds IUnitOfWork. Transactional services retain their existing EF transaction and SaveChanges ordering through the same scoped ApplicationDbContext.
10. **Mapster configuration:** Added SalesOrderListMapping and SalesOrderQuickCreateMapping; extended CustomerMapping for sales-order address/contact/default DTOs. Existing mappings and Shared Mapster scanning remain in use.
11. **Validation changes:** SalesOrderCopyValidator now applies the existing copy-request rules inside SalesOrderCopyService. The controller still returns the same validation status and messages. Other validators and business rules were retained.
12. **Configuration changes:** Only mapping configuration changed. No EF entity configuration was edited.
13. **Dependency Injection changes:** FinanceModule registers the new lookup/read/copy, SalesOrder command, and PriceDisc posting interfaces. Existing number-sequence and domain-helper registrations remain.
14. **Architecture issues fixed:** Removed direct EF access from SalesOrder controllers, moved list/totals/lookup calculations into services, replaced manual customer and sales-order entity-to-DTO mapping, and moved PriceDisc posting behind an interface and IUnitOfWork.
15. **Build/compile result:** Finance module build: 0 errors, 11 warnings. Final incremental IXApi build: 0 errors, 0 warnings. A prior full project build emitted 161 existing project warnings.
16. **API behavior verification:** Compared all route, HTTP method and DomainPermission attributes in the three changed controllers with HEAD; all 42 attributes match. Thirteen focused AccountsReceivable tests pass. No authenticated live API request was run.
17. **Database/schema verification:** No entity, EF mapping or migration file was modified. No live database comparison was run.
18. **Remaining issues:** Existing entity-only folders have no APIs to refactor. RecalculateDiscounts is a mutating POST that still inherits the controller's View permission; its permission was preserved because this task required API authorization compatibility. CustomerQuickService builds an AddressInfoDto command input for the address service. Custom SalesOrder services use EF through IUnitOfWork.Context for multi-entity transactions. Live API/database acceptance remains unverified.

## Complete file and declaration inventory

### Confirm

| File | Declarations | Result |
|---|---|---|
| `Configuration/SalesOrderConfirmationMapping.cs` | SalesOrderConfirmationMapping | Reviewed; retained |
| `Controllers/SalesOrderConfirmationController.cs` | SalesOrderConfirmationController | Reviewed; retained |
| `Dtos/CustConfirmJourDto.cs` | CustConfirmJourDto | Reviewed; retained |
| `Dtos/CustConfirmTransDto.cs` | CustConfirmTransDto | Reviewed; retained |
| `Dtos/SalesOrderConfirmationDtos.cs` | PostConfirmationRequest, ConfirmationResult, ConfirmationListItemDto, ConfirmationHeaderDto, ConfirmationLineDto | Reviewed; retained |
| `Entities/CustConfirmJour.cs` | CustConfirmJour | Reviewed; retained |
| `Entities/CustConfirmTrans.cs` | CustConfirmTrans | Reviewed; retained |
| `Interfaces/ISalesOrderConfirmationService.cs` | ISalesOrderConfirmationService | Reviewed; retained |
| `Services/SalesOrderConfirmationService.cs` | SalesOrderConfirmationService, Calculation | Reviewed; retained |
| `Validation/SalesOrderConfirmationValidator.cs` | SalesOrderConfirmationValidator | Reviewed; retained |

### CustParameters

| File | Declarations | Result |
|---|---|---|
| `Configuration/CustParametersConfiguration.cs` | CustParametersConfiguration | Reviewed; retained |
| `Controllers/CustParametersController.cs` | CustParametersController | Reviewed; retained |
| `Dtos/CustParametersDto.cs` | CustParametersDto | Reviewed; retained |
| `Entities/CustParameters.cs` | CustParameters | Reviewed; retained |
| `Services/CustParametersService.cs` | CustParametersService | Reviewed; retained |
| `Services/ICustParametersService.cs` | ICustParametersService | Reviewed; retained |

### CustTrans

| File | Declarations | Result |
|---|---|---|
| `Configuration/CustTransOpenConfiguration.cs` | CustTransOpenConfiguration | Reviewed; retained |
| `Dtos/CustTransDto.cs` | CustTransDto | Reviewed; retained |
| `Dtos/CustTransDtoValidator.cs` | CustTransDtoValidator | Reviewed; retained |
| `Dtos/CustTransOpenDto.cs` | CustTransOpenDto | Reviewed; retained |
| `Entities/CustTrans.cs` | CustTrans | Reviewed; retained |
| `Entities/CustTransOpen.cs` | CustTransOpen | Reviewed; retained |

### Customer

| File | Declarations | Result |
|---|---|---|
| `Configuration/CustGroupConfiguration.cs` | CustGroupConfiguration | Reviewed; retained |
| `Configuration/CustTableConfiguration.cs` | CustTableConfiguration | Reviewed; retained |
| `Configuration/CustomerMapping.cs` | CustomerMappingSource, CustomerSalesOrderAddressSource, CustomerSalesOrderContactSource, CustomerSalesOrderDefaultsSource, CustomerMapping | Refactored |
| `Configuration/CustomerQuickMapping.cs` | CustomerQuickUpdateSource, CustomerQuickMapping | Reviewed; retained |
| `Controllers/CustomerController.cs` | CustomerController | Reviewed; retained |
| `Controllers/CustomerGroupController.cs` | CustomerGroupController | Reviewed; retained |
| `Dtos/CustGroupDto.cs` | CustGroupDto | Reviewed; retained |
| `Dtos/CustomerDto.cs` | CustomerDto | Reviewed; retained |
| `Dtos/CustomerQuickCreateDto.cs` | CustomerQuickCreateDto, CustomerListDto, CustomerSalesOrderDefaultsDto, CustomerSalesOrderAddressDto, CustomerSalesOrderContactDto | Reviewed; retained |
| `Entities/CustGroup.cs` | CustGroup | Reviewed; retained |
| `Entities/CustTable.cs` | CustTable | Reviewed; retained |
| `Interfaces/ICustomerGroupService.cs` | ICustomerGroupService | Reviewed; retained |
| `Interfaces/ICustomerQuickService.cs` | ICustomerQuickService | Reviewed; retained |
| `Interfaces/ICustomerService.cs` | ICustomerService | Reviewed; retained |
| `Services/CustomerGroupService.cs` | CustomerGroupService | Reviewed; retained |
| `Services/CustomerQuickService.cs` | CustomerQuickUpdateStatus, CustomerQuickUpdateResult, CustomerQuickService | Reviewed; retained |
| `Services/CustomerService.cs` | CustomerService | Refactored |

### Invoice

| File | Declarations | Result |
|---|---|---|
| `Configuration/CustInvoiceJourConfiguration.cs` | CustInvoiceJourConfiguration | Reviewed; retained |
| `Configuration/CustInvoiceLineConfiguration.cs` | CustInvoiceLineConfiguration | Reviewed; retained |
| `Configuration/CustInvoiceTableConfiguration.cs` | CustInvoiceTableConfiguration | Reviewed; retained |
| `Configuration/CustInvoiceTransConfiguration.cs` | CustInvoiceTransConfiguration | Reviewed; retained |
| `Dtos/CustInvoiceJourDto.cs` | CustInvoiceJourDto | Reviewed; retained |
| `Dtos/CustInvoiceJourDtoValidator.cs` | CustInvoiceJourDtoValidator | Reviewed; retained |
| `Dtos/CustInvoiceTableDto.cs` | CustInvoiceTableDto | Reviewed; retained |
| `Dtos/CustInvoiceTransDto.cs` | CustInvoiceTransDto | Reviewed; retained |
| `Entities/CustInvoiceJour.cs` | CustInvoiceJour | Reviewed; retained |
| `Entities/CustInvoiceLine.cs` | CustInvoiceLine | Reviewed; retained |
| `Entities/CustInvoiceTable.cs` | CustInvoiceTable | Reviewed; retained |
| `Entities/CustInvoiceTrans.cs` | CustInvoiceTrans | Reviewed; retained |

### PackingSlip

| File | Declarations | Result |
|---|---|---|
| `Configuration/CustPackingSlipJourConfiguration.cs` | CustPackingSlipJourConfiguration | Reviewed; retained |
| `Configuration/CustPackingSlipTransConfiguration.cs` | CustPackingSlipTransConfiguration | Reviewed; retained |
| `Dtos/CustPackingSlipJourDto.cs` | CustPackingSlipJourDto | Reviewed; retained |
| `Dtos/CustPackingSlipTransDto.cs` | CustPackingSlipTransDto | Reviewed; retained |
| `Entities/CustPackingSlipJour.cs` | CustPackingSlipJour | Reviewed; retained |
| `Entities/CustPackingSlipTrans.cs` | CustPackingSlipTrans | Reviewed; retained |

### PaymMode

| File | Declarations | Result |
|---|---|---|
| `Configuration/CustPaymModeConfiguration.cs` | CustPaymModeConfiguration | Reviewed; retained |
| `Controllers/CustPaymModeController.cs` | CustPaymModeController | Reviewed; retained |
| `Dtos/CustPaymModeDto.cs` | CustPaymModeDto | Reviewed; retained |
| `Entities/CustPaymModeTable.cs` | CustPaymModeTable | Reviewed; retained |
| `Interfaces/ICustPaymModeService.cs` | ICustPaymModeService | Reviewed; retained |
| `Services/CustPaymModeService.cs` | CustPaymModeService | Reviewed; retained |

### PostingProfile

| File | Declarations | Result |
|---|---|---|
| `Configuration/CustLedgerAccountsConfiguration.cs` | CustLedgerAccountsConfiguration | Reviewed; retained |
| `Configuration/CustLedgerConfiguration.cs` | CustLedgerConfiguration | Reviewed; retained |
| `Controllers/CustLedgerAccountsController.cs` | CustLedgerAccountsController | Reviewed; retained |
| `Controllers/CustLedgerController.cs` | CustLedgerController | Reviewed; retained |
| `Dtos/CustLedgerAccountsDto.cs` | CustLedgerAccountsDto | Reviewed; retained |
| `Dtos/CustLedgerDto.cs` | CustLedgerDto | Reviewed; retained |
| `Entities/CustLedger.cs` | CustLedger | Reviewed; retained |
| `Entities/CustLedgerAccounts.cs` | CustLedgerAccounts | Reviewed; retained |
| `Interfaces/ICustPostingProfileService.cs` | ICustPostingProfileService | Reviewed; retained |
| `Services/CustLedgerAccountsService.cs` | CustLedgerAccountsService | Reviewed; retained |
| `Services/CustLedgerService.cs` | CustLedgerService | Reviewed; retained |
| `Services/CustPostingProfileService.cs` | CustPostingProfileService | Reviewed; retained |
| `Services/ICustLedgerAccountsService.cs` | ICustLedgerAccountsService | Reviewed; retained |
| `Services/ICustLedgerService.cs` | ICustLedgerService | Reviewed; retained |

### PriceDisc

| File | Declarations | Result |
|---|---|---|
| `Configuration/PriceDiscAdmNameConfiguration.cs` | PriceDiscAdmNameConfiguration | Reviewed; retained |
| `Configuration/PriceDiscAdmTableConfiguration.cs` | PriceDiscAdmTableConfiguration | Reviewed; retained |
| `Configuration/PriceDiscAdmTransConfiguration.cs` | PriceDiscAdmTransConfiguration | Reviewed; retained |
| `Configuration/PriceDiscGroupConfiguration.cs` | PriceDiscGroupConfiguration | Reviewed; retained |
| `Configuration/PriceDiscTableConfiguration.cs` | PriceDiscTableConfiguration | Reviewed; retained |
| `Controllers/PriceDiscAdmNameController.cs` | PriceDiscAdmNameController | Reviewed; retained |
| `Controllers/PriceDiscAdmTableController.cs` | PriceDiscAdmTableController | Refactored |
| `Controllers/PriceDiscAdmTransController.cs` | PriceDiscAdmTransController | Reviewed; retained |
| `Controllers/PriceDiscGroupController.cs` | PriceDiscGroupController | Reviewed; retained |
| `Controllers/PriceDiscTableController.cs` | PriceDiscTableController | Reviewed; retained |
| `Dtos/PriceDiscAdmNameDto.cs` | PriceDiscAdmNameDto | Reviewed; retained |
| `Dtos/PriceDiscAdmTableDto.cs` | PriceDiscAdmTableDto | Reviewed; retained |
| `Dtos/PriceDiscAdmTransDto.cs` | PriceDiscAdmTransDto | Reviewed; retained |
| `Dtos/PriceDiscGroupDto.cs` | PriceDiscGroupDto | Reviewed; retained |
| `Dtos/PriceDiscTableDto.cs` | PriceDiscTableDto | Reviewed; retained |
| `Entities/PriceDiscAdmName.cs` | PriceDiscAdmName | Reviewed; retained |
| `Entities/PriceDiscAdmTable.cs` | PriceDiscAdmTable | Reviewed; retained |
| `Entities/PriceDiscAdmTrans.cs` | PriceDiscAdmTrans | Reviewed; retained |
| `Entities/PriceDiscGroup.cs` | PriceDiscGroup | Reviewed; retained |
| `Entities/PriceDiscTable.cs` | PriceDiscTable | Reviewed; retained |
| `Services/IPriceDiscAdmNameService.cs` | IPriceDiscAdmNameService | Reviewed; retained |
| `Services/IPriceDiscAdmTableService.cs` | IPriceDiscAdmTableService | Reviewed; retained |
| `Services/IPriceDiscAdmTransService.cs` | IPriceDiscAdmTransService | Reviewed; retained |
| `Services/IPriceDiscGroupService.cs` | IPriceDiscGroupService | Reviewed; retained |
| `Services/IPriceDiscJournalPostingService.cs` | IPriceDiscJournalPostingService | Added |
| `Services/IPriceDiscTableService.cs` | IPriceDiscTableService | Reviewed; retained |
| `Services/PriceDiscAdmNameService.cs` | PriceDiscAdmNameService | Reviewed; retained |
| `Services/PriceDiscAdmTableService.cs` | PriceDiscAdmTableService | Reviewed; retained |
| `Services/PriceDiscAdmTransService.cs` | PriceDiscAdmTransService | Reviewed; retained |
| `Services/PriceDiscGroupService.cs` | PriceDiscGroupService | Reviewed; retained |
| `Services/PriceDiscJournalPostingService.cs` | PriceDiscJournalPostStatus, PriceDiscJournalPostResult, PriceDiscJournalPostingService | Refactored |
| `Services/PriceDiscTableService.cs` | PriceDiscTableService | Reviewed; retained |

### SalesOrder

| File | Declarations | Result |
|---|---|---|
| `Configuration/SalesLineConfiguration.cs` | SalesLineConfiguration | Reviewed; retained |
| `Configuration/SalesOrderListMapping.cs` | SalesOrderListMappingSource, SalesOrderListMapping | Added |
| `Configuration/SalesOrderQuickCreateMapping.cs` | SalesOrderQuickCreateMappingSource, SalesOrderQuickCreateMapping | Added |
| `Configuration/SalesTableConfiguration.cs` | SalesTableConfiguration | Reviewed; retained |
| `Controllers/SalesOrderCopyController.cs` | SalesOrderCopyController, CopyRequest | Refactored |
| `Controllers/SalesTableController.cs` | SalesTableController, AddSalesLineInput, CreateSalesDeliveryAddressInput, UpdateSalesHeaderInput | Refactored |
| `Dtos/SalesLineDto.cs` | SalesLineDto | Reviewed; retained |
| `Dtos/SalesLineInputDto.cs` | SalesLineInputDto | Reviewed; retained |
| `Dtos/SalesOrderListDto.cs` | SalesOrderListDto | Reviewed; retained |
| `Dtos/SalesOrderQuickCreateDto.cs` | SalesOrderQuickCreateDto | Reviewed; retained |
| `Dtos/SalesTableDto.cs` | SalesTableDto | Reviewed; retained |
| `Dtos/SalesTableDtoValidator.cs` | SalesTableDtoValidator | Reviewed; retained |
| `Dtos/UpdateSalesHeaderInputDto.cs` | UpdateSalesHeaderInputDto | Reviewed; retained |
| `Entities/SalesLine.cs` | SalesLine | Reviewed; retained |
| `Entities/SalesTable.cs` | SalesTable | Reviewed; retained |
| `Interfaces/IInventDimensionResolver.cs` | IInventDimensionResolver | Reviewed; retained |
| `Interfaces/ISalesInventoryDemandService.cs` | ISalesInventoryDemandService | Reviewed; retained |
| `Interfaces/ISalesInventoryNumberService.cs` | ISalesInventoryNumberService | Reviewed; retained |
| `Interfaces/ISalesOrderCancellationService.cs` | ISalesOrderCancellationService | Added |
| `Interfaces/ISalesOrderCopyService.cs` | ISalesOrderCopyService, SalesOrderCopyLookupResult | Added |
| `Interfaces/ISalesOrderDeliveryAddressService.cs` | ISalesOrderDeliveryAddressService | Added |
| `Interfaces/ISalesOrderDiscountService.cs` | ISalesOrderDiscountService | Added |
| `Interfaces/ISalesOrderHeaderService.cs` | ISalesOrderHeaderService | Added |
| `Interfaces/ISalesOrderLineService.cs` | ISalesOrderLineService | Added |
| `Interfaces/ISalesOrderLookupService.cs` | ISalesOrderLookupService | Added |
| `Interfaces/ISalesOrderQuickCreateService.cs` | ISalesOrderQuickCreateService | Added |
| `Interfaces/ISalesOrderReadService.cs` | ISalesOrderReadService | Added |
| `Services/InventDimensionResolver.cs` | InventDimensionResolver | Refactored |
| `Services/SalesInventoryDemandService.cs` | SalesInventoryDemandService | Refactored |
| `Services/SalesInventoryNumberService.cs` | SalesInventoryNumberService | Reviewed; retained |
| `Services/SalesOrderCancellationService.cs` | SalesOrderCancelStatus, SalesOrderCancellationService | Refactored |
| `Services/SalesOrderCopyService.cs` | SalesOrderCopyRequest, SalesOrderCopyStatus, SalesOrderCopyResult, SalesOrderCopyService, CopySourceLine | Refactored |
| `Services/SalesOrderDeliveryAddressService.cs` | SalesDeliveryAddressStatus, SalesDeliveryAddressResult, SalesOrderDeliveryAddressService | Refactored |
| `Services/SalesOrderDiscountService.cs` | SalesDiscountRecalculationStatus, SalesDiscountRecalculationResult, SalesOrderDiscountService | Refactored |
| `Services/SalesOrderHeaderService.cs` | SalesHeaderUpdateStatus, SalesHeaderUpdateResult, SalesOrderHeaderService | Refactored |
| `Services/SalesOrderLinePresenter.cs` | SalesOrderLinePresenter | Reviewed; retained |
| `Services/SalesOrderLineService.cs` | SalesLineWriteStatus, SalesLineWriteResult, SalesOrderLineService | Refactored |
| `Services/SalesOrderLookupService.cs` | SalesOrderLookupService | Added |
| `Services/SalesOrderQuickCreateService.cs` | SalesOrderQuickCreateResult, SalesOrderQuickCreateService | Refactored |
| `Services/SalesOrderReadService.cs` | SalesOrderReadService | Added |
| `Services/SalesOrderValidationService.cs` | SalesOrderValidationService | Refactored |
| `Services/SalesUnitConversionService.cs` | SalesUnitConversionService | Refactored |
| `Validation/SalesOrderCopyValidator.cs` | SalesOrderCopyValidator | Added |

### SalesPool

| File | Declarations | Result |
|---|---|---|
| `Configuration/SalesPoolConfiguration.cs` | SalesPoolConfiguration | Reviewed; retained |
| `Controllers/SalesPoolController.cs` | SalesPoolController | Reviewed; retained |
| `Dtos/SalesPoolDto.cs` | SalesPoolDto | Reviewed; retained |
| `Entities/SalesPool.cs` | SalesPool | Reviewed; retained |
| `Services/ISalesPoolService.cs` | ISalesPoolService | Reviewed; retained |
| `Services/SalesPoolService.cs` | SalesPoolService | Reviewed; retained |

### SalesQuotation

| File | Declarations | Result |
|---|---|---|
| `Configuration/CustQuotationJourConfiguration.cs` | CustQuotationJourConfiguration | Reviewed; retained |
| `Configuration/CustQuotationTransConfiguration.cs` | CustQuotationTransConfiguration | Reviewed; retained |
| `Configuration/SalesQuotationConfiguration.cs` | SalesQuotationConfiguration | Reviewed; retained |
| `Configuration/SalesQuotationLineConfiguration.cs` | SalesQuotationLineConfiguration | Reviewed; retained |
| `Dtos/CustQuotationJourDto.cs` | CustQuotationJourDto | Reviewed; retained |
| `Dtos/SalesQuotationDto.cs` | SalesQuotationTableDto | Reviewed; retained |
| `Dtos/SalesQuotationDtoValidator.cs` | SalesQuotationDtoValidator | Reviewed; retained |
| `Entities/CustQuotationJour.cs` | CustQuotationJour | Reviewed; retained |
| `Entities/CustQuotationTrans.cs` | CustQuotationTrans | Reviewed; retained |
| `Entities/SalesQuotationLine.cs` | SalesQuotationLine | Reviewed; retained |
| `Entities/SalesQuotationTable.cs` | SalesQuotationTable | Reviewed; retained |

### Settlement

| File | Declarations | Result |
|---|---|---|
| `Configuration/SpecTransConfiguration.cs` | SpecTransConfiguration | Reviewed; retained |
| `Dtos/CustSettlementDto.cs` | CustSettlementDto | Reviewed; retained |
| `Dtos/CustSettlementDtoValidator.cs` | CustSettlementDtoValidator | Reviewed; retained |
| `Dtos/SpecTransDto.cs` | SpecTransDto | Reviewed; retained |
| `Entities/CustSettlement.cs` | CustSettlement | Reviewed; retained |
| `Entities/SpecTrans.cs` | SpecTrans | Reviewed; retained |
