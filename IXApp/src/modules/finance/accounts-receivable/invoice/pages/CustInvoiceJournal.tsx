import React from 'react';
import CheckIcon from '@mui/icons-material/Check';
import { Alert } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { ROUTE_PATHS } from '@app/routes/routePaths';
import { useAuth } from '@core/auth/useAuth';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import { ActionPaneButton } from '@shared/components/action-pane/ActionPaneButton';
import { ActionPaneRibbonTrigger } from '@shared/components/action-pane/ActionPaneRibbonTrigger';
import type { ActionPaneRibbonGroup } from '@shared/components/action-pane/ActionPaneRibbon';
import { documentApi } from '@shared/components/documents/documentApi';
import { recordTableId } from '@shared/components/documents/recordTableIds';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { custInvoiceJournalApi, type CustInvoiceJournalRecord } from '../api/custInvoiceJournalApi';

type Row = CustInvoiceJournalRecord & { id: string };

/** Attachment identity of the invoice journal's records. */
const INVOICE_JOURNAL_TABLE_ID = recordTableId('CustInvoiceJour');

export function CustInvoiceJournal(): React.ReactElement {
  const company = useCompanyStore((state) => state.currentCompany);
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const customerAccount = searchParams.get('customerAccount');
  const voucherFilter = searchParams.get('voucher');
  const { hasPermission } = useAuth();
  const permitted = hasPermission(PERMISSIONS.SALES_ORDER_VIEW);
  const { t, isRtl } = useAppTranslation();
  const [selected, setSelected] = React.useState<Row | null>(null);
  const query = useQuery({
    queryKey: ['cust-invoice-journal', company],
    queryFn: ({ signal }) => custInvoiceJournalApi.load(signal),
    enabled: permitted && Boolean(company),
  });
  const invoiceIds = (query.data ?? []).map((row) => row.recId);
  const attachments = useQuery({
    queryKey: ['cust-invoice-journal-attachments', company, invoiceIds],
    queryFn: ({ signal }) =>
      documentApi.recordsWithDocuments(INVOICE_JOURNAL_TABLE_ID, invoiceIds, signal),
    enabled: permitted && invoiceIds.length > 0,
  });
  const attachedIds = React.useMemo(() => new Set(attachments.data ?? []), [attachments.data]);
  if (!permitted) return <Alert severity="warning">{t('partyBalance.accessDenied')}</Alert>;
  const number = new Intl.NumberFormat(isRtl ? 'ar-SA' : 'en-US', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
  const columns: ColumnDef<Row>[] = [
    { field: 'invoiceId', headerName: t('custInvoiceJournal.invoice'), width: 130 },
    {
      preview: { entity: 'customer' },
      field: 'invoiceAccount',
      headerName: t('custInvoiceJournal.invoiceAccount'),
      width: 140,
    },
    { field: 'invoicingName', headerName: t('custInvoiceJournal.name'), width: 230 },
    { field: 'salesId', headerName: t('custInvoiceJournal.salesOrder'), width: 140 },
    { field: 'workerId', headerName: t('custInvoiceJournal.workerId'), width: 120 },
    { field: 'workerName', headerName: t('custInvoiceJournal.workerName'), width: 200 },
    { field: 'projId', headerName: t('custInvoiceJournal.project'), width: 130 },
    {
      field: 'hasAttachment',
      headerName: t('custInvoiceJournal.hasAttachment'),
      width: 140,
      align: 'center',
      renderCell: ({ row }) =>
        attachedIds.has(row.recId) ? <CheckIcon sx={{ fontSize: 16 }} /> : null,
    },
    {
      field: 'invoiceDate',
      headerName: t('custInvoiceJournal.date'),
      width: 130,
      valueGetter: ({ row }) => row.invoiceDate?.slice(0, 10),
    },
    { field: 'ledgerVoucher', headerName: t('custInvoiceJournal.voucher'), width: 170 },
    {
      preview: { entity: 'currency' },
      field: 'currencyCode',
      headerName: t('custInvoiceJournal.currency'),
      width: 100,
    },
    {
      field: 'invoiceAmount',
      headerName: t('custInvoiceJournal.amount'),
      width: 150,
      align: 'right',
      renderCell: ({ row }) => number.format(row.invoiceAmount),
    },
    {
      field: 'salesTaxAmount',
      headerName: t('custInvoiceJournal.salesTaxAmount'),
      width: 150,
      align: 'right',
      renderCell: ({ row }) => number.format(row.salesTaxAmount),
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

  const action = (id: string) => t(`custInvoiceJournal.actions.${id}`);
  /** A reference command of the D365 form that this build does not implement yet. */
  const unavailable = (id: string) => ({ id, label: action(id), disabled: true });
  const openVoucher = () =>
    navigate(
      `${ROUTE_PATHS.GENERAL_LEDGER.VOUCHER_TRANSACTIONS}?voucher=${encodeURIComponent(
        selected?.ledgerVoucher ?? ''
      )}&reference=${encodeURIComponent(
        selected ? `${selected.invoiceId} : ${selected.invoiceAccount}` : ''
      )}`
    );
  const invoiceRibbonGroups: ActionPaneRibbonGroup[] = [
    {
      id: 'accounting',
      label: t('custInvoiceJournal.groups.accounting'),
      actions: [
        unavailable('viewDistributions'),
        {
          id: 'viewAccounting',
          label: action('viewAccounting'),
          disabled: !selected?.ledgerVoucher,
          onClick: openVoucher,
        },
      ],
    },
    {
      id: 'ship',
      label: t('custInvoiceJournal.groups.ship'),
      actions: [
        unavailable('shippingSpecifications'),
        unavailable('billOfLading'),
        unavailable('viewHistoricalLoadData'),
      ],
    },
    {
      id: 'details',
      label: t('custInvoiceJournal.groups.details'),
      actions: [unavailable('commissionTransactions')],
    },
    {
      id: 'intercompany',
      label: t('custInvoiceJournal.groups.intercompany'),
      actions: [
        unavailable('synchronizeBatchSerialNumbers'),
        unavailable('generateIntercompanyInvoice'),
      ],
    },
    {
      id: 'document',
      label: t('custInvoiceJournal.groups.document'),
      actions: [
        {
          id: 'view',
          label: action('view'),
          items: [
            { id: 'invoice', label: action('invoice'), disabled: true, onClick: () => undefined },
            {
              id: 'originalPreview',
              label: action('originalPreview'),
              disabled: true,
              onClick: () => undefined,
            },
          ],
        },
        {
          id: 'send',
          label: action('send'),
          items: [
            { id: 'email', label: action('email'), disabled: true, onClick: () => undefined },
            { id: 'print', label: action('print'), disabled: true, onClick: () => undefined },
          ],
        },
      ],
    },
    {
      id: 'properties',
      label: t('custInvoiceJournal.groups.properties'),
      actions: [unavailable('electronicDocumentProperties')],
    },
  ];
  const actionPane = (
    <>
      <ActionPaneButton
        label={action('electronicInvoiceServiceRequests')}
        disabled
        tooltip={t('common.notAvailableYet')}
      />
      <ActionPaneButton
        label={action('postEInvoice')}
        disabled
        tooltip={t('common.notAvailableYet')}
      />
      <ActionPaneButton label={action('totals')} disabled tooltip={t('common.notAvailableYet')} />
      <ActionPaneButton
        label={action('voucher')}
        disabled={!selected?.ledgerVoucher}
        onClick={openVoucher}
      />
      <ActionPaneButton
        label={action('transactions')}
        disabled={!selected?.invoiceAccount}
        onClick={() =>
          navigate(
            ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUST_TRANS.replace(
              ':accountNum',
              encodeURIComponent(selected?.invoiceAccount ?? '')
            )
          )
        }
      />
      <ActionPaneButton label={action('charges')} disabled tooltip={t('common.notAvailableYet')} />
      <ActionPaneButton
        label={action('postedSalesTax')}
        disabled
        tooltip={t('common.notAvailableYet')}
      />
      <ActionPaneRibbonTrigger
        id="cust-invoice-journal-invoice"
        label={action('invoiceTab')}
        groups={invoiceRibbonGroups}
        persistenceKey="ixapp.cust-invoice-journal.invoice-ribbon"
      />
    </>
  );

  const rows = (query.data ?? [])
    .filter((row) => !customerAccount || row.invoiceAccount === customerAccount)
    .filter((row) => !voucherFilter || row.ledgerVoucher === voucherFilter)
    .map((row) => ({ ...row, id: String(row.recId) }));

  return (
    <SimpleListPage<Row>
      title={t('custInvoiceJournal.title')}
      recordTableName="CustInvoiceJour"
      actionPane={actionPane}
      enterpriseConfig={{
        readOnly: true,
        getAuditRecordId: (row) => row.recId,
        getRecordTitle: (row) => `${row.salesId || row.invoiceId} : ${row.invoicingName}`,
        contextLabel: customerAccount
          ? `${t('custInvoiceJournal.title')} | ${customerAccount}`
          : t('custInvoiceJournal.title'),
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
        storageKey: 'accounts-receivable.cust-invoice-journal',
        selectionMode: 'single',
        selectedIds: selected ? [selected.id] : [],
        onSelectionChange: (ids) =>
          setSelected(rows.find((row) => row.id === String(ids[0])) ?? null),
      }}
    />
  );
}
