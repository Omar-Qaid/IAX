import { uiDensity } from '@shared/constants/uiDensity';
import React, { useMemo, useCallback, useImperativeHandle, useRef, memo, forwardRef } from 'react';
import {
  Box,
  Typography,
  CircularProgress,
  useTheme,
  Card,
  IconButton,
  Divider,
  Checkbox,
} from '@mui/material';
import SearchOffIcon from '@mui/icons-material/SearchOff';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import HistoryIcon from '@mui/icons-material/History';
import { useVirtualizer } from '@tanstack/react-virtual';
import { useTranslation } from 'react-i18next';
import type { ColumnDef } from './types';
import type { GridBodyHandle } from './DataGridBody';
import { getNestedValue } from './DataGridUtils';
import { AppBooleanField } from '@shared/components/fields/AppBooleanField';
import { EmptyDataWatermark } from '@shared/components/feedback/EmptyDataWatermark';
import { hasLookupValue } from '@shared/components/lookups/lookupValueStyle';

interface MobileGridBodyProps<T> {
  rows: T[];
  columns: ColumnDef<T>[];
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
  selectionMode?: 'none' | 'single' | 'multiple';
  selectedIds?: (string | number)[];
  onSelectionChange?: (ids: (string | number)[]) => void;
}

/** Cards rendered before the list is measured (first paint, jsdom). */
const UNMEASURED_CARD_COUNT = 10;
const ESTIMATED_CARD_HEIGHT = 180;

function DataGridMobileBodyInternal<T>(
  {
    rows,
    columns,
    getRowId,
    scrollContainerRef,
    loading,
    hasMore,
    hasActiveFilters,
    onRowClick,
    onRowDoubleClick,
    onEdit,
    onDelete,
    onViewHistory,
    selectionMode = 'single',
    selectedIds = [],
    onSelectionChange,
  }: MobileGridBodyProps<T>,
  ref: React.Ref<GridBodyHandle>
) {
  const { t } = useTranslation();
  const theme = useTheme();

  // IDs are compared as strings, as on desktop: the grid reports selection as
  // strings, so a numeric row ID would otherwise never show as selected.
  const selectedSet = useMemo(() => new Set(selectedIds.map(String)), [selectedIds]);
  const selectedIdsRef = useRef(selectedIds);
  selectedIdsRef.current = selectedIds;

  const onToggleRow = useCallback(
    (rowId: string | number, event?: React.MouseEvent) => {
      if (event) {
        event.stopPropagation();
      }
      const key = String(rowId);
      const current = selectedIdsRef.current;
      const isSelected = current.some((id) => String(id) === key);
      if (selectionMode === 'single') {
        onSelectionChange?.(isSelected ? [] : [rowId]);
      } else if (selectionMode === 'multiple') {
        onSelectionChange?.(
          isSelected ? current.filter((id) => String(id) !== key) : [...current, rowId]
        );
      }
    },
    [selectionMode, onSelectionChange]
  );

  // Use a dynamic virtualizer for cards which vary in height
  // TanStack Virtual returns mutable functions by design; React Compiler safely skips this component.
  // eslint-disable-next-line react-hooks/incompatible-library
  const rowVirtualizer = useVirtualizer({
    count: rows.length,
    getScrollElement: () => scrollContainerRef.current,
    estimateSize: () => ESTIMATED_CARD_HEIGHT,
    overscan: 5,
  });

  useImperativeHandle(
    ref,
    () => ({
      scrollToIndex: (index: number) => rowVirtualizer.scrollToIndex(index, { align: 'start' }),
    }),
    [rowVirtualizer]
  );

  // Card layout is the same for every row: work it out once, not per card per render.
  const { titleCol, statusCol, detailCols } = useMemo(() => {
    const visible = columns.filter((c) => !c.hidden);
    const title = visible[0] ?? null;
    const status = visible.find(
      (c) => c !== title && (c.type === 'boolean' || c.field === 'status' || c.field === 'isActive')
    );
    return {
      titleCol: title,
      statusCol: status,
      detailCols: visible.filter((c) => c !== title && c !== status),
    };
  }, [columns]);

  const renderCell = useCallback((row: T, col: ColumnDef<T>, rowIndex: number) => {
    if (col.renderCell) {
      return col.renderCell({ row, value: getNestedValue(row, col.field as string), rowIndex });
    }
    const val = col.valueGetter
      ? col.valueGetter({ row })
      : getNestedValue(row, col.field as string);
    if (col.type === 'singleSelect' && col.valueOptions) {
      const selectedOption = col.valueOptions.find((option) => {
        const optionValue =
          typeof option === 'object' && option !== null && 'value' in option
            ? option.value
            : option;
        return String(optionValue) === String(val);
      });
      const label: React.ReactNode =
        typeof selectedOption === 'object' && selectedOption !== null && 'label' in selectedOption
          ? (selectedOption.label as React.ReactNode)
          : (selectedOption as React.ReactNode);
      return (
        <Typography
          variant="body2"
          sx={{
            width: '100%',
            color: hasLookupValue(val) ? 'primary.main' : 'text.primary',
            fontWeight: 500,
            fontSize: uiDensity.gridFontSize,
            wordBreak: 'break-word',
            textAlign: 'center',
          }}
        >
          {label ?? (val != null ? String(val) : '-')}
        </Typography>
      );
    }
    const isBool =
      col.type === 'boolean' ||
      typeof val === 'boolean' ||
      (typeof val === 'string' && (val === 'true' || val === 'false'));
    if (isBool) {
      const boolVal = val === true || String(val) === 'true' || val === 1;
      return (
        <Box sx={{ pointerEvents: 'none' }}>
          <AppBooleanField name={col.field as string} value={boolVal} readOnly />
        </Box>
      );
    }
    return (
      <Typography
        variant="body2"
        sx={{
          width: '100%',
          color: 'text.primary',
          fontWeight: 500,
          fontSize: uiDensity.gridFontSize,
          wordBreak: 'break-word',
          textAlign: 'center',
        }}
      >
        {val != null ? String(val) : '-'}
      </Typography>
    );
  }, []);

  if (loading && rows.length === 0) {
    return (
      <Box sx={{ p: 2 }}>
        {[1, 2, 3].map((i) => (
          <Card
            key={i}
            sx={{
              mb: 2,
              height: 160,
              bgcolor: theme.palette.action.hover,
              borderRadius: 2,
              boxShadow: 'none',
            }}
          />
        ))}
      </Box>
    );
  }

  if (!loading && rows.length === 0) {
    if (!hasActiveFilters) {
      return (
        <Box sx={{ flex: 1, minHeight: 0, bgcolor: 'background.paper' }}>
          <EmptyDataWatermark />
        </Box>
      );
    }
    return (
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          py: 10,
          px: 4,
          userSelect: 'none',
        }}
      >
        <Box
          sx={{
            width: 72,
            height: 72,
            borderRadius: '50%',
            bgcolor: theme.palette.action.hover,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            mb: 2,
          }}
        >
          <SearchOffIcon sx={{ fontSize: 34, color: theme.palette.text.disabled }} />
        </Box>
        <Typography variant="subtitle1" sx={{ fontWeight: 600, color: 'text.secondary', mb: 0.5 }}>
          {t('grid.no_records')}
        </Typography>
        <Typography
          variant="body2"
          sx={{ color: 'text.disabled', textAlign: 'center', maxWidth: 280, lineHeight: 1.6 }}
        >
          {t('grid.no_results_msg')}
        </Typography>
      </Box>
    );
  }

  const measuredItems = rowVirtualizer.getVirtualItems();
  // Before the scroll element is measured the virtualizer returns nothing; render
  // the first cards so the list never paints empty (desktop rows do the same).
  const virtualItems =
    measuredItems.length > 0
      ? measuredItems
      : rows.slice(0, UNMEASURED_CARD_COUNT).map((_, index) => ({
          index,
          start: index * ESTIMATED_CARD_HEIGHT,
          key: index,
        }));

  return (
    <>
      <Box
        sx={{
          height: `${Math.max(rowVirtualizer.getTotalSize(), virtualItems.length * ESTIMATED_CARD_HEIGHT)}px`,
          width: '100%',
          position: 'relative',
        }}
      >
        {virtualItems.map((virtualRow) => {
          const row = rows[virtualRow.index];
          // Rows can shrink between measuring and rendering (filtering, refresh).
          if (row === undefined) return null;
          const rowId = getRowId(row) ?? virtualRow.index;
          const isSelected = selectedSet.has(String(rowId));

          return (
            <Box
              key={rowId}
              style={{
                position: 'absolute',
                top: 0,
                insetInlineStart: 0,
                width: '100%',
                transform: `translateY(${virtualRow.start}px)`,
                padding: '8px 12px',
              }}
              ref={rowVirtualizer.measureElement}
              data-index={virtualRow.index}
              data-row-id={String(rowId)}
            >
              <Card
                onClick={() => {
                  if (selectionMode !== 'none') onToggleRow(rowId);
                  onRowClick?.(row);
                }}
                onDoubleClick={() => onRowDoubleClick?.(row)}
                sx={{
                  p: 2,
                  borderRadius: 3,
                  boxShadow: '0 2px 8px rgba(0,0,0,0.06)',
                  border: '1px solid',
                  borderColor: isSelected ? 'primary.main' : 'divider',
                  bgcolor: isSelected ? 'primary.50' : 'background.paper',
                  cursor: onRowClick || selectionMode !== 'none' ? 'pointer' : 'default',
                  transition: 'all 0.2s',
                  position: 'relative',
                  textAlign: 'center',
                }}
              >
                {/* Selection Checkbox */}
                {selectionMode !== 'none' && (
                  <Box sx={{ position: 'absolute', top: 8, insetInlineEnd: 8, zIndex: 2 }}>
                    <Checkbox
                      aria-label={t('grid.select_row', { id: rowId })}
                      checked={isSelected}
                      onClick={(e) => onToggleRow(rowId, e)}
                      size="small"
                    />
                  </Box>
                )}

                {/* Header */}
                <Box
                  sx={{
                    display: 'flex',
                    justifyContent: 'center',
                    alignItems: 'center',
                    mb: 2,
                    paddingInlineEnd: selectionMode !== 'none' ? 4 : 0,
                  }}
                >
                  <Box
                    sx={{
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      gap: 1,
                      flexWrap: 'wrap',
                      width: '100%',
                    }}
                  >
                    {titleCol && (
                      <>
                        <Typography
                          sx={{
                            fontWeight: 700,
                            color: 'primary.main',
                            fontSize: uiDensity.gridFontSize,
                          }}
                        >
                          {t(titleCol.headerName)}:
                        </Typography>
                        <Box
                          sx={{
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            '& .MuiTypography-root': {
                              fontWeight: 600,
                              color: 'text.primary',
                              fontSize: uiDensity.gridFontSize,
                              margin: 0,
                            },
                          }}
                        >
                          {renderCell(row, titleCol, virtualRow.index)}
                        </Box>
                      </>
                    )}
                  </Box>
                  {statusCol && (
                    <Box sx={{ marginInlineStart: 2, flexShrink: 0 }}>
                      {renderCell(row, statusCol, virtualRow.index)}
                    </Box>
                  )}
                </Box>

                {/* Details */}
                <Box
                  sx={{
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    gap: 1,
                    mb: 2,
                  }}
                >
                  {detailCols.map((col) => (
                    <Box
                      key={col.field as string}
                      sx={{
                        display: 'flex',
                        flexDirection: 'column',
                        alignItems: 'center',
                        justifyContent: 'center',
                        gap: 0.25,
                        width: '100%',
                      }}
                    >
                      <Typography
                        variant="body2"
                        sx={{ color: 'text.secondary', minWidth: '80px', flexShrink: 0, pt: '1px' }}
                      >
                        {t(col.headerName)}:
                      </Typography>
                      <Box
                        sx={{
                          flexGrow: 1,
                          width: '100%',
                          textAlign: 'center',
                          '& .MuiTypography-root': { color: 'text.primary', fontWeight: 500 },
                        }}
                      >
                        {renderCell(row, col, virtualRow.index)}
                      </Box>
                    </Box>
                  ))}
                </Box>

                {/* Actions */}
                {(onEdit || onDelete || onViewHistory) && (
                  <>
                    <Divider sx={{ mx: -2, mb: 1 }} />
                    <Box sx={{ display: 'flex', justifyContent: 'center', gap: 0.5 }}>
                      {onViewHistory && (
                        <IconButton
                          aria-label={t('common.view_history')}
                          size="small"
                          onClick={(e) => {
                            e.stopPropagation();
                            onViewHistory(row);
                          }}
                          sx={{
                            color: 'text.secondary',
                            border: '1px solid',
                            borderColor: 'divider',
                            borderRadius: 2,
                          }}
                        >
                          <HistoryIcon sx={{ fontSize: 18 }} />
                        </IconButton>
                      )}
                      {onEdit && (
                        <IconButton
                          aria-label={t('common.edit')}
                          size="small"
                          onClick={(e) => {
                            e.stopPropagation();
                            onEdit(row);
                          }}
                          sx={{
                            color: 'primary.main',
                            border: '1px solid',
                            borderColor: 'divider',
                            borderRadius: 2,
                          }}
                        >
                          <EditIcon sx={{ fontSize: 18 }} />
                        </IconButton>
                      )}
                      {onDelete && (
                        <IconButton
                          aria-label={t('common.delete')}
                          size="small"
                          onClick={(e) => {
                            e.stopPropagation();
                            onDelete(row);
                          }}
                          sx={{
                            color: 'error.main',
                            border: '1px solid',
                            borderColor: 'error.light',
                            borderRadius: 2,
                          }}
                        >
                          <DeleteIcon sx={{ fontSize: 18 }} />
                        </IconButton>
                      )}
                    </Box>
                  </>
                )}
              </Card>
            </Box>
          );
        })}
      </Box>

      {hasMore && loading && (
        <Box
          sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: 1, py: 2 }}
        >
          <CircularProgress size={16} thickness={4} />
          <Typography variant="caption" color="text.disabled">
            {t('grid.loading_more')}
          </Typography>
        </Box>
      )}
    </>
  );
}

export const DataGridMobileBody = memo(forwardRef(DataGridMobileBodyInternal)) as <T>(
  props: MobileGridBodyProps<T> & { ref?: React.Ref<GridBodyHandle> }
) => React.ReactElement | null;
