import React from 'react';
import { Alert } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useSearchParams } from 'react-router-dom';
import { useAuth } from '@core/auth/useAuth';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { custInvoiceJournalApi, type CustInvoiceJournalRecord } from '../api/custInvoiceJournalApi';

type Row = CustInvoiceJournalRecord & { id: string };

export function CustInvoiceJournal(): React.ReactElement {
  const company = useCompanyStore((state) => state.currentCompany);
  const [searchParams] = useSearchParams();
  const customerAccount = searchParams.get('customerAccount');
  const { hasPermission } = useAuth();
  const permitted = hasPermission(PERMISSIONS.SALES_ORDER_VIEW);
  const { t, isRtl } = useAppTranslation();
  const query = useQuery({
    queryKey: ['cust-invoice-journal', company],
    queryFn: ({ signal }) => custInvoiceJournalApi.load(signal),
    enabled: permitted && Boolean(company),
  });
  if (!permitted) return <Alert severity="warning">{t('partyBalance.accessDenied')}</Alert>;
  const number = new Intl.NumberFormat(isRtl ? 'ar-SA' : 'en-US', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
  const columns: ColumnDef<Row>[] = [
    { field: 'invoiceId', headerName: t('custInvoiceJournal.invoice'), width: 130 },
    { field: 'invoiceAccount', headerName: t('custInvoiceJournal.invoiceAccount'), width: 140 },
    { field: 'invoicingName', headerName: t('custInvoiceJournal.name'), width: 230 },
    { field: 'salesId', headerName: t('custInvoiceJournal.salesOrder'), width: 140 },
    {
      field: 'invoiceDate',
      headerName: t('custInvoiceJournal.date'),
      width: 130,
      valueGetter: ({ row }) => row.invoiceDate?.slice(0, 10),
    },
    { field: 'ledgerVoucher', headerName: t('custInvoiceJournal.voucher'), width: 140 },
    { field: 'currencyCode', headerName: t('custInvoiceJournal.currency'), width: 100 },
    {
      field: 'invoiceAmount',
      headerName: t('custInvoiceJournal.amount'),
      width: 150,
      align: 'right',
      renderCell: ({ row }) => number.format(row.invoiceAmount),
    },
    {
      field: 'intercompanyCompanyId',
      headerName: t('custInvoiceJournal.intercompany'),
      width: 170,
    },
    {
      field: 'intercompanyPosted',
      headerName: t('custInvoiceJournal.intercompanyPosted'),
      width: 180,
      valueGetter: ({ row }) =>
        t(row.intercompanyPosted === 1 ? 'custInvoiceJournal.yes' : 'custInvoiceJournal.no'),
    },
    {
      field: 'eInvoiceLineSpecific',
      headerName: t('custInvoiceJournal.lineSpecific'),
      width: 130,
      valueGetter: ({ row }) =>
        t(row.eInvoiceLineSpecific === 1 ? 'custInvoiceJournal.yes' : 'custInvoiceJournal.no'),
    },
  ];
  return (
    <SimpleListPage<Row>
      title={t('custInvoiceJournal.title')}
      recordTableName="CustInvoiceJour"
      enterpriseConfig={{
        readOnly: true,
        getAuditRecordId: (row) => row.recId,
        contextLabel: customerAccount
          ? `${t('custInvoiceJournal.title')} | ${customerAccount}`
          : t('custInvoiceJournal.title'),
      }}
      columns={columns}
      dataSource={{
        type: 'controlled',
        rows: (query.data ?? [])
          .filter((row) => !customerAccount || row.invoiceAccount === customerAccount)
          .map((row) => ({ ...row, id: String(row.recId) })),
        refresh: () => void query.refetch(),
      }}
      loading={query.isFetching}
      error={query.isError ? query.error.message : null}
      onRetry={() => void query.refetch()}
      dataGridProps={{ storageKey: 'accounts-receivable.cust-invoice-journal' }}
    />
  );
}
