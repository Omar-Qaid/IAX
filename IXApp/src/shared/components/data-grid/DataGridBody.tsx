import React, { useRef, useState, useMemo, useCallback, memo } from 'react';
import {
  Box, Typography, CircularProgress, useTheme, Chip, TextField, Select, MenuItem
} from '@mui/material';
import {
  Inbox as InboxIcon,
  SearchOff as SearchOffIcon,
} from '@mui/icons-material';
import { NEW_ROW_ID } from './useInlineEdit';
import { useVirtualizer } from '@tanstack/react-virtual';
import { useTranslation } from 'react-i18next';
import type { ColumnDef } from './types';

export interface GridBodyHandle {
  scrollToIndex: (index: number) => void;
}

// New Modular Sub-components
import { GridRow } from './body/GridRow';
import { RowContextMenu } from './body/RowContextMenu';
import { SkeletonRows } from './body/SkeletonRows';
import { getNestedValue } from './DataGridUtils';
import { AppTextField } from '@shared/components/fields/AppTextField';
import { AppSelectField } from '@shared/components/fields/AppSelectField';
import { AppBooleanField } from '@shared/components/fields/AppBooleanField';
import { DataGridEmptyState } from './DataGridEmptyState';
import { DataGridLoadingState } from './DataGridLoadingState';

interface GridBodyProps<T> {
  rows: T[];
  columns: ColumnDef<T>[];
  rowHeight: number;
  headerHeight: number;
  getRowId: (row: T) => string | number;
  scrollContainerRef: React.RefObject<HTMLDivElement | null>;
  loading?: boolean;
  hasMore?: boolean;
  hasActiveFilters?: boolean;
  onRowClick?: (row: T) => void;
  onRowDoubleClick?: (row: T) => void;
  onEdit?: (row: T) => void;
  onDelete?: (row: T) => void;
  onViewHistory?: (row: T) => void;
  onShowAllFields?: (row: T) => void;
  onBuild?: (row: T) => void;
  selectionMode?: 'none' | 'single' | 'multiple';
  selectedIds?: (string | number)[];
  onSelectionChange?: (ids: (string | number)[]) => void;
  showColumnBorders?: boolean;
  showCellBorders?: boolean;
  // -- Master-form inline editing -----------------------------------------
  masterForm?: boolean;
  editingRowId?: string | number | null;
  editValues?: Partial<T>;
  saving?: boolean;
  onFieldChange?: (field: string, value: unknown) => void;
  onSaveEdit?: () => void;
  onCancelEdit?: () => void;
}

export const GridBodyInternal = React.forwardRef(function GridBodyInternal<T>({
  rows, columns, rowHeight, headerHeight, getRowId, scrollContainerRef,
  loading, hasMore, hasActiveFilters, onRowClick, onRowDoubleClick, onEdit, onDelete, onViewHistory, onShowAllFields, onBuild,
  selectionMode = 'single', selectedIds = [], onSelectionChange,
  showColumnBorders = false, showCellBorders = true,
  masterForm = false, editingRowId, editValues = {}, saving = false,
  onFieldChange, onSaveEdit, onCancelEdit,
}: GridBodyProps<T>, ref: React.Ref<GridBodyHandle>) {
  const { t } = useTranslation();
  const [contextMenu, setContextMenu] = useState<{ mouseX: number; mouseY: number; row: T | null } | null>(null);

  const selectedSet = useMemo(() => new Set(selectedIds), [selectedIds]);
  const selectedSetRef = useRef(selectedSet);
  selectedSetRef.current = selectedSet;

  const onToggleRow = useCallback((rowId: string | number) => {
    const set = selectedSetRef.current;
    if (selectionMode === 'single') {
      onSelectionChange?.(set.has(rowId) ? [] : [rowId]);
    } else if (selectionMode === 'multiple') {
      const next = new Set(set);
      if (next.has(rowId)) next.delete(rowId); else next.add(rowId);
      onSelectionChange?.(Array.from(next));
    }
  }, [selectionMode, onSelectionChange]);

  // When masterForm is adding a new row, append a synthetic row at the end.
  const displayRows = useMemo((): T[] => {
    if (masterForm && editingRowId === NEW_ROW_ID) {
      const newRow = { ...editValues, id: NEW_ROW_ID } as unknown as T;
      return [...rows, newRow];
    }
    return rows;
  }, [rows, masterForm, editingRowId, editValues]);

  const rowVirtualizer = useVirtualizer({
    count: displayRows.length,
    getScrollElement: () => scrollContainerRef.current,
    estimateSize: () => rowHeight,
    overscan: 10,
    scrollMargin: headerHeight,
  });

  React.useLayoutEffect(() => {
    rowVirtualizer.measure();
  }, [displayRows.length, headerHeight, rowVirtualizer]);

  const virtualItems = rowVirtualizer.getVirtualItems();

  React.useImperativeHandle(ref, () => ({
    scrollToIndex: (index: number) => {
      rowVirtualizer.scrollToIndex(index);
    }
  }), [rowVirtualizer]);

  const { visibleColumns, pinnedLeftCols, unpinnedCols, pinnedRightCols, pinnedLeftOffsets, pinnedRightOffsets, firstEditableField } = useMemo(() => {
    const visible = columns.filter(c => !c.hidden);
    const left = visible.filter(c => c.pinned === 'left');
    const center = visible.filter(c => !c.pinned);
    const right = visible.filter(c => c.pinned === 'right');

    const leftOffsets = left.reduce((acc, _col, i) => {
      acc.push(i === 0 ? 0 : acc[i - 1] + (left[i - 1].width || 150));
      return acc;
    }, [] as number[]);

    const rightOffsets = [...right].reverse().reduce((acc, _col, i) => {
      acc.push(i === 0 ? 0 : acc[i - 1] + (right[right.length - i].width || 150));
      return acc;
    }, [] as number[]).reverse();

    const firstEditableField = visible.find(c => c.editable)?.field;
    return { visibleColumns: visible, pinnedLeftCols: left, unpinnedCols: center, pinnedRightCols: right, pinnedLeftOffsets: leftOffsets, pinnedRightOffsets: rightOffsets, firstEditableField };
  }, [columns]);

  const handleContextMenu = useCallback((event: React.MouseEvent, row: T) => {
    event.preventDefault();
    setContextMenu({ mouseX: event.clientX, mouseY: event.clientY, row });
  }, []);

  const handleCopyRow = useCallback((row: T) => {
    const vals = visibleColumns.map(col => {
      const val = col.valueGetter ? col.valueGetter({ row }) : row[col.field as keyof T];
      return val != null ? String(val) : '';
    });
    navigator.clipboard.writeText(vals.join('\t')).catch(() => { });
  }, [visibleColumns]);

  const handleExportRow = useCallback((row: T) => {
    const header = visibleColumns.map(c => `"${c.headerName}"`).join(',');
    const values = visibleColumns.map(col => {
      const val = col.valueGetter ? col.valueGetter({ row }) : row[col.field as keyof T];
      const str = val != null ? String(val) : '';
      return `"${str.replace(/"/g, '""')}"`;
    }).join(',');
    const blob = new Blob([`${header}\n${values}`], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = 'row.csv'; a.click();
    URL.revokeObjectURL(url);
  }, [visibleColumns]);

  const renderCell = useCallback((row: T, col: ColumnDef<T>, rowIndex: number) => {
    const rowId = getRowId(row);
    const isEditingThisRow = masterForm && editingRowId != null && String(rowId) === String(editingRowId);

    if (isEditingThisRow && col.editable) {
      const fieldKey = col.field as string;
      const currentValue = (editValues as Record<string, unknown>)[fieldKey] ?? '';
      const isBoolCol = col.type === 'boolean';
      if (isBoolCol) {
        const boolVal = currentValue === true || String(currentValue) === 'true';
        return (
          <Box onClick={(e) => { e.stopPropagation(); onFieldChange?.(fieldKey, !boolVal); }}>
            <AppBooleanField
              name={fieldKey}
              value={boolVal}
              onChange={(v) => onFieldChange?.(fieldKey, v)}
              disabled={saving}
              variant="standard"
            />
          </Box>
        );
      }
      let finalValue = String(currentValue);
      if (col.type === 'date' && currentValue) {
        finalValue = String(currentValue).split('T')[0];
      }

      if (col.type === 'singleSelect' && col.valueOptions) {
        return (
          <Box onClick={(e) => e.stopPropagation()}>
            <AppSelectField
              name={fieldKey}
              variant="standard"
              value={finalValue}
              onChange={v => onFieldChange?.(fieldKey, v)}
              disabled={saving}
              options={col.valueOptions.map(opt => {
                const isObj = typeof opt === 'object' && opt !== null;
                return {
                  value: String(isObj ? (opt as any).value : opt),
                  label: String(isObj ? (opt as any).label : opt),
                };
              })}
            />
          </Box>
        );
      }

      return (
        <Box onClick={(e) => e.stopPropagation()}>
          <AppTextField
            name={fieldKey}
            type={col.type === 'date' ? 'date' : col.type === 'number' ? 'number' : 'text'}
            variant="standard"
            value={finalValue}
            onChange={v => onFieldChange?.(fieldKey, v)}
            disabled={saving}
            slotProps={{
              input: { autoFocus: col.field === firstEditableField }
            }}
          />
        </Box>
      );
    }

    if (col.renderCell) {
      const result = col.renderCell({ row, value: getNestedValue(row, col.field as string), rowIndex });
      if (typeof result === 'string' || typeof result === 'number') {
        return (
          <Typography variant="body2" sx={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', fontSize: '0.75rem', color: 'text.primary' }}>
            {result}
          </Typography>
        );
      }
      return result;
    }
    const val = col.valueGetter ? col.valueGetter({ row }) : getNestedValue(row, col.field as string);

    const isBool = col.type === 'boolean' || typeof val === 'boolean' || (typeof val === 'string' && (val === 'true' || val === 'false'));
    if (isBool) {
      const boolVal = val === true || String(val) === 'true' || val === 1;
      return (
        <Box sx={{ pointerEvents: 'none' }}>
          <AppBooleanField
            name={col.field as string}
            value={boolVal}
            readOnly
          />
        </Box>
      );
    }

    return (
      <Typography variant="body2" sx={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', fontSize: '0.75rem', color: 'text.primary' }}>
        {val != null ? String(val) : ''}
      </Typography>
    );
  }, [t, masterForm, editingRowId, editValues, saving, onFieldChange, getRowId, firstEditableField]);

  const theme = useTheme();


  if (loading && displayRows.length === 0) {
    return (
      <SkeletonRows
        rowHeight={rowHeight}
        pinnedLeftCols={pinnedLeftCols}
        unpinnedCols={unpinnedCols}
        pinnedRightCols={pinnedRightCols}
        visibleColumnsCount={visibleColumns.length}
      />
    );
  }

  if (!loading && displayRows.length === 0) {
    return <DataGridEmptyState hasActiveFilters={hasActiveFilters} />;
  }

  return (
    <>
      <Box sx={{ height: `${rowVirtualizer.getTotalSize()}px`, width: 'max-content', minWidth: '100%', position: 'relative' }}>
        {virtualItems.map(virtualRow => {
          const row = displayRows[virtualRow.index];
          // Guard against a transiently out-of-bounds index (rows can shrink between
          // the virtualizer measuring and this render), and against rows without an id.
          if (!row) return null;
          const rowId = getRowId(row) ?? virtualRow.index;
          const isEditingRow = masterForm && editingRowId != null && String(rowId) === String(editingRowId);
          return (
            <GridRow
              key={rowId}
              row={row}
              index={virtualRow.index}
              virtualRow={virtualRow}
              getRowId={getRowId}
              selectionMode={selectionMode}
              isSelected={selectedSet.has(rowId)}
              onToggleRow={onToggleRow}
              onRowClick={isEditingRow ? undefined : onRowClick}
              onRowDoubleClick={isEditingRow ? undefined : onRowDoubleClick}
              headerHeight={headerHeight}
              rowHeight={rowHeight}
              showColumnBorders={showColumnBorders}
              showCellBorders={showCellBorders}
              pinnedLeftCols={pinnedLeftCols}
              unpinnedCols={unpinnedCols}
              pinnedRightCols={pinnedRightCols}
              pinnedLeftOffsets={pinnedLeftOffsets}
              pinnedRightOffsets={pinnedRightOffsets}
              onContextMenu={handleContextMenu}
              renderCell={renderCell}
              isEditing={isEditingRow}
              saving={saving}
              onSave={onSaveEdit}
              onCancel={onCancelEdit}
            />
          );
        })}
      </Box>

      {hasMore && loading && <DataGridLoadingState />}

      <RowContextMenu
        contextMenu={contextMenu}
        onClose={() => setContextMenu(null)}
        onEdit={onEdit}
        onDelete={onDelete}
        onViewHistory={onViewHistory}
        onShowAllFields={onShowAllFields}
        onBuild={onBuild}
        onCopyRow={handleCopyRow}
        onExportRow={handleExportRow}
      />
    </>
  );
});

export const GridBody = memo(GridBodyInternal) as <T>(
    props: GridBodyProps<T> & { ref?: React.Ref<GridBodyHandle> }
) => React.ReactElement | null;
