# Simple List Page

## 1. Pattern Purpose & When to Use It
- **Purpose**: Provides a single, flat DataGrid interface for managing setup tables, reference data, and lightweight master entities with low complexity.
- **When to Use**: Use for lookup tables, currencies, units of measure, payment terms, tax groups, customer groups, and number sequences where inline grid editing or simple modal dialogs are sufficient.

## 2. UI Structure & Layout
Full-width single-column layout containing a PageHeader, ActionPane, and a central AppDataGrid taking up the remaining vertical viewport height.

## 3. Page Sections & Components
- PageHeader (Title, Subtitle, Help Action)
- ActionPane (New, Edit, Save, Delete, Cancel, Refresh, Export)
- Filter Bar / Search (Quick Filter integrated in DataGrid toolbar)
- AppDataGrid (Virtualized grid container with masterForm=true)
- Dialogs (Delete confirmation modal)

## 4. Folder Structure
```text
src/patterns/simple-list/
├── SimpleListPage.tsx        # Container pattern component
├── useSimpleListPage.ts      # Pattern state hook
├── SimpleListToolbar.tsx     # Custom actions toolbar (optional)
└── types.ts                  # Props & types definitions
```

## 5. Required Reusable Components
- @shared/components/page/PageContainer
- @shared/components/page/PageHeader
- @shared/components/action-pane/ActionPane
- @shared/components/data-grid/DataGrid
- @shared/components/dialogs/DeleteConfirmationDialog

## 6. Data Flow & State Management
- **Data Flow**: 1. Module Page invokes useListPage hook.
2. Service fetches dataset via TanStack Query.
3. Rows passed to AppDataGrid.
4. User edits cell -> triggers onRowSave / markDirty.
5. User clicks Save in ActionPane -> handleSave persists array via API mutation.
- **State Management**: - Local dirty state tracking via useUnsavedChanges.
- Selection state (selectedIds) via useListPage.
- PageMode state ('view' | 'edit' | 'create') managed by usePageMode.

## 7. Actions & Commands
- New (adds empty row or opens creation dialog)
- Save (persists all modified/new rows)
- Delete (removes selected rows with confirmation)
- Refresh (re-fetches latest server data with dirty-state safety check)
- Export (downloads CSV of current grid view)

## 8. Validation Rules
- Grid cell-level inline validation via Zod schemas.
- Duplicate key detection on primary identifier fields before save.

## 9. Naming Conventions & Best Practices
- **Naming Conventions**: - Files: PascalCase (SimpleListPage.tsx)
- Hooks: camelCase starting with use (useSimpleListPage.ts)
- Types: PascalCase ending with Props/State (SimpleListPageProps)
- **Best Practices**: - Always set masterForm={true} on DataGrid for inline grid editing.
- Always memoize columns definition using useMemo.
- Keep primary key columns read-only after creation.

## 10. Do's and Don'ts Rules
DO:
- Use stable row IDs (never array index).
- Support keyboard navigation across grid cells.

DON'T:
- Use row-level Save buttons; always prefer ActionPane global Save.
- Put complex multi-tab details into a Simple List.

## 11. Implementation Example
```tsx
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import { useListPage } from '@shared/hooks/useListPage';

export function CurrenciesPage() {
  const { data, loading, selectedIds, setSelectedIds, handleSave, handleDelete } = useListPage({
    loadData: currencyService.getAll,
    saveData: currencyService.saveAll,
    deleteData: currencyService.delete,
  });

  return (
    <SimpleListPage
      title="Currencies"
      subtitle="General Ledger Setup"
      dataGridProps={{
        rows: data,
        columns: currencyColumns,
        masterForm: true,
        selectedIds,
        onSelectionChange: setSelectedIds,
      }}
    />
  );
}
```
