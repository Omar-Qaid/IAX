import React, { useMemo, useEffect, useCallback, memo, forwardRef, useImperativeHandle, useRef } from 'react';
import { Box, Paper, Typography, useTheme, useMediaQuery } from '@mui/material';
import type { DataGridProps, DataGridHandle } from './types';
import { DataGridToolbar } from './DataGridToolbar';
import { DataGridHeader } from './DataGridHeader';
import { DataGridBody, type GridBodyHandle } from './DataGridBody';
import { DataGridMobileBody } from './DataGridMobileBody';
import { GridSidebar } from './GridSidebar';
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
function DataGridInternal<T>({
    rows,
    columns: rawInitialColumns,
    getRowId = (row: T) => (row as { id: string | number }).id,
    loading = false,
    onRowClick,
    onRowDoubleClick,
    onEdit,
    onDelete,
    onDeleteSelected,
    onViewHistory,
    onShowAllFields,
    onBuild,
    rowHeight = 36,
    headerHeight = 72,
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
    hideFooter = false,
}: DataGridProps<T>, ref: React.Ref<DataGridHandle>) {
    const { notifyError } = useNotifications();
    const searchInputRef = useRef<HTMLInputElement | null>(null);
    const gridRootRef = useRef<HTMLDivElement | null>(null);
    const gridBodyRef = useRef<GridBodyHandle | null>(null);
    const focusedCellRef = useRef({ r: 0, c: 0 });
    const focusTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);
    const initialColumns = rawInitialColumns;

    // â”€â”€ Persistence â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    const { initialState, persist, clear } = useGridPersistence<T>(storageKey, initialColumns);

    // â”€â”€ Inline editing (masterForm mode only) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    const { editingRowId, editValues, saving, startEdit, startAdd, updateField, setSaving, cancelEdit } = useInlineEdit<T>();

    const handleInlineEdit = useCallback((row: T) => {
        if (!masterForm) return;
        startEdit(getRowId(row), row);
    }, [masterForm, startEdit, getRowId]);

    const handleAddRow = useCallback(() => {
        if (!masterForm) return;
        startAdd(onNewRow ? onNewRow() : {});
    }, [masterForm, startAdd, onNewRow]);

    const handleRowDoubleClick = useCallback((row: T) => {
        if (masterForm) {
            startEdit(getRowId(row), row);
        }
        onRowDoubleClick?.(row);
    }, [masterForm, startEdit, getRowId, onRowDoubleClick]);

    const handleCancelEdit = useCallback(() => {
        cancelEdit();
        setTimeout(() => {
            const { r, c } = focusedCellRef.current;
            const cell = gridRootRef.current?.querySelector<HTMLElement>(`[data-row-index="${r}"][data-col-index="${c}"]`);
            if (cell) cell.focus();
        }, 100);
    }, [cancelEdit]);

    const handleSaveEdit = useCallback(async () => {
        if (!onRowSave) { handleCancelEdit(); return; }
        const isNew = editingRowId === NEW_ROW_ID;
        setSaving(true);
        try {
            await onRowSave(editValues, isNew);
            handleCancelEdit();
            if (isNew) {
                setTimeout(() => {
                    gridBodyRef.current?.scrollToIndex(999999);
                }, 300);
            }
        } catch (err: unknown) {
            const msg = err instanceof Error ? err.message : String(err);
            notifyError(msg);
        } finally {
            setSaving(false);
        }
    }, [onRowSave, editingRowId, editValues, handleCancelEdit, setSaving, notifyError]);


    useEffect(() => {
        onEditingChange?.(editingRowId != null);
    }, [editingRowId, onEditingChange]);

    // â”€â”€ UI State â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    const {
        columns, setColumns,
        selectionMode, setSelectionMode,
        localRowHeight, setLocalRowHeight,
        showColumnBorders, setShowColumnBorders,
        showCellBorders, setShowCellBorders,
        isSidebarOpen, setIsSidebarOpen,
        activeSidebarTab, setActiveSidebarTab
    } = useDataGridState({
        initialState,
        initialColumns,
        initialSelectionMode: checkboxSelection !== undefined ? (checkboxSelection ? 'multiple' : 'single') : initialSelectionMode,
        initialShowColumnBorders,
        initialShowCellBorders,
        rowHeight
    });

    // Layout & Scroll
    const {
        containerWidth,
        scrollbarWidth,
        scrollContainerRef,
        headerScrollRef,
        onScrollReset,
        handleBodyScroll
    } = useGridLayout();

    // Data Source
    const {
        sortModel, setSortModel,
        filters, setFilters,
        globalSearch, setGlobalSearch,
        handleSort,
        loadNextPage,
    } = useGridDataSource({
        enabled: serverSide,
        initialSort: initialState.sortModel,
        initialFilters: initialState.filters,
        pageSize,
        onFetchRows,
        onScrollReset,
    });

    const hasMore: boolean = hasMoreProp !== undefined
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

    useImperativeHandle(ref, () => ({
        startAddRow: handleAddRow,
        startEditRow: (id: string | number) => {
            const rowToEdit = processedRows.find(r => getRowId(r) === id);
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
                setIsSidebarOpen(prev => !prev);
            }
        }
    }));

    const hasActiveFilters = !serverSide && (globalSearch.length > 0 || filters.length > 0);

    const { onScroll } = useLoadMore({
        rowCount: rows.length,
        hasMore,
        loading,
        rowHeight: localRowHeight,
        loadNextPage,
        scrollContainerRef,
    });

    // â”€â”€ Reset Scroll on filter change (local only) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    useEffect(() => {
        if (serverSide) return;
        onScrollReset();
    }, [serverSide, globalSearch, filters, onScrollReset]);

    // â”€â”€ Sync Persistence (Debounced) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    useEffect(() => {
        const timer = setTimeout(() => {
            persist({
                columns: columns.map(c => ({
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
    }, [columns, sortModel, filters, localRowHeight, showColumnBorders, showCellBorders, selectionMode, persist]);

    // â”€â”€ Selection â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    const {
        selectedIds,
        setSelectedIds,
        handleSelectionChange,
        handleSelectAll,
        allSelected,
    } = useGridSelection({
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
        const visibleCols = columns.filter(c => !c.hidden).length;
        return visibleCols + (selectionMode === 'multiple' ? 1 : 0);
    }, [columns, selectionMode]);

    const focusCell = useCallback((r: number, c: number) => {
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
            if (targetR < processedRows.length) {
                const targetRowId = getRowId(processedRows[targetR]);
                handleSelectionChange([targetRowId]);
            } else {
                handleSelectionChange([NEW_ROW_ID]);
            }
        }

        if (focusTimeoutRef.current) clearTimeout(focusTimeoutRef.current);
        focusTimeoutRef.current = setTimeout(() => {
            const cell = gridRootRef.current?.querySelector<HTMLElement>(`[data-row-index="${targetR}"][data-col-index="${targetC}"]`);
            if (cell) {
                const input = cell.querySelector('input, textarea') as HTMLElement;
                if (input) input.focus();
                else cell.focus();
            }
        }, 30);
    }, [processedRows, getColCount, selectionMode, getRowId, handleSelectionChange, editingRowId, masterForm]);

    // â”€â”€ Keyboard Shortcuts (Global) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    useEffect(() => {
        const handleKeyDown = (e: KeyboardEvent) => {
            const activeEl = document.activeElement as HTMLElement | null;
            if (!activeEl || !gridRootRef.current?.contains(activeEl)) return;
            const isInputActive = activeEl && (activeEl.tagName === 'INPUT' || activeEl.tagName === 'TEXTAREA' || activeEl.isContentEditable);
            const isGridCellFocused = activeEl && activeEl.hasAttribute('data-row-index');

            // â”€â”€ Print â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (e.ctrlKey && e.key.toLowerCase() === 'p') {
                e.preventDefault();
                if (onPrint) onPrint();
                else window.print();
                return;
            }

            // â”€â”€ Advanced Filter â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (e.ctrlKey && e.shiftKey && e.key.toLowerCase() === 'f') {
                e.preventDefault();
                setActiveSidebarTab('filters');
                setIsSidebarOpen(true);
                return;
            }

            // â”€â”€ Quick Filter / Find â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (e.ctrlKey && !e.shiftKey && e.key.toLowerCase() === 'f') {
                e.preventDefault();
                searchInputRef.current?.focus();
                return;
            }

            // â”€â”€ Select All (Only if grid is focused or not in an input) â”€â”€â”€â”€â”€â”€
            if (e.ctrlKey && e.key.toLowerCase() === 'a' && (!isInputActive || isGridCellFocused)) {
                e.preventDefault();
                handleSelectAll(!allSelected);
                return;
            }

            // â”€â”€ Refresh / Validate / Execute â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (e.key === 'F5') {
                e.preventDefault();
                onRefresh?.();
                return;
            }
            if (e.key === 'F7') {
                e.preventDefault();
                onValidate?.();
                return;
            }
            if (e.key === 'F9') {
                e.preventDefault();
                onExecute?.();
                return;
            }
            if (e.altKey && e.key === 'F4') {
                if (onCloseForm) {
                    e.preventDefault();
                    onCloseForm();
                }
                return;
            }

            // â”€â”€ Navigation (Arrows, Home, End, Tab, PageUp, PageDown) â”€â”€
            const cellContainer = isGridCellFocused ? activeEl : activeEl?.closest('[data-row-index]');
            if (cellContainer) {
                const isInput = isInputActive && activeEl?.tagName === 'INPUT';
                const inputEl = isInput ? activeEl as HTMLInputElement : null;

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

                if (e.key === 'ArrowDown') {
                    if (isInput) return;
                    e.preventDefault();
                    focusCell(r + 1, c);
                    return;
                }
                if (e.key === 'ArrowUp') {
                    if (isInput) return;
                    e.preventDefault();
                    focusCell(r - 1, c);
                    return;
                }
                if (e.key === 'ArrowRight') {
                    if (isInput && inputEl && inputEl.selectionStart !== null && inputEl.selectionStart !== inputEl.value.length) return;
                    e.preventDefault();
                    focusCell(r, c + 1);
                    return;
                }
                if (e.key === 'ArrowLeft') {
                    if (isInput && inputEl && inputEl.selectionStart !== null && inputEl.selectionStart !== 0) return;
                    e.preventDefault();
                    focusCell(r, c - 1);
                    return;
                }
                if (e.key === 'Tab') {
                    e.preventDefault();
                    if (e.shiftKey) {
                        if (c > 0) focusCell(r, c - 1);
                        else focusCell(r - 1, getColCount() - 1);
                    } else {
                        if (c < getColCount() - 1) focusCell(r, c + 1);
                        else focusCell(r + 1, 0);
                    }
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

            // â”€â”€ F2: Edit current field â”€â”€
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

            // â”€â”€ Clipboard â”€â”€
            if (e.ctrlKey && e.key.toLowerCase() === 'c' && isGridCellFocused) {
                if (activeEl && activeEl.hasAttribute('data-row-index')) {
                    const text = activeEl.innerText;
                    navigator.clipboard.writeText(text).catch(() => { });
                }
            }

            if (!masterForm) return;

            // â”€â”€ Master Form Shortcuts â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            // Only trigger if we aren't typing in an input (except for save)
            if ((e.ctrlKey || e.altKey) && e.key.toLowerCase() === 's') {
                e.preventDefault();
                if (editingRowId != null) {
                    handleSaveEdit();
                }
            } else if ((e.ctrlKey || e.altKey) && e.key.toLowerCase() === 'e') {
                e.preventDefault();
                if (editingRowId == null && selectedIds.length === 1) {
                    const rowToEdit = processedRows.find(r => getRowId(r) === selectedIds[0]);
                    if (rowToEdit) {
                        startEdit(selectedIds[0], rowToEdit);
                    }
                }
            } else if ((e.ctrlKey || e.altKey) && e.key.toLowerCase() === 'n') {
                e.preventDefault();
                if (editingRowId == null) {
                    handleAddRow();
                }
            } else if (e.key === 'Escape') {
                if (editingRowId != null) {
                    e.preventDefault();
                    handleCancelEdit();
                }
            } else if (e.key === 'Enter') {
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
            } else if ((e.key === 'Delete' || (e.ctrlKey && e.key.toLowerCase() === 'd')) && !isInputActive) {
                if (editingRowId == null && selectedIds.length > 0) {
                    if (onDeleteSelected) {
                        e.preventDefault();
                        onDeleteSelected();
                    } else if (onDelete && selectedIds.length === 1) {
                        const rowToDelete = processedRows.find(r => getRowId(r) === selectedIds[0]);
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
    }, [masterForm, editingRowId, selectedIds, processedRows, getRowId, handleSaveEdit, startEdit, handleAddRow, cancelEdit, handleCancelEdit, onDeleteSelected, onDelete, handleSelectAll, allSelected, setActiveSidebarTab, setIsSidebarOpen, onRefresh, onValidate, onExecute, onPrint, onCloseForm, focusCell, getColCount]);

    const computedColumns = useMemo(
        () => containerWidth > 0 ? computeFlexWidths(columns, containerWidth) : columns,
        [columns, containerWidth],
    );

    // â”€â”€ Export â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    const handleExport = useCallback(() => {
        const visibleCols = computedColumns.filter(c => !c.hidden);

        if (serverSide && onServerExport) {
            void onServerExport({
                sort: sortModel,
                filters,
                globalSearch,
                columns: visibleCols.map(c => ({ field: String(c.field), headerName: c.headerName })),
            });
            return;
        }

        const csv = generateCSV(processedRows, computedColumns);
        downloadFile(csv, 'export.csv', 'text/csv;charset=utf-8;');
    }, [computedColumns, processedRows, serverSide, onServerExport, sortModel, filters, globalSearch]);

    // â”€â”€ Autosize â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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
    }, [initialColumns, rowHeight, initialShowColumnBorders, initialShowCellBorders, clear, setSortModel, setFilters, setIsAutosized, setColumns, setLocalRowHeight, setShowColumnBorders, setShowCellBorders]);

    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down('md'));

    return (
        <Paper
          ref={gridRootRef}
          role="grid"
          tabIndex={0}
          aria-rowcount={processedRows.length}
          aria-colcount={computedColumns.length + (selectionMode === 'multiple' ? 1 : 0)}
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
            bgcolor: 'background.paper'
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
                <Box sx={{ display: 'flex', flexDirection: 'column', flexGrow: 1, overflow: 'hidden' }}>
                    {!isMobile && (
                        <Box
                            ref={headerScrollRef}
                            sx={{
                                overflow: 'hidden',
                                bgcolor: theme.palette.action.hover,
                                flexShrink: 0,
                                borderBottom: `1px solid ${theme.palette.divider}`,
                                pr: `${scrollbarWidth || 0}px`,
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
                            '&::-webkit-scrollbar': { width: 8, height: 8 },
                            scrollbarColor: `${theme.palette.mode === 'light' ? '#b8b8b8' : '#5f6b7a'} transparent`,
                            scrollbarWidth: 'thin',
                            '&::-webkit-scrollbar-track': { bgcolor: theme.palette.mode === 'light' ? '#f7f7f7' : 'transparent' },
                            '&::-webkit-scrollbar-thumb': {
                                bgcolor: theme.palette.mode === 'light' ? '#b8b8b8' : '#5f6b7a',
                                borderRadius: 4,
                                border: '2px solid transparent',
                                backgroundClip: 'content-box'
                            },
                            '&::-webkit-scrollbar-thumb:hover': {
                                bgcolor: theme.palette.mode === 'light' ? '#8f8f8f' : '#8290a3'
                            },
                        }}
                    >
                        {isMobile ? (
                            <DataGridMobileBody
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
                                onSaveEdit={handleSaveEdit}
                                onCancelEdit={handleCancelEdit}
                                hideInlineEditActions={hideInlineEditActions}
                                ref={gridBodyRef}
                            />
                        )}
                    </Box>
                </Box>

                {!isMobile && !hideSidebar && (
                    <GridSidebar
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
                <Box sx={{
                    height: 32,
                    borderTop: `1px solid ${theme.palette.divider}`,
                    bgcolor: theme.palette.mode === 'light' ? '#f8f9fa' : '#1a202c',
                    display: 'flex',
                    alignItems: 'center',
                    px: 1.5,
                    flexShrink: 0,
                }}>
                    <Typography variant="caption" sx={{ color: 'text.secondary', fontWeight: 600 }}>
                        {serverSide
                            ? (rows.length < (totalRowCount || rows.length)
                                ? `Loaded ${rows.length} of ${totalRowCount}`
                                : `Total: ${rows.length} rows`)
                            : (processedRows.length < rows.length
                                ? `Filtered ${processedRows.length} of ${rows.length} rows`
                                : `Total: ${rows.length} rows`)
                        }
                    </Typography>

                    {selectedIds.length > 0 && (
                        <Typography variant="caption" sx={{ color: 'primary.main', fontWeight: 600, ml: 3 }}>
                            {selectedIds.length} selected
                        </Typography>
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
