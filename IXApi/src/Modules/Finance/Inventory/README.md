# Inventory backend architecture

## Existing feature coverage

| Feature | Architecture |
| --- | --- |
| InventSite | Thin controller, IBaseService contract, BaseService with IUnitOfWork, Mapster configuration, update validator. |
| InventLocation | Thin controller, IBaseService contract, BaseService with IUnitOfWork, Mapster request/response configuration, update validator. Reference existence and deletion guards remain service business rules. |
| InventTable / InventTrans list | Thin controller, query service interface, IUnitOfWork data access, Mapster configuration, separate list DTO. The service owns company checks, joins and total cost calculation. |
| ItemGroup and InventTable DTO validators | Existing validators relocated from Dtos to Validation without changing rules or namespaces. |
| InventBatch, InventClosing, InventCountGroup, InventDim, InventItemGroupItem, InventItemLocation, InventJournal, InventModelGroup, InventPackaging, InventSerial, InventSettlement, InventSum, PostingProfiles, UOM and other InventTable persistence types | Existing entities, DTOs and configurations retained. No new CRUD endpoints are introduced for persistence-only features. |

Controllers retain their existing custom routes and response contracts. The shared BaseController introduces additional generic routes and different CRUD contracts, so these existing custom controllers continue to use ControllerBase. Warehouse and site services reuse BaseService/IBaseService. All Inventory service persistence now flows through IUnitOfWork.

Feature files are grouped under Controllers, Interfaces, Services, Dtos, Configuration, Validation and Entities where those responsibilities exist. Existing entity classes, EF mappings, permissions and company-filter behavior are preserved.

Warehouse lookup projections remain database-side projections of scalar fields. Mapster handles warehouse request/entity/response mapping and inventory transaction list mapping. Warehouse writes preserve normalization, immutable business codes, same-site reference checks and delete guards.

## Verification

`Tests/InventoryArchitectureTests.cs` covers warehouse CRUD guards and company filtering, plus transaction list costs, labels, ordering, optional relations and missing company handling. `Tests/InventSiteTests.cs` covers the existing site behavior. These SQLite tests do not replace authenticated API checks against the deployed database.
