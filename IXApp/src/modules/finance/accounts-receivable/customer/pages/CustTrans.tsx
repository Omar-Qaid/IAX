import React from 'react';
import { Box, Checkbox, FormControlLabel, MenuItem, Stack, TextField } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
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
type ShowFilter = 'all' | 'open' | 'closed';

const money = new Intl.NumberFormat(undefined, {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});
const amountColumn = (
  field: 'amountCur' | 'balanceCur' | 'amountMst' | 'balanceMst'
): ColumnDef<Row> => ({
  field,
  headerName: `customerInquiry.${field}`,
  width: 155,
  align: 'right',
  renderCell: ({ row }) => money.format(row[field]),
});
const buildColumns = (t: (key: string) => string): ColumnDef<Row>[] => [
  { field: 'voucher', headerName: 'customerInquiry.voucher', width: 150 },
  {
    field: 'transactionType',
    headerName: 'customerInquiry.transactionType',
    valueGetter: ({ row }) =>
      row.transactionType === 'Sales' ? t('customerInquiry.salesOrder') : row.transactionType,
  },
  {
    field: 'transDate',
    headerName: 'customerInquiry.date',
    valueGetter: ({ row }) => row.transDate.slice(0, 10),
  },
  { field: 'invoice', headerName: 'customerInquiry.invoice' },
  { field: 'description', headerName: 'customerInquiry.description', width: 230 },
  amountColumn('amountCur'),
  amountColumn('balanceCur'),
  { field: 'currencyCode', headerName: 'customerInquiry.currency' },
  amountColumn('amountMst'),
  amountColumn('balanceMst'),
  { field: 'department', headerName: 'customerInquiry.department' },
];
const showOptions: ShowFilter[] = ['all', 'open', 'closed'];

export function CustTrans(): React.ReactElement {
  const { accountNum } = useParams();
  const { t } = useAppTranslation();
  const company = useCompanyStore((state) => state.currentCompany);
  const [show, setShow] = React.useState<ShowFilter>('all');
  const [hideRevaluations, setHideRevaluations] = React.useState(false);
  const columns = React.useMemo(() => buildColumns(t), [t]);
  const query = useQuery({
    queryKey: ['CustTrans', company, accountNum],
    queryFn: async ({ signal }) => {
      const { data } = await apiClient.get<ApiResponse<Transaction[]>>(
        '/v1/CustTrans/transactions',
        { params: { accountNum }, signal }
      );
      if (!data.success || !data.data)
        throw new Error(data.message || t('customerInquiry.loadFailed'));
      return data.data;
    },
  });
  const rows = (query.data ?? [])
    .filter((row) => {
      const open = row.balanceCur !== 0 || row.balanceMst !== 0;
      return (
        (show === 'all' || (show === 'open') === open) &&
        (!hideRevaluations || row.transactionType !== 'ExchAdjustment')
      );
    })
    .map((row) => ({ ...row, id: String(row.recId) }));
  return (
    <SimpleListPage<Row>
      title={t('customerInquiry.customerTransactions')}
      columns={columns}
      recordTableName="CustTrans"
      enterpriseConfig={{
        readOnly: true,
        contextLabel: `${t('customerInquiry.customerTransactions')} | ${accountNum}`,
        getAuditRecordId: (row) => row.recId,
      }}
      dataSource={{ type: 'controlled', rows, refresh: () => void query.refetch() }}
      loading={query.isPending}
      error={query.error?.message}
      onRetry={() => void query.refetch()}
      dataGridProps={{ storageKey: 'accounts-receivable.customer-transactions.reference-view' }}
      filterBar={
        <Box sx={{ mx: { xs: 1, sm: 2.5 } }}>
          <Stack direction="row" sx={{ gap: 1, flexWrap: 'wrap' }}>
            <TextField
              select
              size="small"
              label={t('customerInquiry.show')}
              value={show}
              onChange={(event) => setShow(event.target.value as ShowFilter)}
            >
              {showOptions.map((option) => (
                <MenuItem key={option} value={option}>
                  {option === 'closed' ? t('customerInquiry.closed') : t(`common.${option}`)}
                </MenuItem>
              ))}
            </TextField>
            <FormControlLabel
              label={t('customerInquiry.hideCurrencyRevaluations')}
              control={
                <Checkbox
                  checked={hideRevaluations}
                  onChange={(event) => setHideRevaluations(event.target.checked)}
                />
              }
            />
          </Stack>
        </Box>
      }
    />
  );
}
