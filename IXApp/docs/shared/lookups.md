# Lookups Component Documentation (`src/shared/components/lookups`)

## 1. Purpose and Responsibilities
The `lookups` sub-system provides Microsoft Dynamics 365 Finance & Operations-style multi-column table dropdown lookups for **IXApp**. 

It features virtualized infinite scrolling popover tables (`LookupGrid.tsx`), React Hook Form integration (`LookupGridField.tsx`), LTR/RTL column localization (`filterLocalizedColumns`), edit-mode record resolution (`fetchById`), and **RBAC Permission Guarding** (`permissionModule` & `permissionResource`).

---

## 2. Folder Structure
```text
src/shared/components/lookups/
├── LookupGrid.tsx             # Multi-column popover table lookup control
├── LookupGridField.tsx        # React Hook Form + RBAC permission guarded lookup field
├── LookupDialog.tsx           # Full dialog lookup modal variant
├── types.ts                   # GridLookupProps, GridLookupColumn, GridLookupAction contracts
└── index.ts                   # Public exports for lookups module
```

---

## 3. Naming Conventions
- **Components:** `PascalCase.tsx` (e.g., `LookupGrid.tsx`, `LookupGridField.tsx`).
- **Interfaces & Types:** `GridLookupProps<T>`, `GridLookupColumn<T>`, `GridLookupAction`.

---

## 4. Key Components & Architecture

### 4.1 `LookupGrid<T>` (`LookupGrid.tsx`)
A virtualized multi-column dropdown table popover anchored to an input element.
- **Smart Position Calculation (`recalcPosition`):** Computes top/left/width/maxHeight relative to input anchor, auto-flips placement (bottom/top) based on viewport room, updates border accent colors, and listens to window resize/scroll events.
- **Virtualization:** Uses `@tanstack/react-virtual` to render thousands of rows smoothly with low DOM overhead.
- **Infinite Scrolling:** Automatically triggers `fetchNextPage()` when scrolling near bottom of list.
- **Keyboard Navigation:** `ArrowUp`, `ArrowDown`, `PageUp`, `PageDown`, `Home`, `End`, `Enter`, `Escape`.
- **Search & Clear:** Debounced inline search input and clear adornment button.

### 4.2 `LookupGridField<T>` (`LookupGridField.tsx`)
React Hook Form Controller integration wrapper for `LookupGrid`.
- **Edit-Mode Resolution (`fetchById`):** Asynchronously fetches the label for pre-selected IDs when editing existing records that are not yet loaded in initial page 1.
- **RBAC Permission Guard:** Checks `permissionModule` and `permissionResource` via `usePermissions()`. If user lacks `View` access, automatically disables the field, renders a lock icon adornment (`LockIcon`), and displays a permission warning helper text.
- **Column Localization:** Uses `filterLocalizedColumns` to show Arabic header names in RTL mode and English header names in LTR mode.

---

## 5. Hooks & Integrations
- `useLookupGridField`: TanStack Query infinite query manager for lookup pagination.
- `usePermissions`: Evaluates RBAC permissions (`permissionModule`, `permissionResource`, `'View'`).

---

## 6. Services & APIs
Consumes generic `fetchPage` function returning `Promise<PagedResult<T>>` and optional `fetchById` function returning `Promise<T | null>`.

---

## 7. State Management
- Selected ID and display text are managed by React Hook Form or standalone `value` / `onChange` props.
- Popover open/closed state, search query, and active highlighted row index are managed locally in `LookupGrid.tsx`.

---

## 8. Design Patterns
- **Popover Dropdown Pattern:** Renders popover via `<Portal>` to avoid overflow clipping inside dialogs or card containers.
- **Guard Pattern:** Enforces security permissions at the input field level.

---

## 9. Architecture & Dependencies
- **Dependencies:** `@mui/material`, `@tanstack/react-virtual`, `@tanstack/react-query`, `react-hook-form`, `@core/auth/usePermissions`.
- **Forbidden:** No business module imports (`@modules/*`).

---

## 10. Best Practices
- Always pass `queryKey` array to ensure TanStack Query caches lookup results efficiently.
- Specify `permissionModule` and `permissionResource` to enforce RBAC field security.

---

## 11. Do's and Don'ts
- **DO:** Provide `fetchById` so existing form values resolve display labels cleanly on initial page load.
- **DON'T:** Use plain select dropdowns for relational master data tables exceeding 50 items. Always use `LookupGridField`.

---

## 12. Code Example
```tsx
<LookupGridField<CustomerGroup>
  name="customerGroupId"
  label="Customer Group"
  queryKey={['customer-groups-lookup']}
  columns={[
    { field: 'code', header: 'Code', width: 120 },
    { field: 'name', header: 'Name', flex: 1 },
  ]}
  fetchPage={customerGroupService.getPagedLookup}
  fetchById={customerGroupService.getById}
  permissionModule="AccountsReceivable"
  permissionResource="CustomerGroups"
/>
```

---

## 13. Decision Rules & Checklist
- [ ] Is `queryKey` unique for the lookup data entity?
- [ ] Are `fetchPage` and `fetchById` provided?
- [ ] Is `permissionModule` specified if field is restricted?
