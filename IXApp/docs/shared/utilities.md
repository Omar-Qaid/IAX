# Shared Utilities Documentation (`src/shared/utilities`)

## 1. Purpose and Responsibilities
The `utilities` sub-system provides pure helper functions for visual controls, grid column formatting, action pane permission filtering, and column localization in **IXApp**.

---

## 2. Folder Structure
```text
src/shared/utilities/
├── localizeColumns.ts        # Filters & maps localized column headers for LTR & RTL
├── actionUtils.ts            # Filters action pane command arrays based on permissions
├── gridUtils.ts              # DataGrid column width calculation & CSV export helpers
├── pageUtils.ts              # Page mode transition & breadcrumbs helpers
└── permissionUtils.ts        # RBAC permission evaluation helpers
```

---

## 3. Key Utility Functions

### 3.1 `localizeColumns.ts` (`filterLocalizedColumns`)
Automatically maps multi-column header labels based on current language direction (`isRtl`):
- In Arabic (`isRtl === true`), replaces column headers with `headerAr` if present, or appends Arabic suffix fields (`nameAr`).
- In English (`isRtl === false`), returns standard `header` / `headerName` and English properties (`name`).

### 3.2 `actionUtils.ts` (`filterActionsByPermission`)
Evaluates an array of `PageAction` items against the user's permission matrix, returning only authorized commands.

---

## 4. Architecture & Dependencies
- **Dependencies:** Pure TypeScript functions without React state.
- **Forbidden:** No direct DOM manipulation or business module imports.

---

## 5. Code Example
```ts
// Localizing DataGrid columns for LTR/RTL
const localizedColumns = useMemo(
  () => filterLocalizedColumns(columns, isRtl),
  [columns, isRtl]
);
```
