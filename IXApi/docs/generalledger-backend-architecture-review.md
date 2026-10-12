# GeneralLedger backend architecture review

Scope: `IXApi/src/Modules/Finance/GeneralLedger`. Reviewed all 48 existing C# files in the six actual feature folders.

## Shared architecture

Checked `BaseController`, `IBaseService`, `BaseService`, `IUnitOfWork`, generic repository, API response and DTO contracts, Mapster discovery, validation base, authorization, company context, pagination/filtering and Finance dependency injection. Baseline build passed with 0 errors.

## Folder-by-folder review

| Folder | Files | Result |
| --- | ---: | --- |
| Bank | 10 | Reviewed; retained existing entities, DTOs, configuration and any implemented API. |
| FiscalCalendar | 25 | Reviewed; retained existing entities, DTOs, configuration and any implemented API. |
| JournalEntry | 3 | Reviewed; retained existing entities, DTOs, configuration and any implemented API. |
| Ledger | 2 | Reviewed; retained existing entities, DTOs, configuration and any implemented API. |
| LedgerAccount | 3 | Reviewed; retained existing entities, DTOs, configuration and any implemented API. |
| LedgerJournalTable | 5 | Reviewed; retained existing entities, DTOs, configuration and any implemented API. |

Bank, JournalEntry, Ledger, LedgerAccount and LedgerJournalTable contain entities and/or DTOs but no controller or service. No API was invented for them. FiscalCalendar has four `BaseService` implementations and `IBaseService` interfaces, three implemented `BaseController` classes and one empty controller file.

## Complete file and declaration inventory

Status for all existing files: retained. `FiscalCalendarYearController.cs` is empty and declares no class.

### Bank

- `Bank/Dtos/BankAccountStatementDto.cs` — BankAccountStatementDto.
- `Bank/Dtos/BankAccountTableDto.cs` — BankAccountTableDto.
- `Bank/Dtos/BankAccountTransDto.cs` — BankAccountTransDto.
- `Bank/Dtos/BankGroupDto.cs` — BankGroupDto.
- `Bank/Dtos/BankTransTypeDto.cs` — BankTransTypeDto.
- `Bank/Entities/BankAccountStatement.cs` — BankAccountStatement.
- `Bank/Entities/BankAccountTable.cs` — BankAccountTable.
- `Bank/Entities/BankAccountTrans.cs` — BankAccountTrans.
- `Bank/Entities/BankGroup.cs` — BankGroup.
- `Bank/Entities/BankTransType.cs` — BankTransType.

### FiscalCalendar

- `FiscalCalendar/Configuration/FiscalCalendarConfiguration.cs` — FiscalCalendarConfiguration.
- `FiscalCalendar/Configuration/FiscalCalendarPeriodConfiguration.cs` — FiscalCalendarPeriodConfiguration.
- `FiscalCalendar/Configuration/FiscalCalendarYearConfiguration.cs` — FiscalCalendarYearConfiguration.
- `FiscalCalendar/Configuration/LedgerFiscalCalendarConfiguration.cs` — LedgerFiscalCalendarConfiguration, LedgerFiscalCalendarPeriodConfiguration, LedgerFiscalCalendarYearConfiguration.
- `FiscalCalendar/Controllers/FiscalCalendarController.cs` — FiscalCalendarController.
- `FiscalCalendar/Controllers/FiscalCalendarPeriodController.cs` — FiscalCalendarPeriodController.
- `FiscalCalendar/Controllers/FiscalCalendarYearController.cs` — no type declaration.
- `FiscalCalendar/Controllers/LedgerFiscalCalendarPeriodController.cs` — LedgerFiscalCalendarPeriodController.
- `FiscalCalendar/Dtos/FiscalCalendarDto.cs` — FiscalCalendarDto.
- `FiscalCalendar/Dtos/FiscalCalendarPeriodDto.cs` — FiscalCalendarPeriodDto.
- `FiscalCalendar/Dtos/FiscalCalendarYearDto.cs` — FiscalCalendarYearDto.
- `FiscalCalendar/Dtos/LedgerFiscalCalendarPeriodDto.cs` — LedgerFiscalCalendarPeriodDto.
- `FiscalCalendar/Entities/FiscalCalendar.cs` — FiscalCalendar.
- `FiscalCalendar/Entities/FiscalCalendarPeriod.cs` — FiscalCalendarPeriod.
- `FiscalCalendar/Entities/FiscalCalendarYear.cs` — FiscalCalendarYear.
- `FiscalCalendar/Entities/LedgerFiscalCalendarPeriod.cs` — LedgerFiscalCalendarPeriod.
- `FiscalCalendar/Entities/LedgerFiscalCalendarYear.cs` — LedgerFiscalCalendarYear.
- `FiscalCalendar/Interfaces/IFiscalCalendarPeriodService.cs` — IFiscalCalendarPeriodService.
- `FiscalCalendar/Interfaces/IFiscalCalendarService.cs` — IFiscalCalendarService.
- `FiscalCalendar/Interfaces/IFiscalCalendarYearService.cs` — IFiscalCalendarYearService.
- `FiscalCalendar/Interfaces/ILedgerFiscalCalendarPeriodService.cs` — ILedgerFiscalCalendarPeriodService.
- `FiscalCalendar/Services/FiscalCalendarPeriodService.cs` — FiscalCalendarPeriodService.
- `FiscalCalendar/Services/FiscalCalendarService.cs` — FiscalCalendarService.
- `FiscalCalendar/Services/FiscalCalendarYearService.cs` — FiscalCalendarYearService.
- `FiscalCalendar/Services/LedgerFiscalCalendarPeriodService.cs` — LedgerFiscalCalendarPeriodService.

### JournalEntry

- `JournalEntry/Entities/GeneralJournalAccountEntry.cs` — GeneralJournalAccountEntry.
- `JournalEntry/Entities/GeneralJournalEntry.cs` — GeneralJournalEntry.
- `JournalEntry/Entities/SubledgerVoucherGeneralJournalEntry.cs` — SubledgerVoucherGeneralJournalEntry.

### Ledger

- `Ledger/Dtos/LedgerDto.cs` — LedgerDto.
- `Ledger/Entities/Ledger.cs` — Ledger.

### LedgerAccount

- `LedgerAccount/Entities/LedgerChartOfAccounts.cs` — LedgerChartOfAccounts.
- `LedgerAccount/Entities/MainAccount.cs` — MainAccount.
- `LedgerAccount/Entities/MainAccountCategory.cs` — MainAccountCategory.

### LedgerJournalTable

- `LedgerJournalTable/Dtos/LedgerJournalTableDto.cs` — LedgerJournalTableDto.
- `LedgerJournalTable/Dtos/LedgerJournalTransDto.cs` — LedgerJournalTransDto.
- `LedgerJournalTable/Entities/LedgerJournalName.cs` — LedgerJournalName.
- `LedgerJournalTable/Entities/LedgerJournalTable.cs` — LedgerJournalTable.
- `LedgerJournalTable/Entities/LedgerJournalTrans.cs` — LedgerJournalTrans.

## Changes and architecture findings

- No existing class was modified or removed; no class was created. This report is the only new artifact.
- FiscalCalendar controller actions remain inherited from `BaseController`; route and `DomainPermission` attributes remain unchanged. No controller accesses EF or `IUnitOfWork` directly.
- Four FiscalCalendar services extend `BaseService<T>` and receive `IUnitOfWork`; their interfaces extend `IBaseService<T>`. Existing Finance DI registrations cover all four services.
- Existing DTOs use shared `EntityDto<long>` and BaseController Mapster adaptation. No new Mapster configuration or validator was needed for the reviewed architecture.
- EF configuration, entities, table names, columns, indexes and relationships were retained. No migration or schema source changed.

## Verification and remaining issue

- API build: 0 errors, 0 warnings in the incremental baseline build.
- Full backend tests: 235 passed, 2 skipped, 0 failed.
- API compatibility: source routes, verbs, request/response types and permissions are unchanged. No live HTTP request was run.
- Database/schema: no GeneralLedger or migration source changed. No live database comparison was run.
- `FiscalCalendar/Controllers/FiscalCalendarYearController.cs` is an empty placeholder. Its interface, service, DTO and entity exist, but it has no API. Creating an endpoint would be new behavior, so it remains a documented gap.
