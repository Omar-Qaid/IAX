import React, {
  useMemo,
  useEffect,
  useCallback,
  memo,
  forwardRef,
  useImperativeHandle,
  useRef,
  useState,
} from 'react';
import { Box, IconButton, Link, Paper, Tooltip, Typography, useTheme, useMediaQuery } from '@mui/material';
import ViewSidebarOutlinedIcon from '@mui/icons-material/ViewSidebarOutlined';
import { Link as RouterLink } from 'react-router-dom';
import type { ColumnDef, DataGridProps, DataGridHandle, GridCellAddress } from './types';
import { DataGridToolbar } from './DataGridToolbar';
import { DataGridHeader } from './DataGridHeader';
import { DataGridBody, type GridBodyHandle } from './DataGridBody';
import { DataGridMobileBody } from './DataGridMobileBody';
import { GridSidebar } from './GridSidebar';
import { ScrollToTopButton } from './ScrollToTopButton';
import {
  useGridPersistence,
  useGridDataSource,
  useLoadMore,
  useGridSelection,
  useGridAutosize,
  useGridDataProcessing,
  useDataGridState,
  useGridLayout,
  useInlineEdit,
  NEW_ROW_ID,
} from './hooks';
import { computeFlexWidths, generateCSV, downloadFile } from './DataGridUtils';
import { useNotifications } from '@shared/hooks/useNotifications';
import { uiDensity } from '@shared/constants/uiDensity';
import { useAppTranslation } from '@core/localization/useAppTranslation';
const getDefaultRowId = <T,>(row: T) => (row as { id: string | number }).id;
function DataGridInternal<T>(
  {
    rows,
    columns: rawInitialColumns,
    getRowId = getDefaultRowId,
    loading = false,
    onRowClick,
    onRowDoubleClick,
    onEdit,
    onDelete,
    onDeleteSelected,
    onViewHistory,
    onShowAllFields,
    onBuild,
    rowHeight = uiDensity.gridRowHeight,
    headerHeight = uiDensity.gridHeaderHeight,
    height,
    onSelectionChange,
    selectionMode: initialSelectionMode,
    checkboxSelection,
    showColumnBorders: initialShowColumnBorders = true,
    showCellBorders: initialShowCellBorders = true,
    serverSide = false,
    onFetchRows,
    pageSize = 20,
    totalRowCount,
    hasMore: hasMoreProp,
    storageKey,
    onServerExport,
    onServerImport,
    onDownloadTemplate,
    masterForm = false,
    onRowSave,
    onNewRow,
    hideAddRowButton = false,
    onEditingChange,
    hideInlineEditActions = false,
    onRefresh,
    onValidate,
    onExecute,
    onPrint,
    onCloseForm,
    hideFilterRow = false,
    hideColumnMenu = false,
    hideToolbar = false,
    selectedIds: controlledSelectedIds,
    hideSidebar = false,
    hideSidebarTabs = false,
    hideFooter = false,
    mobilePresentation = 'cards',
  }: DataGridProps<T>,
  ref: React.Ref<DataGridHandle>
) {
  const { t } = useAppTranslation();
  const { notifyError } = useNotifications();
  const searchInputRef = useRef<HTMLInputElement | null>(null);
  const gridRootRef = useRef<HTMLDivElement | null>(null);
  const gridBodyRef = useRef<GridBodyHandle | null>(null);
  const mobileBodyRef = useRef<GridBodyHandle | null>(null);
  const pendingScrollTopRef = useRef<number | null>(null);
  const saveInFlightRef = useRef<Promise<boolean> | null>(null);
  const focusedCellRef = useRef({ r: 0, c: 0 });
  const initialColumns = useMemo(
    () => rawInitialColumns.map<ColumnDef<T>>((column) => {
      if (!column.getDetailHref) return column;
      return {
        ...column,
        renderCell: (params) => {
          const href = column.getDetailHref?.(params.row);
          const label = column.renderCell
            ? column.renderCell(params)
            : String((column.valueGetter ? column.valueGetter({ row: params.row }) : params.value) ?? '');
          if (!href || label == null || label === '') return label;
          return (
            <Link
              component={RouterLink}
              to={href}
              underline="hover"
              onClick={(event) => event.stopPropagation()}
              onDoubleClick={(event) => event.stopPropagation()}
              onKeyDown={(event) => event.stopPropagation()}
              sx={{ font: 'inherit', maxWidth: '100%', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}
            >
              {label}
            </Link>
          );
        },
      };
    }),
    [rawInitialColumns]
  );
  const [focusedRowId, setFocusedRowId] = useState<string | null>(null);
  const focusRequestRef = useRef(0);
  const resolveCellRef = useRef<
    ((address: GridCellAddress) => (() => Promise<void>) | undefined) | null
  >(null);

  // Layout & Scroll
  const {
    containerWidth,
    scrollbarWidth,
    scrollContainerRef,
    headerScrollRef,
    onScrollReset,
    handleBodyScroll,
  } = useGridLayout();

  // -- Persistence ----------------------------------------------------------
  const { initialState, persist, clear } = useGridPersistence<T>(storageKey, initialColumns);

  // -- Inline editing (masterForm mode only) --------------------------------
  const {
    editingRowId,
    editValues,
    isDirty,
    saving,
    startEdit,
    startAdd,
    updateField,
    setSaving,
    cancelEdit,
  } = useInlineEdit<T>();

  const handleInlineEdit = useCallback(
    (row: T) => {
      if (!masterForm) return;
      startEdit(getRowId(row), row);
    },
    [masterForm, startEdit, getRowId]
  );

  const handleAddRow = useCallback(() => {
    if (!masterForm) return;
    startAdd(onNewRow ? onNewRow() : {});
    focusedCellRef.current = { r: 0, c: 0 };
    requestAnimationFrame(() => {
      if (scrollContainerRef.current) scrollContainerRef.current.scrollTop = 0;
      gridBodyRef.current?.scrollToIndex(0);
    });
  }, [masterForm, startAdd, onNewRow, scrollContainerRef]);

  const handleRowDoubleClick = useCallback(
    (row: T) => {
      if (masterForm) {
        startEdit(getRowId(row), row);
      }
      onRowDoubleClick?.(row);
    },
    [masterForm, startEdit, getRowId, onRowDoubleClick]
  );

  const handleCancelEdit = useCallback((refocus = true) => {
    cancelEdit();
    if (!refocus) return;
    setTimeout(() => {
      const { r, c } = focusedCellRef.current;
      const cell = gridRootRef.current?.querySelector<HTMLElement>(
        `[data-row-index="${r}"][data-col-index="${c}"]`
      );
      if (cell) cell.focus();
    }, 100);
  }, [cancelEdit]);

  const handleSaveEdit = useCallback((refocus = true): Promise<boolean> => {
    if (saveInFlightRef.current) return saveInFlightRef.current;
    if (editingRowId == null) return Promise.resolve(true);

    const saveWork = (async () => {
      // D365 writes a record only when it was modified: leaving an untouched
      // line just ends editing instead of issuing an update.
      if (!onRowSave || !isDirty) {
        handleCancelEdit(refocus);
        return true;
      }
      const isNew = editingRowId === NEW_ROW_ID;
      const scrollTop = scrollContainerRef.current?.scrollTop ?? 0;
      setSaving(true);
      try {
        await onRowSave(editValues, isNew);
        pendingScrollTopRef.current = scrollTop;
        handleCancelEdit(refocus);
        return true;
      } catch (err: unknown) {
        const msg = err instanceof Error ? err.message : String(err);
        notifyError(msg);
        return false;
      } finally {
        setSaving(false);
      }
    })();

    saveInFlightRef.current = saveWork;
    void saveWork.finally(() => {
      if (saveInFlightRef.current === saveWork) saveInFlightRef.current = null;
    });
    return saveWork;
  }, [
    onRowSave,
    editingRowId,
    editValues,
    isDirty,
    handleCancelEdit,
    setSaving,
    notifyError,
    scrollContainerRef,
  ]);

  useEffect(() => {
    if (editingRowId == null || saving) return;

    const saveWhenClickingOutsideRow = (event: MouseEvent) => {
      const target = event.target;
      if (!(target instanceof HTMLElement)) return;
      if (
        target.closest('.MuiAutocomplete-popper, .MuiPopover-root, .MuiDialog-root, .MuiModal-root')
      )
        return;

      const clickedRow = target.closest<HTMLElement>('[data-row-id]');
      if (clickedRow && String(clickedRow.dataset.rowId) === String(editingRowId)) return;

      void handleSaveEdit();
    };

    document.addEventListener('mousedown', saveWhenClickingOutsideRow, true);
    return () => document.removeEventListener('mousedown', saveWhenClickingOutsideRow, true);
  }, [editingRowId, saving, handleSaveEdit]);

  // Remote saves commonly replace the rows array after the mutation. Restore
  // the exact viewport offset after that render instead of jumping to row 0.
  React.useLayoutEffect(() => {
    const savedScrollTop = pendingScrollTopRef.current;
    const container = scrollContainerRef.current;
    if (savedScrollTop == null || !container) return;

    container.scrollTop = savedScrollTop;
    const frame = requestAnimationFrame(() => {
      container.scrollTop = savedScrollTop;
      pendingScrollTopRef.current = null;
    });
    return () => cancelAnimationFrame(frame);
  }, [rows, editingRowId, scrollContainerRef]);

  useEffect(() => {
    onEditingChange?.(editingRowId != null);
  }, [editingRowId, onEditingChange]);

  // -- UI State -------------------------------------------------------------
  const {
    columns,
    setColumns,
    selectionMode,
    setSelectionMode,
    localRowHeight,
    setLocalRowHeight,
    showColumnBorders,
    setShowColumnBorders,
    showCellBorders,
    setShowCellBorders,
    isSidebarOpen,
    setIsSidebarOpen,
    activeSidebarTab,
    setActiveSidebarTab,
  } = useDataGridState({
    initialState,
    initialColumns,
    initialSelectionMode:
      checkboxSelection !== undefined
        ? checkboxSelection
          ? 'multiple'
          : 'single'
        : initialSelectionMode,
    initialShowColumnBorders,
    initialShowCellBorders,
    rowHeight,
  });

  // Data Source
  const {
    sortModel,
    setSortModel,
    filters,
    setFilters,
    globalSearch,
    setGlobalSearch,
    handleSort,
    loadNextPage,
  } = useGridDataSource({
    enabled: serverSide,
    initialSort: initialState.sortModel,
    initialFilters: initialState.filters,
    pageSize,
    loadedRowCount: rows.length,
    onFetchRows,
    onScrollReset,
  });

  const hasMore: boolean =
    hasMoreProp !== undefined
      ? hasMoreProp
      : totalRowCount !== undefined
        ? rows.length < totalRowCount
        : false;

  const processedRows = useGridDataProcessing({
    rows,
    columns,
    globalSearch,
    filters,
    sortModel,
    serverSide,
  });

  const hasActiveFilters = !serverSide && (globalSearch.length > 0 || filters.length > 0);

  const { onScroll } = useLoadMore({
    rowCount: rows.length,
    hasMore,
    loading,
    rowHeight: localRowHeight,
    loadNextPage,
    scrollContainerRef,
  });

  // -- Reset Scroll on filter change (local only) ---------------------------
  useEffect(() => {
    if (serverSide) return;
    onScrollReset();
  }, [serverSide, globalSearch, filters, onScrollReset]);

  // -- Sync Persistence (Debounced) ------------------------------------------
  useEffect(() => {
    const timer = setTimeout(() => {
      persist({
        columns: columns.map((c) => ({
          field: String(c.field),
          hidden: c.hidden,
          width: c.width,
          flex: c.flex,
          pinned: c.pinned,
        })),
        sortModel,
        filters,
        rowHeight: localRowHeight,
        showColumnBorders,
        showCellBorders,
        selectionMode,
      });
    }, 500);
    return () => clearTimeout(timer);
  }, [
    columns,
    sortModel,
    filters,
    localRowHeight,
    showColumnBorders,
    showCellBorders,
    selectionMode,
    persist,
  ]);

  // -- Selection ------------------------------------------------------------
  const { selectedIds, setSelectedIds, handleSelectionChange, handleSelectAll, allSelected } =
    useGridSelection({
      processedRows,
      getRowId,
      selectionMode,
      onSelectionChange: onSelectionChange
        ? (ids) => onSelectionChange(ids.map(String))
        : undefined,
    });

  useEffect(() => {
    if (controlledSelectedIds !== undefined) {
      setSelectedIds(controlledSelectedIds);
    }
  }, [controlledSelectedIds, setSelectedIds]);

  const getColCount = useCallback(() => {
    const visibleCols = columns.filter((c) => !c.hidden).length;
    return visibleCols + (selectionMode === 'multiple' ? 1 : 0);
  }, [columns, selectionMode]);

  const navigationColumns = useMemo(() => {
    const visible = columns.filter((column) => !column.hidden);
    return [
      ...visible.filter((column) => column.pinned === 'left'),
      ...visible.filter((column) => !column.pinned),
      ...visible.filter((column) => column.pinned === 'right'),
    ];
  }, [columns]);

  const focusCell = useCallback(
    async (r: number, c: number) => {
      const isAddingNewRow = masterForm && editingRowId === NEW_ROW_ID;
      const totalDisplayRows = processedRows.length + (isAddingNewRow ? 1 : 0);
      if (totalDisplayRows === 0) return;

      const maxRow = totalDisplayRows - 1;
      const maxCol = getColCount() - 1;
      const targetR = Math.max(0, Math.min(r, maxRow));
      const targetC = Math.max(0, Math.min(c, maxCol));

      focusedCellRef.current = { r: targetR, c: targetC };

      gridBodyRef.current?.scrollToIndex(targetR);

      if (selectionMode) {
        const targetsNewRow = isAddingNewRow && targetR === 0;
        const processedRowIndex = isAddingNewRow ? targetR - 1 : targetR;
        if (targetsNewRow) {
          if (selectedIds.length !== 1 || String(selectedIds[0]) !== String(NEW_ROW_ID)) {
            handleSelectionChange([NEW_ROW_ID]);
          }
        } else if (processedRowIndex >= 0 && processedRowIndex < processedRows.length) {
          const targetRowId = getRowId(processedRows[processedRowIndex]);
          if (selectedIds.length !== 1 || String(selectedIds[0]) !== String(targetRowId)) {
            handleSelectionChange([targetRowId]);
          }
        }
      }

      const request = ++focusRequestRef.current;
      // Visible cells can receive focus immediately. Only wait when a virtual
      // row or an editor disabled by a pending save is not ready yet.
      for (let frame = 0; frame < 12; frame++) {
        if (frame > 0) await new Promise<void>((resolve) => requestAnimationFrame(() => resolve()));
        if (request !== focusRequestRef.current || !gridRootRef.current) return;
        const cell = gridRootRef.current.querySelector<HTMLElement>(
          `[data-row-index="${targetR}"][data-col-index="${targetC}"]`
        );
        if (!cell) {
          // Clearing filters or changing header height can reset the viewport
          // after the initial scroll, especially when adding the last row.
          gridBodyRef.current?.scrollToIndex(targetR);
          continue;
        }
        const control =
          cell.querySelector<HTMLElement>(
            'input:not([type="hidden"]), textarea, [role="combobox"], [data-grid-cell-focus]'
          ) ?? cell;
        if (control.matches(':disabled, [aria-disabled="true"]')) continue;
        control.focus({ preventScroll: true });
        const viewport = scrollContainerRef.current;
        if (viewport && getComputedStyle(cell).position !== 'sticky') {
          const bounds = viewport.getBoundingClientRect();
          const rect = cell.getBoundingClientRect();
          const rtl = getComputedStyle(viewport).direction === 'rtl';
          const start = navigationColumns
            .filter((c) => c.pinned === 'left')
            .reduce((sum, c) => sum + (c.width ?? 150), 0);
          const end = navigationColumns
            .filter((c) => c.pinned === 'right')
            .reduce((sum, c) => sum + (c.width ?? 150), 0);
          const left = bounds.left + (rtl ? end : start);
          const right = bounds.right - (rtl ? start : end);
          if (rect.left < left) viewport.scrollLeft += rect.left - left;
          else if (rect.right > right) viewport.scrollLeft += rect.right - right;
          if (headerScrollRef.current) headerScrollRef.current.scrollLeft = viewport.scrollLeft;
        }
        return;
      }
    },
    [
      processedRows,
      getColCount,
      selectionMode,
      selectedIds,
      getRowId,
      handleSelectionChange,
      editingRowId,
      masterForm,
      navigationColumns,
      scrollContainerRef,
      headerScrollRef,
    ]
  );

  React.useLayoutEffect(() => {
    resolveCellRef.current = (address) => {
      const r = processedRows.findIndex((row) => String(getRowId(row)) === String(address.rowId));
      const c = navigationColumns.findIndex((column) => String(column.field) === address.field);
      return r >= 0 && c >= 0
        ? () => focusCell(r, c + (selectionMode === 'multiple' ? 1 : 0))
        : undefined;
    };
    return () => {
      resolveCellRef.current = null;
    };
  }, [processedRows, navigationColumns, selectionMode, getRowId, focusCell]);

  useImperativeHandle(ref, () => ({
    cancelPendingFocus: () => {
      focusRequestRef.current++;
    },
    focusCell: (r: number, c: number) => focusCell(r, c),
    getCellAddress: (r: number, c: number) => {
      const row = processedRows[r];
      const column = navigationColumns[c - (selectionMode === 'multiple' ? 1 : 0)];
      return row && column ? { rowId: getRowId(row), field: String(column.field) } : undefined;
    },
    focusRecordCell: async (address: GridCellAddress) => {
      const request = ++focusRequestRef.current;
      // A saved new record can arrive through the query cache after the
      // mutation resolves. Resolve against the latest committed row IDs.
      for (let frame = 0; frame < 12; frame++) {
        if (request !== focusRequestRef.current || !gridRootRef.current) return;
        const focus = resolveCellRef.current?.(address);
        if (focus) {
          await focus();
          return;
        }
        await new Promise<void>((resolve) => requestAnimationFrame(() => resolve()));
      }
    },
    clearFilters: () => {
      setFilters([]);
      setGlobalSearch('');
    },
    scrollToTop: () => {
      if (scrollContainerRef.current) scrollContainerRef.current.scrollTop = 0;
    },
    scrollToRow: (id: string | number) => {
      const index = processedRows.findIndex((row) => String(getRowId(row)) === String(id));
      if (index < 0) return false;
      // Desktop rows or mobile cards, whichever body is mounted.
      (gridBodyRef.current ?? mobileBodyRef.current)?.scrollToIndex(index);
      return true;
    },
    focusFilter: () =>
      gridRootRef.current?.querySelector<HTMLElement>('[data-grid-filter-field]')?.focus(),
    startAddRow: handleAddRow,
    startEditRow: (id: string | number) => {
      const rowToEdit = processedRows.find((r) => getRowId(r) === id);
      if (rowToEdit) {
        startEdit(id, rowToEdit);
      }
    },
    saveEdit: handleSaveEdit,
    cancelEdit: cancelEdit,
    toggleSidebar: (tab?: 'columns' | 'filters' | 'features') => {
      if (tab) {
        setActiveSidebarTab(tab);
        setIsSidebarOpen(true);
      } else {
        setIsSidebarOpen((prev) => !prev);
      }
    },
  }));

  // -- D365 line navigation in edit mode -------------------------------------
  const processedRowsRef = useRef(processedRows);
  React.useLayoutEffect(() => {
    processedRowsRef.current = processedRows;
  }, [processedRows]);
  const moveInFlightRef = useRef(false);

  /**
   * Leaves the line being edited for display row `targetR`: saves it (only when
   * changed), then opens the target line in edit mode at column `targetC`.
   * A failed save keeps focus on the current line, as D365 does.
   */
  const moveEditingTo = useCallback(
    async (targetR: number, targetC: number) => {
      if (moveInFlightRef.current || editingRowId == null) return;
      const adding = editingRowId === NEW_ROW_ID;
      const target = adding
        ? targetR === 0
          ? undefined
          : processedRows[targetR - 1]
        : processedRows[targetR];
      if (target === undefined) return; // first/last line: stay put
      const targetId = getRowId(target);
      const offset = selectionMode === 'multiple' ? 1 : 0;
      const column = navigationColumns[Math.max(0, targetC - offset)];
      if (!column) return;

      moveInFlightRef.current = true;
      try {
        if (!(await handleSaveEdit(false))) return;
        const nextFrame = () =>
          new Promise<void>((resolve) => requestAnimationFrame(() => resolve()));
        // A save can replace the rows array; resolve the target by ID on the latest rows.
        for (let frame = 0; frame < 12; frame++) {
          const row = processedRowsRef.current.find(
            (candidate) => String(getRowId(candidate)) === String(targetId)
          );
          if (row) {
            startEdit(targetId, row);
            break;
          }
          await nextFrame();
        }
        for (let frame = 0; frame < 12; frame++) {
          await nextFrame();
          const focus = resolveCellRef.current?.({ rowId: targetId, field: String(column.field) });
          if (focus) {
            await focus();
            return;
          }
        }
      } finally {
        moveInFlightRef.current = false;
      }
    },
    [
      editingRowId,
      processedRows,
      getRowId,
      selectionMode,
      navigationColumns,
      handleSaveEdit,
      startEdit,
    ]
  );

  // -- Keyboard Shortcuts (Global) ------------------------------------------
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // Autofill and IME composition can dispatch keydown without a key string.
      if (typeof e.key !== 'string') return;
      const activeEl = document.activeElement as HTMLElement | null;
      if (!activeEl || !gridRootRef.current?.contains(activeEl)) return;
      const isInputActive =
        activeEl &&
        (activeEl.tagName === 'INPUT' ||
          activeEl.tagName === 'TEXTAREA' ||
          activeEl.isContentEditable);
      const isGridCellFocused = activeEl && activeEl.hasAttribute('data-row-index');
      if (activeEl === gridRootRef.current && ['Enter', 'ArrowDown', 'F2'].includes(e.key)) {
        e.preventDefault();
        void focusCell(0, 0);
        return;
      }

      // -- Print --------------------------------------------------------
      if (e.ctrlKey && e.key.toLowerCase() === 'p') {
        e.preventDefault();
        if (onPrint) onPrint();
        else window.print();
        return;
      }

      // -- Advanced Filter ----------------------------------------------
      if (e.ctrlKey && e.shiftKey && e.key.toLowerCase() === 'f') {
        e.preventDefault();
        setActiveSidebarTab('filters');
        setIsSidebarOpen(true);
        return;
      }

      // -- Quick Filter / Find ------------------------------------------
      if (e.ctrlKey && !e.shiftKey && e.key.toLowerCase() === 'f') {
        e.preventDefault();
        searchInputRef.current?.focus();
        return;
      }

      // -- Select All (Only if grid is focused or not in an input) ------
      if (e.ctrlKey && e.key.toLowerCase() === 'a' && (!isInputActive || isGridCellFocused)) {
        e.preventDefault();
        handleSelectAll(!allSelected);
        return;
      }

      // -- Refresh / Validate / Execute ---------------------------------
      // Only swallow the browser default when the grid handles the key; otherwise
      // F5 inside a grid without onRefresh silently blocks page reload.
      if (e.key === 'F5') {
        if (onRefresh) {
          e.preventDefault();
          onRefresh();
        }
        return;
      }
      if (e.key === 'F7') {
        if (onValidate) {
          e.preventDefault();
          onValidate();
        }
        return;
      }
      if (e.key === 'F9' && !e.altKey) {
        if (onExecute) {
          e.preventDefault();
          onExecute();
        }
        return;
      }
      if (e.altKey && e.key === 'F4') {
        if (onCloseForm) {
          e.preventDefault();
          onCloseForm();
        }
        return;
      }

      // -- Navigation (Arrows, Home, End, Tab, PageUp, PageDown) --
      const cellContainer = isGridCellFocused ? activeEl : activeEl?.closest('[data-row-index]');
      // A control inside the cell (select, lookup, date picker) already used this key.
      if (cellContainer && !e.defaultPrevented) {
        // Checkboxes and radios are inputs but hold no caret: arrows must still navigate.
        const isInput =
          isInputActive &&
          activeEl?.tagName === 'INPUT' &&
          !['checkbox', 'radio'].includes((activeEl as HTMLInputElement).type);
        const inputEl = isInput ? (activeEl as HTMLInputElement) : null;
        const isTextArea = activeEl?.tagName === 'TEXTAREA';
        // An open lookup/select list owns the arrow keys.
        const isListOpen =
          activeEl?.getAttribute('aria-expanded') === 'true' ||
          activeEl?.closest('[aria-expanded="true"]') != null;
        const inEditingRow =
          editingRowId != null &&
          String(cellContainer.closest<HTMLElement>('[data-row-id]')?.dataset.rowId) ===
            String(editingRowId);
        const rtl = getComputedStyle(gridRootRef.current).direction === 'rtl';
        const caretAt = (edge: 'start' | 'end') => {
          if (!inputEl) return true;
          // Number/date inputs expose no caret; treat them as at the edge.
          if (inputEl.selectionStart === null) return true;
          return edge === 'start'
            ? inputEl.selectionStart === 0 && inputEl.selectionEnd === 0
            : inputEl.selectionStart === inputEl.value.length;
        };

        const cellR = parseInt(cellContainer.getAttribute('data-row-index') || '0', 10);
        const cellC = parseInt(cellContainer.getAttribute('data-col-index') || '0', 10);

        // When holding down a key (e.repeat is true), the DOM activeElement might lag
        // behind the actual intended focus due to the 50ms setTimeout in focusCell.
        // We trust our internal ref during repeats to allow smooth rapid movement!
        const r = e.repeat ? focusedCellRef.current.r : cellR;
        const c = e.repeat ? focusedCellRef.current.c : cellC;

        // Re-sync on fresh key press in case user clicked manually
        if (!e.repeat) {
          focusedCellRef.current = { r: cellR, c: cellC };
        }

        if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
          if (isTextArea || isListOpen || e.altKey) return;
          const target = r + (e.key === 'ArrowDown' ? 1 : -1);
          // D365: moving off a line in edit mode saves it and keeps editing the next line.
          if (inEditingRow) {
            e.preventDefault();
            void moveEditingTo(target, c);
            return;
          }
          if (isInput) return;
          e.preventDefault();
          focusCell(target, c);
          return;
        }
        if (e.key === 'ArrowRight' || e.key === 'ArrowLeft') {
          if (isListOpen) return;
          // Columns run toward inline-end, which is visually left in RTL.
          const forward = (e.key === 'ArrowRight') !== rtl;
          // Inside text the caret moves first.
          if (isInput && !caretAt(forward ? 'end' : 'start')) return;
          e.preventDefault();
          focusCell(r, c + (forward ? 1 : -1));
          return;
        }
        if (e.key === 'Tab') {
          if (isListOpen) return;
          e.preventDefault();
          const last = getColCount() - 1;
          let targetR = r;
          let targetC = c;
          if (e.shiftKey) {
            if (c > 0) targetC = c - 1;
            else [targetR, targetC] = [r - 1, last];
          } else if (c < last) targetC = c + 1;
          else [targetR, targetC] = [r + 1, 0];
          if (inEditingRow && targetR !== r) {
            void moveEditingTo(targetR, targetC);
            return;
          }
          focusCell(targetR, targetC);
          return;
        }
        if (e.key === 'Home') {
          if (isInput) return;
          e.preventDefault();
          focusCell(r, 0);
          return;
        }
        if (e.key === 'End') {
          if (isInput) return;
          e.preventDefault();
          focusCell(r, getColCount() - 1);
          return;
        }
        if (e.key === 'PageDown') {
          if (isInput) return;
          e.preventDefault();
          focusCell(r + 10, c);
          return;
        }
        if (e.key === 'PageUp') {
          if (isInput) return;
          e.preventDefault();
          focusCell(r - 10, c);
          return;
        }
      }

      // -- F2: Edit current field --
      if (e.key === 'F2') {
        if (masterForm && editingRowId == null && isGridCellFocused) {
          e.preventDefault();
          const { r, c } = focusedCellRef.current;
          const rowToEdit = processedRows[r];
          if (rowToEdit) {
            startEdit(getRowId(rowToEdit), rowToEdit);
            setTimeout(() => focusCell(r, c), 50);
          }
        }
        return;
      }

      // -- Clipboard --
      if (e.ctrlKey && e.key.toLowerCase() === 'c' && isGridCellFocused) {
        if (activeEl && activeEl.hasAttribute('data-row-index')) {
          const text = activeEl.innerText;
          navigator.clipboard.writeText(text).catch(() => {});
        }
      }

      if (!masterForm) return;

      // -- Master Form Shortcuts ----------------------------------------
      // Only trigger if we aren't typing in an input (except for save)
      if ((e.ctrlKey || e.altKey) && e.key.toLowerCase() === 's') {
        e.preventDefault();
        if (editingRowId != null) {
          handleSaveEdit();
        }
      } else if ((e.ctrlKey || e.altKey) && e.key.toLowerCase() === 'e') {
        e.preventDefault();
        if (editingRowId == null && selectedIds.length === 1) {
          const rowToEdit = processedRows.find((r) => getRowId(r) === selectedIds[0]);
          if (rowToEdit) {
            startEdit(selectedIds[0], rowToEdit);
          }
        }
      } else if ((e.ctrlKey || e.altKey) && e.key.toLowerCase() === 'n') {
        e.preventDefault();
        // D365 Alt+N while a line is open saves it first, then opens the new line.
        if (editingRowId == null) handleAddRow();
        else void handleSaveEdit(false).then((saved) => saved && handleAddRow());
      } else if (e.key === 'Escape') {
        // Esc that closes an open dropdown list must not also discard the line.
        if (e.defaultPrevented) return;
        if (editingRowId != null) {
          e.preventDefault();
          handleCancelEdit();
        }
      } else if (e.key === 'Enter') {
        // A closed dropdown opens its list on Enter; that must not also save the line.
        if (e.defaultPrevented) return;
        if (editingRowId != null) {
          if (activeEl?.tagName !== 'TEXTAREA') {
            e.preventDefault();
            handleSaveEdit();
          }
        } else if (isGridCellFocused && masterForm) {
          e.preventDefault();
          const { r, c } = focusedCellRef.current;
          const rowToEdit = processedRows[r];
          if (rowToEdit) {
            startEdit(getRowId(rowToEdit), rowToEdit);
            setTimeout(() => focusCell(r, c), 50);
          }
        }
      } else if (
        // Alt+F9 is the D365 "Delete record" shortcut and works from inside a field.
        (e.altKey && e.key === 'F9') ||
        ((e.key === 'Delete' || (e.ctrlKey && e.key.toLowerCase() === 'd')) && !isInputActive)
      ) {
        if (editingRowId == null && selectedIds.length > 0) {
          if (onDeleteSelected) {
            e.preventDefault();
            onDeleteSelected();
          } else if (onDelete && selectedIds.length === 1) {
            const rowToDelete = processedRows.find((r) => getRowId(r) === selectedIds[0]);
            if (rowToDelete) {
              e.preventDefault();
              onDelete(rowToDelete);
            }
          }
        }
      } else if (e.key === 'Insert' && !isInputActive) {
        if (editingRowId == null) {
          e.preventDefault();
          handleAddRow();
        }
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [
    masterForm,
    editingRowId,
    selectedIds,
    processedRows,
    getRowId,
    handleSaveEdit,
    startEdit,
    handleAddRow,
    cancelEdit,
    handleCancelEdit,
    onDeleteSelected,
    onDelete,
    handleSelectAll,
    allSelected,
    setActiveSidebarTab,
    setIsSidebarOpen,
    onRefresh,
    onValidate,
    onExecute,
    onPrint,
    onCloseForm,
    focusCell,
    getColCount,
    moveEditingTo,
  ]);

  const computedColumns = useMemo(
    () => (containerWidth > 0 ? computeFlexWidths(columns, containerWidth) : columns),
    [columns, containerWidth]
  );

  // -- Export ---------------------------------------------------------------
  const handleExport = useCallback(() => {
    const visibleCols = computedColumns.filter((c) => !c.hidden);

    if (serverSide && onServerExport) {
      void onServerExport({
        sort: sortModel,
        filters,
        globalSearch,
        columns: visibleCols.map((c) => ({ field: String(c.field), headerName: c.headerName })),
      });
      return;
    }

    const csv = generateCSV(processedRows, computedColumns);
    downloadFile(csv, 'export.csv', 'text/csv;charset=utf-8;');
  }, [
    computedColumns,
    processedRows,
    serverSide,
    onServerExport,
    sortModel,
    filters,
    globalSearch,
  ]);

  // -- Autosize -------------------------------------------------------------
  const {
    isAutosized,
    setIsAutosized,
    handleAutosizeAll,
    handleAutosizeColumn,
    handleUnAutosizeColumn,
  } = useGridAutosize({
    setColumns,
    processedRows,
    initialColumns,
  });

  const handleResetColumns = useCallback(() => {
    setColumns(initialColumns);
    setSortModel([]);
    setFilters([]);
    setLocalRowHeight(rowHeight);
    setShowColumnBorders(initialShowColumnBorders);
    setShowCellBorders(initialShowCellBorders);
    setIsAutosized(false);
    clear();
  }, [
    initialColumns,
    rowHeight,
    initialShowColumnBorders,
    initialShowCellBorders,
    clear,
    setSortModel,
    setFilters,
    setIsAutosized,
    setColumns,
    setLocalRowHeight,
    setShowColumnBorders,
    setShowCellBorders,
  ]);

  const theme = useTheme();
  const compactViewport = useMediaQuery(theme.breakpoints.down('md'));
  const isMobile = mobilePresentation === 'cards' && compactViewport;

  return (
    <Paper
      ref={gridRootRef}
      role="grid"
      onMouseDownCapture={() => {
        focusRequestRef.current++;
      }}
      onFocusCapture={(event) => {
        const row = (event.target as HTMLElement).closest<HTMLElement>('[data-row-id]');
        if (row) setFocusedRowId(row.dataset.rowId ?? null);
      }}
      tabIndex={0}
      aria-rowcount={processedRows.length}
      aria-colcount={
        computedColumns.filter((c) => !c.hidden).length + (selectionMode === 'multiple' ? 1 : 0)
      }
      aria-busy={loading}
      sx={{
        display: 'flex',
        flexDirection: 'column',
        height: height ?? '100%',
        minHeight: typeof height === 'number' ? height : undefined,
        overflow: 'hidden',
        borderRadius: 0,
        border: `1px solid ${theme.palette.mode === 'light' ? '#d6d6d6' : theme.palette.divider}`,
        boxShadow: 'none',
        bgcolor: 'background.paper',
      }}
    >
      {!hideToolbar && (
        <DataGridToolbar
          globalSearch={globalSearch}
          setGlobalSearch={setGlobalSearch}
          loadedRows={processedRows.length}
          totalRowCount={serverSide ? (totalRowCount ?? rows.length) : rows.length}
          filteredRows={serverSide ? rows.length : processedRows.length}
          serverSide={serverSide}
          masterForm={masterForm}
          onAddRow={handleAddRow}
          isEditing={editingRowId != null}
          hideAddRowButton={hideAddRowButton}
          searchInputRef={searchInputRef}
        />
      )}

      <Box sx={{ display: 'flex', flexGrow: 1, overflow: 'hidden' }}>
        <Box
          sx={{
            display: 'flex',
            flexDirection: 'column',
            flexGrow: 1,
            overflow: 'hidden',
            position: 'relative',
          }}
        >
          {!isMobile && (
            <Box
              ref={headerScrollRef}
              sx={{
                overflow: 'hidden',
                bgcolor: theme.palette.mode === 'light' ? '#ffffff' : theme.palette.action.hover,
                flexShrink: 0,
                borderBottom: `1px solid ${theme.palette.divider}`,
                paddingInlineEnd: `${scrollbarWidth || 0}px`,
              }}
            >
              <DataGridHeader
                columns={computedColumns}
                setColumns={setColumns}
                initialColumns={initialColumns}
                sortModel={sortModel}
                onSort={handleSort}
                filters={filters}
                setFilters={setFilters}
                onResetColumns={handleResetColumns}
                headerHeight={headerHeight}
                selectionMode={selectionMode}
                allSelected={allSelected}
                onSelectAll={handleSelectAll}
                showColumnBorders={showColumnBorders}
                hideFilterRow={hideFilterRow}
                hideColumnMenu={hideColumnMenu}
                onAutosizeColumn={handleAutosizeColumn}
                onAutosizeAll={handleAutosizeAll}
              />
            </Box>
          )}

          <Box
            ref={scrollContainerRef}
            onScroll={(e) => {
              handleBodyScroll(e);
              onScroll();
            }}
            sx={{
              flexGrow: 1,
              overflow: 'auto',
              bgcolor: 'background.paper',
              '&::-webkit-scrollbar': {
                width: uiDensity.scrollbarSize,
                height: uiDensity.scrollbarSize,
              },
              scrollbarColor: `${theme.palette.mode === 'light' ? '#b8b8b8' : '#5f6b7a'} transparent`,
              scrollbarWidth: 'thin',
              '&::-webkit-scrollbar-track': {
                bgcolor: theme.palette.mode === 'light' ? '#f7f7f7' : 'transparent',
              },
              '&::-webkit-scrollbar-thumb': {
                bgcolor: theme.palette.mode === 'light' ? '#b8b8b8' : '#5f6b7a',
                borderRadius: 4,
                border: '2px solid transparent',
                backgroundClip: 'content-box',
              },
              '&::-webkit-scrollbar-thumb:hover': {
                bgcolor: theme.palette.mode === 'light' ? '#8f8f8f' : '#8290a3',
              },
            }}
          >
            {isMobile ? (
              <DataGridMobileBody
                ref={mobileBodyRef}
                rows={processedRows}
                columns={computedColumns}
                getRowId={getRowId}
                scrollContainerRef={scrollContainerRef}
                loading={loading}
                hasMore={hasMore}
                hasActiveFilters={hasActiveFilters}
                onRowClick={onRowClick}
                onRowDoubleClick={handleRowDoubleClick}
                onEdit={onEdit}
                onDelete={onDelete}
                onViewHistory={onViewHistory}
                selectionMode={selectionMode}
                selectedIds={selectedIds}
                onSelectionChange={handleSelectionChange}
              />
            ) : (
              <DataGridBody<T>
                focusedRowId={focusedRowId}
                rows={processedRows}
                columns={computedColumns}
                rowHeight={localRowHeight}
                headerHeight={0}
                getRowId={getRowId}
                scrollContainerRef={scrollContainerRef}
                loading={loading}
                hasMore={hasMore}
                hasActiveFilters={hasActiveFilters}
                onRowClick={onRowClick}
                onRowDoubleClick={handleRowDoubleClick}
                onEdit={masterForm ? handleInlineEdit : onEdit}
                onDelete={onDelete}
                onViewHistory={onViewHistory}
                onShowAllFields={onShowAllFields}
                onBuild={onBuild}
                selectionMode={selectionMode}
                selectedIds={selectedIds}
                onSelectionChange={handleSelectionChange}
                showColumnBorders={showColumnBorders}
                showCellBorders={showCellBorders}
                masterForm={masterForm}
                editingRowId={editingRowId}
                editValues={editValues}
                saving={saving}
                onFieldChange={updateField}
                onSaveEdit={() => void handleSaveEdit()}
                onCancelEdit={() => handleCancelEdit()}
                hideInlineEditActions={hideInlineEditActions}
                ref={gridBodyRef}
              />
            )}
          </Box>
          <ScrollToTopButton scrollContainerRef={scrollContainerRef} />
        </Box>

        {!isMobile && !hideSidebar && (
          <GridSidebar
            hideTabs={hideSidebarTabs || (!hideFooter && !isSidebarOpen)}
            open={isSidebarOpen}
            onOpen={() => setIsSidebarOpen(true)}
            onClose={() => setIsSidebarOpen(false)}
            activeTab={activeSidebarTab}
            setActiveTab={setActiveSidebarTab}
            columns={columns}
            setColumns={setColumns}
            filters={filters}
            setFilters={setFilters}
            selectionMode={selectionMode}
            setSelectionMode={setSelectionMode}
            onExport={handleExport}
            onServerImport={onServerImport}
            onDownloadTemplate={onDownloadTemplate}
            onAutosizeAll={handleAutosizeAll}
            onAutosizeColumn={handleAutosizeColumn}
            onUnAutosizeColumn={handleUnAutosizeColumn}
            onResetColumns={handleResetColumns}
            isAutosized={isAutosized}
            rowHeight={localRowHeight}
            setRowHeight={setLocalRowHeight}
            showColumnBorders={showColumnBorders}
            setShowColumnBorders={setShowColumnBorders}
            showCellBorders={showCellBorders}
            setShowCellBorders={setShowCellBorders}
          />
        )}
      </Box>

      {/* Grid Footer */}
      {!hideFooter && (
        <Box
          sx={{
            height: uiDensity.gridFooterHeight,
            borderTop: `1px solid ${theme.palette.divider}`,
            bgcolor: theme.palette.mode === 'light' ? '#f8f9fa' : '#1a202c',
            display: 'flex',
            alignItems: 'center',
            px: 1.5,
            flexShrink: 0,
          }}
        >
          <Typography variant="caption" sx={{ color: 'text.secondary', fontWeight: 600 }}>
            {serverSide
              ? rows.length < (totalRowCount || rows.length)
                ? t('grid.rows_loaded', { loaded: rows.length, total: totalRowCount })
                : t('grid.totalRows', { count: rows.length })
              : processedRows.length < rows.length
                ? t('grid.rows_filtered', { filtered: processedRows.length, total: rows.length })
                : t('grid.totalRows', { count: rows.length })}
          </Typography>

          {selectedIds.length > 0 && (
            <Typography
              variant="caption"
              sx={{ color: 'primary.main', fontWeight: 600, marginInlineStart: 3 }}
            >
              {t('grid.selectedRows', { count: selectedIds.length })}
            </Typography>
          )}
          {!isMobile && !hideSidebar && (
            <Tooltip title={t(isSidebarOpen ? 'grid.hide_sidebar' : 'grid.show_sidebar')}>
              <IconButton
                size="small"
                aria-label={t(isSidebarOpen ? 'grid.hide_sidebar' : 'grid.show_sidebar')}
                aria-expanded={isSidebarOpen}
                onClick={() => setIsSidebarOpen((open) => !open)}
                sx={{ marginInlineStart: 'auto', marginInlineEnd: -1, p: 0.5 }}
              >
                <ViewSidebarOutlinedIcon sx={{ fontSize: 16 }} />
              </IconButton>
            </Tooltip>
          )}
        </Box>
      )}
    </Paper>
  );
}

export const DataGrid = memo(forwardRef(DataGridInternal)) as <T>(
  props: DataGridProps<T> & { ref?: React.Ref<DataGridHandle> }
) => ReturnType<typeof DataGridInternal>;

export const AppDataGrid = DataGrid;
