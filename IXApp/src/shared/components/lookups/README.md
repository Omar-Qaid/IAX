# Lookup subsystem

Provides lookup fields (`LookupField`, `LookupDisplayField`, `LookupGridField`,
`DataGridLookupField`), the `LookupDialog`, `LookupGrid`, `LookupNavigationLabel`,
`lookupValueStyle` and the shared `types`. It supports selectable multi-column datasets without
embedding business endpoint knowledge.

Features supply lookup rows, columns, identity/value mapping, loading/error state, and remote
filtering behavior. Use module lookup configuration for domain-specific choices.

Filter/search controls, value rendering and a `useLookup` hook are **not** provided: those files
existed as empty placeholders and were removed. Filtering and searching live in `LookupDialog` and
`LookupGrid` today.

[Lookup guide](../../../../docs/shared/lookups.md) · [Lookup page pattern](../../../patterns/lookup/README.md)
