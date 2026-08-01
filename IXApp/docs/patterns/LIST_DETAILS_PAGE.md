# List + Details Page

## 1. Pattern Purpose & When to Use It
- **Purpose**: A side-by-side split view with a record browser grid on the left and comprehensive detail FastTabs on the right.
- **When to Use**: - Customers, Vendors, Products, Employees master pages.

## 2. UI Structure & Layout
Left pane: 30-40% width DataGrid. Right pane: 60-70% width FastTabs detail form.

## 3. Page Sections & Components
- PageHeader
- ActionPane
- Split Grid View (Left DataGrid list, Right FastTabs details)

## 4. Folder Structure
```text
src/patterns/list-details/
├── ListDetailsPage.tsx
├── ListDetailsLayout.tsx
└── types.ts
```

## 5. Required Reusable Components
- AppDataGrid
- FastTabs
- LogisticsPostalAddressDrawer

## 6. Data Flow & State Management
- **Data Flow**: Row selected in left grid -> loads right detail form via TanStack Query.
- **State Management**: - Unsaved changes check when switching left grid rows.

## 7. Actions & Commands
- New Record, Save Details, Delete Record, Refresh List

## 8. Validation Rules
- Validate detail form fields on change/submit.

## 9. Naming Conventions & Best Practices
- **Naming Conventions**: - ListDetails*.tsx
- **Best Practices**: - Collapse left grid on small mobile screens.

## 10. Do's and Don'ts Rules
DO: Prompt on unsaved changes when clicking another row.
DON'T: Lose modified detail form input.

## 11. Implementation Example
```tsx
// ListDetailsPage usage
```
