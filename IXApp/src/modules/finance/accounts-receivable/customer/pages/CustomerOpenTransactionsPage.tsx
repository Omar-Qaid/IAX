import React from 'react';
import { Alert } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { useAuth } from '@core/auth/useAuth';
import { PERMISSIONS } from '@core/permissions/permissions';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import {
  customerOpenTransactionsApi,
  type CustomerOpenTransaction,
} from '../api/customerOpenTransactionsApi';

type Row = CustomerOpenTransaction & { id: string };
/** Read-only CustTransOpen inquiry; posting and settlement remain in their own workflows. */
export function CustomerOpenTransactionsPage(): React.ReactElement {
  const { accountNum = '' } = useParams<{ accountNum: string }>();
  const company = useCompanyStore((state) => state.currentCompany);
  const { hasPermission } = useAuth();
  const permitted = hasPermission(PERMISSIONS.CUSTOMER_TRANSACTION_VIEW);
  const { t, isRtl } = useAppTranslation();
  const query = useQuery({
    queryKey: ['customer-open-transactions', company, accountNum],
    queryFn: ({ signal }) => customerOpenTransactionsApi.load(accountNum, signal),
    enabled: permitted && Boolean(accountNum),
  });
  if (!permitted) return <Alert severity="warning">{t('partyBalance.accessDenied')}</Alert>;
  if (!accountNum) return <Alert severity="error">{t('partyBalance.accountRequired')}</Alert>;
  const number = (value: number) =>
    new Intl.NumberFormat(isRtl ? 'ar-SA' : 'en-US', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(value);
  const columns: ColumnDef<Row>[] = [
    {
      field: 'transDate',
      headerName: t('customerInquiry.date'),
      width: 120,
      valueGetter: ({ row }) => row.transDate?.slice(0, 10),
    },
    { field: 'voucher', headerName: t('customerInquiry.voucher'), width: 150 },
    { field: 'invoice', headerName: t('customerInquiry.invoice'), width: 150 },
    {
      field: 'dueDate',
      headerName: t('customerInquiry.dueDate'),
      width: 120,
      valueGetter: ({ row }) => row.dueDate?.slice(0, 10),
    },
    { preview: { entity: 'currency' },  field: 'currencyCode', headerName: t('partyBalance.currency'), width: 95 },
    ...(['amountCur', 'settleAmountCur', 'openAmountCur', 'openAmountMst'] as const).map(
      (field) => ({
        field,
        headerName: t('customerInquiry.' + field),
        width: 155,
        align: 'right' as const,
        renderCell: ({ row }: { row: Row }) => number(row[field]),
      })
    ),
  ];
  return (
    <SimpleListPage<Row>
      title={t('customerInquiry.openTransactions')}
      recordTableName="CustTrans"
      enterpriseConfig={{
        readOnly: true,
        contextLabel: `${t('customerInquiry.openTransactions')} | ${accountNum}`,
        searchFields: [
          { field: 'invoice', label: t('customerInquiry.invoice') },
          { field: 'voucher', label: t('customerInquiry.voucher') },
        ],
        getAuditRecordId: (row) => row.recId,
      }}
      columns={columns}
      dataSource={{
        type: 'controlled',
        rows: (query.data?.transactions ?? []).map((row) => ({ ...row, id: String(row.recId) })),
        refresh: () => void query.refetch(),
      }}
      loading={query.isPending}
      error={query.isError ? query.error.message : null}
      onRetry={() => void query.refetch()}
      dataGridProps={{
        storageKey: 'accounts-receivable.customer-open-transactions.reference-view',
      }}
      belowGridContent={
        query.data?.reconciled === false ? (
          <Alert severity="warning">{t('partyBalance.notReconciled')}</Alert>
        ) : undefined
      }
    />
  );
}
