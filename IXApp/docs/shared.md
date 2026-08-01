# Shared Layer Documentation (`src/shared`)

## 1. Purpose and Responsibilities
The `shared` layer contains reusable UI controls, generic enterprise components, form fields, virtualized data grids, dialogs, action panes, FastTabs, lookups, logistics drawers, and generic hooks for **IXApp**.

All components in the `shared` layer are **100% domain-agnostic**. They must never contain hardcoded business rules, domain-specific endpoints, or module-specific state.

---

## 2. Folder Structure
```text
src/shared/
├── components/
│   ├── action-pane/           # D365 F&O-style grouped action pane
│   │   ├── ActionPane.tsx
│   │   ├── ActionPaneGroup.tsx
│   │   ├── ActionPaneButton.tsx
│   │   └── types.ts
│   ├── app-shell/             # Shell topbar, sidebar, command palette
│   │   ├── AppShell.tsx
│   │   ├── AppTopBar.tsx
│   │   ├── AppSidebar.tsx
│   │   ├── CommandPalette.tsx
│   │   └── AppNotificationDrawer.tsx
│   ├── data-grid/             # Enterprise virtualized DataGrid
│   │   ├── DataGrid.tsx       # AppDataGrid alias & main grid control
│   │   ├── DataGridHeader.tsx # Sticky column header rendering
│   │   ├── DataGridBody.tsx   # Virtualized row list rendering
│   │   ├── DataGridToolbar.tsx# Search, filter, column toggle bar
│   │   └── hooks/             # useGridEditing, useGridLayout, useGridPersistence
│   ├── dialogs/               # Dialog architecture
│   │   ├── AppDialog.tsx      # Base responsive modal container
│   │   ├── ConfirmationDialog.tsx
│   │   └── DeleteConfirmationDialog.tsx
│   ├── fast-tabs/             # D365-style collapsible form sections
│   │   ├── FastTabs.tsx
│   │   ├── FastTab.tsx
│   │   ├── FastTabHeader.tsx
│   │   └── FastTabSummary.tsx
│   ├── feedback/              # Centralized status states
│   │   ├── LoadingState.tsx
│   │   ├── EmptyState.tsx
│   │   ├── ErrorState.tsx
│   │   └── AccessDeniedState.tsx
│   ├── fields/                # React Hook Form + MUI field system
│   │   ├── AppTextField.tsx
│   │   ├── AppNumberField.tsx
│   │   ├── AppCurrencyField.tsx
│   │   ├── AppDateField.tsx
│   │   ├── AppBooleanField.tsx
│   │   ├── AppSelectField.tsx
│   │   ├── AppLookupField.tsx
│   │   └── AppLookupGridField.tsx
│   ├── forms/                 # Form container & row layout system
│   │   ├── FormContainer.tsx
│   │   ├── FormRow.tsx
│   │   └── FormColumn.tsx
│   ├── logistics/             # Logistics address drawer panels
│   │   ├── LogisticsPostalAddressDrawer.tsx
│   │   └── LogisticsElectronicAddressDrawer.tsx
│   ├── lookups/               # Grid lookup & popover controls
│   │   ├── LookupGrid.tsx     # Multi-column popover table
│   │   ├── LookupGridField.tsx# RHF + RBAC guarded lookup field
│   │   └── types.ts
│   └── page/                  # Standard page structure containers
│       ├── PageContainer.tsx
│       ├── PageHeader.tsx
│       ├── PageTitle.tsx
│       └── PageContent.tsx
├── hooks/                     # Generic reusable hooks
│   ├── useNotifications.ts
│   ├── useDebounce.ts
│   ├── useLookupGridField.ts
│   └── useLogisticsAddress.ts
├── types/                     # Shared component types
│   └── logistics.ts
└── utilities/                 # Shared formatting & column helpers
    ├── actionUtils.ts
    ├── gridUtils.ts
    └── localizeColumns.ts
```

---

## 3. File Naming Conventions
- **Components:** `PascalCase.tsx` matching the exported component name (e.g., `LookupGrid.tsx`, `AppTextField.tsx`).
- **Hooks:** `camelCase.ts` starting with `use` (e.g., `useLookupGridField.ts`, `useNotifications.ts`).
- **Utilities:** `camelCase.ts` (e.g., `localizeColumns.ts`, `gridUtils.ts`).

---

## 4. Key Components & Sub-Systems

### 4.1 Action Pane (`@shared/components/action-pane`)
Renders D365-inspired grouped action toolbars.
- **Contract:** Accepts `PageAction[]` containing `id`, `label`, `icon`, `group` (`'New'`, `'Maintain'`, `'Process'`, `'Inquiries'`, `'Print'`, `'Options'`), `permission`, `disabled`, `onClick`.
- **RBAC:** Automatically checks `action.permission` and hides or disables unauthorized commands.

### 4.2 Reusable Field System (`@shared/components/fields`)
All input components (`AppTextField`, `AppNumberField`, `AppCurrencyField`, `AppDateField`, `AppBooleanField`, `AppSelectField`, `AppLookupGridField`) share a standard contract:
- React Hook Form `Controller` integration (with fallback when `control` is omitted).
- Automatic error message display from RHF `fieldState.error`.
- Full width, required indicator (`*`), and disabled/read-only styling.

### 4.3 D365 Multi-Column Grid Lookup (`@shared/components/lookups`)
- **`LookupGrid.tsx`:** Renders a popover table anchored to the input box. Features sticky multi-column headers, debounced inline search, virtual infinite scrolling (`@tanstack/react-virtual`), smart viewport positioning with automatic placement flip, clear adornment, and footer statistics bar.
- **`LookupGridField.tsx` / `AppLookupGridField.tsx`:** Form integration with `fetchById` edit-mode row resolution, LTR/RTL column localization (`filterLocalizedColumns`), and **RBAC Permission Guard** (`permissionModule` & `permissionResource`) showing a lock icon warning when access is restricted.

### 4.4 Logistics Address Drawers (`@shared/components/logistics`)
- **`LogisticsPostalAddressDrawer.tsx`:** Slide-out right panel for managing physical postal addresses. Implements cascading selection reset logic ($\text{Country} \rightarrow \text{State} \rightarrow \text{City}/\text{County}$), date range pickers (`validFrom`, `validTo`), multiline street input, building/zip/postbox fields, and primary switches.
- **`LogisticsElectronicAddressDrawer.tsx`:** Slide-out panel for managing electronic contact info (`Phone`, `Email`, `URL`, `Fax`, `Telex`, `InstantMessage`), extensions, descriptions, and primary switches.

### 4.5 FastTabs (`@shared/components/fast-tabs`)
Collapsible accordion form sections with summary text headers, badge indicators, and error highlighting for hidden invalid fields.

---

## 5. Hooks
- **`useNotifications()`:** Triggers global toast messages (`notify.success()`, `notify.error()`, `notify.info()`, `notify.warning()`).
- **`useDebounce<T>(value, delay)`:** Debounces rapid input changes for search fields.
- **`useLookupGridField()`:** TanStack React Query infinite query manager for paginated lookup dropdowns.
- **`useLogisticsAddress()`:** Cascading geography queries (`useCountryRegions`, `useStates`, `useCities`, `useCounties`).

---

## 6. Icon Import Rule (Crucial for Vitest ESM)
**CRITICAL:** Never import Material UI icons from the barrel index `@mui/icons-material`. Barrel imports resolve to `undefined` during Vitest ESM execution.

```tsx
// INCORRECT (Will crash Vitest ESM test suite):
import { Search, Close, FilterList } from '@mui/icons-material';

// CORRECT (Always use path imports):
import SearchIcon from '@mui/icons-material/Search';
import CloseIcon from '@mui/icons-material/Close';
import FilterListIcon from '@mui/icons-material/FilterList';
```

---

## 7. State Management
- Shared components manage local transient UI state (e.g., popover open/closed, current active row index).
- Form values are managed by React Hook Form.
- Shared components **do not** use Zustand.

---

## 8. Design Patterns
- **Compound Component Pattern (`FastTabs`, `FastTab`, `FastTabHeader`, `FastTabSummary`):** Components work together to share expansion state.
- **Controlled / Uncontrolled Fallback Pattern (`AppTextField`):** Works seamlessly with or without `FormContext`.
- **Render Prop / Slot Pattern (`AppDataGrid`):** Accepts custom cell renderers per column (`col.render(row)`).

---

## 9. Dependencies & Layer Rules
- **Allowed:** `@shared` $\rightarrow$ `@core`.
- **Forbidden:** `@shared` must **never** import from `@patterns`, `@modules`, or `@app`.

---

## 10. Best Practices & Reusability Rules
- Every shared component must support LTR and RTL directions.
- All colors must use Material UI theme tokens (`palette.primary.main`, `palette.divider`, `palette.text.secondary`).
- Never hardcode fixed pixel colors (`#000`, `#fff`, `#0F6CBD`) inside shared components.

---

## 11. Do's and Don'ts
- **DO:** Export named components (`export function LookupGrid()`).
- **DO:** Support `fullWidth`, `disabled`, and `readOnly` props consistently across all input controls.
- **DON'T:** Include domain logic like `customer.balance` inside shared components.
- **DON'T:** Use default exports for shared controls.

---

## 12. Examples

### Using `AppLookupGridField` in a Form
```tsx
<AppLookupGridField<CustomerGroup>
  name="customerGroupId"
  label="Customer Group"
  queryKey={['customer-groups-lookup']}
  columns={[
    { field: 'code', header: 'Group Code', width: 120 },
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
- [ ] Is the component domain-agnostic?
- [ ] Are icon imports written as specific path imports (`@mui/icons-material/IconName`)?
- [ ] Does the component support RTL layout?
- [ ] Are Material UI theme tokens used for colors?
- [ ] Is the component name identical to its file name?

---

## 14. Performance Considerations
- `LookupGrid` uses `@tanstack/react-virtual` to render thousands of rows with minimal DOM nodes.
- Column header definitions are memoized to avoid layout thrashing on scroll.
