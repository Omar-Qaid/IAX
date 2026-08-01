# Workspace Page

## Purpose
An operational dashboard landing page with KPI summary tiles, charts, and data grid work lists.

## When to use
- Module Dashboard (AR Workspace, Inventory Workspace).
- Operational work centers for monitoring KPIs.

## Folder structure
```text
src/patterns/workspace/
├── WorkspacePage.tsx          # Pattern component
├── WorkspaceTile.tsx          # KPI summary tile card
└── types.ts                   # Pattern type exports
```

## Required components
```text
WorkspacePage
├── Summary Tiles Row (WorkspaceTile cards)
├── Charts Section
└── Work Lists (AppDataGrid)
```

## Data flow
```text
Module Page → useQuery for KPIs → WorkspacePage → renders Tiles and Grids.
```

## Examples
See `DashboardPage`.

## Rules
- Tiles use `elevation={0}` with hover animations.
- Tiles accept `onClick` for drill-down.
- Limit to 4–8 KPI tiles.

## Description UI
A high-level landing dashboard. The top row features large, visually distinct KPI metric tiles (e.g., "Total Open Orders: 45"). Below the tiles, the layout is typically split into sections containing embedded DataGrids (Work Lists) showing items requiring immediate attention, alongside quick-action navigation cards.
