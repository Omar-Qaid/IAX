import React from 'react';
import { Box, Checkbox, FormControlLabel, MenuItem, Stack, TextField } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import { ROUTE_PATHS } from '@app/routes/routePaths';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import { ActionPaneButton } from '@shared/components/action-pane/ActionPaneButton';
import { ActionPaneMenu } from '@shared/components/action-pane/ActionPaneMenu';
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
  costCenter: string | null;
  workerId: string | null;
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
  {
    preview: { entity: 'currency' },
    field: 'currencyCode',
    headerName: 'customerInquiry.currency',
  },
  amountColumn('amountMst'),
  amountColumn('balanceMst'),
  { field: 'department', headerName: 'customerInquiry.department' },
  { field: 'costCenter', headerName: 'customerInquiry.costCenter' },
  { field: 'workerId', headerName: 'customerInquiry.workerId' },
];
const showOptions: ShowFilter[] = ['all', 'open', 'closed'];

export function CustTrans(): React.ReactElement {
  const { accountNum } = useParams();
  const navigate = useNavigate();
  const { t } = useAppTranslation();
  const company = useCompanyStore((state) => state.currentCompany);
  const [show, setShow] = React.useState<ShowFilter>('all');
  const [asOfDate, setAsOfDate] = React.useState('');
  const [hideRevaluations, setHideRevaluations] = React.useState(false);
  const [selected, setSelected] = React.useState<Row | null>(null);
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
        (!asOfDate || row.transDate.slice(0, 10) <= asOfDate) &&
        (!hideRevaluations || row.transactionType !== 'ExchAdjustment')
      );
    })
    .map((row) => ({ ...row, id: String(row.recId) }));
  const action = (id: string) => t(`customerInquiry.actions.${id}`);
  /** A reference command of the D365 form that this build does not implement yet. */
  const reference = (id: string) => (
    <ActionPaneButton key={id} label={action(id)} disabled tooltip={t('common.notAvailableYet')} />
  );
  const referenceMenu = (id: string, items: string[]) => (
    <ActionPaneMenu
      key={id}
      label={action(id)}
      actions={items.map((item) => ({ id: item, label: action(item), disabled: true }))}
    />
  );
  const actionPane = (
    <>
      {reference('edit')}
      <ActionPaneButton
        label={action('voucher')}
        disabled={!selected?.voucher}
        onClick={() =>
          navigate(
            `${ROUTE_PATHS.GENERAL_LEDGER.VOUCHER_TRANSACTIONS}?voucher=${encodeURIComponent(
              selected?.voucher ?? ''
            )}&reference=${encodeURIComponent(
              selected?.invoice ? `${selected.invoice} : ${accountNum ?? ''}` : (accountNum ?? '')
            )}`
          )
        }
      />
      {reference('viewSettlements')}
      {referenceMenu('settlement', ['settleOpenTransactions', 'markPaymentForSettlement'])}
      {reference('originalDocument')}
      <ActionPaneButton
        label={action('openDetails')}
        disabled={!accountNum}
        onClick={() =>
          navigate(
            ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_OPEN_TRANSACTIONS.replace(
              ':accountNum',
              encodeURIComponent(accountNum ?? '')
            )
          )
        }
      />
      {referenceMenu('collections', [
        'collectionsCustomerPool',
        'interestNote',
        'collectionLetter',
      ])}
      {reference('billOfExchange')}
      {referenceMenu('reverse', ['reverseTransaction', 'reversalTrace'])}
      {reference('cashFlowForecasts')}
    </>
  );

  return (
    <SimpleListPage<Row>
      title={t('customerInquiry.customerTransactions')}
      actionPane={actionPane}
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
      dataGridProps={{
        storageKey: 'accounts-receivable.customer-transactions.reference-view',
        selectionMode: 'single',
        selectedIds: selected ? [selected.id] : [],
        onSelectionChange: (ids) =>
          setSelected(rows.find((row) => row.id === String(ids[0])) ?? null),
      }}
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
            <TextField
              type="date"
              size="small"
              label={t('customerInquiry.date')}
              value={asOfDate}
              onChange={(event) => setAsOfDate(event.target.value)}
              slotProps={{ inputLabel: { shrink: true } }}
            />
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
