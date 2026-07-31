import type { ColumnDef } from './types';
import { formatCurrency, formatNumber } from '@core/utilities/formatUtils';
import { formatDate } from '@core/utilities/dateUtils';
import { StatusBadge } from '@shared/components/status/StatusBadge';

export const DataGridColumnFactory = {
  createTextColumn(field: string, headerName: string, options?: Partial<ColumnDef<any>>): ColumnDef<any> {
    return {
      field,
      headerName,
      flex: 1,
      minWidth: 120,
      sortable: true,
      filterable: true,
      type: 'text',
      ...options,
    };
  },

  createNumberColumn(field: string, headerName: string, options?: Partial<ColumnDef<any>>): ColumnDef<any> {
    return {
      field,
      headerName,
      type: 'number',
      width: 120,
      align: 'right',
      headerAlign: 'right',
      renderCell: (params) => formatNumber(params.value as number),
      ...options,
    };
  },

  createCurrencyColumn(field: string, headerName: string, currencyCode: string = 'USD', options?: Partial<ColumnDef<any>>): ColumnDef<any> {
    return {
      field,
      headerName,
      type: 'number',
      width: 140,
      align: 'right',
      headerAlign: 'right',
      renderCell: (params) => formatCurrency(params.value as number, currencyCode),
      ...options,
    };
  },

  createDateColumn(field: string, headerName: string, options?: Partial<ColumnDef<any>>): ColumnDef<any> {
    return {
      field,
      headerName,
      type: 'date',
      width: 130,
      renderCell: (params) => formatDate(params.value as string),
      ...options,
    };
  },

  createBooleanColumn(field: string, headerName: string, options?: Partial<ColumnDef<any>>): ColumnDef<any> {
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

  createStatusColumn(field: string, headerName: string, options?: Partial<ColumnDef<any>>): ColumnDef<any> {
    return {
      field,
      headerName,
      width: 120,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params) => <StatusBadge status={params.value as string} />,
      ...options,
    };
  },
};
