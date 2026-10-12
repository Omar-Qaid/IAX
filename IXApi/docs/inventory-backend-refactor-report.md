# Inventory backend refactor report

## Scope and architecture

Reviewed all 83 current C# files across 18 Inventory folders and their nested folders. Before editing, reviewed Shared BaseController, IBaseService, BaseService, IUnitOfWork and its implementation, GenericRepository, APIResponse, DTO/entity conventions, Mapster configuration, BaseValidator, DomainPermission, company context, exception handling, logging, DI, filtering and pagination.

InventSite and InventLocation retain their thin custom controllers for existing HTTP responses. Their interfaces extend IBaseService and their services extend BaseService and use IUnitOfWork. InventTrans is a read-only service using IUnitOfWork and Mapster. Entity-only folders remain persistence-only.

## Changes

- InventLocationService: lookup results now use Mapster projections.
- InventLocationMapping: added explicit site and warehouse lookup mappings.
- InventLocationLookupDto.cs: created three typed DTOs with the existing JSON field names.
- InventoryArchitectureTests.cs: added SQLite-backed assertions for lookup JSON shape and company filtering.
- No classes removed or moved. Existing controller contracts, validation rules, DI and EF mappings were retained.

## Verification

- Finance module build passed (0 warnings, 0 errors).
- Six focused InventoryArchitectureTests, InventSiteTests and SalesInventoryDemandTests passed (0 failures) using the newly compiled test binary.
- Baseline comparison found no entity or EF mapping changes and no controller route, verb or permission changes.
- Live SQL Server and authenticated API verification were not performed.

## Folder results

| Folder | Result |
|---|---|
| InventBatch | Existing entities, DTOs and related classes reviewed; no applicable code change. |
| InventClosing | Existing entities, DTOs and related classes reviewed; no applicable code change. |
| InventCountGroup | Existing entities, DTOs and related classes reviewed; no applicable code change. |
| InventDim | Existing entities, DTOs and related classes reviewed; no applicable code change. |
| InventItemGroupItem | Existing entities, DTOs and related classes reviewed; no applicable code change. |
| InventItemLocation | Existing entities, DTOs and related classes reviewed; no applicable code change. |
| InventJournal | Existing entities, DTOs and related classes reviewed; no applicable code change. |
| InventLocation | Mapster lookup refactor completed and verified. |
| InventModelGroup | Existing entities, DTOs and related classes reviewed; no applicable code change. |
| InventPackaging | Existing entities, DTOs and related classes reviewed; no applicable code change. |
| InventSerial | Existing entities, DTOs and related classes reviewed; no applicable code change. |
| InventSettlement | Existing entities, DTOs and related classes reviewed; no applicable code change. |
| InventSite | Active services, interfaces, controllers, DTOs and validators reviewed; existing architecture retained. |
| InventSum | Existing entities, DTOs and related classes reviewed; no applicable code change. |
| InventTable | Active services, interfaces, controllers, DTOs and validators reviewed; existing architecture retained. |
| ItemGroup | Existing entities, DTOs and related classes reviewed; no applicable code change. |
| PostingProfiles | Existing entities, DTOs and related classes reviewed; no applicable code change. |
| UOM | Existing entities, DTOs and related classes reviewed; no applicable code change. |

## Every class and file reviewed

### InventBatch

| File | Declarations | Result |
|---|---|---|
| `InventBatch/Dtos/InventBatchDto.cs` | `InventBatchDto` | Reviewed; retained |
| `InventBatch/Entities/InventBatch.cs` | `InventBatch` | Reviewed; retained |

### InventClosing

| File | Declarations | Result |
|---|---|---|
| `InventClosing/Entities/InventClosing.cs` | `InventClosing` | Reviewed; retained |

### InventCountGroup

| File | Declarations | Result |
|---|---|---|
| `InventCountGroup/Dtos/InventCountGroupDto.cs` | `InventCountGroupDto` | Reviewed; retained |
| `InventCountGroup/Entities/InventCountGroup.cs` | `InventCountGroup` | Reviewed; retained |

### InventDim

| File | Declarations | Result |
|---|---|---|
| `InventDim/Dtos/InventDimDto.cs` | `InventDimDto` | Reviewed; retained |
| `InventDim/Entities/InventDim.cs` | `InventDim` | Reviewed; retained |

### InventItemGroupItem

| File | Declarations | Result |
|---|---|---|
| `InventItemGroupItem/Dtos/InventItemGroupItemDto.cs` | `InventItemGroupItemDto` | Reviewed; retained |
| `InventItemGroupItem/Entities/InventItemGroupItem.cs` | `InventItemGroupItem` | Reviewed; retained |

### InventItemLocation

| File | Declarations | Result |
|---|---|---|
| `InventItemLocation/Dtos/InventItemLocationDto.cs` | `InventItemLocationDto` | Reviewed; retained |
| `InventItemLocation/Entities/InventItemLocation.cs` | `InventItemLocation` | Reviewed; retained |

### InventJournal

| File | Declarations | Result |
|---|---|---|
| `InventJournal/Dtos/InventCountJourDto.cs` | `InventCountJourDto` | Reviewed; retained |
| `InventJournal/Dtos/InventJournalNameDto.cs` | `InventJournalNameDto` | Reviewed; retained |
| `InventJournal/Dtos/InventJournalTableDto.cs` | `InventJournalTableDto`, `InventJournalTransDto` | Reviewed; retained |
| `InventJournal/Entities/InventCountJour.cs` | `InventCountJour` | Reviewed; retained |
| `InventJournal/Entities/InventJournalName.cs` | `InventJournalName` | Reviewed; retained |
| `InventJournal/Entities/InventJournalTable.cs` | `InventJournalTable` | Reviewed; retained |
| `InventJournal/Entities/InventJournalTrans.cs` | `InventJournalTrans` | Reviewed; retained |

### InventLocation

| File | Declarations | Result |
|---|---|---|
| `InventLocation/Configuration/InventLocationMapping.cs` | `InventLocationMapping` | Modified |
| `InventLocation/Controllers/InventLocationController.cs` | `InventLocationController`, `LocationInput` | Reviewed; retained |
| `InventLocation/Dtos/InventLocationDto.cs` | `InventLocationDto` | Reviewed; retained |
| `InventLocation/Dtos/InventLocationLookupDto.cs` | `InventSiteLookupDto`, `InventWarehouseLookupDto`, `InventLocationLookupDto` | Created |
| `InventLocation/Dtos/InventLocationResponseDto.cs` | `InventLocationResponseDto` | Reviewed; retained |
| `InventLocation/Dtos/LocationInputDto.cs` | `LocationInputDto` | Reviewed; retained |
| `InventLocation/Dtos/LocationOperationResult.cs` | `LocationOperationStatus`, `LocationOperationResult` | Reviewed; retained |
| `InventLocation/Entities/InventLocation.cs` | `InventLocation` | Reviewed; retained |
| `InventLocation/Interfaces/IInventLocationService.cs` | `IInventLocationService` | Reviewed; retained |
| `InventLocation/Services/InventLocationService.cs` | `InventLocationService` | Modified |
| `InventLocation/Validation/InventLocationUpdateValidator.cs` | `InventLocationUpdateValidation`, `InventLocationUpdateValidator` | Reviewed; retained |

### InventModelGroup

| File | Declarations | Result |
|---|---|---|
| `InventModelGroup/Dtos/InventModelGroupDto.cs` | `InventModelGroupDto` | Reviewed; retained |
| `InventModelGroup/Dtos/InventModelGroupItemDto.cs` | `InventModelGroupItemDto` | Reviewed; retained |
| `InventModelGroup/Entities/InventModelGroup.cs` | `InventModelGroup` | Reviewed; retained |
| `InventModelGroup/Entities/InventModelGroupItem.cs` | `InventModelGroupItem` | Reviewed; retained |

### InventPackaging

| File | Declarations | Result |
|---|---|---|
| `InventPackaging/Dtos/InventPackagingDto.cs` | `InventPackagingGroupDto`, `InventPackagingUnitDto` | Reviewed; retained |
| `InventPackaging/Entities/InventPackagingGroup.cs` | `InventPackagingGroup` | Reviewed; retained |
| `InventPackaging/Entities/InventPackagingUnit.cs` | `InventPackagingUnit` | Reviewed; retained |

### InventSerial

| File | Declarations | Result |
|---|---|---|
| `InventSerial/Dtos/InventSerialDto.cs` | `InventSerialDto` | Reviewed; retained |
| `InventSerial/Entities/InventSerial.cs` | `InventSerial` | Reviewed; retained |

### InventSettlement

| File | Declarations | Result |
|---|---|---|
| `InventSettlement/Dtos/InventSettlementDto.cs` | `InventSettlementDto` | Reviewed; retained |
| `InventSettlement/Entities/InventSettlement.cs` | `InventSettlement` | Reviewed; retained |

### InventSite

| File | Declarations | Result |
|---|---|---|
| `InventSite/Configuration/InventSiteMapping.cs` | `InventSiteMappingSource`, `InventSiteMapping` | Reviewed; retained |
| `InventSite/Controllers/InventSiteController.cs` | `InventSiteController`, `SiteInput` | Reviewed; retained |
| `InventSite/Dtos/InventSiteDeleteResult.cs` | `InventSiteDeleteResult` | Reviewed; retained |
| `InventSite/Dtos/InventSiteDto.cs` | `InventSiteDto`, `InventSiteWarehouseDto` | Reviewed; retained |
| `InventSite/Dtos/SiteInputDto.cs` | `SiteInputDto` | Reviewed; retained |
| `InventSite/Entities/InventSite.cs` | `InventSite` | Reviewed; retained |
| `InventSite/Interfaces/IInventSiteService.cs` | `IInventSiteService` | Reviewed; retained |
| `InventSite/Services/InventSiteService.cs` | `InventSiteService` | Reviewed; retained |
| `InventSite/Validation/InventSiteUpdateValidator.cs` | `InventSiteUpdateValidation`, `InventSiteUpdateValidator` | Reviewed; retained |

### InventSum

| File | Declarations | Result |
|---|---|---|
| `InventSum/Dtos/InventSumDto.cs` | `InventSumDto` | Reviewed; retained |
| `InventSum/Entities/InventSum.cs` | `InventSum` | Reviewed; retained |

### InventTable

| File | Declarations | Result |
|---|---|---|
| `InventTable/Configuration/InventTransConfiguration.cs` | `InventTransConfiguration` | Reviewed; retained |
| `InventTable/Configuration/InventTransMapping.cs` | `InventTransMapping` | Reviewed; retained |
| `InventTable/Configuration/InventTransOriginSalesLineConfiguration.cs` | `InventTransOriginSalesLineConfiguration` | Reviewed; retained |
| `InventTable/Controllers/InventTransController.cs` | `InventTransController` | Reviewed; retained |
| `InventTable/Dtos/InventItemBarcodeDto.cs` | `InventItemBarcodeDto` | Reviewed; retained |
| `InventTable/Dtos/InventTableDto.cs` | `InventTableDto` | Reviewed; retained |
| `InventTable/Dtos/InventTableModuleDto.cs` | `InventTableModuleDto` | Reviewed; retained |
| `InventTable/Dtos/InventTransDto.cs` | `InventTransDto` | Reviewed; retained |
| `InventTable/Dtos/InventTransListDto.cs` | `InventTransListDto` | Reviewed; retained |
| `InventTable/Dtos/InventTransMappingSource.cs` | `InventTransMappingSource` | Reviewed; retained |
| `InventTable/Entities/EcoResProduct.cs` | `EcoResProduct` | Reviewed; retained |
| `InventTable/Entities/InventItemBarcode.cs` | `InventItemBarcode` | Reviewed; retained |
| `InventTable/Entities/InventItemPrice.cs` | `InventItemPrice` | Reviewed; retained |
| `InventTable/Entities/InventTable.cs` | `InventTable` | Reviewed; retained |
| `InventTable/Entities/InventTableModule.cs` | `InventTableModule` | Reviewed; retained |
| `InventTable/Entities/InventTrans.cs` | `InventTrans` | Reviewed; retained |
| `InventTable/Entities/InventTransOrigin.cs` | `InventTransOrigin` | Reviewed; retained |
| `InventTable/Entities/InventTransOriginSalesLine.cs` | `InventTransOriginSalesLine` | Reviewed; retained |
| `InventTable/Interfaces/IInventTransService.cs` | `IInventTransService` | Reviewed; retained |
| `InventTable/Services/InventTransService.cs` | `InventTransService` | Reviewed; retained |
| `InventTable/Validation/InventTableDtoValidator.cs` | `InventTableDtoValidator` | Reviewed; retained |
| `InventTable/Validation/InventTransDtoValidator.cs` | `InventTransDtoValidator` | Reviewed; retained |

### ItemGroup

| File | Declarations | Result |
|---|---|---|
| `ItemGroup/Dtos/InventItemGroupDto.cs` | `InventItemGroupDto` | Reviewed; retained |
| `ItemGroup/Entities/InventItemGroup.cs` | `InventItemGroup` | Reviewed; retained |
| `ItemGroup/Validation/InventItemGroupDtoValidator.cs` | `InventItemGroupDtoValidator` | Reviewed; retained |

### PostingProfiles

| File | Declarations | Result |
|---|---|---|
| `PostingProfiles/Entities/InventPosting.cs` | `InventPosting` | Reviewed; retained |

### UOM

| File | Declarations | Result |
|---|---|---|
| `UOM/Dtos/UnitOfMeasureDto.cs` | `UnitOfMeasureDto` | Reviewed; retained |
| `UOM/Entities/UnitOfMeasure.cs` | `UnitOfMeasure` | Reviewed; retained |
| `UOM/Entities/UnitOfMeasureBaseUnit.cs` | `UnitOfMeasureBaseUnit` | Reviewed; retained |
| `UOM/Entities/UnitOfMeasureConversion.cs` | `UnitOfMeasureConversion` | Reviewed; retained |
| `UOM/Entities/UnitOfMeasureConversionCache.cs` | `UnitOfMeasureConversionCache` | Reviewed; retained |
| `UOM/Entities/UnitOfMeasureInternalCode.cs` | `UnitOfMeasureInternalCode` | Reviewed; retained |
