import React from 'react';
import { Checkbox, FormControlLabel, MenuItem, Stack, TextField } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';

interface Transaction {
  recId: number;
  voucher: string;
  transactionType: string;
  transDate: string;
  invoice: string;
  description: string;
  amountCur: number;
  balanceCur: number;
  currencyCode: string;
  amountMst: number;
  balanceMst: number;
  department: string | null;
}
type Row = Transaction & { id: string };
const money = new Intl.NumberFormat(undefined, {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});
const amountColumn = (
  field: 'amountCur' | 'balanceCur' | 'amountMst' | 'balanceMst',
  headerName: string
): ColumnDef<Row> => ({
  field,
  headerName,
  width: 155,
  align: 'right',
  renderCell: ({ row }) => money.format(row[field]),
});
const columns: ColumnDef<Row>[] = [
  { field: 'voucher', headerName: 'Voucher', width: 150 },
  {
    field: 'transactionType',
    headerName: 'Transaction type',
    valueGetter: ({ row }) =>
      row.transactionType === 'Sales' ? 'Sales order' : row.transactionType,
  },
  { field: 'transDate', headerName: 'Date', valueGetter: ({ row }) => row.transDate.slice(0, 10) },
  { field: 'invoice', headerName: 'Invoice' },
  { field: 'description', headerName: 'Description', width: 230 },
  amountColumn('amountCur', 'Amount in transaction currency'),
  amountColumn('balanceCur', 'Balance'),
  { field: 'currencyCode', headerName: 'Currency' },
  amountColumn('amountMst', 'Amount'),
  amountColumn('balanceMst', 'Balance'),
  { field: 'department', headerName: 'Department' },
];

export function CustTrans() {
  const { accountNum } = useParams();
  const company = useCompanyStore((state) => state.currentCompany);
  const [show, setShow] = React.useState('All');
  const [search, setSearch] = React.useState('');
  const [hideRevaluations, setHideRevaluations] = React.useState(false);
  const query = useQuery({
    queryKey: ['CustTrans', company, accountNum],
    queryFn: async ({ signal }) => {
      const { data } = await apiClient.get<ApiResponse<Transaction[]>>(
        '/v1/CustTrans/transactions',
        { params: { accountNum }, signal }
      );
      if (!data.success || !data.data)
        throw new Error(data.message || 'Unable to load customer transactions');
      return data.data;
    },
  });
  const rows = (query.data ?? [])
    .filter((row) => {
      const open = row.balanceCur !== 0 || row.balanceMst !== 0;
      return (
        (show === 'All' || (show === 'Open') === open) &&
        (!hideRevaluations || row.transactionType !== 'ExchAdjustment') &&
        [
          row.voucher,
          row.invoice,
          row.description,
          row.currencyCode,
          row.transactionType,
          row.department,
        ]
          .join(' ')
          .toLowerCase()
          .includes(search.trim().toLowerCase())
      );
    })
    .map((row) => ({ ...row, id: String(row.recId) }));
  return (
    <SimpleListPage<Row>
      recordTableName="CustTrans"
      title={`Customer transactions | ${accountNum}`}
      columns={columns}
      dataSource={{ type: 'controlled', rows, refresh: () => void query.refetch() }}
      loading={query.isPending}
      error={query.error?.message}
      onRetry={() => void query.refetch()}
      dataGridProps={{ storageKey: 'CustTrans', mobilePresentation: 'table' }}
      filterBar={
        <Stack direction="row" sx={{ gap: 1, flexWrap: 'wrap', mx: 2.5, my: 1 }}>
          <TextField
            size="small"
            label="Filter"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          <TextField
            select
            size="small"
            label="Show"
            value={show}
            onChange={(e) => setShow(e.target.value)}
          >
            {['All', 'Open', 'Closed'].map((option) => (
              <MenuItem key={option} value={option}>
                {option}
              </MenuItem>
            ))}
          </TextField>
          <TextField size="small" label="Date" value={new Date().toLocaleDateString()} disabled />
          <FormControlLabel
            label="Hide currency revaluations"
            control={
              <Checkbox
                checked={hideRevaluations}
                onChange={(e) => setHideRevaluations(e.target.checked)}
              />
            }
          />
        </Stack>
      }
    />
  );
}
