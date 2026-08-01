# Master Details Page

## Purpose
A vertically stacked layout showing a master record (form or grid) on top and dependent child collections in a detail DataGrid below.

## When to use
- Journals and Journal Lines, Warehouses and Locations, Chart of Accounts and Sub-accounts.
- Any parent → child (1:N) relationship where both levels are visible simultaneously.

## Folder structure
```text
src/patterns/master-detail/
├── MasterDetailPage.tsx       # Pattern component
├── MasterDetailLayout.tsx     # Split layout
├── useMasterDetailPage.ts     # Pattern state hook
└── types.ts                   # Pattern type exports
```

## Required components
```text
MasterDetailPage
├── PageHeader
├── ActionPane
├── Master Section (Form or DataGrid)
├── Detail Toolbar
├── Detail DataGrid (filtered by master ID)
└── Dialogs
```

## Data flow
```text
Master selection changes → Detail query key updates → Detail grid re-fetches.
```

## Examples
Journals and Journal Lines view.

## Rules
- Detail DataGrid must re-fetch when master selection changes.
- Save operations may need to persist both atomically.
- Detail grid shows EmptyState when no master is selected.

## Description UI
The page is split horizontally. The top section presents the Master record (either as a concise form or a compact grid). The bottom section is a DataGrid showing child records tied to the selected master. Selecting a different master dynamically refreshes the lower grid without reloading the page.
