# Dashboard Page

## Purpose
An aggregate analytics view, often interchangeable with Workspace, but specifically tailored for high-level charts, graphs, and cross-module reporting rather than operational worklists.

## When to use
- Executive overviews, Financial Analytics, Sales Performance Dashboard.

## Folder structure
```text
src/patterns/dashboard/
├── DashboardPage.tsx          # Pattern component
└── types.ts                   # Pattern type exports
```

## Required components
```text
DashboardPage
├── PageHeader
├── Filter/Date Range Bar
├── Chart Grid (Pie, Bar, Line charts)
└── Summary DataGrids
```

## Data flow
```text
Global date/filter context changes → triggers multiple charting API endpoints → charts animate to new data.
```

## Examples
Executive Financial Dashboard.

## Rules
- Charts must be responsive.
- Filters should apply globally to all widgets on the dashboard.

## Description UI
A visually rich, widget-based layout heavily featuring data visualizations (Bar, Line, Pie charts). The dashboard is highly responsive, with widgets reflowing based on screen size. Unlike operational Workspaces, Dashboards prioritize trend analysis and aggregated metrics over row-level data grids.
