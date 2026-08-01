# Data Grid Component Documentation (`src/shared/components/data-grid`)

## 1. Purpose and Responsibilities
The `data-grid` sub-system provides **`AppDataGrid`** (`DataGrid.tsx`), the standard high-performance virtualized data table for **IXApp**. Inspired by Microsoft Dynamics 365 Finance & Operations grid views, it delivers compact row rendering, client/server pagination, multi-column sorting, filter bar filtering, column pinning, column autosizing, selection management, inline master-form editing, CSV export, and complete keyboard navigation (`F2`, `F5`, `F7`, `F9`, `Ctrl+F`, `Ctrl+P`, `Tab`, `Arrow` keys).

---

## 2. Folder Structure
```text
src/shared/components/data-grid/
├── DataGrid.tsx               # Main AppDataGrid component & export
├── DataGridHeader.tsx         # Sticky multi-column header & filter bar
├── DataGridBody.tsx           # Virtualized table body using TanStack Virtual
├── DataGridMobileBody.tsx     # Card-based responsive view for mobile screens
├── DataGridToolbar.tsx        # Search input, status counts, add button
├── GridSidebar.tsx            # Column manager, filter drawer & view settings
├── DataGridUtils.ts           # CSV export, column flex width calculation
├── types.ts                   # DataGridProps, DataGridColumn, DataGridHandle contracts
└── hooks/                     # Dedicated grid state hooks
    ├── useGridAutosize.ts     # Double-click column width auto-calculation
    ├── useGridDataProcessing.ts# Client-side filtering & sorting processor
    ├── useGridDataSource.ts   # Server-side pagination & fetch manager
    ├── useGridEditing.ts      # Inline cell editing hook
    ├── useGridLayout.ts       # Layout resize observer & scrollbar calculator
    ├── useGridPersistence.ts  # LocalStorage persistence hook for user views
    ├── useGridSelection.ts    # Single & multi-row selection state
    └── useLoadMore.ts         # Infinite scroll pagination trigger
```

---

## 3. Naming Conventions
- **Components:** `PascalCase.tsx` (e.g., `DataGrid.tsx`, `DataGridHeader.tsx`, `DataGridBody.tsx`).
- **Hooks:** `camelCase.ts` starting with `use` (e.g., `useGridSelection.ts`, `useGridPersistence.ts`).
- **Contracts:** `PascalCase` (e.g., `DataGridProps<T>`, `DataGridColumn<T>`, `DataGridHandle`).

---

## 4. Key Components & Architecture

### 4.1 `AppDataGrid` (`DataGrid.tsx`)
The master component wrapping toolbar, sticky header, virtualized body, side drawer, and footer bar.
- Accepts generic row items `rows: T[]` and typed column array `columns: DataGridColumn<T>[]`.
- Exports imperative ref `DataGridHandle` (`startAddRow`, `startEditRow`, `saveEdit`, `cancelEdit`, `toggleSidebar`).

### 4.2 Keyboard Navigation & Shortcuts
`AppDataGrid` supports standard enterprise keyboard shortcuts:
- `ArrowUp` / `ArrowDown` / `ArrowLeft` / `ArrowRight`: Cell focus navigation.
- `Tab` / `Shift+Tab`: Next / previous cell navigation across rows.
- `Home` / `End` / `PageUp` / `PageDown`: Jump to start/end of row or skip 10 rows.
- `F2` or `Enter`: Enter inline edit mode.
- `Escape`: Cancel active inline edit.
- `Ctrl+S`: Save active inline edit.
- `Ctrl+F`: Focus search input.
- `Ctrl+Shift+F`: Open filter drawer.
- `Ctrl+P`: Trigger print dialog.
- `Ctrl+A`: Select all rows.

### 4.3 JSDOM Virtualization Fallback Rule
In Vitest JSDOM test environments, DOM containers have 0 height by default, causing `@tanstack/react-virtual` to return `virtualItems = []`. `DataGridBody.tsx` contains a mandatory fallback:
```tsx
const displayVirtualItems =
  virtualItems.length > 0
    ? virtualItems
    : rows.map((_, index) => ({
        index,
        key: index,
        start: index * rowHeight,
        size: rowHeight,
      }));
```

---

## 5. Hooks
- **`useGridPersistence`:** Saves user column order, column widths, hidden states, and sort filters in `localStorage`.
- **`useGridSelection`:** Manages single and multi-row selected ID arrays.
- **`useInlineEdit`:** Manages master-form temporary editing row state (`NEW_ROW_ID`, field updates, validation).

---

## 6. Icon Import Rule (Vitest ESM Safety)
All icons used in DataGrid sub-components (`DataGridHeader.tsx`, `DataGridToolbar.tsx`) **must** use specific path imports:
```tsx
import FilterListIcon from '@mui/icons-material/FilterList';
import ViewColumnIcon from '@mui/icons-material/ViewColumn';
```

---

## 7. State Management
- Grid preference state (column widths, pinning) is persisted via `useGridPersistence`.
- Filtering and search state are stored locally or passed through server-side callbacks.
- API data is **not** stored in Zustand.

---

## 8. Best Practices
- Define `columns` array outside render or wrap with `useMemo`.
- Always provide a stable `getRowId={(row) => row.id}` callback. Do not use array index.
- Use `masterForm={true}` for inline grid editing.

---

## 9. Do's and Don'ts
- **DO:** Enable `serverSide={true}` for datasets exceeding 1,000 items.
- **DON'T:** Place Save/Cancel action buttons inside individual table cells. Use page-level Save/Cancel in `ActionPane`.

---

## 10. Code Example
```tsx
const columns: DataGridColumn<Customer>[] = [
  { field: 'code', headerName: 'Customer Code', width: 130, pinned: 'left' },
  { field: 'name', headerName: 'Customer Name', flex: 1 },
  { field: 'currencyCode', headerName: 'Currency', width: 100 },
  {
    field: 'balance',
    headerName: 'Balance',
    width: 140,
    align: 'right',
    renderCell: (row) => formatCurrency(row.balance, row.currencyCode),
  },
];

<AppDataGrid<Customer>
  rows={customers}
  columns={columns}
  getRowId={(row) => row.id}
  onRowClick={(row) => console.log('Selected:', row)}
/>
```

---

## 11. Performance Considerations
- Uses `@tanstack/react-virtual` to render only visible rows ($\sim 20$ DOM rows rendered even for 100,000 data items).
- Uses `memo` and `useCallback` on column renderers to eliminate unnecessary re-renders.
