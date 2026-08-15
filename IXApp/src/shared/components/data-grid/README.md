# Custom DataGrid subsystem

## Overview

`DataGrid.tsx` exports `DataGrid` and the `AppDataGrid` compatibility alias. This is a project-owned grid built from Material UI and TanStack Virtual, not MUI X.

## Structure

| Area | Responsibility | Documentation |
| --- | --- | --- |
| Root files | Public grid, toolbar, states, mobile body, filters, factories, types, utilities | This document |
| `body` | Virtual row/cell rendering, skeletons, row context menu | [README](body/README.md) |
| `header` | Sorting, filter popovers, pinned headers, header menus | [README](header/README.md) |
| `hooks` | Processing, data source, selection, editing, layout, persistence, autosize/load-more | [README](hooks/README.md) |
| `sidebar` | Column, filter, and feature panels | [README](sidebar/README.md) |

At widths below the Material UI `md` breakpoint, `DataGridMobileBody` replaces the desktop table body. Features supply stable row IDs, columns, data/mutations, and controlled behavior where required.

[Complete DataGrid guide](../../../../docs/shared/data-grid.md) · [Responsive standards](../../../../docs/ui-ux-and-responsive.md)
