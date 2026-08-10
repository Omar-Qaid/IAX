import React from 'react';
import { Box, Button } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlined';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { d365 } from './d365Tokens';

export interface TabularDetailAction { id: string; label: string; ariaLabel?: string; onClick?: () => void; disabled?: boolean; icon?: React.ReactNode }

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
  rowHeight?: number;
  disabled?: boolean;
  actions?: TabularDetailAction[];
}

export function TabularDetailPanel<T extends { id: string }>({ rows, columns, addLabel, removeLabel, selectedIds, onSelectionChange, onAdd, onRemove, filterContent, storageKey, height = 218, rowHeight = d365.gridRowHeight, disabled = false, actions }: TabularDetailPanelProps<T>): React.ReactElement {
  return <Box sx={{ minWidth: 0 }}>
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0, height: 25, mb: '5px' }}>
      {onAdd && <Button size="small" startIcon={<AddIcon sx={{ fontSize: 16 }} />} disabled={disabled} onClick={onAdd} sx={actionSx}>{addLabel}</Button>}
      {onRemove && <Button size="small" startIcon={<DeleteOutlineIcon sx={{ fontSize: 16 }} />} disabled={disabled || selectedIds.length === 0} onClick={onRemove} sx={actionSx}>{removeLabel}</Button>}
      {actions?.map((action) => <Button key={action.id} aria-label={action.ariaLabel} size="small" startIcon={action.icon} disabled={disabled || action.disabled} onClick={action.onClick} sx={actionSx}>{action.label}</Button>)}
    </Box>
    {filterContent && <Box sx={{ mb: 0.75 }}>{filterContent}</Box>}
    <DataGrid rows={rows} columns={columns} height={height} hideToolbar hideFooter hideSidebar hideFilterRow selectionMode="single" selectedIds={selectedIds} onSelectionChange={(ids) => onSelectionChange(ids as (string | number)[])} storageKey={storageKey} rowHeight={rowHeight} headerHeight={d365.gridHeaderHeight} />
  </Box>;
}

const actionSx = { minWidth: 0, px: '7px', py: 0, height: 25, borderRadius: 0, color: d365.primary, fontFamily: d365.fontFamily, fontWeight: 400, fontSize: d365.fontSize, '& .MuiButton-startIcon': { mr: '4px' } };
