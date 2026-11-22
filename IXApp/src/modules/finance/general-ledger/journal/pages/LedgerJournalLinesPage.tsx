import React, { useMemo, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { useMutation, useQuery } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '@core/auth/useAuth';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { PERMISSIONS } from '@core/permissions/permissions';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import { ActionPaneButton } from '@shared/components/action-pane/ActionPaneButton';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { LookupField } from '@shared/components/lookups/LookupField';
import type { LookupOption } from '@shared/components/lookups/types';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';
import { useNotifications } from '@shared/hooks/useNotifications';
import { currencyApi } from '../../../foundation/currency/api/currencyApi';
import { GENERAL_LEDGER_ROUTE_PATHS } from '../../../general-ledger/routes/generalLedgerRoutePaths';
import type { JournalOpenInvoice } from '../api/journalPartyApi';
import type { LedgerJournalKind } from '../config/journalKinds';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS as paths } from '../../../accounts-receivable/routes/accountsReceivableRoutePaths';
import {
  ledgerJournalApiFor,
  newPaymentJournalLine,
  paymentJournalTotals,
  sameAmount,
  type PaymentJournalLine,
} from '../api/ledgerJournalApi';
import {
  JournalDetails,
  JournalFieldGroups,
  type JournalFieldGroup,
  JournalTabs,
  paymentJournalKeys,
  usePaymentJournalActions,
} from '../components/JournalShared';

const money = (value: number) =>
  value.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const fieldSx = { '& .MuiInputBase-root': { fontSize: 11 }, '& input': { py: 0.5 } };

const accountWords = { 0: 'ledger', 1: 'customer', 2: 'vendor' } as const;

/** The lines (LedgerJournalTrans) of one journal of one type: customer/vendor payment or general. */
export function LedgerJournalLinesPage({ kind }: { kind: LedgerJournalKind }) {
  const company = useCompanyStore((state) => state.currentCompany);
  const { journalId = '' } = useParams();
  return (
    <JournalLines
      key={`${kind.journalType}:${company}:${journalId}`}
      company={company}
      journalId={journalId}
      kind={kind}
    />
  );
}
function JournalLines({
  company,
  journalId,
  kind,
}: {
  company: string;
  journalId: string;
  kind: LedgerJournalKind;
}) {
  const api = ledgerJournalApiFor(kind.journalType);
  const navigate = useNavigate();
  const { hasPermission } = useAuth();
  const { notifyError } = useNotifications();
  const actions = usePaymentJournalActions(kind.journalType, kind.title.toLowerCase());
  const [selectedId, setSelectedId] = useState('');
  const [draft, setDraft] = useState<PaymentJournalLine | null>(null);
  const [tab, setTab] = useState('List');
  const [settleOpen, setSettleOpen] = useState(false);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [markedOnly, setMarkedOnly] = useState(false);
  useUnsavedChanges(!!draft);
  const headerQuery = useQuery({
    queryKey: [...paymentJournalKeys(company, kind.journalType), 'header', journalId],
    queryFn: ({ signal }) => api.get(journalId, signal),
    enabled: !!journalId,
  });
  const header = headerQuery.data;
  const lineQuery = useQuery({
    queryKey: [...paymentJournalKeys(company, kind.journalType), 'lines', header?.journalNum],
    queryFn: ({ signal }) => api.journalLines(header!.journalNum, signal),
    enabled: !!header,
  });
  // The party module that owns the line accounts (customers or vendors); none for a ledger journal.
  const partyApi = kind.partyApi;
  const parties = useQuery({
    queryKey: [...paymentJournalKeys(company, kind.journalType), 'parties'],
    queryFn: ({ signal }) => partyApi!.list(signal),
    enabled: !!partyApi,
  });
  const currencies = useQuery({
    queryKey: [...paymentJournalKeys(company, kind.journalType), 'currencies'],
    queryFn: ({ signal }) => currencyApi.list(signal),
  });
  const methods = useQuery({
    queryKey: [...paymentJournalKeys(company, kind.journalType), 'methods'],
    queryFn: ({ signal }) => partyApi!.paymentMethods(signal),
    enabled: !!partyApi,
  });
  const offsets = useQuery({
    queryKey: [...paymentJournalKeys(company, kind.journalType), 'offsets'],
    queryFn: ({ signal }) => api.offsets(signal),
  });
  /** The line account choices: the party accounts, or ledger account combinations. */
  const accounts = useMemo(
    () =>
      partyApi
        ? (parties.data ?? []).map((row) => ({
            id: row.accountNumber,
            code: row.accountNumber,
            name: row.name,
            nameAlias: row.nameAlias,
            currencyCode: row.currencyCode ?? '',
            paymModeId: row.paymModeId ?? '',
            paymTermId: row.paymTermId ?? '',
            recordId: row.id,
          }))
        : (offsets.data ?? [])
            .filter((row) => row.accountType === 0)
            .map((row) => ({
              id: String(row.id),
              code: row.code,
              name: row.name,
              nameAlias: undefined,
              currencyCode: '',
              paymModeId: '',
              paymTermId: '',
              recordId: '',
            })),
    [partyApi, parties.data, offsets.data]
  );
  const accountsQuery = partyApi ? parties : offsets;
  /** A ledger line names its account combination (LedgerDimension); a party line its account. */
  const accountKey = (row: PaymentJournalLine) =>
    kind.accountType === 0
      ? row.ledgerDimension
        ? String(row.ledgerDimension)
        : ''
      : row.ledgerDimensionName;
  const accountOf = (row: PaymentJournalLine) =>
    accounts.find((account) => account.id === accountKey(row));
  const accountName = (row: PaymentJournalLine) => accountOf(row)?.name ?? '';
  const accountCode = (row: PaymentJournalLine) =>
    kind.accountType === 0
      ? (accountOf(row)?.code ?? row.ledgerDimensionName)
      : row.ledgerDimensionName;
  /** The amount a payment line settles: customers pay a credit, vendors are paid a debit. */
  const paymentAmount = (row: PaymentJournalLine) =>
    kind.settlement === 'vend' ? row.amountCurDebit : row.amountCurCredit;
  const rows = useMemo(
    () =>
      draft
        ? draft.recId
          ? (lineQuery.data ?? []).map((row) => (row.id === draft.id ? draft : row))
          : [...(lineQuery.data ?? []), draft]
        : (lineQuery.data ?? []),
    [lineQuery.data, draft]
  );
  const selected = draft ?? rows.find((row) => row.id === selectedId) ?? rows[0] ?? null;
  const locked = !header || !!header.posted || !!header.systemBlocked;
  const canEdit = !locked && hasPermission(PERMISSIONS.LEDGER_JOURNAL_EDIT);
  const canCreate = !locked && hasPermission(PERMISSIONS.LEDGER_JOURNAL_CREATE);
  const canDelete = !locked && hasPermission(PERMISSIONS.LEDGER_JOURNAL_DELETE);
  const openTransactions = useQuery({
    queryKey: [
      ...paymentJournalKeys(company, kind.journalType),
      'open',
      selected?.ledgerDimensionName,
    ],
    // The party API gives each open invoice as the positive amount the payment settles.
    queryFn: ({ signal }) => partyApi!.openInvoices(selected!.ledgerDimensionName, signal),
    enabled: settleOpen && !!kind.settlement && !!partyApi && !!selected?.ledgerDimensionName,
  });
  const change = (values: Partial<PaymentJournalLine>) =>
    setDraft((current) => (current ? { ...current, ...values } : current));
  // Date inputs edit plain dates; the server sends date-times.
  const asDraft = (row: PaymentJournalLine): PaymentJournalLine => ({
    ...row,
    transDate: row.transDate.slice(0, 10),
    documentDate: row.documentDate.slice(0, 10),
    due: row.due.slice(0, 10),
  });
  const edit = (row: PaymentJournalLine) => {
    if (canEdit && !draft && !row.noEdit) {
      setSelectedId(row.id);
      setDraft(asDraft(row));
    }
  };
  /** The selected line may be marked: open, same currency, amount equal to the credit, not later. */
  const canMark = (invoice: JournalOpenInvoice) =>
    !!selected &&
    !selected.noEdit &&
    invoice.currencyCode === selected.currencyCode &&
    sameAmount(invoice.amount, paymentAmount(selected)) &&
    invoice.transDate.slice(0, 10) <= selected.transDate.slice(0, 10);
  const save = useMutation({
    mutationFn: async () => {
      if (!draft || locked || (draft.recId ? !canEdit : !canCreate))
        throw new Error('This journal cannot be edited.');
      if (!draft.transDate || !accountKey(draft) || !draft.currencyCode)
        throw new Error(
          `Date, ${accountWords[kind.accountType]} account and currency are required.`
        );
      if (
        draft.amountCurDebit < 0 ||
        draft.amountCurCredit < 0 ||
        draft.amountCurDebit > 0 === draft.amountCurCredit > 0
      )
        throw new Error('Enter a positive debit or credit, not both.');
      if (!draft.offsetLedgerDimension) throw new Error('Select an offset account.');
      const refusal = header && kind.customize?.validateLine?.(draft, header);
      if (refusal) throw new Error(refusal);
      return draft.recId ? api.lines.update(draft) : api.lines.create(draft);
    },
    onSuccess: async (row) => {
      setSelectedId(row.id);
      setDraft(null);
      await actions.refresh();
    },
    onError: (error: Error) => notifyError(error.message),
  });
  const remove = useMutation({
    mutationFn: async (row: PaymentJournalLine) => {
      if (!canDelete || row.noEdit) throw new Error('This line cannot be deleted.');
      await api.lines.delete(row);
    },
    onSuccess: async () => {
      setSelectedId('');
      await actions.refresh();
    },
    onError: (error: Error) => notifyError(error.message),
  });
  const busy = save.isPending || remove.isPending || actions.busy;
  const clearMark = {
    markedInvoiceRecId: 0,
    markedInvoiceCompany: '',
    settleVoucher: 0,
    invoice: '',
  };
  const accountOptions = accounts.map(({ id, code, name, nameAlias }) => ({
    id,
    code,
    name,
    nameAlias,
  }));
  const currencyOptions = (currencies.data ?? []).map((row) => ({
    id: row.currencyCode,
    code: row.currencyCode,
    name: row.txt,
  }));
  const methodOptions = (methods.data ?? []).map((row) => ({
    id: row.code,
    code: row.code,
    name: row.description,
  }));
  const lookup = (
    field: keyof PaymentJournalLine,
    label: string,
    options: LookupOption[],
    onSelect?: (value: string | number) => void,
    disabled = false
  ) => (
    <LookupField
      name={String(field)}
      label={label}
      value={(draft?.[field] as string | number) || null}
      onChange={(value) => {
        const next = typeof value === 'string' || typeof value === 'number' ? value : '';
        if (onSelect) onSelect(next);
        else change({ [field]: next });
      }}
      options={options}
      displayMode="select"
      searchable
      compact
      gridCell={!label}
      disabled={busy || disabled}
    />
  );
  const textEditor = (field: keyof PaymentJournalLine, type = 'text', label = '') => (
    <TextField
      label={label}
      variant="standard"
      size="small"
      fullWidth
      type={type}
      value={type === 'date' ? String(draft?.[field] ?? '').slice(0, 10) : (draft?.[field] ?? '')}
      disabled={busy}
      onChange={(event) =>
        change({ [field]: event.target.value, ...(field === 'transDate' ? clearMark : {}) })
      }
      slotProps={{ htmlInput: { 'aria-label': label || String(field) } }}
      sx={fieldSx}
    />
  );
  const offsetLabel = (row: PaymentJournalLine) =>
    offsets.data?.find((offset) => Number(offset.id) === row.offsetLedgerDimension)?.code ??
    (row.offsetLedgerDimension ? String(row.offsetLedgerDimension) : '');
  const renderField = (
    field: keyof PaymentJournalLine,
    row: PaymentJournalLine,
    label = ''
  ): React.ReactNode => {
    if (row.id !== draft?.id) {
      if (['transDate', 'documentDate', 'due'].includes(field))
        return String(row[field] ?? '').slice(0, 10);
      if (field === 'offsetLedgerDimension') return offsetLabel(row);
      if (field === 'ledgerDimensionName') return accountCode(row);
      if (field === 'offsetAccountType') return row.offsetAccountType === 5 ? 'Bank' : 'Ledger';
      if (field === 'amountCurDebit' || field === 'amountCurCredit')
        return row[field] ? money(row[field]) : '';
      return String(row[field] ?? '');
    }
    if (field === 'ledgerDimensionName')
      return (
        <LookupField
          name="ledgerDimensionName"
          label={label}
          value={accountKey(draft) || null}
          onChange={(value) => {
            const key = typeof value === 'string' || typeof value === 'number' ? String(value) : '';
            const account = accounts.find((item) => item.id === key);
            change(
              kind.accountType === 0
                ? // A ledger line stores the account combination; its display value is kept.
                  {
                    ledgerDimension: Number(key) || 0,
                    ledgerDimensionName: account?.code ?? '',
                    ...clearMark,
                  }
                : {
                    ledgerDimensionName: key,
                    ledgerDimension: 0,
                    ...clearMark,
                    currencyCode: account?.currencyCode || draft.currencyCode,
                    paymMode: account?.paymModeId || '',
                    payment: account?.paymTermId || '',
                  }
            );
          }}
          options={accountOptions}
          displayMode="select"
          searchable
          compact
          gridCell={!label}
          disabled={busy || accountsQuery.isLoading || accountsQuery.isError}
        />
      );
    if (field === 'currencyCode')
      return lookup(
        field,
        label,
        currencyOptions,
        (value) => change({ currencyCode: String(value), ...clearMark }),
        currencies.isLoading || currencies.isError
      );
    if (field === 'paymMode')
      return lookup(
        field,
        label,
        methodOptions,
        (value) => {
          const method = methods.data?.find((item) => item.code === value);
          const dimension = Number(method?.source.paymentLedgerDimension || 0);
          const accountType = Number(method?.source.accountType || 0);
          change({
            paymMode: String(value),
            ...(!header?.fixedOffsetAccount && dimension && [0, 5].includes(accountType)
              ? { offsetLedgerDimension: dimension, offsetAccountType: accountType }
              : {}),
          });
        },
        methods.isLoading || methods.isError
      );
    if (field === 'offsetAccountType')
      return (
        <TextField
          select
          variant="standard"
          value={draft.offsetAccountType}
          disabled={busy || !!header?.fixedOffsetAccount}
          fullWidth
          label={label}
          onChange={(event) =>
            change({ offsetAccountType: Number(event.target.value), offsetLedgerDimension: 0 })
          }
          sx={fieldSx}
        >
          <MenuItem value={0}>Ledger</MenuItem>
          <MenuItem value={5}>Bank</MenuItem>
        </TextField>
      );
    if (field === 'offsetLedgerDimension')
      return lookup(
        field,
        label,
        (offsets.data ?? []).filter((item) => item.accountType === draft.offsetAccountType),
        (value) => change({ offsetLedgerDimension: Number(value) }),
        !!header?.fixedOffsetAccount || offsets.isLoading || offsets.isError
      );
    if (field === 'amountCurDebit' || field === 'amountCurCredit')
      return (
        <TextField
          variant="standard"
          type="number"
          value={draft[field] || ''}
          disabled={busy}
          fullWidth
          label={label}
          slotProps={{
            htmlInput: {
              min: 0,
              step: '0.01',
              'aria-label': label || (field === 'amountCurDebit' ? 'Debit' : 'Credit'),
            },
          }}
          onChange={(event) =>
            change({
              [field]: Number(event.target.value),
              [field === 'amountCurDebit' ? 'amountCurCredit' : 'amountCurDebit']: 0,
              ...clearMark,
            })
          }
          sx={fieldSx}
        />
      );
    return textEditor(
      field,
      ['transDate', 'documentDate', 'due'].includes(field) ? 'date' : 'text',
      label
    );
  };
  const standardColumns: ColumnDef<PaymentJournalLine>[] = [
    { field: 'transDate', headerName: 'Date', width: 120, editable: true },
    { field: 'voucher', headerName: 'Voucher', width: 140 },
    { field: 'company', headerName: 'Company', width: 85 },
    {
      field: 'ledgerDimensionName',
      headerName: 'Account',
      width: 145,
      editable: true,
      ...(kind.accountType === 1
        ? {
            preview: { entity: 'customer' },
            getDetailHref: (row: PaymentJournalLine) => {
              const account = accountOf(row);
              return account ? paths.customer(account.recordId) : undefined;
            },
          }
        : {}),
    },
    {
      field: 'accountName',
      headerName: 'Account name',
      width: 200,
      renderCell: ({ row }: { row: PaymentJournalLine }) => accountName(row),
    },
    { field: 'invoice', headerName: 'Invoice', width: 140 },
    { field: 'txt', headerName: 'Description', width: 260, editable: true },
    { field: 'amountCurDebit', headerName: 'Debit', width: 110, type: 'number', editable: true },
    { field: 'amountCurCredit', headerName: 'Credit', width: 110, type: 'number', editable: true },
    {
      field: 'currencyCode',
      headerName: 'Currency',
      width: 90,
      editable: true,
      preview: { entity: 'currency' },
    },
    { field: 'offsetAccountType', headerName: 'Offset account type', width: 145, editable: true },
    { field: 'offsetLedgerDimension', headerName: 'Offset account', width: 220, editable: true },
    {
      field: 'paymMode',
      headerName: 'Method of payment',
      width: 150,
      editable: true,
      ...(kind.settlement === 'cust' ? { preview: { entity: 'customerPaymentMethod' } } : {}),
    },
    { field: 'paymReference', headerName: 'Payment reference', width: 160, editable: true },
    { field: 'documentNum', headerName: 'Document', width: 130, editable: true },
    { field: 'documentDate', headerName: 'Document date', width: 125, editable: true },
    { field: 'due', headerName: 'Due date', width: 125, editable: true },
  ].map((column) => ({
    ...column,
    // No hover preview while a line is edited: it would wrap the editors.
    ...(draft ? { preview: undefined } : {}),
    renderCell:
      column.renderCell ??
      (({ row }: { row: PaymentJournalLine }) =>
        renderField(column.field as keyof PaymentJournalLine, row)),
  })) as ColumnDef<PaymentJournalLine>[];
  const columns = kind.customize?.lineColumns?.(standardColumns) ?? standardColumns;
  const totals = (scope: PaymentJournalLine[]) => {
    const currenciesInScope = new Set(scope.map((row) => row.currencyCode));
    if (currenciesInScope.size > 1)
      return { debit: 'Multiple currencies', credit: 'Multiple currencies', balance: '—' };
    const value = paymentJournalTotals(scope);
    return {
      debit: money(value.debit),
      credit: money(value.credit),
      balance: money(value.balance),
    };
  };
  const voucherRows = selected
    ? rows.filter((row) =>
        selected.voucher ? row.voucher === selected.voucher : row.id === selected.id
      )
    : [];
  const errors = [accountsQuery.error, currencies.error, methods.error, offsets.error].filter(
    Boolean
  );
  const selectInvoice = (invoice: JournalOpenInvoice) => {
    if (!selected || !canMark(invoice)) return;
    setDraft({
      ...asDraft(selected),
      invoice: invoice.invoice,
      markedInvoiceRecId: invoice.transRecId,
      markedInvoiceCompany: company,
      settleVoucher: 2,
    });
    setSettleOpen(false);
  };
  /** Editor while the line is edited; otherwise the read-only value. */
  const editable = (field: keyof PaymentJournalLine, label: string) =>
    draft ? { editor: renderField(field, draft, label) } : {};
  const missing = (label: string, toggle = false) => ({ label, toggle, unavailable: true });
  const date = (value?: string) => (value && !value.startsWith('0001') ? value.slice(0, 10) : '');
  const rate = (value?: number) => (value ? value.toFixed(4) : '');
  const settlementTypes = ['None', 'Selected transactions', 'Marked transactions'];
  /** The line tabs, laid out as the D365 Journal voucher form (column by column). */
  /** The line tabs after List; a journal can change them. */
  const standardTabs = [
    'General',
    'Invoice',
    'Payment',
    'Payment fee',
    'Fixed assets',
    'Remittance',
    'History',
  ];
  const lineTabs = ['List', ...(kind.customize?.lineTabs?.(standardTabs) ?? standardTabs)];
  /** A tab's field groups: the journal's own, or the standard ones. */
  const tabGroups = (current: string, line: PaymentJournalLine) => {
    const standard = lineGroups(current, line);
    return kind.customize?.lineTabGroups?.(current, line, standard) ?? standard;
  };
  const lineGroups = (current: string, line: PaymentJournalLine): JournalFieldGroup[] => {
    if (current === 'General')
      return [
        {
          title: 'Identification',
          fields: [
            { label: 'Voucher', value: line.voucher },
            { label: 'Quantity', value: (line.qty ?? 0).toFixed(2) },
            {
              label: 'Reason code',
              value: line.reasonRefRecId ? String(line.reasonRefRecId) : '',
              heading: 'Reason',
            },
            missing('Reason comment'),
          ],
        },
        {
          title: 'Currency',
          fields: [
            {
              label: 'Currency',
              value: line.currencyCode,
              ...editable('currencyCode', 'Currency'),
            },
            { label: 'Exchange rate', value: rate(line.exchRate) },
            { label: 'Secondary exchange rate', value: rate(line.exchRateSecond) },
            missing('Cross rate'),
            missing('Calculation method'),
          ],
        },
        {
          title: 'Reporting currency',
          fields: [
            { label: 'Exchange rate', value: rate(line.reportingCurrencyExchRate) },
            {
              label: 'Secondary exchange rate',
              value: rate(line.reportingCurrencyExchRateSecondary),
            },
            missing('Cross rate'),
            missing('Calculation method'),
          ],
        },
        {
          title: 'Tax',
          fields: [
            { label: 'Sales tax group', value: line.taxGroup },
            { label: 'Item sales tax group', value: line.taxItemGroup },
            { label: 'Sales tax code', value: line.taxCode },
          ],
        },
        {
          fields: [
            { label: 'Sales tax direction', value: line.taxDirectionControl, toggle: true },
            missing('Sales tax amount'),
            { label: 'Withholding tax group', value: line.taxWithholdGroup },
          ],
        },
        {
          fields: [
            missing('Calculate withholding tax', true),
            missing('Item withholding tax group'),
            { label: 'Company', value: line.company, heading: 'Legal entity' },
            { label: 'Offset company', value: line.offsetCompany },
          ],
        },
        {
          title: 'Posting',
          fields: [
            { label: 'Release date', value: date(line.releaseDate) },
            { label: 'Reversing entry', value: line.reverseEntry, toggle: true },
            { label: 'Reversing date', value: line.reverseEntry ? date(line.reverseDate) : '' },
            { label: 'Transfer', value: line.transfer, toggle: true },
          ],
        },
      ];
    if (current === 'Invoice')
      return [
        {
          title: 'Posting',
          fields: [
            { label: 'Posting profile', value: line.postingProfile },
            { label: 'Settlement type', value: settlementTypes[line.settleVoucher] ?? '' },
            missing('Offset posting profile'),
          ],
        },
        {
          title: 'Invoice',
          fields: [
            { label: 'Invoice', value: line.invoice },
            { label: 'Due date', value: date(line.due), ...editable('due', 'Due date') },
          ],
        },
        {
          fields: [
            { label: 'Terms of payment', value: line.payment },
            missing('Payment ID'),
            { label: 'VAT Number', value: line.vatNumJournal },
          ],
        },
        {
          fields: [
            { label: 'Cash discount', value: line.cashDiscCode },
            { label: 'Payment terms base date', value: date(line.cashDiscBaseDate) },
          ],
        },
        {
          fields: [
            { label: 'Cash discount date', value: date(line.dateCashDisc) },
            { label: 'Cash discount amount', value: money(line.cashDiscAmount ?? 0) },
          ],
        },
        {
          title: 'Document',
          fields: [
            { label: 'Document', value: line.documentNum, ...editable('documentNum', 'Document') },
            {
              label: 'Document date',
              value: date(line.documentDate),
              ...editable('documentDate', 'Document date'),
            },
            { label: 'Date of vendor VAT register', value: date(line.vendorVatDate) },
          ],
        },
        {
          title: 'Approve',
          fields: [
            { label: 'Approved by', value: line.approver ? String(line.approver) : '' },
            { label: 'Approved', value: line.approved, toggle: true },
          ],
        },
      ];
    if (current === 'Payment')
      return [
        {
          title: 'Payment',
          fields: [
            {
              label: 'Method of payment',
              value: line.paymMode,
              ...editable('paymMode', 'Method of payment'),
            },
          ],
        },
        {
          fields: [
            { label: 'Payment specification', value: line.paymSpec },
            { label: 'Prepayment journal voucher', value: line.prepayment, toggle: true },
          ],
        },
        {
          title: 'Amount',
          fields: [
            missing('Balance'),
            { label: 'Cash discount amount', value: money(line.cashDiscAmount ?? 0) },
          ],
        },
        {
          title: 'Bank',
          fields: [
            { label: 'Bank transaction type', value: line.bankTransType },
            {
              label: 'Payment reference',
              value: line.paymReference,
              ...editable('paymReference', 'Payment reference'),
            },
          ],
        },
        {
          fields: [
            { label: 'Use a deposit slip', value: line.bankDepositVoucher, toggle: true },
            { label: 'Deposit slip', value: line.bankDepositNum },
          ],
        },
        { fields: [missing('Bank transaction summarization ID')] },
        {
          title: 'Representative',
          fields: [missing('Representative type'), missing('Name of person')],
        },
      ];
    if (current === 'History')
      return [
        {
          title: 'Whole journal',
          fields: [missing('Reported as ready by'), missing('Approved by')],
        },
        {
          fields: [
            missing('Rejected by'),
            {
              label: 'Posted on',
              value: header?.posted ? header.postedDateTime.replace('T', ' ').slice(0, 19) : '',
            },
          ],
        },
        {
          title: 'Intercompany accounting',
          fields: [
            { label: 'Foreign company accounts', value: line.foreignCompany },
            { label: 'Foreign voucher', value: line.foreignVoucher },
          ],
        },
        {
          title: 'Transfer',
          fields: [{ label: 'Transferred on', value: date(line.transferredOn) }],
        },
        {
          fields: [
            { label: 'Transferred by', value: line.transferredBy },
            { label: 'Transferred to journal', value: line.transferredTo },
          ],
        },
        { title: 'Error messages reported for the voucher', fields: [missing('Errors detected')] },
      ];
    return [];
  };
  const footerLine = draft ?? selected;
  const offsetName = (row: PaymentJournalLine) =>
    offsets.data?.find((offset) => Number(offset.id) === row.offsetLedgerDimension)?.name ?? '';
  const footerBoxSx = {
    minHeight: 26,
    px: 0.75,
    display: 'flex',
    alignItems: 'center',
    fontSize: 12,
    border: 1,
    borderColor: 'divider',
    borderRadius: 0.5,
    bgcolor: 'action.hover',
    overflow: 'hidden',
    whiteSpace: 'nowrap',
    textOverflow: 'ellipsis',
  } as const;
  return (
    <SimpleListPage<PaymentJournalLine>
      title={kind.linesTitle}
      contextLabel={`${kind.linesTitle} | ${header?.journalNum ?? ''} : ${header?.journalName ?? ''}`}
      recordTableName="LedgerJournalTrans"
      columns={columns}
      dataSource={{
        type: 'controlled',
        rows,
        loading: headerQuery.isLoading || lineQuery.isLoading,
        error: headerQuery.error?.message || lineQuery.error?.message,
        refresh: () => {
          if (!draft) void actions.refresh();
        },
      }}
      enterpriseConfig={{
        readOnly: true,
        backCommand: { onClick: () => navigate(kind.listPath) },
        // D365 order: Post, Post in batch, Validate, General journals, Approval, Period journal,
        // Functions, Fixed assets, Inquiries, Print. Not yet available here: shown disabled.
        commands: [
          {
            id: 'post',
            label: 'Post',
            disabled: locked || busy || !!draft,
            permission: PERMISSIONS.LEDGER_JOURNAL_EDIT,
            onClick: () => actions.run(header ?? null, 'post'),
          },
          { id: 'post-batch', label: 'Post in batch', disabled: true },
          {
            id: 'validate',
            label: 'Validate',
            disabled: locked || busy || !!draft,
            onClick: () => actions.run(header ?? null, 'validate'),
          },
          {
            id: 'simulate',
            label: 'Simulate posting',
            disabled: locked || busy || !!draft,
            onClick: () => actions.run(header ?? null, 'simulate'),
          },
          ...(kind.accountTransactions
            ? [
                {
                  id: 'transactions',
                  label: kind.accountTransactions.label,
                  disabled: !selected?.ledgerDimensionName || !!draft,
                  onClick: () =>
                    selected &&
                    navigate(kind.accountTransactions!.path(selected.ledgerDimensionName)),
                },
              ]
            : []),
          {
            id: 'general-journals',
            label: 'General journals',
            disabled: !!draft,
            onClick: () => navigate(kind.listPath),
          },
          { id: 'approval', label: 'Approval', disabled: true },
          { id: 'period-journal', label: 'Period journal', disabled: true },
          { id: 'functions', label: 'Functions', disabled: true },
          { id: 'fixed-assets', label: 'Fixed assets', disabled: true },
          { id: 'inquiries', label: 'Inquiries', disabled: true },
          { id: 'print', label: 'Print', disabled: true },
          ...(kind.customize?.lineCommands?.({
            header,
            line: selected,
            editing: !!draft,
            navigate: (path) => navigate(path),
          }) ?? []),
        ],
      }}
      actionPane={
        <>
          <ActionPaneButton label="Save" disabled={!draft || busy} onClick={() => save.mutate()} />
          {draft && (
            <ActionPaneButton
              label="Cancel"
              disabled={busy}
              onClick={() => {
                setDraft(null);
              }}
            />
          )}
          <ActionPaneButton
            label="Edit"
            disabled={!selected || !canEdit || !!draft || busy || !!selected?.noEdit}
            onClick={() => selected && edit(selected)}
          />
        </>
      }
      filterBar={
        <Stack spacing={0.5}>
          {!!header?.posted && <Alert severity="info">Posted journal — lines are read-only.</Alert>}
          {!!header?.systemBlocked && <Alert severity="warning">This journal is blocked.</Alert>}
          {errors.map((error, index) => (
            <Alert key={index} severity="error">
              {error?.message}
            </Alert>
          ))}
          <JournalTabs tabs={lineTabs} value={tab} onChange={setTab} />
          <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}>
            <Button
              size="small"
              disabled={!canCreate || !!draft || busy}
              onClick={() => {
                if (header) {
                  const standard = {
                    ...newPaymentJournalLine(header, kind.accountType),
                    id: `new-${crypto.randomUUID()}`,
                  };
                  const row = kind.customize?.newLine?.(standard, header) ?? standard;
                  setDraft(row);
                  setSelectedId(row.id);
                  setTab('List');
                }
              }}
            >
              + New
            </Button>
            <Button
              size="small"
              disabled={!selected || !canDelete || !!draft || busy || !!selected?.noEdit}
              onClick={() => setDeleteOpen(true)}
            >
              Delete
            </Button>
            <Button
              size="small"
              disabled={
                !kind.settlement ||
                !selected?.ledgerDimensionName ||
                !paymentAmount(selected) ||
                !!selected?.noEdit ||
                locked ||
                busy ||
                !(draft ? (draft.recId ? canEdit : canCreate) : canEdit)
              }
              onClick={() => {
                setMarkedOnly(false);
                setSettleOpen(true);
              }}
            >
              Settle transactions
            </Button>
            <Button
              size="small"
              disabled={!selected?.markedInvoiceRecId || busy}
              onClick={() => {
                setMarkedOnly(true);
                setSettleOpen(true);
              }}
            >
              View marked transactions
            </Button>
            <Button
              size="small"
              disabled={!header?.posted || !selected?.voucher || !!draft}
              onClick={() =>
                selected &&
                navigate(
                  `${GENERAL_LEDGER_ROUTE_PATHS.VOUCHER_TRANSACTIONS}?voucher=${encodeURIComponent(selected.voucher)}`
                )
              }
            >
              Voucher
            </Button>
          </Stack>
        </Stack>
      }
      detailContent={
        tab === 'List'
          ? undefined
          : () =>
              !selected ? (
                <Alert severity="info">Select a journal line.</Alert>
              ) : tab === 'Payment fee' ? (
                <Alert severity="info">
                  Payment fees are not available for this journal: no payment fee is set up.
                </Alert>
              ) : tab === 'Fixed assets' || tab === 'Remittance' ? (
                <Alert severity="info">
                  {tab === 'Fixed assets'
                    ? 'Fixed asset transactions are not part of this journal here.'
                    : 'Remittance details are not available for this journal yet.'}
                </Alert>
              ) : (
                <JournalFieldGroups groups={tabGroups(tab, draft ?? selected)} />
              )
      }
      belowGridContent={
        <Box sx={{ py: 1, flexShrink: 0 }}>
          {/* D365 line footer: the selected line's account, offset, tax, invoice and due date. */}
          <Stack direction="row" spacing={1.5} sx={{ mb: 1.5, flexWrap: 'wrap', rowGap: 1 }}>
            {(
              [
                ['Account name', footerLine ? accountName(footerLine) : '', 150],
                ['Offset account name', footerLine ? offsetName(footerLine) : '', 150],
                ['Calculated sales tax amount', null, 100],
                ['Actual sales tax amount', null, 100],
                ['Invoice', footerLine?.invoice ?? '', 100],
                ['Due date', footerLine ? (footerLine.due ?? '').slice(0, 10) : '', 150],
              ] as const
            ).map(([label, value, width]) => (
              <Box
                key={label}
                sx={{ width }}
                title={value === null ? 'Sales tax is not calculated on this journal.' : undefined}
              >
                <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
                  {label}
                </Typography>
                <Box sx={footerBoxSx}>{value ?? ''}</Box>
              </Box>
            ))}
          </Stack>
          <Stack direction="row" spacing={2} sx={{ alignItems: 'flex-end', flexWrap: 'wrap' }}>
            <Box
              role="table"
              aria-label="Journal totals"
              sx={{
                display: 'grid',
                gridTemplateColumns: '80px repeat(3, 100px)',
                gap: 0.5,
                width: 'fit-content',
                fontSize: 11,
              }}
            >
              <span />
              <Typography variant="caption">DEBIT</Typography>
              <Typography variant="caption">CREDIT</Typography>
              <Typography variant="caption">BALANCE</Typography>
              {(
                [
                  ['VOUCHER', totals(voucherRows)],
                  ['JOURNAL', totals(rows)],
                ] as const
              ).map(([label, total]) => (
                <React.Fragment key={label}>
                  <Typography variant="caption" sx={{ alignSelf: 'center', fontWeight: 600 }}>
                    {label}
                  </Typography>
                  {[total.debit, total.credit, total.balance].map((value, index) => (
                    <Box key={index} sx={{ ...footerBoxSx, justifyContent: 'flex-end' }}>
                      {value}
                    </Box>
                  ))}
                </React.Fragment>
              ))}
            </Box>
            <Box sx={{ width: 200 }} title="Budget control is not part of this journal.">
              <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
                Budget check results
              </Typography>
              <Box sx={footerBoxSx}>Budget check not performed</Box>
            </Box>
          </Stack>
        </Box>
      }
      dataGridProps={{
        storageKey: `LedgerJournalTrans:${kind.journalType}:${company}`,
        selectedIds: selected ? [selected.id] : [],
        navigationDisabled: !!draft,
        editDisabled: locked || busy,
        onRowClick: (row) => {
          if (!draft) setSelectedId(row.id);
        },
        onSelectionChange: (ids) => {
          if (!draft) setSelectedId(String(ids.at(-1) ?? ''));
        },
        onRowDoubleClick: edit,
        // The page edits with its own editors (not the grid's edit mode), so the grid
        // cannot tell a line is being edited: only offer its view-mode pencil when idle.
        onCellEditRequest: draft || !canEdit ? undefined : (row) => edit(row),
        hideToolbar: true,
        mobilePresentation: 'table',
      }}
      dialogs={
        <>
          {actions.dialogs}
          <Dialog open={deleteOpen} onClose={() => setDeleteOpen(false)}>
            <DialogTitle>Delete payment line?</DialogTitle>
            <DialogContent>Delete the selected unposted payment line?</DialogContent>
            <DialogActions>
              <Button onClick={() => setDeleteOpen(false)}>Cancel</Button>
              <Button
                onClick={() => {
                  if (selected) remove.mutate(selected);
                  setDeleteOpen(false);
                }}
              >
                Delete
              </Button>
            </DialogActions>
          </Dialog>
          <Dialog open={settleOpen} onClose={() => setSettleOpen(false)} fullWidth maxWidth="md">
            <DialogTitle>
              {markedOnly ? 'Marked transactions' : 'Settle transactions'} —{' '}
              {selected?.ledgerDimensionName}
            </DialogTitle>
            <DialogContent>
              <Stack spacing={1}>
                <Alert severity="info">
                  This journal supports one invoice with an open amount equal to the payment amount
                  and the same currency. Save the selection, then post the journal to complete
                  settlement. You can also post on account and settle later.
                </Alert>
                {openTransactions.isLoading && <Typography>Loading open transactions…</Typography>}
                {openTransactions.isError && (
                  <Alert severity="error">{openTransactions.error.message}</Alert>
                )}
                {markedOnly ? (
                  <JournalDetails
                    values={{
                      Invoice: selected?.invoice,
                      'Transaction reference': selected?.markedInvoiceRecId,
                    }}
                  />
                ) : (
                  (openTransactions.data ?? []).map((row) => (
                    <Stack
                      key={row.transRecId}
                      direction="row"
                      spacing={2}
                      sx={{ alignItems: 'center' }}
                    >
                      <Typography sx={{ flex: 1 }}>
                        {row.invoice} · {row.transDate.slice(0, 10)} · {money(row.amount)}{' '}
                        {row.currencyCode}
                      </Typography>
                      <Button disabled={!canMark(row)} onClick={() => selectInvoice(row)}>
                        Mark
                      </Button>
                    </Stack>
                  ))
                )}
                {!markedOnly &&
                  openTransactions.isSuccess &&
                  openTransactions.data.length === 0 && <Typography>No open invoices.</Typography>}
              </Stack>
            </DialogContent>
            <DialogActions>
              {selected?.markedInvoiceRecId && !locked && canEdit && !selected.noEdit ? (
                <Button
                  onClick={() => {
                    setDraft({ ...asDraft(selected), ...clearMark });
                    setSettleOpen(false);
                  }}
                >
                  Clear marking
                </Button>
              ) : null}
              <Button onClick={() => setSettleOpen(false)}>Close</Button>
            </DialogActions>
          </Dialog>
        </>
      }
    />
  );
}
