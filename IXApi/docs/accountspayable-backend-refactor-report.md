# AccountsPayable backend architecture review

Scope: `IXApi/src/Modules/Finance/AccountsPayable`. Reviewed all 12 existing C# files in folder order. The attachment's `GeneralLedger` paths are copied examples; AccountsPayable is the requested scope.

## Shared architecture

Inspected `BaseController`, `IBaseService`, `BaseService`, `IUnitOfWork`, generic repository, DTO base, Mapster discovery, and the Finance module registrations. The two existing controllers use `BaseController` and module interfaces. Both services extend `BaseService`, which uses `IUnitOfWork` and the generic repository. Finance registers both service interfaces and its assembly for Mapster scanning.

## Folder and class inventory

| Folder | Class | Result |
| --- | --- | --- |
| Configuration | `VendTableConfiguration` | Retained: table, keys, relationships and indexes reviewed. |
| Configuration | `VendGroupConfiguration` | Retained: table, keys, relationship and index reviewed. |
| Configuration | `VendorMapping` | Added Mapster vendor-group code mapping. |
| Vendor/Entities | `VendTable` | Retained: entity and relationship attributes reviewed. |
| Vendor/Entities | `VendGroup` | Retained: entity and relationship attributes reviewed. |
| Vendor | `VendorDto` | Retained: serialized contract reviewed. |
| Vendor | `VendorGroupDto` | Retained: serialized contract reviewed. |
| Vendor/Services | `IVendorService` | Retained: extends `IBaseService<VendTable>`. |
| Vendor/Services | `IVendorGroupService` | Retained: extends `IBaseService<VendGroup>`. |
| Vendor/Services | `VendorService` | Retained: `BaseService<VendTable>` and `IUnitOfWork`. |
| Vendor/Services | `VendorGroupService` | Retained: `BaseService<VendGroup>` and `IUnitOfWork`. |
| Vendor/Controllers | `VendorController` | Retained: thin generic CRUD controller with existing route and permission. |
| Vendor/Controllers | `VendorGroupController` | Retained: thin generic CRUD controller with existing route and permission. |

No Validation or Interfaces folder exists; interfaces are in Vendor/Services. No class was removed or modified. One mapping class and one test class were added. No DI changes were needed because Finance already scans its assembly for Mapster and registers both services.

## Mapping and remaining contract limits

`VendorGroupDto.VendGroup` and `VendGroup.VendGroupCode` use different names. The new Mapster configuration maps them in both directions and ignores the entity key on DTO-to-entity mapping. A regression test checks both directions and identity preservation.

`VendorDto.Name`, `NameAR`, `Phone`, and `Email` are not scalar properties of `VendTable`; party and contact data live in related tables. `VendorGroupDto.NameAR` and `ClearingAccount` also lack matching scalar entity properties. These fields remain in the API contract with their existing behavior; persisting or loading them requires a separate business behavior change with explicit related-record rules. `VendorDto.Blocked` is boolean while `VendTable.Blocked` has five enum states; the existing mapping behavior remains unchanged. These are outstanding domain/API limitations, not architecture bypasses.

## Verification

- Baseline API build: 0 errors, 0 warnings (incremental).
- Final API build: 0 errors, 170 warnings across the project, with no AccountsPayable errors.
- Test suite: 235 passed, 2 skipped, 0 failed; includes the new mapping regression test.
- Existing controller route, verb, permission, and response code was unchanged. The mapping fix affects the vendor-group code field as intended; no live HTTP request was run.
- No entity, EF configuration, migration, or schema file changed. No live database verification was run.
- Existing Administration and Communication working-tree changes were left untouched.
