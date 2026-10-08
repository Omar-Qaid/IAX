# Markup backend structure

- `Controllers`: HTTP routes, permissions, and response envelopes. `MarkupTableController` uses the shared `BaseController`; document-specific charge routes delegate to `IMarkupTransService`.
- `Interfaces`: contracts for charge setup CRUD, transaction orchestration, transaction commands, and charge calculations.
- `Services`: `MarkupTableService` uses the shared `BaseService`. `MarkupTransService` owns document lookup and validation sequencing. `MarkupTransCommandService` applies permitted edits and persists through `IUnitOfWork`. `MarkupChargeCalculator` retains the existing fixed, quantity, and percentage formulas.
- `Validation`: FluentValidation request rules, invoked by the transaction service to preserve the existing first-error response.
- `Configuration`: existing EF configuration and assembly-scanned Mapster registrations. Request mapping explicitly limits editable fields; response mapping preserves the existing DTO fields.
- `Dtos`: existing transaction/setup contracts plus the typed charge-code response and internal service result.
- `Entities`: existing persistence models.

Company filtering, legacy sales header table ID handling, routes, permissions, calculation rounding, and validation messages are retained. Automatic-charge entities remain configuration models; this refactor does not introduce an automatic-charge execution feature.

Regression coverage: `Tests/MarkupTests.cs` verifies protected-field mapping, normalization, validation ordering, and percentage boundaries. These tests do not establish live database or authenticated API acceptance.
