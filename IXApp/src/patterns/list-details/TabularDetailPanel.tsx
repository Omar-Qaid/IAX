import React from 'react';
import { Box, Button } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlined';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';

export interface TabularDetailAction { id: string; label: string; onClick?: () => void; disabled?: boolean; icon?: React.ReactNode }

export interface TabularDetailPanelProps<T extends { id: string }> {
  rows: T[];
  columns: ColumnDef<T>[];
  addLabel: string;
  removeLabel: string;
  selectedIds: (string | number)[];
  onSelectionChange: (ids: (string | number)[]) => void;
  onAdd?: () => void;
  onRemove?: () => void;
  filterContent?: React.ReactNode;
  storageKey?: string;
  height?: number;
  disabled?: boolean;
  actions?: TabularDetailAction[];
}

export function TabularDetailPanel<T extends { id: string }>({ rows, columns, addLabel, removeLabel, selectedIds, onSelectionChange, onAdd, onRemove, filterContent, storageKey, height = 218, disabled = false, actions }: TabularDetailPanelProps<T>): React.ReactElement {
  return <Box sx={{ minWidth: 0 }}>
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.25, mb: 1.15 }}>
      {onAdd && <Button size="small" startIcon={<AddIcon />} disabled={disabled} onClick={onAdd} sx={{ minWidth: 0, px: 0.75, fontSize: '0.75rem' }}>{addLabel}</Button>}
      {onRemove && <Button size="small" startIcon={<DeleteOutlineIcon />} disabled={disabled || selectedIds.length === 0} onClick={onRemove} sx={{ minWidth: 0, px: 0.75, fontSize: '0.75rem' }}>{removeLabel}</Button>}
      {actions?.map((action) => <Button key={action.id} size="small" startIcon={action.icon} disabled={disabled || action.disabled} onClick={action.onClick} sx={{ minWidth: 0, px: 0.75, fontSize: '0.75rem' }}>{action.label}</Button>)}
    </Box>
    {filterContent && <Box sx={{ mb: 0.75 }}>{filterContent}</Box>}
    <DataGrid rows={rows} columns={columns} height={height} hideToolbar hideFooter hideSidebar hideFilterRow selectionMode="single" selectedIds={selectedIds} onSelectionChange={(ids) => onSelectionChange(ids as (string | number)[])} storageKey={storageKey} rowHeight={31} headerHeight={31} />
  </Box>;
}
