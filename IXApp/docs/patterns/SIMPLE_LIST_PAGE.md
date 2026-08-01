# Simple List Page

## Purpose
A single flat DataGrid for managing setup tables, reference data, and lightweight master data entities. Supports page-level Save/Cancel with inline editing or popup form dialogs.

## When to use
- Currencies, Units, Tax Groups, Customer Groups, Payment Terms, Number Sequences.
- Any reference table where the primary interaction is a single editable grid.

## Folder structure
```text
src/patterns/simple-list/
├── SimpleListPage.tsx         # Pattern component
├── useSimpleListPage.ts       # Pattern state hook
└── types.ts                   # Pattern type exports
```

## Required components
```text
SimpleListPage
├── PageHeader (title, subtitle)
├── ActionPane (New, Save, Cancel, Delete, Refresh)
├── AppDataGrid (masterForm=true for inline editing)
│   ├── DataGridToolbar
│   ├── DataGridHeader
│   └── DataGridBody
├── ErrorState / LoadingState
└── Dialogs
```

## Data flow
```text
Module Page
  → useListPage({ loadData, saveData, deleteData })
  → SimpleListPage
    → AppDataGrid (onRowSave, onNewRow)
    → ActionPane (Save/Delete hooks)
```

## Examples
See `src/modules/accounts-receivable/currencies/pages/CurrenciesPage.tsx`

## Rules
- Page-level Save/Cancel — NOT row-level buttons.
- Use `masterForm={true}` for inline editing.
- Always track `isDirty` and confirm unsaved changes.
- Memoize `columns` array outside render.

## Description UI
The UI is dominated by a single, full-width DataGrid that fills the majority of the page below the standard PageHeader and ActionPane. It features a flat list with rows of data. If inline editing is enabled, cells become text inputs/dropdowns when clicked. The design emphasizes data density and quick tab-navigation between cells, similar to a spreadsheet view.
