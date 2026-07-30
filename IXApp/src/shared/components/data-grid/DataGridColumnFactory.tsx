import type { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { formatCurrency, formatNumber } from '@core/utilities/formatUtils';
import { formatDate } from '@core/utilities/dateUtils';
import { StatusBadge } from '@shared/components/status/StatusBadge';

export const DataGridColumnFactory = {
  createTextColumn(field: string, headerName: string, options?: Partial<GridColDef>): GridColDef {
    return {
      field,
      headerName,
      flex: 1,
      minWidth: 120,
      sortable: true,
      filterable: true,
      ...options,
    };
  },

  createNumberColumn(field: string, headerName: string, options?: Partial<GridColDef>): GridColDef {
    return {
      field,
      headerName,
      type: 'number',
      width: 120,
      align: 'right',
      headerAlign: 'right',
      valueFormatter: (value) => formatNumber(value as number),
      ...options,
    };
  },

  createCurrencyColumn(field: string, headerName: string, currencyCode: string = 'USD', options?: Partial<GridColDef>): GridColDef {
    return {
      field,
      headerName,
      type: 'number',
      width: 140,
      align: 'right',
      headerAlign: 'right',
      renderCell: (params: GridRenderCellParams) =>
        formatCurrency(params.value as number, currencyCode),
      ...options,
    };
  },

  createDateColumn(field: string, headerName: string, options?: Partial<GridColDef>): GridColDef {
    return {
      field,
      headerName,
      type: 'date',
      width: 130,
      valueFormatter: (value) => formatDate(value as string),
      ...options,
    };
  },

  createBooleanColumn(field: string, headerName: string, options?: Partial<GridColDef>): GridColDef {
    return {
      field,
      headerName,
      type: 'boolean',
      width: 100,
      align: 'center',
      headerAlign: 'center',
      ...options,
    };
  },

  createStatusColumn(field: string, headerName: string, options?: Partial<GridColDef>): GridColDef {
    return {
      field,
      headerName,
      width: 120,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams) => (
        <StatusBadge status={params.value as string} />
      ),
      ...options,
    };
  },
};
