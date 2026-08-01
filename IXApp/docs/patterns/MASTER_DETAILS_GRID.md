# Master-Detail + DataGrid Page

## 1. Pattern Purpose & When to Use It
- **Purpose**: A dual-grid layout where both Master and Detail entities are presented as virtualized DataGrids.
- **When to Use**: - Viewing Voucher Headers & Voucher Lines, Batch Jobs & Execution Tasks.

## 2. UI Structure & Layout
Split layout (vertical or horizontal split) containing two synchronized AppDataGrids.

## 3. Page Sections & Components
- PageHeader
- ActionPane
- Master DataGrid (Top/Left)
- Detail DataGrid (Bottom/Right)

## 4. Folder Structure
```text
src/patterns/master-detail-grid/
├── MasterDetailGridPage.tsx
└── types.ts
```

## 5. Required Reusable Components
- AppDataGrid
- SplitView / Box containers

## 6. Data Flow & State Management
- **Data Flow**: Master Grid row click -> updates selected master ID -> triggers detail grid refetch.
- **State Management**: - Selected master ID in state drives detail grid query key.

## 7. Actions & Commands
- Filter Master, Refresh Grids, Export Master/Detail Data

## 8. Validation Rules
- Primary key constraints across grids.

## 9. Naming Conventions & Best Practices
- **Naming Conventions**: - MasterDetailGrid*.tsx
- **Best Practices**: - Show empty state in detail grid when no master row selected.

## 10. Do's and Don'ts Rules
DO: Synchronize selection immediately.
DON'T: Fetch all child records at once without filtering.

## 11. Implementation Example
```tsx
// Dual grid usage
```
