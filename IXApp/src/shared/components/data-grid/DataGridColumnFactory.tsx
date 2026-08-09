import type { ColumnDef } from './types';
import { formatCurrency, formatNumber } from '@core/utilities/formatUtils';
import { formatDate } from '@core/utilities/dateUtils';
import { StatusBadge } from '@shared/components/status/StatusBadge';

export const DataGridColumnFactory = {
  createTextColumn<T>(
    field: keyof T | string,
    headerName: string,
    options?: Partial<ColumnDef<T>>
  ): ColumnDef<T> {
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

  createNumberColumn<T>(
    field: keyof T | string,
    headerName: string,
    options?: Partial<ColumnDef<T>>
  ): ColumnDef<T> {
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

  createCurrencyColumn<T>(
    field: keyof T | string,
    headerName: string,
    currencyCode: string = 'USD',
    options?: Partial<ColumnDef<T>>
  ): ColumnDef<T> {
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

  createDateColumn<T>(
    field: keyof T | string,
    headerName: string,
    options?: Partial<ColumnDef<T>>
  ): ColumnDef<T> {
    return {
      field,
      headerName,
      type: 'date',
      width: 130,
      renderCell: (params) => formatDate(params.value as string),
      ...options,
    };
  },

  createBooleanColumn<T>(
    field: keyof T | string,
    headerName: string,
    options?: Partial<ColumnDef<T>>
  ): ColumnDef<T> {
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

  createStatusColumn<T>(
    field: keyof T | string,
    headerName: string,
    options?: Partial<ColumnDef<T>>
  ): ColumnDef<T> {
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
