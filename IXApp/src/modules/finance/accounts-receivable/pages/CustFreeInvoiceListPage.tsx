import React from 'react';
import { Alert, Button, MenuItem, Stack, TextField, Typography } from '@mui/material';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAuth } from '@core/auth/useAuth';
import { PERMISSIONS } from '@core/permissions/permissions';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import { AppDialog } from '@shared/components/dialogs/AppDialog';
import { ConfirmationDialog } from '@shared/components/dialogs/ConfirmationDialog';
import { useNotifications } from '@shared/hooks/useNotifications';
import { localizedName } from '@shared/utilities/localizedName';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { freeTextInvoiceApi, type FreeTextInvoiceRecord } from '../api/freeTextInvoiceApi';
import { customerQuickCreateApi } from '../api/customerQuickCreateApi';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../routes/accountsReceivableRoutePaths';

type InvoiceRow = FreeTextInvoiceRecord & { customerName: string; customerRecId?: number };
const localDate = (date: Date) =>
  `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
const amount = (value: number) =>
  new Intl.NumberFormat(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(
    value
  );

export function CustFreeInvoiceListPage(): React.ReactElement {
  const company = useCompanyStore((state) => state.currentCompany);
  return <CompanyFreeInvoiceListPage key={company} company={company} />;
}
function CompanyFreeInvoiceListPage({ company }: { company: string }) {
  const { t, isRtl, currentLanguage } = useAppTranslation();
  const { hasPermission } = useAuth();
  const cache = useQueryClient();
  const navigate = useNavigate();
  const { notifySuccess } = useNotifications();
  const [params] = useSearchParams();
  const customerAccount = params.get('customerAccount');
  const [filter, setFilter] = React.useState('');
  const [invoiceNumber, setInvoiceNumber] = React.useState('');
  const [status, setStatus] = React.useState('notPosted');
  const [dateRange, setDateRange] = React.useState('30');
  const [pendingPost, setPendingPost] = React.useState<InvoiceRow | null>(null);
  const [totals, setTotals] = React.useState<InvoiceRow | null>(null);
  const queryKey = ['accounts-receivable', 'free-text-invoices', company];
  const query = useQuery({ queryKey, queryFn: ({ signal }) => freeTextInvoiceApi.list(signal) });
  const customers = useQuery({
    queryKey: ['accounts-receivable', 'customers', company],
    queryFn: ({ signal }) => customerQuickCreateApi.list(signal),
    enabled: hasPermission(PERMISSIONS.CUSTOMER_VIEW),
  });
  const postedLabel = t('freeTextInvoices.posted', 'Posted');
  const draftLabel = t('freeTextInvoices.notPosted', 'Not posted');
  const post = useMutation({
    mutationFn: async (row: InvoiceRow) => {
      if (row.posted)
        throw new Error(t('freeTextInvoices.postedReadOnly', 'Posted invoices cannot be changed.'));
      return freeTextInvoiceApi.post(row.recId);
    },
    onSuccess: async () => {
      await cache.invalidateQueries({ queryKey });
      notifySuccess(t('freeTextInvoices.postedSuccess', 'Invoice posted.'));
    },
  });
  const remove = useMutation({
    mutationFn: async (rows: InvoiceRow[]) => {
      if (rows.length !== 1 || rows[0].posted)
        throw new Error(
          t('freeTextInvoices.deleteDraftOnly', 'Select one unposted invoice to delete.')
        );
      await freeTextInvoiceApi.remove(rows[0].recId);
    },
    onSuccess: async () => {
      await cache.invalidateQueries({ queryKey });
      notifySuccess(t('messages.deletedSuccessfully'));
    },
  });
  const busy = post.isPending || remove.isPending;
  const rows = React.useMemo(() => {
    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), today.getDate() - 29);
    const from = localDate(firstDay),
      through = localDate(today);
    return (query.data ?? [])
      .map((invoice) => {
        const customer = hasPermission(PERMISSIONS.CUSTOMER_VIEW)
          ? customers.data?.find((row) => row.accountNumber === invoice.customerAccount)
          : undefined;
        return {
          ...invoice,
          customerName: customer
            ? localizedName({ name: customer.name, nameAlias: customer.nameAr }, isRtl)
            : '',
          customerRecId: customer?.recId,
        };
      })
      .filter(
        (row) =>
          (!customerAccount || row.customerAccount === customerAccount) &&
          (status === 'all' || row.posted === (status === 'posted')) &&
          (dateRange === 'all' ||
            (row.invoiceDate.slice(0, 10) >= from && row.invoiceDate.slice(0, 10) <= through)) &&
          row.invoiceId.toLocaleLowerCase().includes(invoiceNumber.trim().toLocaleLowerCase()) &&
          [
            row.invoiceId,
            row.customerAccount,
            row.invoiceAccount,
            row.customerName,
            row.description,
            row.currencyCode,
          ]
            .join(' ')
            .toLocaleLowerCase()
            .includes(filter.trim().toLocaleLowerCase())
      );
  }, [
    query.data,
    customers.data,
    customerAccount,
    status,
    dateRange,
    invoiceNumber,
    filter,
    isRtl,
    hasPermission,
  ]);
  const columns = React.useMemo<ColumnDef<InvoiceRow>[]>(
    () => [
      {
        field: 'invoiceId',
        headerName: t('freeTextInvoices.invoice', 'Invoice'),
        width: 190,
        pinned: 'left',
        valueGetter: ({ row }) => row.invoiceId || draftLabel,
        getDetailHref: (row) => ACCOUNTS_RECEIVABLE_ROUTE_PATHS.freeTextInvoice(row.recId),
      },
      {
        field: 'customerAccount',
        headerName: t('fields.customerAccount'),
        width: 160,
        getDetailHref: (row) =>
          row.customerRecId
            ? ACCOUNTS_RECEIVABLE_ROUTE_PATHS.customer(String(row.customerRecId))
            : '',
      },
      { field: 'invoiceAccount', headerName: t('fields.invoiceAccount'), width: 160 },
      { field: 'customerName', headerName: t('fields.customerName'), minWidth: 220, flex: 1 },
      {
        field: 'invoiceDate',
        headerName: t('freeTextInvoices.date', 'Date'),
        width: 140,
        type: 'date',
      },
      { field: 'currencyCode', headerName: t('fields.currency'), width: 100 },
      { field: 'posted', headerName: postedLabel, width: 100, type: 'boolean' },
      { field: 'description', headerName: t('fields.description', 'Description'), width: 220 },
      {
        field: 'invoiceAmount',
        headerName: t('freeTextInvoices.invoiceAmount', 'Invoice amount'),
        width: 150,
        type: 'number',
        align: 'right',
        renderCell: ({ row }) => amount(row.invoiceAmount),
      },
    ],
    [t, draftLabel, postedLabel]
  );
  const config: EnterpriseListConfig<InvoiceRow> = {
    recordTableName: 'CustInvoiceTable',
    initialSelection: 'first',
    getAuditRecordId: (row) => row.recId,
    contextLabel: customerAccount
      ? `${t('freeTextInvoices.title', 'Free text invoices')} | ${customerAccount}`
      : t('freeTextInvoices.title', 'Free text invoices'),
    viewLabel: t('common.standardView'),
    filterLabel: t('actions.filter'),
    informationLabel: t('common.information'),
    locale: currentLanguage.code,
    backCommand: { label: t('actions.back'), onClick: () => navigate(-1) },
    searchFields: [
      { field: 'invoiceId', label: t('freeTextInvoices.invoice', 'Invoice') },
      { field: 'customerAccount', label: t('fields.customerAccount') },
      { field: 'customerName', label: t('fields.name', 'Name') },
    ],
    crud: {
      editLabel: t('actions.edit'),
      newLabel: t('actions.new'),
      deleteLabel: t('actions.delete'),
      editPermission: PERMISSIONS.FREE_TEXT_INVOICE_EDIT,
      newPermission: PERMISSIONS.FREE_TEXT_INVOICE_CREATE,
      deletePermission: PERMISSIONS.FREE_TEXT_INVOICE_DELETE,
      onNew: () => navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.newFreeTextInvoice(customerAccount ?? undefined)),
      onEdit: (row) => { if (!row.posted) navigate(`${ACCOUNTS_RECEIVABLE_ROUTE_PATHS.freeTextInvoice(row.recId)}?edit=true`); },
      canEdit: (row) => !row.posted,
      canDelete: (selected) => !busy && selected.length === 1 && !selected[0].posted,
      onDelete: (selected) => remove.mutate(selected),
    },
    getCommands: (row) => [
      {
        id: 'post',
        label: t('freeTextInvoices.post', 'Post'),
        permission: PERMISSIONS.FREE_TEXT_INVOICE_POST,
        disabled: !row || row.posted || busy,
        onClick: () => setPendingPost(row),
      },
      {
        id: 'totals',
        label: t('freeTextInvoices.totals', 'Totals'),
        disabled: !row || busy,
        onClick: () => setTotals(row),
      },
    ],
    utilities: {
      personalizeLabel: t('utilities.personalize'),
      guideLabel: t('utilities.guide'),
      notificationsLabel: t('common.notifications'),
      refreshLabel: t('actions.refresh'),
      openWindowLabel: t('utilities.openWindow'),
    },
    relatedInformation: {
      title: t('relatedInformation.title'),
      sections: (row) => [
        {
          id: 'invoice',
          label: t('freeTextInvoices.invoice', 'Invoice'),
          defaultExpanded: true,
          content: (
            <Typography variant="body2" sx={{ whiteSpace: 'pre-line' }}>
              {row
                ? `${row.invoiceId || draftLabel}\n${row.customerAccount}\n${row.currencyCode} ${amount(row.invoiceAmount)}\n${row.posted ? postedLabel : draftLabel}`
                : t('freeTextInvoices.selectInvoice', 'Select an invoice.')}
            </Typography>
          ),
        },
      ],
    },
  };
  return (
    <SimpleListPage<InvoiceRow>
      title={t('freeTextInvoices.title', 'Free text invoices')}
      recordTableName="CustInvoiceTable"
      enterpriseConfig={config}
      dataSource={{
        type: 'controlled',
        rows,
        refresh: () => {
          post.reset();
          remove.reset();
          void query.refetch();
        },
      }}
      columns={columns}
      loading={query.isLoading}
      error={query.error?.message}
      onRetry={() => void query.refetch()}
      dataGridProps={{
        storageKey: 'accounts-receivable.free-text-invoices',
        selectionMode: 'single',
      }}
      filterBar={
        <Stack direction="row" sx={{ flexWrap: 'wrap', gap: 1, mx: { xs: 1, sm: 2.5 }, mb: 1 }}>
          {(post.error || remove.error) && (
            <Alert severity="error" sx={{ width: '100%' }}>
              {(post.error || remove.error)?.message}
            </Alert>
          )}
          <TextField
            size="small"
            label={t('actions.filter')}
            value={filter}
            onChange={(event) => setFilter(event.target.value)}
          />
          <TextField
            size="small"
            label={t('freeTextInvoices.invoiceNumber', 'Invoice number')}
            value={invoiceNumber}
            onChange={(event) => setInvoiceNumber(event.target.value)}
          />
          <TextField
            size="small"
            select
            label={t('common.status')}
            value={status}
            onChange={(event) => setStatus(event.target.value)}
          >
            <MenuItem value="notPosted">{draftLabel}</MenuItem>
            <MenuItem value="posted">{postedLabel}</MenuItem>
            <MenuItem value="all">{t('common.all', 'All')}</MenuItem>
          </TextField>
          <TextField
            size="small"
            select
            label={t('freeTextInvoices.invoiceDate', 'Invoice date')}
            value={dateRange}
            onChange={(event) => setDateRange(event.target.value)}
          >
            <MenuItem value="30">{t('freeTextInvoices.last30Days', 'Last 30 days')}</MenuItem>
            <MenuItem value="all">{t('freeTextInvoices.allDates', 'All dates')}</MenuItem>
          </TextField>
        </Stack>
      }
      dialogs={
        <>
          <ConfirmationDialog
            open={Boolean(pendingPost)}
            title={t('freeTextInvoices.post', 'Post')}
            message={t(
              'freeTextInvoices.confirmPost',
              'Post the selected free text invoice? This creates posted accounting entries.'
            )}
            confirmLabel={t('freeTextInvoices.post', 'Post')}
            loading={post.isPending}
            onClose={() => setPendingPost(null)}
            onConfirm={() => {
              if (pendingPost) {
                post.mutate(pendingPost);
                setPendingPost(null);
              }
            }}
          />
          <AppDialog
            open={Boolean(totals)}
            title={t('freeTextInvoices.totals', 'Totals')}
            onClose={() => setTotals(null)}
            actions={<Button onClick={() => setTotals(null)}>{t('actions.close')}</Button>}
          >
            {totals && (
              <Stack spacing={1}>
                <Typography>
                  {totals.invoiceId || draftLabel} | {totals.customerAccount} |{' '}
                  {totals.currencyCode}
                </Typography>
                <Typography>
                  {t('freeTextInvoices.netAmount', 'Net amount')}: {amount(totals.netAmount)}
                </Typography>
                <Typography>
                  {t('freeTextInvoices.taxAmount', 'Sales tax')}: {amount(totals.taxAmount)}
                </Typography>
                <Typography>
                  {t('freeTextInvoices.invoiceAmount', 'Invoice amount')}:{' '}
                  {amount(totals.invoiceAmount)}
                </Typography>
              </Stack>
            )}
          </AppDialog>
        </>
      }
    />
  );
}
