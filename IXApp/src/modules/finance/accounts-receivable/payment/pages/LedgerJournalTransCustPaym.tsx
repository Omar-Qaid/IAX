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
import { customerQuickCreateApi } from '../../customer/api/customerQuickCreateApi';
import { customerPaymentSetupApi } from '../../setup/api/customerPaymentSetupApi';
import { custOpenTransApi, type CustOpenTransRow } from '../../settlement/api/custOpenTransApi';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS as paths } from '../../routes/accountsReceivableRoutePaths';
import {
  ledgerJournalCustPaymApi as api,
  newPaymentJournalLine,
  paymentJournalTotals,
  type PaymentJournalLine,
} from '../api/ledgerJournalCustPaymApi';
import {
  JournalDetails,
  JournalTabs,
  paymentJournalKeys,
  usePaymentJournalActions,
} from '../components/PaymentJournalShared';

const paymentMethodsApi = customerPaymentSetupApi('methods');
const money = (value: number) =>
  value.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const fieldSx = { '& .MuiInputBase-root': { fontSize: 11 }, '& input': { py: 0.5 } };

export function LedgerJournalTransCustPaym() {
  const company = useCompanyStore((state) => state.currentCompany);
  const { journalId = '' } = useParams();
  return <JournalLines key={`${company}:${journalId}`} company={company} journalId={journalId} />;
}
function JournalLines({ company, journalId }: { company: string; journalId: string }) {
  const navigate = useNavigate();
  const { hasPermission } = useAuth();
  const { notifyError } = useNotifications();
  const actions = usePaymentJournalActions();
  const [selectedId, setSelectedId] = useState('');
  const [draft, setDraft] = useState<PaymentJournalLine | null>(null);
  const [tab, setTab] = useState('List');
  const [settleOpen, setSettleOpen] = useState(false);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [markedOnly, setMarkedOnly] = useState(false);
  useUnsavedChanges(!!draft);
  const headerQuery = useQuery({
    queryKey: [...paymentJournalKeys(company), 'header', journalId],
    queryFn: ({ signal }) => api.get(journalId, signal),
    enabled: !!journalId,
  });
  const header = headerQuery.data;
  const lineQuery = useQuery({
    queryKey: [...paymentJournalKeys(company), 'lines', header?.journalNum],
    queryFn: ({ signal }) => api.journalLines(header!.journalNum, signal),
    enabled: !!header,
  });
  const customers = useQuery({
    queryKey: [...paymentJournalKeys(company), 'customers'],
    queryFn: ({ signal }) => customerQuickCreateApi.list(signal),
  });
  const currencies = useQuery({
    queryKey: [...paymentJournalKeys(company), 'currencies'],
    queryFn: ({ signal }) => currencyApi.list(signal),
  });
  const methods = useQuery({
    queryKey: [...paymentJournalKeys(company), 'methods'],
    queryFn: ({ signal }) => paymentMethodsApi.list(signal),
  });
  const offsets = useQuery({
    queryKey: [...paymentJournalKeys(company), 'offsets'],
    queryFn: ({ signal }) => api.offsets(signal),
  });
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
    queryKey: [...paymentJournalKeys(company), 'open', selected?.ledgerDimensionName],
    queryFn: ({ signal }) => custOpenTransApi.load(selected!.ledgerDimensionName, signal),
    enabled: settleOpen && !!selected?.ledgerDimensionName,
  });
  const change = (values: Partial<PaymentJournalLine>) =>
    setDraft((current) => (current ? { ...current, ...values } : current));
  const edit = (row: PaymentJournalLine) => {
    if (canEdit && !draft && !row.noEdit) {
      setSelectedId(row.id);
      setDraft({
        ...row,
        transDate: row.transDate.slice(0, 10),
        documentDate: row.documentDate.slice(0, 10),
        due: row.due.slice(0, 10),
      });
    }
  };
  const save = useMutation({
    mutationFn: async () => {
      if (!draft || locked || (draft.recId ? !canEdit : !canCreate))
        throw new Error('This journal cannot be edited.');
      if (!draft.transDate || !draft.ledgerDimensionName || !draft.currencyCode)
        throw new Error('Date, customer account and currency are required.');
      if (
        draft.amountCurDebit < 0 ||
        draft.amountCurCredit < 0 ||
        draft.amountCurDebit > 0 === draft.amountCurCredit > 0
      )
        throw new Error('Enter a positive debit or credit, not both.');
      if (!draft.offsetLedgerDimension) throw new Error('Select an offset account.');
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
  const customerOptions = (customers.data ?? []).map((row) => ({
    id: row.accountNumber,
    code: row.accountNumber,
    name: row.name,
    nameAlias: row.nameAr,
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
  const customerName = (row: PaymentJournalLine) =>
    customers.data?.find((customer) => customer.accountNumber === row.ledgerDimensionName)?.name ??
    '';
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
      if (field === 'offsetAccountType') return row.offsetAccountType === 5 ? 'Bank' : 'Ledger';
      if (field === 'amountCurDebit' || field === 'amountCurCredit')
        return row[field] ? money(row[field]) : '';
      return String(row[field] ?? '');
    }
    if (field === 'ledgerDimensionName')
      return lookup(
        field,
        label,
        customerOptions,
        (value) => {
          const customer = customers.data?.find((item) => item.accountNumber === value);
          change({
            ledgerDimensionName: String(value),
            ledgerDimension: 0,
            ...clearMark,
            currencyCode: customer?.currencyCode || draft.currencyCode,
            paymMode: customer?.paymModeId || '',
            payment: customer?.paymTermId || '',
          });
        },
        customers.isLoading || customers.isError
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
  const columns: ColumnDef<PaymentJournalLine>[] = [
    { field: 'transDate', headerName: 'Date', width: 120, editable: true },
    { field: 'voucher', headerName: 'Voucher', width: 140 },
    { field: 'company', headerName: 'Company', width: 85 },
    {
      field: 'ledgerDimensionName',
      headerName: 'Account',
      width: 145,
      editable: true,
      preview: { entity: 'customer' },
      getDetailHref: (row: PaymentJournalLine) => {
        const customer = customers.data?.find((item) => item.accountNumber === row.ledgerDimensionName);
        return customer ? paths.customer(customer.id) : undefined;
      },
    },
    {
      field: 'accountName',
      headerName: 'Account name',
      width: 200,
      renderCell: ({ row }: { row: PaymentJournalLine }) => customerName(row),
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
      preview: { entity: 'customerPaymentMethod' },
    },
    { field: 'paymReference', headerName: 'Payment reference', width: 160, editable: true },
    { field: 'documentNum', headerName: 'Document', width: 130, editable: true },
    { field: 'documentDate', headerName: 'Document date', width: 125, editable: true },
    { field: 'due', headerName: 'Due date', width: 125, editable: true },
  ].map((column) => ({
    ...column,
    renderCell:
      column.renderCell ??
      (({ row }: { row: PaymentJournalLine }) =>
        renderField(column.field as keyof PaymentJournalLine, row)),
  })) as ColumnDef<PaymentJournalLine>[];
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
  const errors = [customers.error, currencies.error, methods.error, offsets.error].filter(Boolean);
  const selectInvoice = (invoice: CustOpenTransRow) => {
    if (
      !selected ||
      invoice.currencyCode !== selected.currencyCode ||
      invoice.amountCur !== selected.amountCurCredit ||
      invoice.transDate.slice(0, 10) > selected.transDate.slice(0, 10)
    )
      return;
    setDraft({
      ...selected,
      invoice: invoice.invoice,
      markedInvoiceRecId: invoice.transRecId,
      markedInvoiceCompany: company,
      settleVoucher: 2,
    });
    setSettleOpen(false);
  };
  const detailFields: Record<string, (keyof PaymentJournalLine)[]> = {
    General: ['transDate', 'txt', 'documentNum', 'documentDate', 'due'],
    Bank: ['offsetAccountType', 'offsetLedgerDimension', 'paymReference'],
    Payment: ['paymMode', 'paymReference'],
  };
  const labels: Partial<Record<keyof PaymentJournalLine, string>> = {
    transDate: 'Date',
    txt: 'Description',
    documentNum: 'Document',
    documentDate: 'Document date',
    due: 'Due date',
    offsetAccountType: 'Offset account type',
    offsetLedgerDimension: 'Offset account',
    paymReference: 'Payment reference',
    paymMode: 'Method of payment',
  };
  return (
    <SimpleListPage<PaymentJournalLine>
      title="Customer payments"
      contextLabel={`Customer payments | ${header?.journalNum ?? ''} : ${header?.journalName ?? ''}`}
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
        backCommand: { onClick: () => navigate(paths.LEDGER_JOURNAL_LIST_CUST_PAYM) },
        commands: [
          {
            id: 'post',
            label: 'Post',
            disabled: locked || busy || !!draft,
            permission: PERMISSIONS.LEDGER_JOURNAL_EDIT,
            onClick: () => actions.run(header ?? null, 'post'),
          },
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
          {
            id: 'transactions',
            label: 'Customer transactions',
            disabled: !selected?.ledgerDimensionName || !!draft,
            onClick: () => selected && navigate(paths.custTrans(selected.ledgerDimensionName)),
          },
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
          <JournalTabs
            tabs={['List', 'General', 'Payment fee', 'Bank', 'History', 'Payment']}
            value={tab}
            onChange={setTab}
          />
          <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}>
            <Button
              size="small"
              disabled={!canCreate || !!draft || busy}
              onClick={() => {
                if (header) {
                  const row = {
                    ...newPaymentJournalLine(header),
                    id: `new-${crypto.randomUUID()}`,
                  };
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
                !selected?.ledgerDimensionName ||
                !selected?.amountCurCredit ||
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
                  Payment fee processing is not available for this journal.
                </Alert>
              ) : tab === 'History' ? (
                <JournalDetails
                  values={{
                    Voucher: selected.voucher,
                    'Created by': selected.createdBy,
                    'Created on': selected.createdAt,
                    'Modified by': selected.lastModifiedBy,
                    'Modified on': selected.lastModifiedAt,
                  }}
                />
              ) : (
                <Box
                  sx={{
                    display: 'grid',
                    gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
                    gap: 2,
                    p: 2,
                    alignContent: 'start',
                  }}
                >
                  {(detailFields[tab] ?? []).map((field) => (
                    <Box key={field}>
                      {draft ? (
                        renderField(field, draft, labels[field])
                      ) : (
                        <JournalDetails
                          values={{ [labels[field] || field]: renderField(field, selected) }}
                        />
                      )}
                    </Box>
                  ))}
                </Box>
              )
      }
      belowGridContent={
        <Box sx={{ py: 1, flexShrink: 0 }}>
          <Stack direction="row" spacing={3} sx={{ mb: 2 }}>
            <Box sx={{ width: 180 }}>
              {draft ? (
                renderField('paymMode', draft, 'Method of payment')
              ) : (
                <JournalDetails values={{ 'Method of payment': selected?.paymMode }} />
              )}
            </Box>
            <JournalDetails
              values={{
                'Marked invoice': selected?.invoice,
                'Payment reference': selected?.paymReference,
              }}
            />
          </Stack>
          <Box
            role="table"
            aria-label="Journal totals"
            sx={{
              display: 'grid',
              gridTemplateColumns: '80px repeat(3, 130px)',
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
                <Typography variant="caption">{label}</Typography>
                {[total.debit, total.credit, total.balance].map((value, index) => (
                  <Box
                    key={index}
                    sx={{
                      border: '1px solid',
                      borderColor: 'divider',
                      px: 1,
                      py: 0.3,
                      textAlign: 'right',
                    }}
                  >
                    {value}
                  </Box>
                ))}
              </React.Fragment>
            ))}
          </Box>
        </Box>
      }
      dataGridProps={{
        storageKey: `LedgerJournalTransCustPaym:${company}`,
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
        onCellEditRequest: (row) => edit(row),
        hideToolbar: true,
        hideFooter: true,
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
                  This journal supports one invoice with an open amount equal to the payment credit
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
                  (openTransactions.data?.transactions ?? [])
                    .filter((row) => row.amountCur > 0 && row.invoice)
                    .map((row) => (
                      <Stack
                        key={row.transRecId}
                        direction="row"
                        spacing={2}
                        sx={{ alignItems: 'center' }}
                      >
                        <Typography sx={{ flex: 1 }}>
                          {row.invoice} · {row.transDate.slice(0, 10)} · {money(row.amountCur)}{' '}
                          {row.currencyCode}
                        </Typography>
                        <Button
                          disabled={
                            !selected ||
                            row.currencyCode !== selected.currencyCode ||
                            row.amountCur !== selected.amountCurCredit ||
                            row.transDate.slice(0, 10) > selected.transDate.slice(0, 10)
                          }
                          onClick={() => selectInvoice(row)}
                        >
                          Mark
                        </Button>
                      </Stack>
                    ))
                )}
                {!markedOnly &&
                  openTransactions.isSuccess &&
                  !openTransactions.data.transactions.some(
                    (row) => row.amountCur > 0 && row.invoice
                  ) && <Typography>No open invoices.</Typography>}
              </Stack>
            </DialogContent>
            <DialogActions>
              {selected?.markedInvoiceRecId && !locked && canEdit ? (
                <Button
                  onClick={() => {
                    setDraft({ ...selected, ...clearMark });
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
