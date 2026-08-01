# List Details Page

## Purpose
A split-view layout with a left-pane searchable grid and a right-pane detail form (FastTabs).

## When to use
- Customers, Vendors, Products, Employees, Warehouses.
- Any master entity requiring a summary grid alongside rich multi-tab detail forms.

## Folder structure
```text
src/patterns/list-details/
├── ListDetailsPage.tsx        # Pattern component
├── ListDetailsLayout.tsx      # Split layout helper
├── useListDetailsPage.ts      # Pattern state hook
└── types.ts                   # Pattern type exports
```

## Required components
```text
ListDetailsPage
├── SplitView Container
│   ├── Left: AppDataGrid (record list)
│   └── Right: DetailPane (FastTabs)
└── Dialogs
```

## Data flow
```text
Module Page → useListPage → ListDetailsPage → DataGrid selection → DetailsPane rendering.
```

## Examples
See `CustomersPage`.

## Rules
- Grid occupies full width when no record selected; splits to side-by-side on selection.
- Switching records prompts unsaved changes if form is dirty.
- Detail content conditionally rendered.

## Description UI
By default, presents a full-width list (DataGrid). Upon selecting a row, the list shrinks to a left-side panel (e.g., 30-40% width), and a rich detail form slides into the right-side pane containing FastTabs (General, Addresses, Financials). This provides a seamless "inbox-like" navigation experience.
