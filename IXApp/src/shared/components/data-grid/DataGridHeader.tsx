import React, { useState, useEffect, useMemo, memo } from 'react';
import {
  Box, Typography,
  Dialog, DialogTitle, DialogContent, DialogActions,
  FormGroup, FormControlLabel, Button, Checkbox, useTheme
} from '@mui/material';
import {
  DndContext, closestCenter, KeyboardSensor, PointerSensor,
  useSensor, useSensors, type DragEndEvent,
} from '@dnd-kit/core';
import {
  arrayMove, SortableContext,
  sortableKeyboardCoordinates, horizontalListSortingStrategy,
} from '@dnd-kit/sortable';
import { useTranslation } from 'react-i18next';
import type { ColumnDef, SortModel, FilterModel } from './types';
import { AppBooleanField } from '@shared/components/fields/AppBooleanField';
import { GRID_SELECTION_COLUMN_WIDTH } from './constants';

// New Modular Sub-components
import { PinnedHeaderCell } from './header/PinnedHeaderCell';
import { SortableHeader } from './header/SortableHeader';
import { HeaderMenu } from './header/HeaderMenu';
import { FilterPopover } from './header/FilterPopover';

interface GridHeaderProps<T> {
  columns: ColumnDef<T>[];
  setColumns: React.Dispatch<React.SetStateAction<ColumnDef<T>[]>>;
  initialColumns: ColumnDef<T>[];
  sortModel: SortModel[];
  onSort: (field: string, direction?: 'asc' | 'desc') => void;
  filters: FilterModel[];
  setFilters: React.Dispatch<React.SetStateAction<FilterModel[]>>;
  onResetColumns: () => void;
  headerHeight: number;
  selectionMode?: 'none' | 'single' | 'multiple';
  onSelectAll?: (checked: boolean) => void;
  allSelected?: boolean;
  showColumnBorders?: boolean;
  hideFilterRow?: boolean;
  hideColumnMenu?: boolean;
}

export function DataGridHeaderInternal<T>({
  columns, setColumns, initialColumns, sortModel, onSort,
  filters, setFilters, onResetColumns, headerHeight,
  selectionMode = 'single', onSelectAll, allSelected,
  showColumnBorders = false, hideFilterRow = false, hideColumnMenu = false,
}: GridHeaderProps<T>) {
  const { t } = useTranslation();
  const theme = useTheme();

  // --- UI Anchors ---
  const [menuAnchor, setMenuAnchor] = useState<HTMLElement | null>(null);
  const [activeColumn, setActiveColumn] = useState<ColumnDef<T> | null>(null);
  const [filterPopoverAnchor, setFilterPopoverAnchor] = useState<HTMLElement | null>(null);
  const [filterColumn, setFilterColumn] = useState<ColumnDef<T> | null>(null);
  
  // --- Functional State ---
  const [resizing, setResizing] = useState<{ field: string; startX: number; startWidth: number } | null>(null);
  const [isChooseColumnsOpen, setIsChooseColumnsOpen] = useState(false);

  // --- DND Sensors ---
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 5 } }),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }),
  );

  // --- Resize Logic ---
  useEffect(() => {
    if (!resizing) return;
    const onMouseMove = (e: MouseEvent) => {
      setColumns(prev => {
        const col = prev.find(c => c.field === resizing.field);
        if (!col) return prev;

        let delta = e.clientX - resizing.startX;
        if (col.pinned === 'right') {
          delta = -delta;
        } else if (!col.pinned) {
          if (theme.direction === 'rtl') {
            delta = -delta;
          }
        }

        const newWidth = Math.max(50, resizing.startWidth + delta);
        return prev.map(c =>
          c.field === resizing.field ? { ...c, width: newWidth, flex: undefined } : c
        );
      });
    };
    const onMouseUp = () => setResizing(null);
    window.addEventListener('mousemove', onMouseMove);
    window.addEventListener('mouseup', onMouseUp);
    return () => {
      window.removeEventListener('mousemove', onMouseMove);
      window.removeEventListener('mouseup', onMouseUp);
    };
  }, [resizing, setColumns, theme]);

  // --- Handlers ---
  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>, column: ColumnDef<T>) => {
    setMenuAnchor(event.currentTarget);
    setActiveColumn(column);
  };

  const handleResizeStart = (e: React.MouseEvent, field: string) => {
    e.preventDefault();
    e.stopPropagation();
    const col = columns.find(c => c.field === field);
    if (col) setResizing({ field, startX: e.clientX, startWidth: col.width || 150 });
  };

  const handleFilterChange = (field: string, value: string) => {
    setFilters(prev => {
      const existing = prev.find(f => f.field === field);
      if (existing) {
        return value ? prev.map(f => f.field === field ? { ...f, value } : f) : prev.filter(f => f.field !== field);
      }
      return value ? [...prev, { field, operator: 'contains', value }] : prev;
    });
  };

  const handleFilterIconClick = (event: React.MouseEvent<HTMLElement>, column: ColumnDef<T>) => {
    event.stopPropagation();
    setFilterPopoverAnchor(event.currentTarget);
    setFilterColumn(column);
  };

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    if (over && active.id !== over.id) {
      setColumns(items => {
        const oldIndex = items.findIndex(i => i.field === active.id);
        const newIndex = items.findIndex(i => i.field === over.id);
        return arrayMove(items, oldIndex, newIndex);
      });
    }
  };

  // --- Derived Layout Values ---
  const { pinnedLeftCols, unpinnedCols, pinnedRightCols, pinnedLeftOffsets, pinnedRightOffsets, unpinnedColIds } = useMemo(() => {
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

    return {
      pinnedLeftCols: left,
      unpinnedCols: center,
      pinnedRightCols: right,
      pinnedLeftOffsets: leftOffsets,
      pinnedRightOffsets: rightOffsets,
      unpinnedColIds: center.map(c => c.field as string)
    };
  }, [columns]);

  return (
    <Box sx={{ 
      display: 'flex', 
      height: headerHeight,
      bgcolor: '#ffffff',
      width: 'max-content',
      minWidth: '100%',
      position: 'relative'
    }}>
      
      {selectionMode === 'multiple' && (
        <Box sx={{
          display: 'flex', flexDirection: 'column',
          boxSizing: 'border-box',
          bgcolor: '#ffffff',
          width: GRID_SELECTION_COLUMN_WIDTH, minWidth: GRID_SELECTION_COLUMN_WIDTH, maxWidth: GRID_SELECTION_COLUMN_WIDTH,
          position: 'sticky', insetInlineStart: 0, zIndex: 6,
          borderInlineEnd: `1px solid ${theme.palette.divider}`,
          flexShrink: 0,
          flex: 'none'
        }}>
          <Box sx={{
            display: 'flex', alignItems: 'center', justifyContent: 'center', p: 0,
            borderBottom: `1px solid ${theme.palette.divider}`, height: headerHeight,
            boxSizing: 'border-box', overflow: 'hidden',
          }}>
            <Checkbox
              size="small"
              checked={Boolean(allSelected)}
              onChange={(event) => onSelectAll?.(event.target.checked)}
              slotProps={{ input: { 'aria-label': t('grid.select_all') } }}
              sx={{ p: 0.5, m: 0, color: 'text.secondary', '&.Mui-checked': { color: 'primary.main' } }}
            />
          </Box>
        </Box>
      )}

      {pinnedLeftCols.map((col, i) => (
        <PinnedHeaderCell
          key={col.field as string}
          column={col}
          offset={pinnedLeftOffsets[i] + (selectionMode === 'multiple' ? GRID_SELECTION_COLUMN_WIDTH : 0)}
          side="left"
          filters={filters}
          onFilterChange={handleFilterChange}
          onFilterIconClick={handleFilterIconClick}
          onMenuOpen={handleMenuOpen}
          onResizeStart={handleResizeStart}
          showColumnBorders={showColumnBorders}
          hideFilterRow={hideFilterRow}
          hideColumnMenu={hideColumnMenu}
        />
      ))}

      <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
        <SortableContext items={unpinnedColIds} strategy={horizontalListSortingStrategy}>
          {unpinnedCols.map(col => (
            <SortableHeader
              key={col.field as string}
              column={col}
              sortModel={sortModel}
              onSort={onSort}
              filters={filters}
              onFilterChange={handleFilterChange}
              onFilterIconClick={handleFilterIconClick}
              onMenuOpen={handleMenuOpen}
              onResizeStart={handleResizeStart}
              showColumnBorders={showColumnBorders}
              hideFilterRow={hideFilterRow}
              hideColumnMenu={hideColumnMenu}
            />
          ))}
        </SortableContext>
      </DndContext>

      {/* Filler to absorb remaining space and keep header/body widths aligned */}
      <Box sx={{
        flexGrow: 1,
        flexShrink: 0,
        minWidth: 0,
        borderBottom: `1px solid ${theme.palette.divider}`,
        bgcolor: '#ffffff',
      }} />

      {pinnedRightCols.map((col, i) => (
        <PinnedHeaderCell
          key={col.field as string}
          column={col}
          offset={pinnedRightOffsets[i]}
          side="right"
          filters={filters}
          onFilterChange={handleFilterChange}
          onFilterIconClick={handleFilterIconClick}
          onMenuOpen={handleMenuOpen}
          onResizeStart={handleResizeStart}
          showColumnBorders={showColumnBorders}
          hideFilterRow={hideFilterRow}
          hideColumnMenu={hideColumnMenu}
        />
      ))}

      {/* Extracted Overlay Components */}
      <HeaderMenu
        anchorEl={menuAnchor}
        onClose={() => setMenuAnchor(null)}
        activeColumn={activeColumn}
        initialColumns={initialColumns}
        setColumns={setColumns}
        onSort={onSort}
        onResetColumns={onResetColumns}
        onOpenChooseColumns={() => setIsChooseColumnsOpen(true)}
      />

      <FilterPopover
        anchorEl={filterPopoverAnchor}
        onClose={() => setFilterPopoverAnchor(null)}
        column={filterColumn}
        filters={filters}
        setFilters={setFilters}
        onSort={onSort}
      />

      {/* Choose Columns dialog */}
      <Dialog open={isChooseColumnsOpen} onClose={() => setIsChooseColumnsOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle sx={{ fontSize: '1rem', fontWeight: 600 }}>{t('grid.choose_columns')}</DialogTitle>
        <DialogContent dividers>
          <FormGroup>
            {initialColumns.map(col => (
              <FormControlLabel
                key={col.field as string}
                control={
                  <AppBooleanField
                    value={!columns.find(c => c.field === col.field)?.hidden}
                    onChange={(val) => setColumns(prev => prev.map(c =>
                      c.field === col.field ? { ...c, hidden: !val } : c
                    ))}
                  />
                }
                label={<Typography variant="body2">{t(col.headerName || '')}</Typography>}
              />
            ))}
          </FormGroup>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setIsChooseColumnsOpen(false)} size="small" variant="contained">{t('common.close')}</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export const DataGridHeader = memo(DataGridHeaderInternal) as typeof DataGridHeaderInternal;
