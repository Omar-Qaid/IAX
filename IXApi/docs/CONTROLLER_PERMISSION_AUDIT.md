# Controller Permission Audit

Date: 2026-09-03

Status: Waves 1 through 4 are complete. The two read-only enum metadata controllers remain
authenticated shared-reference endpoints. Workflow request, notification, and chat record
authorization is complete. Generic bulk routes are disabled, and audit logs are read-only.

## Scope and rules

This checkpoint classifies every concrete controller that does not currently declare
`DomainPermissionAttribute` or another application-permission attribute. Authentication
inherited from `BaseController` is not considered resource authorization.

The target permission format is `Module.Resource.Action`, with `View`, `Create`, `Edit`,
and `Delete` inferred from the HTTP method. Custom operations must declare their explicit actions
when CRUD inference would be misleading.

## Permission contract findings

1. Frontend Finance permissions used legacy keys such as `customer.view`,
   `customerGroup.manage`, and `currency.manage`. These do not match the backend catalog,
   which seeds keys such as `AccountsReceivable.Customers.View` and
   `GeneralLedger.Currencies.Edit`. Wave 1 aligned the active frontend constants.
2. `IdentitySeeder.PermissionDefs` contained corrupted Inventory resources: `" "` and
   `" roups"`. Wave 1 replaced them with `Items` and `ItemGroups` and removes the corrupt
   persisted rows during idempotent seeding.
3. The catalog did not cover Settings, DataManagement, BackgroundJobs, Chat,
   Notifications, DataTypes, fiscal calendars, posting profiles, sales pools, markup, or tax.
   Wave 1 added these resources and the required custom action keys.
4. Workflow request transaction access cannot be represented safely by a controller-wide
   CRUD permission alone. Request ownership and assignment checks are also required.

## Administration and Communication

| Controller | Current protection | Classification | Proposed authorization |
|---|---|---|---|
| `SysSettingsController` | Authenticated only | Mixed global and self-service | Add `System.Settings` catalog. Global reads/updates require explicit `View`/`Edit`; personal settings remain authenticated self-service. |
| `SysDataManagementController` | Authenticated only | Privileged system operation | Add `System.DataManagement`; map discovery/template/export to `View`, import to explicit `Import` or `Create`, and export to explicit `Export` if separate grants are required. |
| `SysBackgroundJobController` | Authenticated only | Privileged system operation | Add `System.BackgroundJobs`; reads use `View`, configuration changes use `Create`/`Edit`/`Delete`, and run/cancel/retry operations use explicit actions. |
| `SysNotificationController` | Authenticated only | User-owned transaction data | Keep authentication, but enforce current-user recipient ownership in the service. Administrative send operations require a separate `System.Notifications` permission. |
| `SysNotificationPreferenceController` | Authenticated only | Self-service | Keep authentication and force all reads/writes to the current user; never accept another user ID from the request. |
| `SysChatController` | Authenticated only | Participant-owned transaction data | Keep authentication, then require room membership for history/read/send operations. Administrative moderation requires a separate permission. |

## Workflow

| Controller | Current protection | Classification | Proposed authorization |
|---|---|---|---|
| `WfDataTypeController` | Authenticated CRUD | Setup/master data | Add catalog entry and `DomainPermission("Workflow", "DataTypes")`. |
| `WfRequestMappingVariableController` | Authenticated CRUD | Process Builder child configuration | Use `Workflow.RequestControls` temporarily, or add `Workflow.RequestMappingVariables` if independently delegated. |
| `WfRequestControlsValidationController` | Authenticated CRUD | Process Builder child configuration | Use `Workflow.RequestControls`; mutations also require Process Builder edit authority if aggregate-level enforcement is introduced. |
| `WfRequestControlsOptionController` | Authenticated CRUD | Process Builder child configuration | Use `Workflow.RequestControls`; mutations also require Process Builder edit authority if aggregate-level enforcement is introduced. |
| `WfActivityMappingVariableController` | Authenticated CRUD | Process Builder child configuration | Use `Workflow.ActivityControls` temporarily, or add an independently delegated resource. |
| `WfActivityControlsValidationController` | Authenticated CRUD | Process Builder child configuration | Use `Workflow.ActivityControls`. |
| `WfActivityControlsOptionController` | Authenticated CRUD | Process Builder child configuration | Use `Workflow.ActivityControls`. |
| `WfDataManagementController` | Authenticated import/export | Privileged workflow administration | Use `Workflow.ProcessBuilder` with explicit `Import`/`Export`, or add `Workflow.DataManagement`. File limits and workbook validation are required separately. |
| `WfRequestController` | Authenticated CRUD and transaction actions | User/performer-owned transaction data | Split by action: form definition may use authenticated `View`; submit uses `Create`; mail/details require owner, participant, or privileged `Workflow.Requests.View`; inherited update/delete/range endpoints should be disabled unless explicitly required. |

## Accounts Payable and Accounts Receivable

| Controller | Existing catalog resource | Proposed authorization |
|---|---|---|
| `VendorController` | `AccountsPayable.Vendors` | Add matching `DomainPermission`. |
| `VendorGroupController` | `AccountsPayable.VendorGroups` | Add matching `DomainPermission`. |
| `CustomerController` | `AccountsReceivable.Customers` | Add matching `DomainPermission`. |
| `CustomerGroupController` | `AccountsReceivable.CustomerGroups` | Add matching `DomainPermission`. |
| `SalesPoolController` | Missing | Add `AccountsReceivable.SalesPools`, then add matching attribute. |
| `CustLedgerController` | Missing | Add `AccountsReceivable.PostingProfiles`, then add matching attribute. |
| `CustLedgerAccountsController` | Missing | Use the same `AccountsReceivable.PostingProfiles` aggregate permission. |
| `CustPaymModeController` | Missing | Add `AccountsReceivable.PaymentMethods`, then add matching attribute. |

## General Ledger and Finance foundation

| Controller | Existing catalog resource | Proposed authorization |
|---|---|---|
| `CurrencyController` | `GeneralLedger.Currencies` | Add matching `DomainPermission`. |
| `ExchangeRateController` | `GeneralLedger.ExchangeRates` | Add matching `DomainPermission`. |
| `ExchangeRateTypeController` | `GeneralLedger.ExchangeRateTypes` | Add matching `DomainPermission`. |
| `ExchangeRateCurrencyPairController` | `GeneralLedger.ExchangeRateCurrencyPairs` | Add matching permission; `BulkSave` maps to `Edit`, not inferred `Create`. |
| `FiscalCalendarController` | Missing | Add `GeneralLedger.FiscalCalendars`. |
| `FiscalCalendarYearController` | Missing | Use `GeneralLedger.FiscalCalendars` as aggregate authorization. |
| `FiscalCalendarPeriodController` | Missing | Use `GeneralLedger.FiscalCalendars` as aggregate authorization. |
| `LedgerFiscalCalendarPeriodController` | Missing | Use `GeneralLedger.FiscalCalendars` as aggregate authorization. |
| `MarkupTableController` | Missing | Add `Finance.MarkupCodes` or the confirmed owning business module. |
| `TaxTableController` | Missing | Add `Tax.TaxCodes`. |
| `TaxGroupController` | Missing | Add `Tax.TaxGroups`; line operations inherit aggregate authorization. |
| `TaxItemGroupController` | Missing | Add `Tax.ItemTaxGroups`; line operations inherit aggregate authorization. |
| `TaxAuthorityAddressController` | Missing | Add `Tax.TaxAuthorities`. |
| `TaxLedgerAccountGroupController` | Missing | Add `Tax.LedgerAccountGroups`. |
| `TaxPeriodHeadController` | Missing | Add `Tax.SettlementPeriods`. |
| `TaxExemptCodeController` | Missing | Add `Tax.ExemptCodes`. |

## Reference enum controllers

| Controller | Classification | Proposed authorization |
|---|---|---|
| `EnumsController` | Read-only technical metadata | Keep authenticated-only only if every returned enum is safe for all signed-in users; otherwise add a shared reference-data `View` permission. Ensure no inherited write endpoints exist. |
| `ErpEnumsController` | Read-only technical metadata | Same treatment as `EnumsController`; consolidate the duplicate endpoints if contracts allow. |

## Implementation waves

### Wave 1: repair the permission contract

1. Correct the corrupted Inventory catalog entries.
2. Add missing backend catalog resources and explicit custom actions.
3. Replace legacy frontend Finance keys with the exact backend keys.
4. Add a contract test comparing frontend-visible keys, seeded backend keys, and controller requirements.

### Wave 2: close privileged administration endpoints

Apply permissions to data management, background jobs, settings-global operations, workflow
data management, and all Finance master-data controllers. Add 401/403/allowed tests for read
and mutation actions.

### Wave 3: enforce record-level access

Implement workflow-request ownership/assignment checks, notification-recipient ownership,
notification-preference self-service, and chat-room membership. These checks belong in
application services/policies so alternate endpoints cannot bypass them.

Status: workflow-request ownership/assignment, notification-recipient self-service,
notification-preference self-service, privileged notification send, and chat-room membership
are enforced. Chat supports the shared `general` room and exact participants encoded in
`dm:{userId}:{userId}` room identifiers; arbitrary named rooms are rejected until a persisted
room-membership model is introduced.

### Wave 4: remove unsafe inherited surface

Disable inherited CRUD/range endpoints on transaction controllers unless each operation is an
intentional public contract. In particular, audit `WfRequestController` before attaching a
single controller-wide permission.

Status: complete. The generic `CreateRange`, `UpdateRange`, and `DeleteRange` methods are no
longer MVC actions because no application caller or explicitly approved contract was found.
`WfRequestController` also disables generic create and paged transaction routes, and
`SysAuditLogController` disables inherited create, update, and delete actions.

## Acceptance gates

- Every concrete controller is classified as public, authenticated self-service,
  record-authorized, or domain-permission protected.
- Every required permission exists in the seeded catalog with exact casing.
- Frontend route/action guards use the same exact keys.
- Non-admin users receive 403 without the required permission.
- Admin behavior remains unchanged.
- Record-level endpoints reject cross-user and cross-company access.
- Existing routes and response DTOs remain unchanged unless separately approved.
