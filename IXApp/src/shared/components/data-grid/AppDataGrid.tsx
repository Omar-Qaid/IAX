import React from 'react';
import { Box } from '@mui/material';
import { DataGrid, type GridColDef, type GridRowParams } from '@mui/x-data-grid';

export interface AppDataGridProps<T = Record<string, unknown>> {
  rows: T[];
  columns: GridColDef[];
  loading?: boolean;
  onRowDoubleClick?: (params: GridRowParams) => void;
  onSelectionChange?: (selectedIds: string[]) => void;
  pageSize?: number;
  height?: number | string;
  checkboxSelection?: boolean;
  getRowId?: (row: T) => string;
}

export function AppDataGrid<T extends { id: string } = { id: string }>({
  rows,
  columns,
  loading = false,
  onRowDoubleClick,
  onSelectionChange,
  pageSize = 25,
  height = 420,
  checkboxSelection = true,
  getRowId = (row) => row.id,
}: AppDataGridProps<T>): React.ReactElement {
  return (
    <Box sx={{ height, width: '100%' }}>
      <DataGrid
        rows={rows}
        columns={columns}
        loading={loading}
        density="compact"
        getRowId={getRowId}
        checkboxSelection={checkboxSelection}
        disableRowSelectionOnClick
        onRowDoubleClick={onRowDoubleClick}
        onRowSelectionModelChange={(newModel) => {
          if (onSelectionChange) {
            const ids = Array.from(newModel as unknown as Iterable<string | number>).map(String);
            onSelectionChange(ids);
          }
        }}
        initialState={{
          pagination: {
            paginationModel: { pageSize, page: 0 },
          },
        }}
        pageSizeOptions={[10, 25, 50, 100]}
        sx={{
          bgcolor: 'background.paper',
          borderRadius: 1,
        }}
      />
    </Box>
  );
}
