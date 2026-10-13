# Inventory backend architecture review

Scope: `IXApi/src/Modules/Finance/Inventory`. The attachment uses `GeneralLedger` in copied examples; Inventory is the requested target.

## Shared architecture and baseline

Checked `BaseController`, `IBaseService`, `BaseService`, `IUnitOfWork`, repository, API response and DTO contracts, Mapster discovery, validation base, authorization and company context, Finance dependency injection, and paging/filtering. Baseline API build passed with 0 errors.

## Folder-by-folder review

All 18 existing feature folders were checked in alphabetical order. Entity/DTO-only folders were retained without inventing APIs. Three feature folders expose implemented controllers and services.

| Folder | C# files | Result |
| --- | ---: | --- |
| InventBatch | 2 | Reviewed; retained existing contracts and persistence behavior. |
| InventClosing | 1 | Reviewed; retained existing contracts and persistence behavior. |
| InventCountGroup | 2 | Reviewed; retained existing contracts and persistence behavior. |
| InventDim | 2 | Reviewed; retained existing contracts and persistence behavior. |
| InventItemGroupItem | 2 | Reviewed; retained existing contracts and persistence behavior. |
| InventItemLocation | 2 | Reviewed; retained existing contracts and persistence behavior. |
| InventJournal | 7 | Reviewed; retained existing contracts and persistence behavior. |
| InventLocation | 11 | Reviewed; retained existing contracts and persistence behavior. |
| InventModelGroup | 4 | Reviewed; retained existing contracts and persistence behavior. |
| InventPackaging | 3 | Reviewed; retained existing contracts and persistence behavior. |
| InventSerial | 2 | Reviewed; retained existing contracts and persistence behavior. |
| InventSettlement | 2 | Reviewed; retained existing contracts and persistence behavior. |
| InventSite | 9 | Reviewed; retained existing contracts and persistence behavior. |
| InventSum | 2 | Reviewed; retained existing contracts and persistence behavior. |
| InventTable | 22 | Reviewed; retained existing contracts and persistence behavior. |
| ItemGroup | 3 | Reviewed; retained existing contracts and persistence behavior. |
| PostingProfiles | 1 | Reviewed; retained existing contracts and persistence behavior. |
| UOM | 6 | Reviewed; retained existing contracts and persistence behavior. |

## Complete source inventory

Every existing C# file below was reviewed for declarations and its architecture role. Status: retained.

### InventBatch

- `InventBatch/Dtos/InventBatchDto.cs` — InventBatchDto.
- `InventBatch/Entities/InventBatch.cs` — InventBatch.

### InventClosing

- `InventClosing/Entities/InventClosing.cs` — InventClosing.

### InventCountGroup

- `InventCountGroup/Dtos/InventCountGroupDto.cs` — InventCountGroupDto.
- `InventCountGroup/Entities/InventCountGroup.cs` — InventCountGroup.

### InventDim

- `InventDim/Dtos/InventDimDto.cs` — InventDimDto.
- `InventDim/Entities/InventDim.cs` — InventDim.

### InventItemGroupItem

- `InventItemGroupItem/Dtos/InventItemGroupItemDto.cs` — InventItemGroupItemDto.
- `InventItemGroupItem/Entities/InventItemGroupItem.cs` — InventItemGroupItem.

### InventItemLocation

- `InventItemLocation/Dtos/InventItemLocationDto.cs` — InventItemLocationDto.
- `InventItemLocation/Entities/InventItemLocation.cs` — InventItemLocation.

### InventJournal

- `InventJournal/Dtos/InventCountJourDto.cs` — InventCountJourDto.
- `InventJournal/Dtos/InventJournalNameDto.cs` — InventJournalNameDto.
- `InventJournal/Dtos/InventJournalTableDto.cs` — InventJournalTableDto, InventJournalTransDto.
- `InventJournal/Entities/InventCountJour.cs` — InventCountJour.
- `InventJournal/Entities/InventJournalName.cs` — InventJournalName.
- `InventJournal/Entities/InventJournalTable.cs` — InventJournalTable.
- `InventJournal/Entities/InventJournalTrans.cs` — InventJournalTrans.

### InventLocation

- `InventLocation/Configuration/InventLocationMapping.cs` — InventLocationMapping.
- `InventLocation/Controllers/InventLocationController.cs` — InventLocationController, LocationInput.
- `InventLocation/Dtos/InventLocationDto.cs` — InventLocationDto.
- `InventLocation/Dtos/InventLocationLookupDto.cs` — InventSiteLookupDto, InventWarehouseLookupDto, InventLocationLookupDto.
- `InventLocation/Dtos/InventLocationResponseDto.cs` — InventLocationResponseDto.
- `InventLocation/Dtos/LocationInputDto.cs` — LocationInputDto.
- `InventLocation/Dtos/LocationOperationResult.cs` — LocationOperationStatus, LocationOperationResult.
- `InventLocation/Entities/InventLocation.cs` — InventLocation.
- `InventLocation/Interfaces/IInventLocationService.cs` — IInventLocationService.
- `InventLocation/Services/InventLocationService.cs` — InventLocationService.
- `InventLocation/Validation/InventLocationUpdateValidator.cs` — InventLocationUpdateValidation, InventLocationUpdateValidator.

### InventModelGroup

- `InventModelGroup/Dtos/InventModelGroupDto.cs` — InventModelGroupDto.
- `InventModelGroup/Dtos/InventModelGroupItemDto.cs` — InventModelGroupItemDto.
- `InventModelGroup/Entities/InventModelGroup.cs` — InventModelGroup.
- `InventModelGroup/Entities/InventModelGroupItem.cs` — InventModelGroupItem.

### InventPackaging

- `InventPackaging/Dtos/InventPackagingDto.cs` — InventPackagingGroupDto, InventPackagingUnitDto.
- `InventPackaging/Entities/InventPackagingGroup.cs` — InventPackagingGroup.
- `InventPackaging/Entities/InventPackagingUnit.cs` — InventPackagingUnit.

### InventSerial

- `InventSerial/Dtos/InventSerialDto.cs` — InventSerialDto.
- `InventSerial/Entities/InventSerial.cs` — InventSerial.

### InventSettlement

- `InventSettlement/Dtos/InventSettlementDto.cs` — InventSettlementDto.
- `InventSettlement/Entities/InventSettlement.cs` — InventSettlement.

### InventSite

- `InventSite/Configuration/InventSiteMapping.cs` — InventSiteMappingSource, InventSiteMapping.
- `InventSite/Controllers/InventSiteController.cs` — InventSiteController, SiteInput.
- `InventSite/Dtos/InventSiteDeleteResult.cs` — InventSiteDeleteResult.
- `InventSite/Dtos/InventSiteDto.cs` — InventSiteDto, InventSiteWarehouseDto.
- `InventSite/Dtos/SiteInputDto.cs` — SiteInputDto.
- `InventSite/Entities/InventSite.cs` — InventSite.
- `InventSite/Interfaces/IInventSiteService.cs` — IInventSiteService.
- `InventSite/Services/InventSiteService.cs` — InventSiteService.
- `InventSite/Validation/InventSiteUpdateValidator.cs` — InventSiteUpdateValidation, InventSiteUpdateValidator.

### InventSum

- `InventSum/Dtos/InventSumDto.cs` — InventSumDto.
- `InventSum/Entities/InventSum.cs` — InventSum.

### InventTable

- `InventTable/Configuration/InventTransConfiguration.cs` — InventTransConfiguration.
- `InventTable/Configuration/InventTransMapping.cs` — InventTransMapping.
- `InventTable/Configuration/InventTransOriginSalesLineConfiguration.cs` — InventTransOriginSalesLineConfiguration.
- `InventTable/Controllers/InventTransController.cs` — InventTransController.
- `InventTable/Dtos/InventItemBarcodeDto.cs` — InventItemBarcodeDto.
- `InventTable/Dtos/InventTableDto.cs` — InventTableDto.
- `InventTable/Dtos/InventTableModuleDto.cs` — InventTableModuleDto.
- `InventTable/Dtos/InventTransDto.cs` — InventTransDto.
- `InventTable/Dtos/InventTransListDto.cs` — InventTransListDto.
- `InventTable/Dtos/InventTransMappingSource.cs` — InventTransMappingSource.
- `InventTable/Entities/EcoResProduct.cs` — EcoResProduct.
- `InventTable/Entities/InventItemBarcode.cs` — InventItemBarcode.
- `InventTable/Entities/InventItemPrice.cs` — InventItemPrice.
- `InventTable/Entities/InventTable.cs` — InventTable.
- `InventTable/Entities/InventTableModule.cs` — InventTableModule.
- `InventTable/Entities/InventTrans.cs` — InventTrans.
- `InventTable/Entities/InventTransOrigin.cs` — InventTransOrigin.
- `InventTable/Entities/InventTransOriginSalesLine.cs` — InventTransOriginSalesLine.
- `InventTable/Interfaces/IInventTransService.cs` — IInventTransService.
- `InventTable/Services/InventTransService.cs` — InventTransService.
- `InventTable/Validation/InventTableDtoValidator.cs` — InventTableDtoValidator.
- `InventTable/Validation/InventTransDtoValidator.cs` — InventTransDtoValidator.

### ItemGroup

- `ItemGroup/Dtos/InventItemGroupDto.cs` — InventItemGroupDto.
- `ItemGroup/Entities/InventItemGroup.cs` — InventItemGroup.
- `ItemGroup/Validation/InventItemGroupDtoValidator.cs` — InventItemGroupDtoValidator.

### PostingProfiles

- `PostingProfiles/Entities/InventPosting.cs` — InventPosting.

### UOM

- `UOM/Dtos/UnitOfMeasureDto.cs` — UnitOfMeasureDto.
- `UOM/Entities/UnitOfMeasure.cs` — UnitOfMeasure.
- `UOM/Entities/UnitOfMeasureBaseUnit.cs` — UnitOfMeasureBaseUnit.
- `UOM/Entities/UnitOfMeasureConversion.cs` — UnitOfMeasureConversion.
- `UOM/Entities/UnitOfMeasureConversionCache.cs` — UnitOfMeasureConversionCache.
- `UOM/Entities/UnitOfMeasureInternalCode.cs` — UnitOfMeasureInternalCode.

## Architecture findings and changes

- No existing class was modified or removed; no class was created. This report is the only new artifact.
- InventSite and InventLocation expose custom operations through thin controllers and dedicated `IBaseService` interfaces; their services extend `BaseService` and use `IUnitOfWork`/repository. InventTrans uses a dedicated read service with `IUnitOfWork`.
- Controllers do not query or save through EF directly. Services own company scope, validation, calculations, persistence and transaction ordering.
- Existing Mapster `IRegister` classes cover InventSite, InventLocation and InventTrans nontrivial mappings. Existing Finance DI registers their services and update validators; no registration change was needed.
- `InventLocationService` constructs a lookup response container; it is not manual entity-to-DTO property mapping.
- The three existing DTO validators for InventItemGroup, InventTable and InventTrans contain no rules. They were retained to avoid inventing validation behavior.
- No entity, EF configuration, migration, table, column, key, index or relationship source changed.

## Verification and limits

- API build: 0 errors, 5 package advisory warnings in the baseline build.
- Full backend tests: 235 passed, 2 skipped, 0 failed. The first test invocation exceeded the tool timeout; a retry completed and passed.
- API route, verb, request/response and permission declarations are unchanged. No live HTTP request was run.
- Database/schema source is unchanged. No live database comparison was run.
