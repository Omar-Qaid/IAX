# Workspace / Dashboard Page

## 1. Pattern Purpose & When to Use It
- **Purpose**: An operational hub featuring summary KPI tiles, work lists, charts, and quick actions.
- **When to Use**: - Accounts Receivable Workspace, Inventory Management Dashboard, General Ledger Overview.

## 2. UI Structure & Layout
Grid layout: Top row KPI Tiles, Middle row Work List DataGrids, Right/Bottom row Quick Links & Charts.

## 3. Page Sections & Components
- PageHeader
- Summary Tiles Grid (WorkspaceTile cards)
- Active Worklists (AppDataGrids)
- Quick Action Links & Analytics Charts

## 4. Folder Structure
```text
src/patterns/workspace/
├── WorkspacePage.tsx
├── WorkspaceTile.tsx
└── types.ts
```

## 5. Required Reusable Components
- WorkspaceTile
- AppDataGrid
- Grid container

## 6. Data Flow & State Management
- **Data Flow**: Independent concurrent queries fetch KPI metrics and worklist records.
- **State Management**: - Dashboard filter state (Date range, Company / Legal Entity).

## 7. Actions & Commands
- Tile Drill-down, Quick Create, Refresh Metrics

## 8. Validation Rules
- Filter bar date ranges validation.

## 9. Naming Conventions & Best Practices
- **Naming Conventions**: - *WorkspacePage.tsx or *DashboardPage.tsx
- **Best Practices**: - Keep tiles interactive (clickable for drill-down).

## 10. Do's and Don'ts Rules
DO: Highlight urgent items with red/warning colors.
DON'T: Overcrowd workspace with more than 8 KPI tiles.

## 11. Implementation Example
```tsx
// ARWorkspacePage usage
```
