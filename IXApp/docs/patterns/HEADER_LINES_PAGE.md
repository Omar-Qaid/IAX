# Header-Lines Document Page

## 1. Pattern Purpose & When to Use It
- **Purpose**: A transactional document page with order header metadata, line items DataGrid, and financial summary totals.
- **When to Use**: - Sales Orders, Purchase Orders, Invoices, Transfer Orders, Quotations.

## 2. UI Structure & Layout
Stacked document layout: Header FastTabs at top, Lines DataGrid in middle, Totals panel at bottom right.

## 3. Page Sections & Components
- PageHeader with Status Badge (Draft, Posted, Confirmed)
- ActionPane (Workflow: Confirm, Post, Print)
- Header Paper (FastTabs)
- Lines Paper (AppDataGrid with masterForm=true)
- Totals Summary Panel

## 4. Folder Structure
```text
src/patterns/document/
├── DocumentPage.tsx
├── DocumentHeader.tsx
├── DocumentLines.tsx
└── DocumentTotals.tsx
```

## 5. Required Reusable Components
- PageContainer
- ActionPane
- AppDataGrid
- FastTabs
- StatusBadge

## 6. Data Flow & State Management
- **Data Flow**: 1. Document loaded by ID.
2. Header form & lines grid populated.
3. Line modifications update financial totals dynamically.
4. Process action (Post/Confirm) executes workflow API.
- **State Management**: - Managed via useDocumentPage hook.
- Lifecycle status transitions ('Draft' -> 'Confirmed').

## 7. Actions & Commands
- Save, Confirm, Post, Cancel Order, Print Document

## 8. Validation Rules
- Must have at least 1 line item before posting/confirming.

## 9. Naming Conventions & Best Practices
- **Naming Conventions**: - *DocumentPage.tsx or *OrderPage.tsx
- **Best Practices**: - Right-align financial numeric fields in grid and totals.

## 10. Do's and Don'ts Rules
DO: Show clear status badges in header.
DON'T: Allow line modifications on Posted documents.

## 11. Implementation Example
```tsx
// SalesOrderPage usage
```
