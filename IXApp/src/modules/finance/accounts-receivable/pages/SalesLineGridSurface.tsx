import React, { useRef } from 'react';
import { Box } from '@mui/material';
import type { DataGridHandle } from '@shared/components/data-grid/types';

interface Props {
  children: React.ReactNode;
  gridRef: React.RefObject<DataGridHandle | null>;
  commit: () => Promise<boolean>;
  onAdd: () => void;
  onRemove: () => void;
  onFilter: () => void;
}

/** Keeps keyboard and mouse navigation within the sales grid's save lifecycle. */
export function SalesLineGridSurface({
  children,
  gridRef,
  commit,
  onAdd,
  onRemove,
  onFilter,
}: Props) {
  const navigating = useRef(false);
  const move = async (row: number, column: number) => {
    if (navigating.current) return;
    navigating.current = true;
    try {
      if (await commit()) {
        requestAnimationFrame(() => gridRef.current?.focusCell(row, column));
      }
    } finally {
      navigating.current = false;
    }
  };
  return (
    <Box
      onMouseDownCapture={(event) => {
        const target = event.target as HTMLElement;
        // Lookup menus use portals and must retain their native interaction.
        if (!event.currentTarget.contains(target) || event.button !== 0) return;
        const next = target.closest<HTMLElement>('[role="gridcell"]');
        const current = document.activeElement?.closest('[role="gridcell"]');
        if (!next || !current || next === current) return;
        event.preventDefault();
        event.stopPropagation();
        void move(Number(next.dataset.rowIndex), Number(next.dataset.colIndex));
      }}
      onKeyDown={(event) => {
        const target = event.target as HTMLElement;
        if (target.getAttribute('role') === 'combobox' || target.getAttribute('aria-expanded') === 'true') event.stopPropagation();
      }}
      onKeyDownCapture={(event) => {
        const target = event.target as HTMLElement;
        if (!event.currentTarget.contains(target) || event.nativeEvent.isComposing) return;
        const cell = target.closest<HTMLElement>('[role="gridcell"]');
        if (!cell) return;
        const input = target.matches('input, textarea, [role="combobox"]');
        if (event.altKey && event.key === 'Delete') {
          event.preventDefault();
          event.stopPropagation();
          onRemove();
          return;
        }
        if (event.key === 'Insert' || (event.altKey && event.key.toLowerCase() === 'n')) {
          event.preventDefault();
          event.stopPropagation();
          onAdd();
          return;
        }
        if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'f') {
          event.preventDefault();
          event.stopPropagation();
          onFilter();
          return;
        }
        // Do not steal arrows, Enter, or Escape from an open lookup/select.
        if (target.getAttribute('aria-expanded') === 'true') return;
        if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 's') {
          event.preventDefault();
          event.stopPropagation();
          void commit();
          return;
        }
        if (event.key === 'F2') {
          event.preventDefault();
          event.stopPropagation();
          (
            cell.querySelector<HTMLElement>('[data-grid-cell-focus], input, [role="combobox"]') ??
            cell
          ).focus();
          return;
        }
        const grid = cell.closest('[role="grid"]');
        const rowCount = Number(grid?.getAttribute('aria-rowcount') ?? 0);
        const columnCount = cell.parentElement?.querySelectorAll('[role="gridcell"]').length ?? 0;
        let r = Number(cell.dataset.rowIndex);
        let c = Number(cell.dataset.colIndex);
        if (event.key === 'Tab' || event.key === 'Enter') {
          c += event.shiftKey ? -1 : 1;
          if (c < 0) {
            r--;
            c = columnCount - 1;
          }
          if (c >= columnCount) {
            r++;
            c = 0;
          }
          if (r < 0 || r >= rowCount) {
            // Let Tab leave the grid at its boundaries.
            event.stopPropagation();
            if (event.key === 'Enter') {
              event.preventDefault();
              void commit();
            }
            return;
          }
        } else if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
          if (target.getAttribute('role') === 'combobox' || target.getAttribute('type') === 'date') return;
          r += event.key === 'ArrowDown' ? 1 : -1;
        } else if (!input && (event.key === 'ArrowLeft' || event.key === 'ArrowRight')) {
          const rtl = getComputedStyle(cell).direction === 'rtl';
          c += (event.key === 'ArrowRight' ? 1 : -1) * (rtl ? -1 : 1);
        } else {
          // Preserve text selection, numeric/date inputs, and native control shortcuts.
          if (input && ['ArrowLeft', 'ArrowRight', 'Home', 'End'].includes(event.key))
            event.stopPropagation();
          return;
        }
        event.preventDefault();
        event.stopPropagation();
        void move(
          Math.max(0, Math.min(r, rowCount - 1)),
          Math.max(0, Math.min(c, columnCount - 1))
        );
      }}
      sx={{
        '& [role="gridcell"]': { px: 1, fontSize: 13, transition: 'background-color 100ms ease' },
        '& [role="gridcell"]:focus-within': {
          outline: '2px solid',
          outlineColor: 'primary.main',
          outlineOffset: '-2px',
          zIndex: 3,
        },
        '& [role="gridcell"] .MuiInputBase-root': { fontSize: 13, minHeight: 28 },
        '& [role="gridcell"] .MuiInputBase-input': { py: 0.5 },
        '& [data-grid-resize-handle]:hover': { bgcolor: 'primary.main' },
        '& [role="row"][aria-selected="true"]': {
          boxShadow: 'inset 3px 0 0 currentColor',
          color: 'text.primary',
        },
      }}
    >
      {children}
    </Box>
  );
}
