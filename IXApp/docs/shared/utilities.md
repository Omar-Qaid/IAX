# Shared utilities

Shared utilities are route- and domain-neutral helpers:

- `utilities/localizeColumns.ts`: chooses localized column definitions.
- `utilities/localizedName.ts`: chooses the localized name of a record.
- `utils/deepEqual.ts`: structural equality used for dirty-state checks.

Action filtering, grid helpers, page helpers and UI permission helpers were listed here but existed
only as empty files, which have been removed. Permission checks go through `core/permissions`.

General infrastructure utilities live under `core/utilities`. Put a helper at the lowest layer that can own it without importing upward. Pure utilities should not call React hooks, manipulate routes, or know module DTOs.
