import React from 'react';
import CheckIcon from '@mui/icons-material/Check';
import { Alert, Box, Stack, Tab, Tabs, Typography } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { ROUTE_PATHS } from '@app/routes/routePaths';
import { useAuth } from '@core/auth/useAuth';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import { ActionPaneButton } from '@shared/components/action-pane/ActionPaneButton';
import { ActionPaneMenu } from '@shared/components/action-pane/ActionPaneMenu';
import { ListInfoValue } from '@patterns/simple-list/ListInfoValue';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { ledgerTransVoucherApi, type LedgerTransVoucherRecord } from '../api/ledgerTransVoucherApi';

type Row = LedgerTransVoucherRecord & { id: string };
type VoucherTab = 'overview' | 'general';

/** One labelled value of the voucher's General tab and its related information. */
function VoucherField({
  label,
  value,
  empty,
}: {
  label: string;
  value?: string | null;
  empty: string;
}): React.ReactElement {
  return (
    <Box>
      <Typography sx={{ fontSize: '0.7rem', color: 'text.secondary' }}>{label}</Typography>
      <ListInfoValue value={value} empty={empty} />
    </Box>
  );
}

export function LedgerTransVoucher(): React.ReactElement {
  const company = useCompanyStore((state) => state.currentCompany);
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const voucher = searchParams.get('voucher');
  const reference = searchParams.get('reference');
  const { hasPermission } = useAuth();
  const permitted = hasPermission(PERMISSIONS.LEDGER_JOURNAL_ENTRY_VIEW);
  const { t, isRtl } = useAppTranslation();
  const [tab, setTab] = React.useState<VoucherTab>('overview');
  const [selected, setSelected] = React.useState<Row | null>(null);
  const query = useQuery({
    queryKey: ['ledger-trans-voucher', company, voucher],
    queryFn: ({ signal }) => ledgerTransVoucherApi.load({ voucher }, signal),
    enabled: permitted && Boolean(company),
  });
  if (!permitted) return <Alert severity="warning">{t('partyBalance.accessDenied')}</Alert>;

  const number = new Intl.NumberFormat(isRtl ? 'ar-SA' : 'en-US', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
  const columns: ColumnDef<Row>[] = [
    { field: 'journalNumber', headerName: t('ledgerTransVoucher.journalNumber'), width: 150 },
    { field: 'voucher', headerName: t('ledgerTransVoucher.voucher'), width: 180 },
    {
      field: 'transDate',
      headerName: t('ledgerTransVoucher.date'),
      width: 120,
      valueGetter: ({ row }) => row.transDate.slice(0, 10),
    },
    {
      field: 'yearClosed',
      headerName: t('ledgerTransVoucher.yearClosed'),
      width: 110,
      align: 'center',
      renderCell: ({ row }) => (row.yearClosed ? <CheckIcon sx={{ fontSize: 16 }} /> : null),
    },
    { field: 'ledgerAccount', headerName: t('ledgerTransVoucher.ledgerAccount'), width: 300 },
    { field: 'accountName', headerName: t('ledgerTransVoucher.accountName'), width: 260 },
    { field: 'description', headerName: t('ledgerTransVoucher.description'), width: 260 },
    { field: 'currencyCode', headerName: t('ledgerTransVoucher.currency'), width: 100 },
    {
      field: 'amountCur',
      headerName: t('ledgerTransVoucher.amountCur'),
      width: 185,
      align: 'right',
      renderCell: ({ row }) => number.format(row.amountCur),
    },
    {
      field: 'amountMst',
      headerName: t('ledgerTransVoucher.amountMst'),
      width: 185,
      align: 'right',
      renderCell: ({ row }) => number.format(row.amountMst),
    },
    { field: 'postingType', headerName: t('ledgerTransVoucher.postingType'), width: 170 },
  ];

  const unavailable = (id: string) => (
    <ActionPaneButton
      key={id}
      label={t(`ledgerTransVoucher.actions.${id}`)}
      disabled
      tooltip={t('common.notAvailableYet')}
    />
  );
  const menu = (id: string, items: string[]) => (
    <ActionPaneMenu
      key={id}
      label={t(`ledgerTransVoucher.actions.${id}`)}
      actions={items.map((item) => ({
        id: item,
        label: t(`ledgerTransVoucher.actions.${item}`),
        disabled: true,
      }))}
    />
  );
  const actionPane = (
    <>
      {unavailable('viewSubledgerJournal')}
      {unavailable('transactionOrigin')}
      <ActionPaneButton
        label={t('ledgerTransVoucher.actions.transactions')}
        disabled={!selected}
        onClick={() =>
          navigate(
            `${ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUST_INVOICE_JOURNAL}?voucher=${encodeURIComponent(selected?.voucher ?? '')}`
          )
        }
      />
      {unavailable('auditTrail')}
      {menu('editVoucher', ['editZakatTransactions', 'editDimensions'])}
      {unavailable('postedSalesTax')}
      {unavailable('originalDocument')}
      {unavailable('accountingSourceExplorer')}
      {menu('reverseTransactions', ['reverse', 'reverseAndTrace'])}
      {unavailable('reversedTracing')}
      {unavailable('relatedVouchers')}
      {unavailable('allRelatedVouchers')}
      {unavailable('viewReversalFailures')}
    </>
  );

  const empty = t('ledgerTransVoucher.noRecordSelected');
  const detailFields: { label: string; value: (row: Row) => string }[] = [
    { label: t('ledgerTransVoucher.journalNumber'), value: (row) => row.journalNumber },
    { label: t('ledgerTransVoucher.voucher'), value: (row) => row.voucher },
    { label: t('ledgerTransVoucher.date'), value: (row) => row.transDate.slice(0, 10) },
    { label: t('ledgerTransVoucher.ledgerAccount'), value: (row) => row.ledgerAccount },
    { label: t('ledgerTransVoucher.accountName'), value: (row) => row.accountName },
    { label: t('ledgerTransVoucher.description'), value: (row) => row.description },
    { label: t('ledgerTransVoucher.currency'), value: (row) => row.currencyCode },
    { label: t('ledgerTransVoucher.amountCur'), value: (row) => number.format(row.amountCur) },
    { label: t('ledgerTransVoucher.amountMst'), value: (row) => number.format(row.amountMst) },
    { label: t('ledgerTransVoucher.postingType'), value: (row) => row.postingType },
  ];
  const rows = (query.data ?? []).map((row) => ({ ...row, id: String(row.recId) }));
  const contextLabel = voucher
    ? `${t('ledgerTransVoucher.title')} | ${reference ? `${reference} : ` : ''}${voucher}`
    : t('ledgerTransVoucher.title');

  return (
    <SimpleListPage<Row>
      title={t('ledgerTransVoucher.title')}
      recordTableName="GeneralJournalAccountEntry"
      actionPane={actionPane}
      enterpriseConfig={{
        readOnly: true,
        contextLabel,
        getAuditRecordId: (row) => row.recId,
        relatedInformation: {
          sections: (row) => [
            {
              id: 'voucher',
              label: t('ledgerTransVoucher.title'),
              content: (
                <Stack sx={{ gap: 0.5 }}>
                  <VoucherField
                    label={t('ledgerTransVoucher.journalNumber')}
                    value={row?.journalNumber}
                    empty={empty}
                  />
                  <VoucherField
                    label={t('ledgerTransVoucher.voucher')}
                    value={row?.voucher}
                    empty={empty}
                  />
                  <VoucherField
                    label={t('ledgerTransVoucher.ledgerAccount')}
                    value={row?.ledgerAccount}
                    empty={empty}
                  />
                </Stack>
              ),
            },
          ],
        },
      }}
      columns={columns}
      dataSource={{
        type: 'controlled',
        rows,
        refresh: () => void query.refetch(),
      }}
      loading={query.isFetching}
      error={query.isError ? query.error.message : null}
      onRetry={() => void query.refetch()}
      dataGridProps={{
        storageKey: 'general-ledger.ledger-trans-voucher',
        selectionMode: 'single',
        selectedIds: selected ? [selected.id] : [],
        onSelectionChange: (ids) =>
          setSelected(rows.find((row) => row.id === String(ids[0])) ?? null),
      }}
      filterBar={
        <Box sx={{ mx: { xs: 1, sm: 2.5 } }}>
          <Tabs value={tab} onChange={(_, value: VoucherTab) => setTab(value)}>
            <Tab value="overview" label={t('ledgerTransVoucher.overview')} />
            <Tab value="general" label={t('ledgerTransVoucher.general')} />
          </Tabs>
        </Box>
      }
      detailContent={
        tab === 'general'
          ? (row) => (
              <Stack sx={{ gap: 0.75, p: 2 }}>
                {detailFields.map((field) => (
                  <VoucherField
                    key={field.label}
                    label={field.label}
                    value={row ? field.value(row) : ''}
                    empty={empty}
                  />
                ))}
              </Stack>
            )
          : undefined
      }
    />
  );
}
