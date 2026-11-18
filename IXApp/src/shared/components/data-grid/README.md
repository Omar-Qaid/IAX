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

Column pinning and resize handles use logical inline-start/inline-end edges. In RTL, a `pinned: 'left'` column is the leading column on the right and dragging its inner handle outward increases its width. Desktop cell text follows the language direction; mobile cards center labels, values, and actions.

[Complete DataGrid guide](../../../../docs/shared/data-grid.md) · [Responsive standards](../../../../docs/ui-ux-and-responsive.md)

## Edit from view (pencil)

Editable cells (`editable: true`) show a pencil on hover while no row is being edited, as in D365:

- **Always-editable grids** (`masterForm`): the pencil edits that row and focuses the clicked cell, the same as a double-click.
- **Grids whose page owns edit mode** (document lines): pass `onCellEditRequest`. The pencil calls it so the page switches to edit mode. Once `masterForm` is on, the grid edits the row and focuses the clicked cell. Master-detail pages get this through `MasterDetailLinesGrid`.

Each pencil is named "Edit <column>" for screen readers. Read-only grids show no pencil. `editRowHighlight: 'none'` drops the edited-row outline, giving the cell-editing look.
