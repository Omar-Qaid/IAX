import React, { useMemo, useState } from 'react';
import { Button, Stack, Typography } from '@mui/material';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import { ListInfoValue } from '@patterns/simple-list/ListInfoValue';
import { AppDialog } from '@shared/components/dialogs/AppDialog';
import { ConfirmationDialog } from '@shared/components/dialogs/ConfirmationDialog';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useNotifications } from '@shared/hooks/useNotifications';
import { localizedName } from '@shared/utilities/localizedName';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAuth } from '@core/auth/useAuth';
import { PERMISSIONS } from '@core/permissions/permissions';
import { FOUNDATION_RECORD_PREVIEWS } from '../../../foundation/previews/foundationRecordPreviews';
import { freeTextInvoiceApi, type FreeTextInvoiceRecord } from '../api/freeTextInvoiceApi';
import { customerQuickCreateApi } from '../../customer/api/customerQuickCreateApi';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../../routes/accountsReceivableRoutePaths';
import { ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS } from '../../previews/accountsReceivableRecordPreviews';
import { freeTextInvoiceQueryKeys } from '../../queries/accountsReceivableQueryKeys';

type InvoiceRow = FreeTextInvoiceRecord & { customerName: string };

const amount = (value: number) =>
  new Intl.NumberFormat(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(value);

/** Free text invoice list on SimpleList (reference: `SalesOrderListPage`); only page-specific config. */
export function CustFreeInvoiceListPage(): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const navigate = useNavigate();
  const cache = useQueryClient();
  const { hasPermission } = useAuth();
  const { notifySuccess, notifyError } = useNotifications();
  const company = useCompanyStore((state) => state.currentCompany);
  const [pendingPost, setPendingPost] = useState<InvoiceRow | null>(null);
  const [totals, setTotals] = useState<InvoiceRow | null>(null);
  const invoiceQuery = useQuery({
    queryKey: freeTextInvoiceQueryKeys.list(company),
    queryFn: ({ signal }) => freeTextInvoiceApi.list(signal),
  });
  const customerQuery = useQuery({
    queryKey: freeTextInvoiceQueryKeys.customers(company),
    queryFn: ({ signal }) => customerQuickCreateApi.list(signal),
    enabled: hasPermission(PERMISSIONS.CUSTOMER_VIEW),
  });
  const [searchParams] = useSearchParams();
  const customerAccount = searchParams.get('customerAccount');
  const postedLabel = t('freeTextInvoices.posted', 'Posted');
  const draftLabel = t('freeTextInvoices.notPosted', 'Not posted');

  const refreshList = () => cache.invalidateQueries({ queryKey: freeTextInvoiceQueryKeys.list(company) });
  const post = useMutation({
    mutationFn: (row: InvoiceRow) => freeTextInvoiceApi.post(row.recId),
    onSuccess: async () => {
      await refreshList();
      notifySuccess(t('freeTextInvoices.postedSuccess', 'Invoice posted.'));
    },
    onError: (error) => notifyError(error.message),
  });
  const remove = useMutation({
    mutationFn: (row: InvoiceRow) => freeTextInvoiceApi.remove(row.recId),
    onSuccess: async () => {
      await refreshList();
      notifySuccess(t('messages.deletedSuccessfully'));
    },
    onError: (error) => notifyError(error.message),
  });
  const busy = post.isPending || remove.isPending;

  const invoices = useMemo<InvoiceRow[]>(
    () =>
      (invoiceQuery.data ?? [])
        .filter((invoice) => !customerAccount || invoice.customerAccount === customerAccount)
        .map((invoice) => {
          const customer = customerQuery.data?.find((row) => row.accountNumber === invoice.customerAccount);
          return {
            ...invoice,
            customerName: customer ? localizedName({ name: customer.name, nameAlias: customer.nameAr }, isRtl) : '',
          };
        }),
    [invoiceQuery.data, customerQuery.data, customerAccount, isRtl]
  );

  const columns = useMemo<ColumnDef<InvoiceRow>[]>(
    () => [
      {
        field: 'invoiceId',
        headerName: 'freeTextInvoices.invoice',
        width: 160,
        pinned: 'left',
        valueGetter: ({ row }) => row.invoiceId || draftLabel,
        getDetailHref: (row) => ACCOUNTS_RECEIVABLE_ROUTE_PATHS.freeTextInvoice(row.recId),
      },
      { field: 'customerAccount', headerName: 'fields.customerAccount', width: 150, preview: { entity: ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS.CUSTOMER } },
      { field: 'customerName', headerName: 'fields.customerName', minWidth: 220, flex: 1 },
      { field: 'invoiceAccount', headerName: 'fields.invoiceAccount', width: 150, preview: { entity: ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS.CUSTOMER } },
      { field: 'currencyCode', headerName: 'fields.currency', width: 95, preview: { entity: FOUNDATION_RECORD_PREVIEWS.CURRENCY } },
      {
        field: 'invoiceDate',
        headerName: 'freeTextInvoices.invoiceDate',
        width: 135,
        type: 'date',
        // The API sends date-times; the list shows the date only.
        valueGetter: ({ row }) => row.invoiceDate.slice(0, 10),
      },
      { field: 'description', headerName: 'fields.description', width: 220 },
      {
        field: 'invoiceAmount',
        headerName: 'freeTextInvoices.invoiceAmount',
        width: 140,
        type: 'number',
        align: 'right',
        renderCell: ({ row }) => amount(row.invoiceAmount),
      },
      { field: 'posted', headerName: 'freeTextInvoices.posted', width: 95, type: 'boolean' },
    ],
    [draftLabel]
  );

  const config: EnterpriseListConfig<InvoiceRow> = {
    showSearchCommand: true,
    recordTableName: 'CustInvoiceTable',
    getAuditRecordId: (invoice) => invoice.recId,
    contextLabel: customerAccount ? `${t('freeTextInvoices.title')} | ${customerAccount}` : undefined,
    searchFields: [
      { field: 'invoiceId', label: t('freeTextInvoices.invoice') },
      { field: 'customerAccount', label: t('fields.customerAccount') },
      { field: 'customerName', label: t('fields.customerName') },
    ],
    crud: {
      editPermission: PERMISSIONS.FREE_TEXT_INVOICE_EDIT,
      newPermission: PERMISSIONS.FREE_TEXT_INVOICE_CREATE,
      deletePermission: PERMISSIONS.FREE_TEXT_INVOICE_DELETE,
      onEdit: (invoice) => navigate(`${ACCOUNTS_RECEIVABLE_ROUTE_PATHS.freeTextInvoice(invoice.recId)}?edit=true`),
      onNew: () => navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.newFreeTextInvoice(customerAccount ?? undefined)),
      // Posted invoices are read-only; delete works on one draft at a time.
      canEdit: (invoice) => !invoice.posted,
      canDelete: (selected) => !busy && selected.length === 1 && !selected[0].posted,
      onDelete: (selected) => remove.mutate(selected[0]),
    },
    getCommands: (invoice) => [
      {
        id: 'post',
        label: t('freeTextInvoices.post'),
        permission: PERMISSIONS.FREE_TEXT_INVOICE_POST,
        disabled: !invoice || invoice.posted || busy,
        onClick: () => setPendingPost(invoice),
      },
      {
        id: 'totals',
        label: t('freeTextInvoices.totals'),
        disabled: !invoice || busy,
        onClick: () => setTotals(invoice),
      },
    ],
    advancedFilter: {
      fieldLabel: t('freeTextInvoices.invoice'),
      operatorLabel: t('filters.contains'),
      fields: [
        { field: 'invoiceId', label: t('freeTextInvoices.invoice') },
        { field: 'customerAccount', label: t('fields.customerAccount') },
        { field: 'customerName', label: t('fields.customerName') },
        { field: 'invoiceAccount', label: t('fields.invoiceAccount') },
        { field: 'currencyCode', label: t('fields.currency') },
        { field: 'description', label: t('fields.description') },
      ],
      getValue: (invoice) => invoice.invoiceId,
    },
    relatedInformation: {
      sections: (invoice) => [
        {
          id: 'customer',
          label: t('fields.customer'),
          defaultExpanded: true,
          content: (
            <ListInfoValue
              value={invoice ? `${invoice.customerAccount}\n${invoice.customerName}` : ''}
              empty={t('freeTextInvoices.selectInvoice')}
            />
          ),
        },
        {
          id: 'amounts',
          label: t('freeTextInvoices.totals'),
          content: (
            <ListInfoValue
              value={invoice ? `${invoice.currencyCode} ${amount(invoice.invoiceAmount)}` : ''}
              empty={t('freeTextInvoices.selectInvoice')}
            />
          ),
        },
        {
          id: 'status',
          label: t('common.status'),
          content: (
            <ListInfoValue
              value={invoice ? (invoice.posted ? postedLabel : draftLabel) : ''}
              empty={t('freeTextInvoices.selectInvoice')}
            />
          ),
        },
      ],
    },
  };

  return (
    <SimpleListPage
      title={t('freeTextInvoices.title')}
      enterpriseConfig={config}
      dataSource={{ type: 'controlled', rows: invoices }}
      columns={columns}
      loading={invoiceQuery.isLoading}
      error={invoiceQuery.error instanceof Error ? invoiceQuery.error.message : null}
      onRetry={() => invoiceQuery.refetch()}
      dataGridProps={{
        storageKey: 'accounts-receivable.free-text-invoices.reference-view',
        // Post and Delete act on one invoice.
        selectionMode: 'single',
      }}
      dialogs={
        <>
          <ConfirmationDialog
            open={Boolean(pendingPost)}
            title={t('freeTextInvoices.post')}
            message={t(
              'freeTextInvoices.confirmPost',
              'Post the selected free text invoice? This creates posted accounting entries.'
            )}
            confirmLabel={t('freeTextInvoices.post')}
            loading={post.isPending}
            onClose={() => setPendingPost(null)}
            onConfirm={() => {
              if (pendingPost) post.mutate(pendingPost);
              setPendingPost(null);
            }}
          />
          <AppDialog
            open={Boolean(totals)}
            title={t('freeTextInvoices.totals')}
            onClose={() => setTotals(null)}
            actions={<Button onClick={() => setTotals(null)}>{t('actions.close')}</Button>}
          >
            {totals && (
              <Stack spacing={1}>
                <Typography>
                  {totals.invoiceId || draftLabel} | {totals.customerAccount} | {totals.currencyCode}
                </Typography>
                <Typography>
                  {t('freeTextInvoices.netAmount', 'Net amount')}: {amount(totals.netAmount)}
                </Typography>
                <Typography>
                  {t('freeTextInvoices.taxAmount', 'Sales tax')}: {amount(totals.taxAmount)}
                </Typography>
                <Typography>
                  {t('freeTextInvoices.invoiceAmount', 'Invoice amount')}: {amount(totals.invoiceAmount)}
                </Typography>
              </Stack>
            )}
          </AppDialog>
        </>
      }
    />
  );
}
