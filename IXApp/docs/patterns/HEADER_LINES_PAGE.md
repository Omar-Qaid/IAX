# Header Lines Page

## Purpose
A transactional document page with a header form section, a lines DataGrid, and a totals summary panel.

## When to use
- Sales Orders, Purchase Orders, Transfer Orders, Quotations.
- Any transactional document following the Header + Lines + Totals structure.

## Folder structure
```text
src/patterns/document/
├── DocumentPage.tsx           # Pattern component
├── DocumentHeader.tsx         # Header form
├── DocumentLines.tsx          # Lines grid
└── types.ts                   # Pattern type exports
```

## Required components
```text
DocumentPage
├── PageHeader (statusBadge)
├── ActionPane (Confirm, Post)
├── Header Paper (FastTabs)
├── Lines Paper (AppDataGrid)
└── Totals Paper (right-aligned summary)
```

## Data flow
```text
useDocumentPage(id) → loads order → updates header/lines → executeProcessAction() for lifecycle changes.
```

## Examples
See `SalesOrderPage`.

## Rules
- `statusBadge` reflects lifecycle state.
- Process actions must handle loading/errors.
- Line item DataGrid uses `masterForm=true` for inline editing.

## Description UI
A comprehensive, document-centric layout. The top section is a Paper panel showing the Header (key metadata like Customer, Date, Currency in collapsed FastTabs). Below it is a full-width DataGrid for Lines (items, quantities, prices). At the bottom right, a distinct Totals panel aggregates the financial numbers (Subtotal, Tax, Grand Total).
