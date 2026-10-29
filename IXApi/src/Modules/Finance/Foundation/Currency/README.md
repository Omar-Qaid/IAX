# Effective exchange-rate lookup

`IExchangeRateService.FindEffectiveAsync` selects a persisted direct currency pair and its latest applicable rate for the current company, rate type and date. It uses the existing entities and adds no database fields. The company scope follows the current IXApi model; it is not a claim that standard D365 exchange-rate setup is company-specific.

Inactive/deleted setup is excluded. Dates are compared as calendar dates, including the end date. A default end date is open-ended. Missing rates, duplicate direct pairs, duplicate latest start dates and nonpositive effective rates return explicit errors. No inverse-pair or triangulation fallback is assumed.

The result contains the original stored rate and pair/display-factor metadata. It is **not** a conversion factor or a posting authorization. Existing seed values include USD/SAR `3.75` with display factor One. Do not silently rescale those values or introduce 1:1 posting fallbacks.

Microsoft distinguishes `ExchangeRateForStorage`, `Rate` and `ConversionFactor` in its published exchange-rate entity. The provider framework also adjusts imported values for display-factor differences. The current project storage contract must be reconciled with those semantics before integrating financial conversion; no value migration or financial posting change is included here.

- [Microsoft exchange-rate entity metadata](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/entities/system/systemadministration/exchangerateentity)
- [Microsoft exchange-rate provider framework](https://learn.microsoft.com/en-us/dynamics365/fin-ops-core/dev-itpro/financial/create-exchange-rate-providers)

Focused tests in `EffectiveExchangeRateTests` cover date selection, expiration, duplicate start dates, nonpositive values, inactive/deleted rows and preservation of the raw stored amount. They do not certify SQL Server runtime, currency conversion or D365 version-specific parity.

## General ledger integration (2026-10-29)

`LedgerJournalSetupCheck` now calls `LedgerExchangeRateCheck` for persisted journal lines. Accounting rate availability uses the ledger's default rate type; reporting uses its own rate type or falls back to the default when unspecified. Both lookups start with the transaction currency. Same-currency paths need no rate; undefined reporting currency is skipped. Repeated requests for the same type/pair/date/purpose are checked once.

Journal line rate overrides and intercompany lines are reported as unverified instead of silently ignoring their special processing or requiring the ordinary direct pair. Availability failures are diagnostic errors; scaling, conversion, rounding, triangulation and full posting validation remain outside this check. No posted amounts or rate values are changed.

The SQLite-backed test in `LedgerJournalSetupCheckTests` verifies separate accounting/reporting types, the fallback, direct transaction-to-reporting lookup, expiration, overrides, company isolation, no tracked writes and unchanged raw values.

Reference: [Microsoft dual currency posting and rate-type fallback](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/dual-currency).
