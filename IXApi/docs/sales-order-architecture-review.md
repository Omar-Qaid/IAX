# Sales Order backend architecture review — 2026-10-13

Scope: `IXApi/src/Modules/Finance/AccountsReceivable/SalesOrder/`. The adjacent `Confirm` and Foundation `Markup` features were checked as API dependencies but are outside this refactor.

## Shared architecture checked

`BaseController<TEntity,TDto>` and `IBaseService<T>`/`BaseService<T>` provide generic entity CRUD. `IUnitOfWork` exposes repositories, `Context`, commit, and transaction operations. The existing Sales Order routes are workflow and multi-entity commands, so the dedicated interfaces and thin controllers retain their route and response contracts. Services use the existing unit of work. Mapster `IRegister` mappings support list and quick-create DTOs. The line response is a multi-source projection with inventory dimension and display fields, kept as a presenter in Configuration to preserve its serialized contract. FluentValidation validates copy commands; attributes and service validation cover the remaining inputs. `DomainPermission` protects controller methods, and `ICompanyExecutionContext` scopes data access. The API uses `APIResponse` and existing exception/logging middleware. Lookup paging follows existing request parameters. No new infrastructure was introduced.

## Class-by-class inventory

### Configuration

- `Configuration/SalesLineConfiguration.cs` — SalesLineConfiguration. Retained; reviewed.
- `Configuration/SalesOrderLinePresenter.cs` — SalesOrderLinePresenter. Relocated from Services; output unchanged.
- `Configuration/SalesOrderListMapping.cs` — SalesOrderListMappingSource, SalesOrderListMapping. Retained; reviewed.
- `Configuration/SalesOrderQuickCreateMapping.cs` — SalesOrderQuickCreateMappingSource, SalesOrderQuickCreateMapping. Retained; reviewed.
- `Configuration/SalesTableConfiguration.cs` — SalesTableConfiguration. Retained; reviewed.

### Entities

- `Entities/SalesLine.cs` — SalesLine. Retained; reviewed.
- `Entities/SalesTable.cs` — SalesTable. Retained; reviewed.

### Dtos

- `Dtos/SalesLineDto.cs` — SalesLineDto. Retained; reviewed.
- `Dtos/SalesLineInputDto.cs` — SalesLineInputDto. Retained; reviewed.
- `Dtos/SalesOrderListDto.cs` — SalesOrderListDto. Retained; reviewed.
- `Dtos/SalesOrderQuickCreateDto.cs` — SalesOrderQuickCreateDto. Retained; reviewed.
- `Dtos/SalesTableDto.cs` — SalesTableDto. Retained; reviewed.
- `Dtos/UpdateSalesHeaderInputDto.cs` — UpdateSalesHeaderInputDto. Retained; reviewed.

### Interfaces

- `Interfaces/IInventDimensionResolver.cs` — IInventDimensionResolver. Retained; reviewed.
- `Interfaces/ISalesInventoryDemandService.cs` — ISalesInventoryDemandService. Retained; reviewed.
- `Interfaces/ISalesInventoryNumberService.cs` — ISalesInventoryNumberService. Retained; reviewed.
- `Interfaces/ISalesOrderCancellationService.cs` — ISalesOrderCancellationService. Retained; reviewed.
- `Interfaces/ISalesOrderCopyService.cs` — ISalesOrderCopyService, SalesOrderCopyLookupResult. Retained; reviewed.
- `Interfaces/ISalesOrderDeliveryAddressService.cs` — ISalesOrderDeliveryAddressService. Retained; reviewed.
- `Interfaces/ISalesOrderDiscountService.cs` — ISalesOrderDiscountService. Retained; reviewed.
- `Interfaces/ISalesOrderHeaderService.cs` — ISalesOrderHeaderService. Retained; reviewed.
- `Interfaces/ISalesOrderLineService.cs` — ISalesOrderLineService. Retained; reviewed.
- `Interfaces/ISalesOrderLookupService.cs` — ISalesOrderLookupService. Retained; reviewed.
- `Interfaces/ISalesOrderQuickCreateService.cs` — ISalesOrderQuickCreateService. Retained; reviewed.
- `Interfaces/ISalesOrderReadService.cs` — ISalesOrderReadService. Retained; reviewed.

### Validation

- `Validation/SalesOrderCopyValidator.cs` — SalesOrderCopyValidator. Retained; reviewed.

### Services

- `Services/InventDimensionResolver.cs` — InventDimensionResolver. Retained; reviewed.
- `Services/SalesDeliveryAddressCleanup.cs` — SalesDeliveryAddressCleanup. Retained; reviewed.
- `Services/SalesInventoryDemandService.cs` — SalesInventoryDemandService. Retained; reviewed.
- `Services/SalesInventoryNumberService.cs` — SalesInventoryNumberService. Retained; reviewed.
- `Services/SalesOrderCancellationService.cs` — SalesOrderCancelStatus, SalesOrderCancellationService. Retained; reviewed.
- `Services/SalesOrderChargeRefresh.cs` — SalesOrderChargeRefresh. Retained; reviewed.
- `Services/SalesOrderCopyService.cs` — SalesOrderCopyRequest, SalesOrderCopyStatus, SalesOrderCopyResult, SalesOrderCopyService, CopySourceLine. Retained; reviewed.
- `Services/SalesOrderDeliveryAddressService.cs` — SalesDeliveryAddressStatus, SalesDeliveryAddressResult, SalesOrderDeliveryAddressService. Retained; reviewed.
- `Services/SalesOrderDiscountService.cs` — SalesDiscountRecalculationStatus, SalesDiscountRecalculationResult, SalesOrderDiscountService. Retained; reviewed.
- `Services/SalesOrderHeaderService.cs` — SalesHeaderUpdateStatus, SalesHeaderUpdateResult, SalesOrderHeaderService. Retained; reviewed.
- `Services/SalesOrderLineService.cs` — SalesLineWriteStatus, SalesLineWriteResult, SalesOrderLineService. Retained; reviewed.
- `Services/SalesOrderLookupService.cs` — SalesOrderLookupService. Retained; reviewed.
- `Services/SalesOrderQuickCreateService.cs` — SalesOrderQuickCreateResult, SalesOrderQuickCreateService. Retained; reviewed.
- `Services/SalesOrderReadService.cs` — SalesOrderReadService. Retained; reviewed.
- `Services/SalesOrderValidationService.cs` — SalesOrderValidationService. Retained; reviewed.
- `Services/SalesUnitConversionService.cs` — SalesUnitConversionService, yet. Retained; reviewed.

### Controllers

- `Controllers/SalesOrderCopyController.cs` — SalesOrderCopyController, CopyRequest. Retained; reviewed.
- `Controllers/SalesTableController.cs` — SalesTableController, AddSalesLineInput, CreateSalesDeliveryAddressInput, UpdateSalesHeaderInput. Retained; reviewed.

- Removed `Dtos/SalesTableDtoValidator.cs` (`SalesTableDtoValidator`): it had no rules, registration, or consumers.

## Architecture and compatibility

- Controllers remain thin HTTP adapters. `BaseController` is unsuitable because these endpoints are not generic CRUD and changing to it would change routes, verbs, response envelopes, or command transactions.
- Operation services retain dedicated interfaces. Stateful persistence flows use `IUnitOfWork`; pure helpers receive data from those services or the unit-of-work context. `SalesInventoryNumberService` delegates to the existing number-sequence service.
- Entity mappings, migrations, database tables, keys, indexes, and constraints were not changed in this refactor. The existing Mapster list and quick-create registrations remain in place.
- No dependency-injection registration or API permission changed. The unused validator was not registered.
- Existing API response payloads, company scoping, validation statuses, and transaction boundaries were preserved.

## Verification

- Focused Sales Order, inventory, and table-number tests: 14 passed after the folder changes.
- Full backend suite: 239 passed, 2 existing skips, 0 failed.
- Live API/database verification was not repeated for this architecture-only relocation.
