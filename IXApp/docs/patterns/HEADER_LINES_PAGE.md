# Header-lines document usage

Header/lines/totals pages use the implemented [`DocumentPage`](DOCUMENT_PAGE.md). Pass already-composed header form, typed/virtualized `DataGrid` lines, calculated totals, commands, and dialogs.

The module owns status transitions, line validation, read-only rules, totals calculations, persistence, and confirmation. The pattern does not enforce “at least one line,” posted-document locking, or API process actions by itself.
