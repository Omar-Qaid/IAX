import React from 'react';
import { Box, Button, type Theme } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlined';
import { PatternDataGrid as DataGrid } from './PatternDataGrid';
import { PATTERN_GRID_DEFAULTS } from './patternGridDefaults';
import { PageEditSessionContext } from './PageEditSession';
import type { ColumnDef, DataGridHandle, DataGridProps } from './types';
import { d365 } from '@shared/constants/enterpriseUiTokens';
import { enterpriseSurfaces } from '@shared/constants/enterpriseSurfaces';

export interface TabularDetailAction {
  id: string;
  label: string;
  ariaLabel?: string;
  onClick?: () => void;
  disabled?: boolean;
  allowWhenReadOnly?: boolean;
  icon?: React.ReactNode;
}

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
  showFilterRow?: boolean;
  actions?: TabularDetailAction[];
  gridRef?: React.Ref<DataGridHandle>;
  masterForm?: boolean;
  onRowSave?: (row: Partial<T>, isNew: boolean) => Promise<void> | void;
  onNewRow?: () => Partial<T>;
  onEditingChange?: (isEditing: boolean) => void;
  onCellEditRequest?: DataGridProps<T>['onCellEditRequest'];
}

export function TabularDetailPanel<T extends { id: string }>({
  rows,
  columns,
  addLabel,
  removeLabel,
  selectedIds,
  onSelectionChange,
  onAdd,
  onRemove,
  filterContent,
  storageKey,
  height = PATTERN_GRID_DEFAULTS.height,
  rowHeight = PATTERN_GRID_DEFAULTS.rowHeight,
  disabled: panelDisabled = false,
  showFilterRow = false,
  actions,
  gridRef,
  masterForm = false,
  onRowSave,
  onNewRow,
  onEditingChange,
  onCellEditRequest,
}: TabularDetailPanelProps<T>): React.ReactElement {
  const session = React.useContext(PageEditSessionContext);
  const disabled = panelDisabled || Boolean(session && (!session.editing || session.saving));
  return (
    <Box sx={{ minWidth: 0 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0, height: 25, mb: '5px' }}>
        {onAdd && (
          <Button
            size="small"
            startIcon={<AddIcon sx={{ fontSize: 16 }} />}
            disabled={disabled}
            onClick={onAdd}
            sx={actionSx}
          >
            {addLabel}
          </Button>
        )}
        {onRemove && (
          <Button
            size="small"
            startIcon={<DeleteOutlineIcon sx={{ fontSize: 16 }} />}
            disabled={disabled || selectedIds.length === 0}
            onClick={onRemove}
            sx={actionSx}
          >
            {removeLabel}
          </Button>
        )}
        {actions?.map((action) => (
          <Button
            key={action.id}
            aria-label={action.ariaLabel}
            size="small"
            startIcon={action.icon}
            disabled={(disabled && !action.allowWhenReadOnly) || action.disabled || !action.onClick}
            onClick={action.onClick}
            sx={actionSx}
          >
            {action.label}
          </Button>
        ))}
      </Box>
      {filterContent && <Box sx={{ mb: 0.75 }}>{filterContent}</Box>}
      <DataGrid
        ref={gridRef}
        rows={rows}
        columns={columns}
        height={height}
        hideToolbar
        hideFooter
        hideSidebar
        hideFilterRow={!showFilterRow}
        selectionMode="single"
        selectedIds={selectedIds}
        onSelectionChange={(ids) => onSelectionChange(ids as (string | number)[])}
        storageKey={storageKey}
        rowHeight={rowHeight}
        headerHeight={PATTERN_GRID_DEFAULTS.headerHeight}
        masterForm={masterForm}
        editDisabled={panelDisabled || session?.saving}
        onRowSave={onRowSave}
        onNewRow={onNewRow}
        onEditingChange={onEditingChange}
        onCellEditRequest={panelDisabled || session?.saving ? undefined : onCellEditRequest}
        hideAddRowButton
      />
    </Box>
  );
}

const actionSx = {
  minWidth: 0,
  px: '7px',
  py: 0,
  height: 25,
  borderRadius: 0,
  color: (theme: Theme) => enterpriseSurfaces(theme).accent,
  fontFamily: d365.fontFamily,
  fontWeight: 400,
  fontSize: d365.fontSize,
  '& .MuiButton-startIcon': { marginInlineEnd: '4px', marginInlineStart: 0 },
};
