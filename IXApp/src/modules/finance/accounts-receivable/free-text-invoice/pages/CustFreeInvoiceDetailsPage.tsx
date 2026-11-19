import React from 'react';
import { Alert, Box, TextField, Typography } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlined';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useAuth } from '@core/auth/useAuth';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { PERMISSIONS } from '@core/permissions/permissions';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import {
  MasterDetailLinesGrid,
  MasterDetailField,
  MasterDetailLineDetails,
  MasterDetailLines,
  MasterDetailPage,
  MasterDetailTabs,
  MasterDetailTotals,
} from '@patterns/master-detail';
import { freeTextInvoiceQueryKeys } from '../../queries/accountsReceivableQueryKeys';
import { ActionPaneBackButton } from '@shared/components/action-pane/ActionPaneBackButton';
import { recordTableId } from '@shared/components/documents';
import SearchIcon from '@mui/icons-material/Search';
import type {
  DetailFieldConfig,
  DetailSectionConfig,
  DetailValues,
} from '@patterns/list-details/types';
import { DocumentTotalsDrawer } from '@patterns/document/DocumentTotalsDrawer';
import { EnterpriseCrudActions } from '@shared/components/action-pane/EnterpriseCrudActions';
import { ConfirmationDialog } from '@shared/components/dialogs/ConfirmationDialog';
import type { ColumnDef, DataGridHandle } from '@shared/components/data-grid/types';
import { useGridFilterRow } from '@shared/components/data-grid/hooks/useGridFilterRow';
import { FieldPresentationProvider } from '@shared/components/fields/FieldPresentationContext';
import { LookupField } from '@shared/components/lookups/LookupField';
import { DataGridLookupField } from '@shared/components/lookups/DataGridLookupField';
import type { LookupOption } from '@shared/components/lookups/types';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';
import { useNotifications } from '@shared/hooks/useNotifications';
import { localizedName } from '@shared/utilities/localizedName';
import { customerQuickCreateApi } from '../../customer/api/customerQuickCreateApi';
import {
  freeTextInvoiceApi,
  type FreeTextInvoiceRecord,
  type FreeTextInvoiceLine,
  type FreeTextInvoiceInput,
} from '../api/freeTextInvoiceApi';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS as paths } from '../../routes/accountsReceivableRoutePaths';
import { ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS as AR_PREVIEWS } from '../../previews/accountsReceivableRecordPreviews';
import { FOUNDATION_RECORD_PREVIEWS as FOUNDATION_PREVIEWS } from '../../../foundation/previews/foundationRecordPreviews';

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

/**
 * Free text invoice details on the master-detail pattern, laid out like sales
 * order: Lines tab (header FastTab, lines, line details, totals) and Header tab
 * (General / Setup / Payment FastTabs), ribbon, record list and status.
 */
export function CustFreeInvoiceDetailsPage(): React.ReactElement {
  const company = useCompanyStore((state) => state.currentCompany);
  const { invoiceRecId } = useParams<{ invoiceRecId: string }>();
  const [params] = useSearchParams();
  const isNew = !invoiceRecId || invoiceRecId === 'new';
  const { t } = useAppTranslation();
  const query = useQuery({
    queryKey: freeTextInvoiceQueryKeys.invoice(company, invoiceRecId),
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
    queryKey: freeTextInvoiceQueryKeys.customers(company),
    queryFn: ({ signal }) => customerQuickCreateApi.list(signal),
    enabled: hasPermission(PERMISSIONS.CUSTOMER_VIEW),
  });
  const lookups = useQuery({
    queryKey: freeTextInvoiceQueryKeys.lookup(company, 'options'),
    queryFn: ({ signal }) => freeTextInvoiceApi.headerLookups(signal),
    enabled: editing,
  });
  const taxes = useQuery({
    queryKey: freeTextInvoiceQueryKeys.lookup(company, 'tax-groups'),
    queryFn: ({ signal }) => freeTextInvoiceApi.taxGroups(signal),
    enabled: editing,
  });
  const ledgerReferences = useQuery({
    queryKey: freeTextInvoiceQueryKeys.lookup(company, 'ledger-references'),
    queryFn: ({ signal }) =>
      freeTextInvoiceApi.ledgerDimensions({ pageNumber: 1, pageSize: 100, search: '', signal }),
  });
  const ledgerText = (id: number) =>
    ledgerLabels[id] ||
    ledgerReferences.data?.data.find((option) => String(option.id) === String(id))?.code;
  const invoices = useQuery({
    queryKey: freeTextInvoiceQueryKeys.list(company),
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
      cache.invalidateQueries({ queryKey: freeTextInvoiceQueryKeys.list(company) }),
      cache.invalidateQueries({
        queryKey: freeTextInvoiceQueryKeys.invoice(company, String(invoice.recId)),
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
        cache.setQueryData(freeTextInvoiceQueryKeys.invoice(company, String(invoice.recId)), {
          ...posted,
          id: String(posted.recId),
        });
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
  // View mode: plain field configs, rendered by the shared engine from `values`
  // (hover cards for master data, pencil to start editing). Edit mode: editors.
  const readOnlyFields = new Set(['customerName', 'invoiceId', 'dueDate', 'posted']);
  const values: DetailValues = {
    customerAccount: display.customerAccount,
    customerName,
    invoiceAccount: display.invoiceAccount,
    description: display.description,
    invoiceId: display.invoiceId || t('freeTextInvoices.notPosted'),
    invoiceDate: display.invoiceDate.slice(0, 10),
    dueDate: (display.dueDate ?? '').slice(0, 10),
    currencyCode: display.currencyCode,
    posted: display.posted,
    taxGroup: display.taxGroup,
    paymentTerms: display.paymentTerms,
    paymentMethod: display.paymentMethod,
    postingProfile: display.postingProfile,
  };
  const textField = (
    name: keyof Invoice,
    label: string,
    type: 'text' | 'date' = 'text'
  ): DetailFieldConfig =>
    editing && !readOnlyFields.has(name)
      ? {
          name,
          label,
          render: () => (
            <TextField
              size="small"
              fullWidth
              type={type}
              value={String(display[name] ?? '').slice(0, type === 'date' ? 10 : undefined)}
              disabled={busy}
              slotProps={{ htmlInput: { 'aria-label': label } }}
              onChange={(event) => update({ [name]: event.target.value })}
            />
          ),
        }
      : { name, label, type: readOnlyFields.has(name) ? 'display' : 'text' };
  const displayField = (
    name: string,
    label: string,
    type: 'display' | 'boolean' = 'display'
  ): DetailFieldConfig =>
    type === 'boolean' && !editing
      ? { name, label, type: 'boolean' }
      : { name, label, type: 'display' };
  const lookupField = (
    name: keyof Invoice,
    label: string,
    options: LookupOption[],
    previewEntity: string,
    onChange?: (value: string) => void
  ): DetailFieldConfig =>
    editing
      ? {
          name,
          label,
          renderOwnLabel: true,
          render: () => (
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
          ),
        }
      : { name, label, previewEntity };
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

  // Header fields, shared by the compact header (Lines tab) and the Header tab FastTabs.
  const fields = {
    customerAccount: lookupField(
      'customerAccount',
      t('fields.customerAccount'),
      customerOptions,
      AR_PREVIEWS.CUSTOMER,
      chooseCustomer
    ),
    customerName: displayField('customerName', t('fields.name')),
    invoiceAccount: lookupField(
      'invoiceAccount',
      t('fields.invoiceAccount'),
      customerOptions,
      AR_PREVIEWS.CUSTOMER
    ),
    description: textField('description', t('fields.description')),
    invoiceId: displayField('invoiceId', t('freeTextInvoices.invoice')),
    invoiceDate: textField('invoiceDate', t('freeTextInvoices.invoiceDate'), 'date'),
    dueDate: displayField('dueDate', t('freeTextInvoices.dueDate')),
    posted: displayField('posted', t('freeTextInvoices.posted'), 'boolean'),
    currencyCode: lookupField(
      'currencyCode',
      t('fields.currency'),
      optionRows(lookups.data?.currencies),
      FOUNDATION_PREVIEWS.CURRENCY
    ),
    taxGroup: lookupField(
      'taxGroup',
      t('fields.salesTaxGroup'),
      optionRows(lookups.data?.salesTaxGroups),
      FOUNDATION_PREVIEWS.SALES_TAX_GROUP
    ),
    paymentTerms: lookupField(
      'paymentTerms',
      t('freeTextInvoices.paymentTerms'),
      optionRows(lookups.data?.paymentTerms),
      AR_PREVIEWS.PAYMENT_TERMS
    ),
    paymentMethod: lookupField(
      'paymentMethod',
      t('freeTextInvoices.paymentMethod'),
      optionRows(lookups.data?.paymentMethods),
      AR_PREVIEWS.PAYMENT_METHOD
    ),
    postingProfile: textField('postingProfile', t('freeTextInvoices.postingProfile')),
  };
  const group = (id: string, title: string, groupFields: DetailFieldConfig[]) => ({
    id,
    title: title.toUpperCase(),
    fields: groupFields,
  });
  const fastTab = (id: string, title: string, content: React.ReactNode): DetailSectionConfig => ({
    id,
    title,
    visualVariant: 'legalEntity',
    content: <Box sx={{ gridColumn: '1 / -1', minWidth: 0 }}>{content}</Box>,
  });

  // Lines tab: compact header FastTab (one row of field groups), like sales order.
  const compactHeader: DetailSectionConfig = {
    id: 'invoice-header',
    title: t('freeTextInvoices.header'),
    visualVariant: 'legalEntity',
    columns: 5,
    groups: [
      group('customer', t('fields.customer'), [fields.customerAccount, fields.customerName]),
      group('invoice', t('freeTextInvoices.invoice'), [fields.invoiceDate, fields.dueDate]),
      group('references', t('fields.description'), [fields.invoiceAccount, fields.description]),
      group('sales-tax', t('freeTextInvoices.salesTax'), [fields.currencyCode, fields.taxGroup]),
      group('payment', t('freeTextInvoices.payment'), [fields.paymentTerms, fields.paymentMethod]),
    ],
  };
  // Header tab: the full header as FastTabs with key values in the heading.
  const headerSections: DetailSectionConfig[] = [
    {
      id: 'header-general',
      title: t('common.general'),
      visualVariant: 'legalEntity',
      columns: 4,
      summaryItems: [
        {
          id: 'invoiceId',
          label: t('freeTextInvoices.invoice'),
          value: values.invoiceId,
          accent: true,
        },
        {
          id: 'customerAccount',
          label: t('fields.customerAccount'),
          value: display.customerAccount,
          accent: true,
        },
        { id: 'customerName', label: t('fields.customerName'), value: customerName },
      ],
      groups: [
        group('customer', t('fields.customer'), [
          fields.customerAccount,
          fields.customerName,
          fields.invoiceAccount,
        ]),
        group('invoice', t('freeTextInvoices.invoice'), [
          fields.invoiceId,
          fields.invoiceDate,
          fields.dueDate,
        ]),
        group('description', t('fields.description'), [fields.description]),
        group('status', t('common.status'), [fields.posted]),
      ],
    },
    {
      id: 'header-setup',
      title: t('freeTextInvoices.setup'),
      visualVariant: 'legalEntity',
      columns: 4,
      summaryItems: [
        {
          id: 'currencyCode',
          label: t('fields.currency'),
          value: display.currencyCode,
          accent: true,
        },
        { id: 'taxGroup', label: t('fields.salesTaxGroup'), value: display.taxGroup, accent: true },
      ],
      groups: [
        group('currency', t('fields.currency'), [fields.currencyCode]),
        group('sales-tax', t('freeTextInvoices.salesTax'), [fields.taxGroup]),
        group('posting', t('freeTextInvoices.posting'), [fields.postingProfile]),
      ],
    },
    {
      id: 'header-payment',
      title: t('freeTextInvoices.payment'),
      visualVariant: 'legalEntity',
      columns: 4,
      summaryItems: [
        {
          id: 'paymentTerms',
          label: t('freeTextInvoices.paymentTerms'),
          value: display.paymentTerms,
          accent: true,
        },
      ],
      groups: [
        group('payment', t('freeTextInvoices.payment'), [
          fields.paymentTerms,
          fields.paymentMethod,
        ]),
      ],
    },
  ];
  const lineColumns: ColumnDef<Line>[] = [
    { field: 'description', headerName: t('fields.description'), width: 250, editable: true },
    {
      field: 'ledgerDimension',
      headerName: t('freeTextInvoices.ledgerAccount'),
      width: 210,
      editable: true,
      renderCell: ({ row }) =>
        ledgerText(row.ledgerDimension) ||
        row.ledgerDisplay ||
        (row.ledgerDimension ? String(row.ledgerDimension) : ''),
      renderEditCell: ({ row, value, onChange, disabled }) => (
        <>
          <DataGridLookupField
            name="ledgerDimension"
            label={t('freeTextInvoices.ledgerAccount')}
            value={Number(value) || null}
            options={
              value
                ? [
                    {
                      id: Number(value),
                      code: ledgerText(Number(value)) || row.ledgerDisplay || String(value),
                      name: '',
                    },
                  ]
                : []
            }
            queryKey={freeTextInvoiceQueryKeys.lookup(company, 'ledger-dimensions')}
            fetchPage={freeTextInvoiceApi.ledgerDimensions}
            searchable
            lazyLoading
            sideMode="server"
            disabled={disabled}
            onChange={(next, option) => {
              const selected = Array.isArray(option) ? option[0] : option;
              if (selected)
                setLedgerLabels((current) => ({
                  ...current,
                  [Number(next)]: selected.code || selected.name,
                }));
              onChange(Number(next) || 0);
            }}
          />
        </>
      ),
    },
    ...(['taxGroup', 'taxItemGroup'] as const).map((field): ColumnDef<Line> => ({
      field,
      headerName:
        field === 'taxGroup' ? t('fields.salesTaxGroup') : t('freeTextInvoices.itemTaxGroup'),
      width: 170,
      editable: true,
      preview: {
        entity:
          field === 'taxGroup'
            ? FOUNDATION_PREVIEWS.SALES_TAX_GROUP
            : FOUNDATION_PREVIEWS.ITEM_SALES_TAX_GROUP,
      },
      renderEditCell: ({ value, onChange, disabled }) => (
        <>
          <DataGridLookupField
            name={field}
            label={
              field === 'taxGroup' ? t('fields.salesTaxGroup') : t('freeTextInvoices.itemTaxGroup')
            }
            value={String(value ?? '') || null}
            options={(field === 'taxGroup'
              ? taxes.data?.salesTaxGroups
              : taxes.data?.itemSalesTaxGroups
            )?.map((option) => ({ id: option.id, code: String(option.id), name: option.name }))}
            searchable
            lazyLoading={false}
            disabled={disabled || taxes.isLoading}
            onChange={(next) => onChange(String(next ?? ''))}
          />
        </>
      ),
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
      headerName: t('fields.unitPrice'),
      width: 120,
      type: 'number',
      editable: true,
    },
    {
      field: 'netAmount',
      headerName: t('freeTextInvoices.amount'),
      width: 135,
      type: 'number',
      renderCell: ({ row }) => (editing ? '—' : amount(row.netAmount)),
    },
  ];
  const addLine = async () => {
    // View mode: Add line switches the page to edit mode first (D365).
    if (!editing) beginEdit();
    if (!draftRef.current || busy || !(await grid.current?.saveEdit())) return;
    const line = {
      ...newLine(),
      lineNum: draftRef.current.lines.length + 1,
      taxGroup: draftRef.current.taxGroup,
    };
    update({ lines: [...draftRef.current.lines, line] });
    setSelectedLineId(line.id);
  };
  const removeLine = () => {
    grid.current?.cancelEdit();
    update({ lines: display.lines.filter((line) => line.id !== selectedLineId) });
    setSelectedLineId(undefined);
  };
  const ledgerOf = (line: Line) =>
    ledgerText(line.ledgerDimension) ||
    line.ledgerDisplay ||
    (line.ledgerDimension ? String(line.ledgerDimension) : '');
  const lineDetailFields = (line: Line) =>
    lineTab === 'general' ? (
      <>
        <MasterDetailField label={t('freeTextInvoices.invoiceText')} value={line.description} />
        <MasterDetailField label={t('fields.quantity')} value={line.quantity} numeric />
        <MasterDetailField label={t('fields.unitPrice')} value={amount(line.unitPrice)} numeric />
        <MasterDetailField
          label={t('freeTextInvoices.amount')}
          value={editing ? '—' : amount(line.netAmount)}
          numeric
        />
      </>
    ) : lineTab === 'setup' ? (
      <>
        <MasterDetailField
          label={t('fields.salesTaxGroup')}
          value={line.taxGroup}
          previewEntity={FOUNDATION_PREVIEWS.SALES_TAX_GROUP}
        />
        <MasterDetailField
          label={t('freeTextInvoices.itemTaxGroup')}
          value={line.taxItemGroup}
          previewEntity={FOUNDATION_PREVIEWS.ITEM_SALES_TAX_GROUP}
        />
      </>
    ) : (
      <>
        <MasterDetailField
          label={t('freeTextInvoices.ledgerAccount')}
          value={ledgerOf(line)}
          lookup
        />
        <MasterDetailField
          label={t('freeTextInvoices.defaultDimension')}
          value={line.defaultDimension || ''}
        />
      </>
    );
  const totalsSection = fastTab(
    'totals',
    t('freeTextInvoices.totals'),
    <MasterDetailTotals
      items={[
        { id: 'net', label: t('freeTextInvoices.netAmount'), value: amount(display.netAmount) },
        { id: 'tax', label: t('freeTextInvoices.salesTax'), value: amount(display.taxAmount) },
        {
          id: 'total',
          label: t('freeTextInvoices.invoiceAmount'),
          value: `${amount(display.invoiceAmount)} ${display.currencyCode}`,
          emphasis: true,
        },
      ]}
    />
  );
  const problem = error || lookups.error?.message || taxes.error?.message;
  const sections: DetailSectionConfig[] = [
    ...(problem
      ? [fastTab('error', t('common.information'), <Alert severity="error">{problem}</Alert>)]
      : []),
    ...(tab === 'header'
      ? headerSections
      : [
          compactHeader,
          fastTab(
            'invoice-lines',
            t('freeTextInvoices.lines'),
            <MasterDetailLines
              toolbarLabel={t('freeTextInvoices.lines')}
              actions={[
                {
                  id: 'add',
                  label: t('freeTextInvoices.addLine'),
                  icon: <AddIcon />,
                  disabled: (!editing && !allowedEdit) || busy,
                  onClick: () => void addLine(),
                },
                {
                  id: 'remove',
                  label: t('actions.remove'),
                  icon: <DeleteOutlineIcon />,
                  disabled: !editing || busy || !selectedLine,
                  onClick: removeLine,
                },
              ]}
              saving={busy}
              grid={
                <>
                  <MasterDetailLinesGrid<Line>
                    editing={editing}
                    busy={busy}
                    canEdit={allowedEdit}
                    onEdit={beginEdit}
                    ref={grid}
                    rows={display.lines}
                    columns={lineColumns}
                    storageKey="accounts-receivable.free-invoice-lines"
                    hideFilterRow={!filterVisible}
                    selectionMode="single"
                    selectedIds={selectedLineId ? [selectedLineId] : []}
                    onRowClick={(line) => setSelectedLineId(line.id)}
                    onSelectionChange={(ids) =>
                      setSelectedLineId(ids[0] ? String(ids[0]) : undefined)
                    }
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
                                ledgerDimension: Number(
                                  row.ledgerDimension ?? line.ledgerDimension
                                ),
                              }
                            : line
                        ),
                      });
                    }}
                  />
                  {editing && (
                    <Typography
                      variant="caption"
                      color="text.secondary"
                      sx={{ display: 'block', mt: 0.5 }}
                    >
                      {t('freeTextInvoices.serverAmounts')}
                    </Typography>
                  )}
                </>
              }
            />
          ),
          fastTab(
            'line-details',
            t('freeTextInvoices.lineDetails'),
            <MasterDetailLineDetails
              ariaLabel={t('freeTextInvoices.lineDetails')}
              tabs={[
                { value: 'general', label: t('common.general') },
                { value: 'setup', label: t('freeTextInvoices.setup') },
                { value: 'dimensions', label: t('freeTextInvoices.financialDimensions') },
              ]}
              value={lineTab}
              onChange={setLineTab}
              hasLine={Boolean(selectedLine)}
              emptyLabel={t('freeTextInvoices.selectLine')}
            >
              {selectedLine && lineDetailFields(selectedLine)}
            </MasterDetailLineDetails>
          ),
        ]),
    totalsSection,
  ];
  const records = React.useMemo(
    () => (isNew ? [invoice] : (invoices.data ?? [invoice]).map(asInvoice)),
    [isNew, invoice, invoices.data]
  );
  // Navigation pane rows: invoice number (or "not posted") and customer account.
  const nameOf = (account: string) => {
    const row = customers.data?.find((item) => item.accountNumber === account);
    return row ? localizedName({ name: row.name, nameAlias: row.nameAr }, isRtl) : '';
  };
  const navigationRecords = records.map((record) => ({
    id: record.id,
    title: record.invoiceId || t('freeTextInvoices.notPosted'),
    subtitle: [record.customerAccount, nameOf(record.customerAccount)].filter(Boolean).join(' - '),
    recId: record.recId,
  }));
  // "INV-001 : Customer name", like "SO-001 : Customer name".
  const title = isNew
    ? t('freeTextInvoices.newInvoice', 'New free text invoice')
    : `${invoice.invoiceId || t('freeTextInvoices.notPosted')} : ${customerName}`;
  return (
    <>
      <MasterDetailPage
        records={navigationRecords}
        selectedId={invoice.id}
        onSelect={(id) => {
          const record = navigationRecords.find((row) => row.id === id);
          if (record && id !== invoice.id && !editing && !busy)
            navigate(paths.freeTextInvoice(record.recId));
        }}
        title={title}
        subtitle={t('freeTextInvoices.details')}
        viewLabel={t('common.standardView')}
        status={
          isNew
            ? undefined
            : invoice.posted
              ? t('freeTextInvoices.posted')
              : t('freeTextInvoices.notPosted')
        }
        filterLabel={t('actions.filter')}
        emptyLabel={t('messages.noRecords', 'No records')}
        editing={editing || busy}
        navigationWidthStorageKey="free-invoice"
        backAction={
          <ActionPaneBackButton
            label={t('actions.back')}
            onClick={() => navigate(paths.FREE_TEXT_INVOICES)}
            disabled={editing || busy}
          />
        }
        actionPane={
          <>
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
          </>
        }
        ribbons={[
          {
            id: 'free-text-invoice',
            label: t('freeTextInvoices.freeTextInvoice'),
            persistenceKey: 'accounts-receivable.free-text-invoice.ribbon-pinned',
            groups: [
              {
                id: 'post',
                label: t('freeTextInvoices.post'),
                actions: hasPermission(PERMISSIONS.FREE_TEXT_INVOICE_POST)
                  ? [
                      {
                        id: 'post',
                        label: t('freeTextInvoices.post'),
                        disabled: busy || isNew || invoice.posted,
                        onClick: () => setConfirmation('post'),
                      },
                    ]
                  : [],
              },
              {
                id: 'view',
                label: t('freeTextInvoices.view'),
                actions: [
                  {
                    id: 'totals',
                    label: t('freeTextInvoices.totals'),
                    disabled: busy || isNew,
                    onClick: () => setTotalsOpen(true),
                  },
                ],
              },
            ],
          },
        ]}
        commands={[
          { id: 'general', label: t('common.general'), disabled: true },
          {
            id: 'search',
            label: t('common.search'),
            icon: <SearchIcon />,
            onClick: () => {
              setTab('lines');
              setFilterVisible((value) => !value);
            },
          },
        ]}
        values={values}
        onFieldEdit={beginEdit}
        canEditField={(name) => allowedEdit && !busy && !readOnlyFields.has(name)}
        tabs={
          <MasterDetailTabs
            ariaLabel={t('freeTextInvoices.title')}
            value={tab}
            onChange={(value) => void switchTab(value)}
            tabs={[
              { value: 'lines', label: t('freeTextInvoices.linesTab', 'Lines') },
              { value: 'header', label: t('freeTextInvoices.headerTab', 'Header') },
            ]}
          />
        }
        sections={sections}
        recordTableName={isNew ? undefined : 'CustInvoiceTable'}
        getAuditRecordId={(record) => record.recId}
        attachments={
          !isNew && !editing
            ? { refTableId: recordTableId('CustInvoiceTable'), refRecId: invoice.recId }
            : undefined
        }
        onRefresh={() => {
          if (!editing && !busy) void invalidate();
        }}
        relatedInformation={{
          title: t('relatedInformation.title'),
          sections: [
            {
              id: 'customer',
              label: t('fields.customer'),
              defaultExpanded: true,
              content: (
                <Typography sx={{ fontSize: '0.75rem', whiteSpace: 'pre-line' }}>
                  {`${invoice.customerAccount}\n${customerName}`}
                </Typography>
              ),
            },
          ],
        }}
      />
      <ConfirmationDialog
        open={Boolean(confirmation)}
        title={confirmation === 'post' ? t('freeTextInvoices.post') : t('actions.delete')}
        message={
          confirmation === 'post'
            ? t('freeTextInvoices.confirmPost')
            : t('freeTextInvoices.confirmDelete', 'Delete this unposted free text invoice?')
        }
        confirmLabel={confirmation === 'post' ? t('freeTextInvoices.post') : t('actions.delete')}
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
  );
}
