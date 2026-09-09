import React, { useRef, useEffect } from 'react';
import { Box } from '@mui/material';
import type { DataGridHandle } from '@shared/components/data-grid/types';

interface Props {
  children: React.ReactNode;
  gridRef: React.RefObject<DataGridHandle | null>;
  commit: () => Promise<boolean>;
  onAdd: () => void;
  onRemove: () => void;
  onFilter: () => void;
  resolveRowId?: (id: string | number) => string | number;
  onPasteError?: () => void;
}

/** Keeps keyboard and mouse navigation within the sales grid's save lifecycle. */
export function SalesLineGridSurface({
  children,
  gridRef,
  commit,
  onAdd,
  onRemove,
  onFilter,
  resolveRowId,
  onPasteError,
}: Props) {
  const navigating = useRef(false);
  const pointerClick = useRef(false);
  const lastCell = useRef<HTMLElement | null>(null);
  const mounted = useRef(true);
  useEffect(() => {
    mounted.current = true;
    return () => {
      mounted.current = false;
    };
  }, []);
  const move = async (row: number, column: number) => {
    if (navigating.current) return;
    const address = gridRef.current?.getCellAddress?.(row, column);
    navigating.current = true;
    try {
      if (await commit()) {
        if (!mounted.current) return;
        if (address && gridRef.current?.focusRecordCell) {
          await gridRef.current.focusRecordCell({
            ...address,
            rowId: resolveRowId?.(address.rowId) ?? address.rowId,
          });
        } else await gridRef.current?.focusCell(row, column);
      }
    } finally {
      navigating.current = false;
    }
  };
  return (
    <Box
      onFocusCapture={(event) => {
        const cell = (event.target as HTMLElement).closest<HTMLElement>('[role="gridcell"]');
        if (cell && event.currentTarget.contains(cell)) lastCell.current = cell;
      }}
      onClickCapture={(event) => {
        if (pointerClick.current) {
          pointerClick.current = false;
          event.preventDefault();
          event.stopPropagation();
        }
      }}
      onCopy={(event) => {
        const target = event.target as HTMLElement;
        const cell = target.closest<HTMLElement>('[role="gridcell"]');
        if (!cell || !event.currentTarget.contains(cell)) return;
        if (target instanceof HTMLInputElement || target instanceof HTMLTextAreaElement) {
          if (target.selectionStart !== target.selectionEnd) return;
          event.clipboardData.setData('text/plain', target.value);
        } else {
          if (window.getSelection()?.toString()) return;
          event.clipboardData.setData('text/plain', cell.textContent ?? '');
        }
        event.preventDefault();
        event.stopPropagation();
      }}
      onPasteCapture={(event) => {
        const target = event.target as HTMLElement;
        if (!event.currentTarget.contains(target) || !target.closest('[role="gridcell"]')) return;
        const value = event.clipboardData.getData('text/plain').trim();
        if (/[\t\r\n]/.test(value)) {
          event.preventDefault();
          event.stopPropagation();
          onPasteError?.();
        }
      }}
      onMouseDownCapture={(event) => {
        // The previous target may have unmounted before its click was dispatched.
        // Only suppress the click belonging to this pointer interaction.
        pointerClick.current = false;
        const target = event.target as HTMLElement;
        // Lookup menus use portals and must retain their native interaction.
        if (!event.currentTarget.contains(target) || event.button !== 0) return;
        const next = target.closest<HTMLElement>('[role="gridcell"]');
        const current = document.activeElement?.closest('[role="gridcell"]') ?? lastCell.current;
        if (!next || !current || !event.currentTarget.contains(current) || next === current) return;
        pointerClick.current = true;
        event.preventDefault();
        event.stopPropagation();
        void move(Number(next.dataset.rowIndex), Number(next.dataset.colIndex));
      }}
      onKeyDown={(event) => {
        const target = event.target as HTMLElement;
        if (
          target.getAttribute('role') === 'combobox' ||
          target.getAttribute('aria-expanded') === 'true'
        )
          event.stopPropagation();
      }}
      onKeyDownCapture={(event) => {
        const target = event.target as HTMLElement;
        if (!event.currentTarget.contains(target) || event.nativeEvent.isComposing) return;
        const cell = target.closest<HTMLElement>('[role="gridcell"]');
        if (
          (event.ctrlKey || event.metaKey) &&
          !event.shiftKey &&
          event.key.toLowerCase() === 'f'
        ) {
          event.preventDefault();
          event.stopPropagation();
          onFilter();
          return;
        }
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
          void (async () => {
            if (navigating.current) return;
            navigating.current = true;
            try {
              if (await commit()) requestAnimationFrame(onAdd);
            } finally {
              navigating.current = false;
            }
          })();
          return;
        }
        if (event.key === 'F5') {
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
        } else if (
          (event.key === 'Home' || event.key === 'End') &&
          (!input || event.ctrlKey || event.metaKey)
        ) {
          c = event.key === 'Home' ? 0 : columnCount - 1;
          if (event.ctrlKey || event.metaKey) r = event.key === 'Home' ? 0 : rowCount - 1;
        } else if (!input && (event.key === 'PageUp' || event.key === 'PageDown')) {
          r += event.key === 'PageDown' ? 6 : -6;
        } else if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
          if (target.getAttribute('role') === 'combobox' || target.getAttribute('type') === 'date')
            return;
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
        '& [role="gridcell"]': { px: 1, fontSize: 13 },
        '& [role="gridcell"]:focus-within': {
          outline: '1px solid',
          outlineColor: 'primary.main',
          outlineOffset: '-1px',
          bgcolor: 'inherit',
          zIndex: 3,
        },
        '& [role="gridcell"] :focus, & [role="gridcell"] :focus-visible': { outline: 'none' },
        '& [role="gridcell"]:has([aria-invalid="true"])': {
          outline: '1px solid',
          outlineColor: 'error.main',
          outlineOffset: '-1px',
        },
        '& [role="gridcell"] .MuiInputBase-root': { fontSize: 13, minHeight: 28 },
        '& [role="gridcell"] .MuiInputBase-input': { py: 0.5 },
        '& [role="gridcell"] .MuiInput-root::before, & [role="gridcell"] .MuiInput-root::after': {
          display: 'none',
        },
        '& [role="gridcell"] .MuiOutlinedInput-notchedOutline': { border: 0 },
        '& [role="gridcell"] .MuiSelect-select:focus': { bgcolor: 'transparent' },
        '& [role="row"][data-row-id]:hover': {
          bgcolor: (theme) =>
            `color-mix(in srgb, ${theme.palette.primary.main} 2%, ${theme.palette.background.paper})`,
        },
        '& [data-grid-resize-handle]:hover': { bgcolor: 'primary.main' },
        '& [role="row"][aria-selected="true"], & [role="row"][aria-selected="true"]:hover': {
          bgcolor: (theme) =>
            `color-mix(in srgb, ${theme.palette.primary.main} 3%, ${theme.palette.background.paper})`,
          boxShadow: 'none',
        },
      }}
    >
      {children}
    </Box>
  );
}
