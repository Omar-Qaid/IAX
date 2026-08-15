# Shared utilities

Shared utilities are route- and domain-neutral helpers:

- `utilities/actionUtils.ts`: permission-aware action filtering.
- `utilities/gridUtils.ts`: generic grid helpers.
- `utilities/localizeColumns.ts`: chooses localized column definitions.
- `utilities/pageUtils.ts`: page-related helpers.
- `utilities/permissionUtils.ts`: UI permission helpers.
- `utils/deepEqual.ts`: structural equality used for dirty-state checks.

Browser persistence adapters live under `shared/services`, while general infrastructure utilities live under `core/utilities`. Put a helper at the lowest layer that can own it without importing upward. Pure utilities should not call React hooks, manipulate routes, or know module DTOs.
