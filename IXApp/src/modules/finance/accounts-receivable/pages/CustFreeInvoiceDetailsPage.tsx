import React from 'react';
import { Alert, Box, Button, Stack, Tab, Tabs, TextField, Typography } from '@mui/material';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useAuth } from '@core/auth/useAuth';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { PERMISSIONS } from '@core/permissions/permissions';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailFieldConfig, DetailSectionConfig } from '@patterns/list-details/types';
import { DocumentTotalsDrawer } from '@patterns/document/DocumentTotalsDrawer';
import { EnterpriseCrudActions } from '@shared/components/action-pane/EnterpriseCrudActions';
import { ConfirmationDialog } from '@shared/components/dialogs/ConfirmationDialog';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef, DataGridHandle } from '@shared/components/data-grid/types';
import { useGridFilterRow } from '@shared/components/data-grid/hooks/useGridFilterRow';
import { EditableViewField } from '@shared/components/fields/EditableViewField';
import { FieldPresentationProvider } from '@shared/components/fields/FieldPresentationContext';
import { LookupField } from '@shared/components/lookups/LookupField';
import type { LookupOption } from '@shared/components/lookups/types';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';
import { useNotifications } from '@shared/hooks/useNotifications';
import { localizedName } from '@shared/utilities/localizedName';
import { customerQuickCreateApi } from '../api/customerQuickCreateApi';
import {
  freeTextInvoiceApi,
  type FreeTextInvoiceRecord,
  type FreeTextInvoiceLine,
  type FreeTextInvoiceInput,
} from '../api/freeTextInvoiceApi';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS as paths } from '../routes/accountsReceivableRoutePaths';

type Line = FreeTextInvoiceLine & { id: string; ledgerDisplay?: string };
type Invoice = Omit<FreeTextInvoiceRecord, 'lines'> & { lines: Line[] };
const asInvoice = (record: FreeTextInvoiceRecord): Invoice => ({
  ...record,
  lines: record.lines.map((line) => ({ ...line, id: String(line.recId) })),
});
const localDate = () => {
  const date = new Date();
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
};
const newLine = (): Line => ({
  id: crypto.randomUUID(),
  recId: 0,
  lineNum: 1,
  description: '',
  quantity: 1,
  unitPrice: 0,
  netAmount: 0,
  taxAmount: 0,
  grossAmount: 0,
  ledgerDimension: 0,
  defaultDimension: 0,
  taxGroup: '',
  taxItemGroup: '',
});
const newInvoice = (customerAccount: string): Invoice => ({
  id: 'new',
  recId: 0,
  invoiceId: '',
  customerAccount,
  invoiceAccount: '',
  description: '',
  invoiceDate: localDate(),
  dueDate: '',
  currencyCode: '',
  paymentTerms: '',
  paymentMethod: '',
  postingProfile: '',
  defaultDimension: 0,
  taxGroup: '',
  posted: false,
  netAmount: 0,
  taxAmount: 0,
  invoiceAmount: 0,
  lines: [newLine()],
});

/** The same enterprise master/details shell used by SalesOrderDetailsPage. */
export function CustFreeInvoiceDetailsPage(): React.ReactElement {
  const company = useCompanyStore((state) => state.currentCompany);
  const { invoiceRecId } = useParams<{ invoiceRecId: string }>();
  const [params] = useSearchParams();
  const isNew = !invoiceRecId || invoiceRecId === 'new';
  const { t } = useAppTranslation();
  const query = useQuery({
    queryKey: ['accounts-receivable', 'free-text-invoice', company, invoiceRecId],
    queryFn: ({ signal }) => freeTextInvoiceApi.get(Number(invoiceRecId), signal),
    enabled: !isNew && /^\d+$/.test(invoiceRecId ?? ''),
  });
  const accountContext = params.get('customerAccount') ?? '';
  const blank = React.useMemo(() => newInvoice(accountContext), [accountContext]);
  if (!isNew && query.isLoading) return <LoadingState message={t('common.loading')} />;
  if (!isNew && (!query.data || query.error))
    return (
      <ErrorState
        title={t('errors.generic')}
        message={
          query.error?.message ?? t('freeTextInvoices.notFound', 'Free text invoice was not found.')
        }
        onRetry={() => void query.refetch()}
      />
    );
  return (
    <InvoiceEditor
      key={`${company}:${invoiceRecId ?? 'new'}`}
      company={company}
      invoice={isNew ? blank : asInvoice(query.data!)}
      isNew={isNew}
      startEditing={params.get('edit') === 'true'}
    />
  );
}

function InvoiceEditor({
  company,
  invoice,
  isNew,
  startEditing,
}: {
  company: string;
  invoice: Invoice;
  isNew: boolean;
  startEditing: boolean;
}) {
  const { t, isRtl, currentLanguage } = useAppTranslation();
  const { hasPermission } = useAuth();
  const navigate = useNavigate();
  const cache = useQueryClient();
  const { notifySuccess } = useNotifications();
  const allowedEdit =
    hasPermission(
      isNew ? PERMISSIONS.FREE_TEXT_INVOICE_CREATE : PERMISSIONS.FREE_TEXT_INVOICE_EDIT
    ) && !invoice.posted;
  const [draft, setDraft] = React.useState<Invoice | null>(() =>
    allowedEdit && (isNew || startEditing) ? structuredClone(invoice) : null
  );
  const draftRef = React.useRef(draft);
  const [busy, setBusy] = React.useState(false);
  const operationLock = React.useRef(false);
  const [error, setError] = React.useState('');
  const [confirmation, setConfirmation] = React.useState<'post' | 'delete' | null>(null);
  const [totalsOpen, setTotalsOpen] = React.useState(false);
  const [tab, setTab] = React.useState('lines');
  const [lineTab, setLineTab] = React.useState('general');
  const [selectedLineId, setSelectedLineId] = React.useState<string | undefined>(
    invoice.lines[0]?.id
  );
  const [filterVisible, setFilterVisible] = useGridFilterRow(
    'accounts-receivable.free-invoice-lines'
  );
  const [ledgerLabels, setLedgerLabels] = React.useState<Record<number, string>>({});
  const grid = React.useRef<DataGridHandle>(null);
  const editing = Boolean(draft);
  const display = draft ?? invoice;
  const selectedLine = display.lines.find((line) => line.id === selectedLineId);
  useUnsavedChanges(editing);
  const changeDraft = (next: Invoice | null) => {
    draftRef.current = next;
    setDraft(next);
  };
  const update = (change: Partial<Invoice>) => {
    setDraft((current) => {
      if (!current) return current;
      const next = { ...current, ...change };
      draftRef.current = next;
      return next;
    });
  };
  const customers = useQuery({
    queryKey: ['accounts-receivable', 'customers', company],
    queryFn: ({ signal }) => customerQuickCreateApi.list(signal),
    enabled: hasPermission(PERMISSIONS.CUSTOMER_VIEW),
  });
  const lookups = useQuery({
    queryKey: ['accounts-receivable', 'free-invoice-lookups', company],
    queryFn: ({ signal }) => freeTextInvoiceApi.headerLookups(signal),
    enabled: editing,
  });
  const taxes = useQuery({
    queryKey: ['accounts-receivable', 'free-invoice-tax-groups', company],
    queryFn: ({ signal }) => freeTextInvoiceApi.taxGroups(signal),
    enabled: editing,
  });
  const ledgerReferences = useQuery({
    queryKey: ['accounts-receivable', 'free-invoice-ledger-references', company],
    queryFn: ({ signal }) =>
      freeTextInvoiceApi.ledgerDimensions({ pageNumber: 1, pageSize: 100, search: '', signal }),
  });
  const ledgerText = (id: number) =>
    ledgerLabels[id] ||
    ledgerReferences.data?.data.find((option) => String(option.id) === String(id))?.code;
  const invoices = useQuery({
    queryKey: ['accounts-receivable', 'free-text-invoices', company],
    queryFn: ({ signal }) => freeTextInvoiceApi.list(signal),
    enabled: !isNew,
  });
  const customer = customers.data?.find((row) => row.accountNumber === display.customerAccount);
  const customerName = customer
    ? localizedName({ name: customer.name, nameAlias: customer.nameAr }, isRtl)
    : display.customerAccount;
  const customerOptions: LookupOption[] = (customers.data ?? []).map((row) => ({
    id: row.accountNumber,
    code: row.accountNumber,
    name: row.name,
    nameAlias: row.nameAr,
  }));
  React.useEffect(() => {
    const current = draftRef.current;
    if (!isNew || !current || current.currencyCode || current.invoiceAccount) return;
    const row = customers.data?.find((item) => item.accountNumber === current.customerAccount);
    if (row)
      changeDraft({
        ...current,
        invoiceAccount: row.invoiceAccount || row.accountNumber,
        currencyCode: row.currencyCode,
        paymentTerms: row.paymTermId,
        paymentMethod: row.paymModeId,
        taxGroup: row.taxGroupId,
      });
  }, [customers.data, isNew]);
  const amount = (value: number) =>
    new Intl.NumberFormat(currentLanguage.code, {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(value);
  const beginEdit = () => {
    if (allowedEdit && !busy) {
      setError('');
      changeDraft(structuredClone(invoice));
    }
  };
  const switchTab = async (value: string) => {
    if (busy || (grid.current && !(await grid.current.saveEdit()))) return;
    setTab(value);
  };
  const invalidate = async () => {
    await Promise.all([
      cache.invalidateQueries({ queryKey: ['accounts-receivable', 'free-text-invoices', company] }),
      cache.invalidateQueries({
        queryKey: ['accounts-receivable', 'free-text-invoice', company, String(invoice.recId)],
      }),
    ]);
  };
  const save = async () => {
    if (!allowedEdit || !draftRef.current || operationLock.current) return;
    operationLock.current = true;
    setBusy(true);
    setError('');
    try {
      if (grid.current && !(await grid.current.saveEdit())) return;
      const record = draftRef.current!;
      if (
        !record.customerAccount.trim() ||
        !record.invoiceDate ||
        !record.lines.length ||
        record.lines.some(
          (line) =>
            !line.description.trim() ||
            !Number.isFinite(line.quantity) ||
            line.quantity <= 0 ||
            !Number.isFinite(line.unitPrice) ||
            line.unitPrice < 0 ||
            line.ledgerDimension <= 0
        )
      ) {
        throw new Error(
          t(
            'freeTextInvoices.requiredLines',
            'Select a customer and invoice date. Add at least one line with a description, ledger account, positive quantity and non-negative unit price.'
          )
        );
      }
      const input: FreeTextInvoiceInput = {
        customerAccount: record.customerAccount,
        invoiceAccount: record.invoiceAccount || undefined,
        invoiceDate: record.invoiceDate.slice(0, 10),
        currencyCode: record.currencyCode || undefined,
        paymentTerms: record.paymentTerms || undefined,
        paymentMethod: record.paymentMethod || undefined,
        postingProfile: record.postingProfile || undefined,
        taxGroup: record.taxGroup || undefined,
        description: record.description,
        defaultDimension: isNew ? record.defaultDimension || undefined : record.defaultDimension,
        lines: record.lines.map((line) => ({
          description: line.description,
          quantity: line.quantity,
          unitPrice: line.unitPrice,
          ledgerDimension: line.ledgerDimension,
          defaultDimension: line.recId ? line.defaultDimension : undefined,
          taxGroup: line.taxGroup || undefined,
          taxItemGroup: line.taxItemGroup || undefined,
        })),
      };
      const saved = await freeTextInvoiceApi.save(input, isNew ? undefined : invoice.recId);
      cache.setQueryData(
        ['accounts-receivable', 'free-text-invoice', company, String(saved.recId)],
        saved
      );
      changeDraft(null);
      navigate(paths.freeTextInvoice(saved.recId), { replace: true });
      await invalidate();
      setSelectedLineId(saved.lines[0] ? String(saved.lines[0].recId) : undefined);
      notifySuccess(t('messages.savedSuccessfully', 'Saved successfully.'));
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : t('errors.generic'));
    } finally {
      operationLock.current = false;
      setBusy(false);
    }
  };
  const runConfirmed = async () => {
    if (operationLock.current || editing || isNew || invoice.posted || !confirmation) return;
    if (
      !hasPermission(
        confirmation === 'post'
          ? PERMISSIONS.FREE_TEXT_INVOICE_POST
          : PERMISSIONS.FREE_TEXT_INVOICE_DELETE
      )
    )
      return;
    operationLock.current = true;
    setBusy(true);
    setError('');
    try {
      if (confirmation === 'post') {
        const posted = await freeTextInvoiceApi.post(invoice.recId);
        cache.setQueryData(
          ['accounts-receivable', 'free-text-invoice', company, String(invoice.recId)],
          { ...posted, id: String(posted.recId) }
        );
        notifySuccess(t('freeTextInvoices.postedSuccess', 'Invoice posted.'));
      } else {
        await freeTextInvoiceApi.remove(invoice.recId);
        navigate(paths.FREE_TEXT_INVOICES);
      }
      setConfirmation(null);
      await invalidate();
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : t('errors.generic'));
      setConfirmation(null);
    } finally {
      operationLock.current = false;
      setBusy(false);
    }
  };
  const textField = (
    name: keyof Invoice,
    label: string,
    readOnly = false,
    type: 'text' | 'date' = 'text'
  ): DetailFieldConfig => ({
    name,
    label,
    render: () =>
      editing && !readOnly ? (
        <TextField
          size="small"
          fullWidth
          type={type}
          value={String(display[name] ?? '').slice(0, type === 'date' ? 10 : undefined)}
          disabled={busy}
          slotProps={{ htmlInput: { 'aria-label': label } }}
          onChange={(event) => update({ [name]: event.target.value })}
        />
      ) : (
        <EditableViewField
          label={label}
          value={
            typeof display[name] === 'boolean'
              ? display[name]
                ? t('common.yes')
                : t('common.no')
              : String(display[name] ?? '').slice(0, type === 'date' ? 10 : undefined)
          }
          onEdit={!readOnly && allowedEdit ? beginEdit : undefined}
        />
      ),
  });
  const lookupField = (
    name: keyof Invoice,
    label: string,
    options: LookupOption[],
    onChange?: (value: string) => void
  ): DetailFieldConfig => ({
    name,
    label,
    render: () =>
      editing ? (
        <FieldPresentationProvider viewMode={false}>
          <LookupField
            name={name}
            label={label}
            value={String(display[name] ?? '')}
            options={options}
            displayMode="select"
            searchable
            disabled={busy}
            onChange={(value) =>
              onChange ? onChange(String(value ?? '')) : update({ [name]: String(value ?? '') })
            }
          />
        </FieldPresentationProvider>
      ) : (
        <EditableViewField
          label={label}
          lookup
          value={String(display[name] ?? '')}
          onEdit={allowedEdit ? beginEdit : undefined}
        />
      ),
    renderOwnLabel: editing,
  });
  const optionRows = (rows?: { value: string; label: string }[]) =>
    (rows ?? []).map((row) => ({ id: row.value, code: row.value, name: row.label }));
  const chooseCustomer = (account: string) => {
    const row = customers.data?.find((item) => item.accountNumber === account);
    update({
      customerAccount: account,
      invoiceAccount: row?.invoiceAccount || account,
      currencyCode: row?.currencyCode ?? '',
      paymentTerms: row?.paymTermId ?? '',
      paymentMethod: row?.paymModeId ?? '',
      taxGroup: row?.taxGroupId ?? '',
    });
  };
  const header: DetailSectionConfig = {
    id: 'invoice-header',
    editing,
    title: t('freeTextInvoices.header', 'Free text invoice header'),
    visualVariant: 'legalEntity',
    columns: 5,
    groups: [
      {
        id: 'customer',
        title: t('fields.customer', 'Customer').toUpperCase(),
        fields: [
          lookupField(
            'customerAccount',
            t('fields.customerAccount'),
            customerOptions,
            chooseCustomer
          ),
          {
            name: 'customerName',
            label: t('fields.name'),
            render: () => <EditableViewField label={t('fields.name')} value={customerName} />,
          },
        ],
      },
      {
        id: 'invoice-account',
        fields: [
          lookupField('invoiceAccount', t('fields.invoiceAccount'), customerOptions),
          textField('description', t('fields.description')),
        ],
      },
      {
        id: 'invoice',
        title: t('freeTextInvoices.invoice', 'Invoice').toUpperCase(),
        fields: [
          textField('invoiceDate', t('freeTextInvoices.invoiceDate'), false, 'date'),
          textField('dueDate', t('fields.dueDate', 'Due date'), true, 'date'),
          textField('invoiceId', t('freeTextInvoices.invoice'), true),
        ],
      },
      {
        id: 'currency',
        fields: [
          lookupField('currencyCode', t('fields.currency'), optionRows(lookups.data?.currencies)),
          textField('posted', t('freeTextInvoices.posted'), true),
          lookupField(
            'taxGroup',
            t('fields.salesTaxGroup', 'Sales tax group'),
            optionRows(lookups.data?.salesTaxGroups)
          ),
        ],
      },
      {
        id: 'payment',
        title: t('fields.payment', 'Payment').toUpperCase(),
        fields: [
          lookupField(
            'paymentTerms',
            t('freeTextInvoices.paymentTerms', 'Terms of payment'),
            optionRows(lookups.data?.paymentTerms)
          ),
          lookupField(
            'paymentMethod',
            t('fields.paymentMethod', 'Method of payment'),
            optionRows(lookups.data?.paymentMethods)
          ),
          textField('postingProfile', t('fields.postingProfile', 'Posting profile')),
        ],
      },
    ],
  };
  const lineColumns: ColumnDef<Line>[] = [
    { field: 'description', headerName: t('fields.description'), width: 250, editable: true },
    {
      field: 'ledgerDimension',
      headerName: t('freeTextInvoices.ledgerAccount', 'Ledger account'),
      width: 210,
      editable: true,
      renderCell: ({ row }) =>
        ledgerText(row.ledgerDimension) ||
        row.ledgerDisplay ||
        (row.ledgerDimension ? String(row.ledgerDimension) : ''),
      renderEditCell: ({ row, value, onChange, disabled }) => (
        <FieldPresentationProvider viewMode={false}>
          <LookupField
            name="ledgerDimension"
            label={t('freeTextInvoices.ledgerAccount', 'Ledger account')}
            value={Number(value) || undefined}
            options={
              value
                ? [
                    {
                      id: Number(value),
                      code: ledgerText(Number(value)) || row.ledgerDisplay || String(value),
                      name: ledgerText(Number(value)) || row.ledgerDisplay || String(value),
                    },
                  ]
                : []
            }
            queryKey={['free-invoice-ledger-dimensions', company]}
            fetchPage={freeTextInvoiceApi.ledgerDimensions}
            displayMode="select"
            searchable
            lazyLoading
            sideMode="server"
            disabled={disabled}
            onChange={(value, option) => {
              const selected = Array.isArray(option) ? option[0] : option;
              if (selected)
                setLedgerLabels((current) => ({
                  ...current,
                  [Number(value)]: selected.code || selected.name,
                }));
              onChange(Number(value) || 0);
            }}
          />
        </FieldPresentationProvider>
      ),
    },
    ...(['taxGroup', 'taxItemGroup'] as const).map((field): ColumnDef<Line> => ({
      field,
      headerName:
        field === 'taxGroup'
          ? t('fields.salesTaxGroup', 'Sales tax group')
          : t('freeTextInvoices.itemTaxGroup', 'Item sales tax group'),
      width: 170,
      editable: true,
      type: 'singleSelect',
      valueOptions: (field === 'taxGroup'
        ? taxes.data?.salesTaxGroups
        : taxes.data?.itemSalesTaxGroups
      )?.map((option) => ({ value: option.id, label: option.name })),
    })),
    {
      field: 'quantity',
      headerName: t('fields.quantity'),
      width: 110,
      type: 'number',
      editable: true,
    },
    {
      field: 'unitPrice',
      headerName: t('fields.unitPrice', 'Unit price'),
      width: 120,
      type: 'number',
      editable: true,
    },
    {
      field: 'netAmount',
      headerName: t('fields.amount', 'Amount'),
      width: 135,
      type: 'number',
      renderCell: ({ row }) => (editing ? '—' : amount(row.netAmount)),
    },
  ];
  const addLine = async () => {
    if (!draftRef.current || busy || !(await grid.current?.saveEdit())) return;
    const line = {
      ...newLine(),
      lineNum: draftRef.current.lines.length + 1,
      taxGroup: draftRef.current.taxGroup,
    };
    update({ lines: [...draftRef.current.lines, line] });
    setSelectedLineId(line.id);
  };
  const section = (id: string, title: string, content: React.ReactNode): DetailSectionConfig => ({
    id,
    title,
    visualVariant: 'legalEntity',
    content: <Box sx={{ gridColumn: '1 / -1', minWidth: 0 }}>{content}</Box>,
  });
  const sections: DetailSectionConfig[] = [
    ...(error || lookups.error || taxes.error
      ? [
          section(
            'error',
            t('common.information'),
            <Alert severity="error">
              {error || lookups.error?.message || taxes.error?.message}
            </Alert>
          ),
        ]
      : []),
    header,
    ...(tab === 'lines'
      ? [
          section(
            'invoice-lines',
            t('freeTextInvoices.lines', 'Invoice lines'),
            <>
              <Stack direction="row" spacing={1} sx={{ mb: 1 }}>
                <Button size="small" disabled={!editing || busy} onClick={() => void addLine()}>
                  + {t('freeTextInvoices.addLine', 'Add line')}
                </Button>
                <Button
                  size="small"
                  disabled={!editing || busy || !selectedLine}
                  onClick={() => {
                    grid.current?.cancelEdit();
                    update({ lines: display.lines.filter((line) => line.id !== selectedLineId) });
                    setSelectedLineId(undefined);
                  }}
                >
                  {t('actions.remove', 'Remove')}
                </Button>
              </Stack>
              <DataGrid<Line>
                ref={grid}
                rows={display.lines}
                columns={lineColumns}
                storageKey="accounts-receivable.free-invoice-lines"
                height={340}
                masterForm={editing && !busy}
                mobilePresentation="table"
                hideAddRowButton
                hideInlineEditActions
                hideFilterRow={!filterVisible}
                selectionMode="single"
                selectedIds={selectedLineId ? [selectedLineId] : []}
                onRowClick={(line) => setSelectedLineId(line.id)}
                onSelectionChange={(ids) => setSelectedLineId(ids[0] ? String(ids[0]) : undefined)}
                onRowSave={(row) => {
                  const current = draftRef.current;
                  if (!current || !row.id) return;
                  changeDraft({
                    ...current,
                    lines: current.lines.map((line) =>
                      line.id === row.id
                        ? {
                            ...line,
                            ...row,
                            quantity: Number(row.quantity ?? line.quantity),
                            unitPrice: Number(row.unitPrice ?? line.unitPrice),
                            ledgerDimension: Number(row.ledgerDimension ?? line.ledgerDimension),
                          }
                        : line
                    ),
                  });
                }}
              />
              {editing && (
                <Typography variant="caption">
                  {t(
                    'freeTextInvoices.serverAmounts',
                    'Amounts, sales tax and due date are calculated when you save.'
                  )}
                </Typography>
              )}
            </>
          ),
          section(
            'line-details',
            t('freeTextInvoices.lineDetails', 'Line details'),
            <>
              <Tabs
                value={lineTab}
                onChange={(_, value: string) => setLineTab(value)}
                aria-label={t('freeTextInvoices.lineDetails', 'Line details')}
                sx={{ minHeight: 32, mb: 1, '& .MuiTab-root': { minHeight: 32, fontSize: 12 } }}
              >
                <Tab value="general" label={t('common.general', 'General')} />
                <Tab
                  value="dimensions"
                  label={t('fields.financialDimensions', 'Financial dimensions line')}
                />
              </Tabs>
              {selectedLine ? (
                lineTab === 'general' ? (
                  <Box sx={{ maxWidth: 400 }}>
                    <Typography variant="caption">
                      {t('freeTextInvoices.invoiceText', 'Invoice text')}
                    </Typography>
                    <EditableViewField
                      label={t('fields.description')}
                      value={selectedLine.description}
                    />
                  </Box>
                ) : (
                  <Stack direction="row" spacing={4}>
                    <Box>
                      <Typography variant="caption">
                        {t('freeTextInvoices.ledgerAccount', 'Ledger account')}
                      </Typography>
                      <EditableViewField
                        label={t('freeTextInvoices.ledgerAccount')}
                        value={
                          ledgerText(selectedLine.ledgerDimension) ||
                          selectedLine.ledgerDisplay ||
                          selectedLine.ledgerDimension ||
                          ''
                        }
                      />
                    </Box>
                    <Box>
                      <Typography variant="caption">
                        {t('fields.defaultDimension', 'Default dimension')}
                      </Typography>
                      <EditableViewField
                        label={t('fields.defaultDimension')}
                        value={selectedLine.defaultDimension || ''}
                      />
                    </Box>
                  </Stack>
                )
              ) : (
                <Typography variant="body2">
                  {t('freeTextInvoices.selectLine', 'Select an invoice line.')}
                </Typography>
              )}
            </>
          ),
        ]
      : []),
  ];
  const records = React.useMemo(
    () => (isNew ? [invoice] : (invoices.data ?? [invoice]).map(asInvoice)),
    [isNew, invoice, invoices.data]
  );
  return (
    <ListDetailsPage<Invoice>
      variant="enterprise"
      title={
        isNew
          ? t('freeTextInvoices.newInvoice', 'New free text invoice')
          : `${invoice.customerAccount} - ${customerName}`
      }
      config={{
        readOnly: true,
        interactionLocked: editing || busy,
        initialSelectedId: invoice.id,
        dataSource: {
          type: 'controlled',
          records,
          onRecordsChange: () => undefined,
          refresh: () => {
            if (!editing && !busy) void invalidate();
          },
        },
        createRecord: () => invoice,
        getPrimaryText: (record) => record.invoiceId || t('freeTextInvoices.notPosted'),
        getSecondaryText: (record) => record.customerAccount,
        getValues: () => ({}),
        setValues: (record) => record,
        headerFields: [],
        sections,
        recordTableName: isNew ? undefined : 'CustInvoiceTable',
        getAuditRecordId: (record) => record.recId,
        showAttachmentAction: !isNew && !editing,
        onSelectionChange: (record) => {
          if (record && record.id !== invoice.id && !editing && !busy)
            navigate(paths.freeTextInvoice(record.recId));
        },
        backCommand: { onClick: () => navigate(paths.FREE_TEXT_INVOICES) },
        detailHeader: (
          <>
            <Typography color="primary" variant="body2" sx={{ mb: 1 }}>
              {t('freeTextInvoices.title')} | {t('common.standardView')}
            </Typography>
            <Typography variant="h5" sx={{ mb: 1 }}>
              {isNew
                ? t('freeTextInvoices.newInvoice', 'New free text invoice')
                : `${invoice.customerAccount} - ${customerName}`}
            </Typography>
            <Tabs
              value={tab}
              onChange={(_, value: string) => void switchTab(value)}
              aria-label={t('freeTextInvoices.title')}
              sx={{ minHeight: 32, mb: 1, '& .MuiTab-root': { minHeight: 32, fontSize: 12 } }}
            >
              <Tab value="lines" label={t('freeTextInvoices.linesTab', 'Lines')} />
              <Tab value="header" label={t('freeTextInvoices.headerTab', 'Header')} />
            </Tabs>
          </>
        ),
        onSearch: () => {
          setTab('lines');
          setFilterVisible((value) => !value);
        },
        presentation: {
          mode: 'list',
          listInitiallyVisible: false,
          listWidth: 280,
          listResizable: true,
          listVisibilityStorageKey: 'ixapp.free-invoice.list-visible',
          detailEndPadding: 8,
        },
        actionPaneAfterListContent: (
          <EnterpriseCrudActions
            editLabel={t('actions.edit')}
            newLabel={t('actions.new')}
            deleteLabel={t('actions.delete')}
            saveLabel={t('actions.save')}
            cancelLabel={t('actions.cancel')}
            editPermission={PERMISSIONS.FREE_TEXT_INVOICE_EDIT}
            newPermission={PERMISSIONS.FREE_TEXT_INVOICE_CREATE}
            deletePermission={PERMISSIONS.FREE_TEXT_INVOICE_DELETE}
            savePermission={
              isNew ? PERMISSIONS.FREE_TEXT_INVOICE_CREATE : PERMISSIONS.FREE_TEXT_INVOICE_EDIT
            }
            canEdit={allowedEdit && !isNew && !busy}
            canNew={!busy}
            canDelete={!isNew && !invoice.posted && !busy}
            editing={editing}
            saving={busy}
            onEdit={beginEdit}
            onNew={() => navigate(paths.newFreeTextInvoice(invoice.customerAccount))}
            onDelete={() => setConfirmation('delete')}
            onSave={() => void save()}
            onCancel={() => {
              grid.current?.cancelEdit();
              changeDraft(null);
              setError('');
              if (isNew) navigate(paths.FREE_TEXT_INVOICES);
              else navigate(paths.freeTextInvoice(invoice.recId), { replace: true });
            }}
          />
        ),
        commands: [
          ...(hasPermission(PERMISSIONS.FREE_TEXT_INVOICE_POST)
            ? [
                {
                  id: 'post',
                  label: t('freeTextInvoices.post'),
                  disabled: editing || busy || isNew || invoice.posted,
                  onClick: () => setConfirmation('post'),
                },
              ]
            : []),
          {
            id: 'totals',
            label: t('freeTextInvoices.totals'),
            disabled: editing || busy || isNew,
            onClick: () => setTotalsOpen(true),
          },
        ],
        relatedInformation: {
          title: t('relatedInformation.title'),
          sections: () => [
            {
              id: 'customer',
              label: t('fields.customer'),
              content: (
                <Typography>
                  {invoice.customerAccount} — {customerName}
                </Typography>
              ),
            },
          ],
        },
      }}
      dialogs={
        <>
          <ConfirmationDialog
            open={Boolean(confirmation)}
            title={confirmation === 'post' ? t('freeTextInvoices.post') : t('actions.delete')}
            message={
              confirmation === 'post'
                ? t('freeTextInvoices.confirmPost')
                : t('freeTextInvoices.confirmDelete', 'Delete this unposted free text invoice?')
            }
            confirmLabel={
              confirmation === 'post' ? t('freeTextInvoices.post') : t('actions.delete')
            }
            loading={busy}
            onClose={() => {
              if (!busy) setConfirmation(null);
            }}
            onConfirm={() => void runConfirmed()}
          />
          <DocumentTotalsDrawer
            open={totalsOpen}
            onClose={() => setTotalsOpen(false)}
            title={t('freeTextInvoices.totals')}
            viewLabel={t('common.standardView')}
            sections={[
              {
                id: 'totals',
                title: invoice.currencyCode,
                columns: [
                  {
                    id: 'amounts',
                    fields: [
                      {
                        id: 'net',
                        label: t('freeTextInvoices.netAmount'),
                        value: amount(invoice.netAmount),
                      },
                      {
                        id: 'tax',
                        label: t('freeTextInvoices.salesTax'),
                        value: amount(invoice.taxAmount),
                      },
                      {
                        id: 'total',
                        label: t('freeTextInvoices.invoiceAmount'),
                        value: amount(invoice.invoiceAmount),
                        emphasized: true,
                      },
                    ],
                  },
                ],
              },
            ]}
          />
        </>
      }
    />
  );
}
