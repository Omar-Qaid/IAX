# Master-Detail Page

## 1. Pattern Purpose & When to Use It
- **Purpose**: A parent-child stacked interface presenting a single master record on top and its child detail records in a DataGrid below.
- **When to Use**: - Parent-child 1:N relations like Warehouse & Locations, Customer Group & Members, Journal Header & Lines.

## 2. UI Structure & Layout
Top half features master record summary form/card; bottom half features child DataGrid. Connected via master selection context.

## 3. Page Sections & Components
- PageHeader
- ActionPane (Master actions + Line actions)
- Master Form/Card Region (Parent attributes)
- Line Details Toolbar (Add Line, Remove Line)
- Child AppDataGrid (Lines list)

## 4. Folder Structure
```text
src/patterns/master-detail/
├── MasterDetailPage.tsx
├── MasterDetailLayout.tsx
└── types.ts
```

## 5. Required Reusable Components
- PageContainer
- ActionPane
- AppDataGrid
- FastTabs

## 6. Data Flow & State Management
- **Data Flow**: 1. Master record loaded -> populates top form.
2. Master ID passed as query parameter to child lines query.
3. Child lines rendered in bottom grid.
- **State Management**: - Master dirty state + Detail lines dirty state unified via page hook.

## 7. Actions & Commands
- Add Line, Remove Line, Save All, Process Master

## 8. Validation Rules
- Validate Master header rules before allowing line creation.

## 9. Naming Conventions & Best Practices
- **Naming Conventions**: - Components named MasterDetail*.tsx
- **Best Practices**: - Auto-focus newly created line item grid row.

## 10. Do's and Don'ts Rules
DO: Keep master and child relationship synchronized.
DON'T: Orphan child records on delete.

## 11. Implementation Example
```tsx
// MasterDetailPage usage
```
