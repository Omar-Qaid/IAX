import React, { useMemo, useState } from 'react';
import { Alert, Checkbox, FormControlLabel, MenuItem, Stack, TextField } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@core/auth/useAuth';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { PERMISSIONS } from '@core/permissions/permissions';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { LookupField } from '@shared/components/lookups/LookupField';
import { useNotifications } from '@shared/hooks/useNotifications';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS as paths } from '../../routes/accountsReceivableRoutePaths';
import {
  ledgerJournalCustPaymApi as api,
  newPaymentJournal,
  type PaymentJournal,
} from '../api/ledgerJournalCustPaymApi';
import {
  JournalFieldGroups,
  type JournalFieldGroup,
  JournalTabs,
  paymentJournalKeys,
  usePaymentJournalActions,
} from '../components/PaymentJournalShared';

export function LedgerJournalListCustPaym() {
  const company = useCompanyStore((state) => state.currentCompany);
  return <JournalList key={company} company={company} />;
}
function JournalList({ company }: { company: string }) {
  const navigate = useNavigate();
  const { user, hasPermission } = useAuth();
  const { notifyError } = useNotifications();
  const actions = usePaymentJournalActions();
  const [show, setShow] = useState('notPosted');
  const [mine, setMine] = useState(false);
  const [tab, setTab] = useState('List');
  const [editing, setEditing] = useState(false);
  const query = useQuery({
    queryKey: [...paymentJournalKeys(company), 'list'],
    queryFn: ({ signal }) => api.list(signal),
  });
  const names = useQuery({
    queryKey: [...paymentJournalKeys(company), 'names'],
    queryFn: ({ signal }) => api.names(signal),
  });
  // Only to name the offset account on the Setup tab; the tab still shows the id without it.
  const offsets = useQuery({
    queryKey: [...paymentJournalKeys(company), 'offsets'],
    queryFn: ({ signal }) => api.offsets(signal),
    enabled: tab === 'Setup',
    retry: false,
  });
  const offsetLabel = (row: PaymentJournal) => {
    if (!row.offsetLedgerDimension) return '';
    const offset = offsets.data?.find((item) => Number(item.id) === row.offsetLedgerDimension);
    return offset
      ? `${offset.code}${offset.name ? ` - ${offset.name}` : ''}`
      : String(row.offsetLedgerDimension);
  };
  const rate = (value?: number) => (value ? String(value) : '');
  const dateTime = (value?: string) => (value ? value.replace('T', ' ').slice(0, 19) : '');
  /** D365 fields this journal does not store yet: same place as in D365, shown empty. */
  const missing = (label: string, toggle = false) => ({ label, toggle, unavailable: true });
  const workflowStatus = [
    'Not submitted',
    'Submitted',
    'In review',
    'Approved',
    'Rejected',
    'Cancelled',
  ];
  /** The header tabs, laid out as the D365 Customer payment journal form (column by column). */
  const headerGroups = (current: string, row: PaymentJournal): JournalFieldGroup[] => {
    if (current === 'General')
      return [
        {
          title: 'Identification',
          fields: [{ label: 'Journal batch number', value: row.journalNum }],
        },
        { title: 'Journal', fields: [{ label: 'Name', value: row.journalName }] },
        {
          fields: [
            { label: 'Journal type', value: 'Customer payment' },
            { label: 'Description', value: row.name },
          ],
        },
        {
          title: 'Posting',
          // D365 CurrentOperationsTax: 0 = Current (the only layer posted here).
          fields: [
            {
              label: 'Posting layer',
              value: row.currentOperationsTax ? 'Other (not posted here)' : 'Current',
            },
          ],
        },
        {
          fields: [
            {
              label: 'Detail level',
              value: row.detailSummaryPosting === 1 ? 'Summary' : 'Details',
            },
            { label: 'Lines limit', value: row.linesLimitBeforeDistribution || '' },
          ],
        },
        {
          fields: [
            { label: 'Parent journal number', value: row.parentJournalNum },
            missing('Requires validation', true),
          ],
        },
        { title: 'Payment codes', fields: [missing('Category purpose')] },
        {
          fields: [missing('Charge bearer'), missing('Local instrument'), missing('Service level')],
        },
      ];
    if (current === 'Setup')
      return [
        {
          title: 'Offset account',
          fields: [
            { label: 'Account type', value: row.offsetAccountType === 5 ? 'Bank' : 'Ledger' },
            { label: 'Offset account', value: offsetLabel(row) },
          ],
        },
        {
          title: 'Document',
          fields: [
            { label: 'Document', value: row.documentNum },
            {
              label: 'Number allocation at posting',
              value: row.voucherAllocatedAtPosting,
              toggle: true,
              heading: 'Voucher allocation',
            },
          ],
        },
        {
          title: 'Sales tax',
          fields: [
            { label: 'Amounts include sales tax', value: row.ledgerJournalInclTax, toggle: true },
            {
              label: 'Legal entity for intercompany tax posting',
              value: row.taxObligationCompany ? 'Source' : 'Destination',
            },
          ],
        },
        {
          title: 'Currency',
          fields: [
            { label: 'Currency', value: row.currencyCode },
            { label: 'Fixed rate', value: row.fixedExchRate, toggle: true },
            { label: 'Exchange rate', value: rate(row.exchRate) },
            { label: 'Secondary exchange rate', value: rate(row.exchRateSecondary) },
          ],
        },
        {
          title: 'Reporting currency',
          fields: [
            { label: 'Fixed rate', value: row.reportingCurrencyFixedExchRate, toggle: true },
            { label: 'Exchange rate', value: rate(row.reportingCurrencyExchRate) },
            {
              label: 'Secondary exchange rate',
              value: rate(row.reportingCurrencyExchRateSecondary),
            },
          ],
        },
        {
          title: 'Bank',
          fields: [
            {
              label: 'Summarize amounts in bank account',
              value: row.bankTransSummarizationEnabled,
              toggle: true,
            },
            {
              label: 'Summarization criteria',
              value: row.bankTransSummarizationEnabled
                ? row.bankTransSummarizationCriteria === 1
                  ? 'Custom'
                  : 'Standard'
                : 'Do not summarize',
            },
          ],
        },
      ];
    if (current === 'Blocking')
      return [
        { fields: [{ label: 'In use', value: row.sessionId, toggle: true }] },
        { fields: [missing('Private', true)] },
        { fields: [missing('Used by user')] },
        { fields: [missing('Private for user')] },
        { fields: [{ label: 'Locked by system', value: row.systemBlocked, toggle: true }] },
        {
          fields: [
            {
              label: 'System locked reason',
              value: row.systemBlocked ? row.systemBlockedReason || 'Blocked' : 'None',
            },
          ],
        },
        { fields: [missing('Private for user group')] },
      ];
    if (current === 'Financial dimensions')
      return [
        {
          title: 'Financial dimensions',
          fields: [
            {
              label: 'Default dimensions',
              value: row.defaultDimension ? 'Configured on this journal' : 'Not specified',
            },
          ],
        },
      ];
    return [
      { fields: [missing('Reported as ready by')] },
      { fields: [missing('Approved by')] },
      { fields: [missing('Rejected by')] },
      {
        fields: [
          {
            label: 'Workflow approval status',
            value: workflowStatus[row.workflowApprovalStatus ?? 0] ?? 'None',
          },
        ],
      },
      {
        title: 'Origin',
        fields: [
          { label: 'Intercompany accounting', value: row.originalCompany },
          { label: 'Original journal No.', value: row.originalJournalNum },
        ],
      },
      { fields: [{ label: 'Posted', value: row.posted, toggle: true }] },
      { fields: [{ label: 'Posted on', value: row.posted ? dateTime(row.postedDateTime) : '' }] },
      { fields: [{ label: 'Log', value: row.log }] },
    ];
  };
  const rows = useMemo(
    () =>
      (query.data ?? []).filter(
        (row) =>
          (show === 'all' || row.posted === (show === 'posted' ? 1 : 0)) &&
          (!mine || row.createdBy === user?.id)
      ),
    [query.data, show, mine, user?.id]
  );
  const editable = (row: PaymentJournal) => !row.posted && !row.systemBlocked && !actions.busy;
  const columns = useMemo<ColumnDef<PaymentJournal>[]>(
    () => [
      {
        field: 'journalNum',
        headerName: 'Journal batch number',
        width: 200,
        getDetailHref: (row) => paths.ledgerJournalTransCustPaym(row.id),
      },
      {
        field: 'journalName',
        headerName: 'Name',
        width: 130,
        editable: true,
        renderEditCell: ({ row, value, onChange, disabled }) => (
          <LookupField
            name="journalName"
            label=""
            value={String(value || '')}
            options={(names.data ?? []).map((name) => ({
              id: name.journalName,
              code: name.journalName,
              name: name.name,
            }))}
            onChange={onChange}
            disabled={disabled || !!row.recId || names.isLoading || names.isError}
            displayMode="select"
            searchable
            compact
            gridCell
          />
        ),
      },
      {
        field: 'name',
        headerName: 'Description',
        minWidth: 300,
        flex: 1,
        editable: true,
        renderEditCell: ({ row, value, onChange, disabled }) => (
          <TextField
            variant="standard"
            fullWidth
            value={value ?? ''}
            onChange={(event) => onChange(event.target.value)}
            disabled={
              disabled ||
              !!row.posted ||
              !!row.systemBlocked ||
              (!!row.recId && !hasPermission(PERMISSIONS.LEDGER_JOURNAL_EDIT))
            }
            slotProps={{ htmlInput: { 'aria-label': 'Description' } }}
          />
        ),
      },
      {
        field: 'posted',
        headerName: 'Posted',
        width: 85,
        type: 'boolean',
        valueGetter: ({ row }) => row.posted === 1,
      },
      {
        field: 'postedDateTime',
        headerName: 'Posted on',
        width: 170,
        valueGetter: ({ row }) =>
          row.posted ? row.postedDateTime.replace('T', ' ').slice(0, 19) : '',
      },
      {
        field: 'systemBlocked',
        headerName: 'Blocked',
        width: 85,
        type: 'boolean',
        valueGetter: ({ row }) => row.systemBlocked === 1,
      },
      { field: 'lastModifiedBy', headerName: 'Modified by', width: 140 },
      { field: 'createdBy', headerName: 'Created by', width: 140 },
    ],
    [names.data, names.isLoading, names.isError, hasPermission]
  );
  return (
    <SimpleListPage<PaymentJournal>
      title="Customer payment journal"
      recordTableName="LedgerJournalTable"
      columns={columns}
      dataSource={{
        type: 'controlled',
        rows,
        loading: query.isLoading,
        error: query.error?.message,
        refresh: () => {
          void actions.refresh();
        },
      }}
      enterpriseConfig={{
        readOnly: false,
        initialSelection: 'first',
        crud: {
          newPermission: PERMISSIONS.LEDGER_JOURNAL_CREATE,
          editPermission: PERMISSIONS.LEDGER_JOURNAL_EDIT,
          deletePermission: PERMISSIONS.LEDGER_JOURNAL_DELETE,
          canEdit: (row) => tab === 'List' && editable(row),
          canDelete: (selected) => selected.every(editable),
          onDelete: (selected) => {
            void (async () => {
              try {
                for (const row of selected) await api.headers.delete(row);
                await actions.refresh();
              } catch (error) {
                notifyError((error as Error).message);
              }
            })();
          },
        },
        // D365 order: Enter customer payments, Lines, Open lines in Excel, Validate, Post,
        // Approval, Inquiries, Print, Unlock. Not yet available here: shown disabled.
        getCommands: (row) => [
          { id: 'enter-payments', label: 'Enter customer payments', disabled: true },
          {
            id: 'lines',
            label: 'Lines',
            requiresSelection: true,
            onClick: () => row && navigate(paths.ledgerJournalTransCustPaym(row.id)),
          },
          { id: 'open-lines-excel', label: 'Open lines in Excel', disabled: true },
          {
            id: 'validate',
            label: 'Validate',
            requiresSelection: true,
            disabled: !row || !editable(row),
            onClick: () => actions.run(row, 'validate'),
          },
          {
            id: 'simulate',
            label: 'Simulate posting',
            requiresSelection: true,
            disabled: !row || !editable(row),
            onClick: () => actions.run(row, 'simulate'),
          },
          {
            id: 'post',
            label: 'Post',
            permission: PERMISSIONS.LEDGER_JOURNAL_EDIT,
            requiresSelection: true,
            disabled: !row || !editable(row),
            onClick: () => actions.run(row, 'post'),
          },
          { id: 'approval', label: 'Approval', disabled: true },
          { id: 'inquiries', label: 'Inquiries', disabled: true },
          { id: 'print', label: 'Print', disabled: true },
          { id: 'unlock', label: 'Unlock', disabled: true },
        ],
      }}
      filterBar={
        <Stack spacing={1}>
          {names.isError && <Alert severity="error">{names.error.message}</Alert>}
          <Stack direction="row" spacing={2} sx={{ alignItems: 'center' }}>
            <TextField
              select
              size="small"
              label="Show"
              value={show}
              disabled={editing}
              onChange={(event) => setShow(event.target.value)}
              sx={{ width: 160 }}
            >
              <MenuItem value="notPosted">Not posted</MenuItem>
              <MenuItem value="posted">Posted</MenuItem>
              <MenuItem value="all">All</MenuItem>
            </TextField>
            <FormControlLabel
              label="Show user-created only"
              control={
                <Checkbox
                  size="small"
                  checked={mine}
                  disabled={editing}
                  onChange={(_, value) => setMine(value)}
                />
              }
            />
          </Stack>
          <JournalTabs
            tabs={['List', 'General', 'Setup', 'Blocking', 'Financial dimensions', 'History']}
            value={tab}
            onChange={setTab}
            disabled={editing}
          />
        </Stack>
      }
      detailContent={
        tab === 'List'
          ? undefined
          : (row) =>
              row ? (
                <JournalFieldGroups groups={headerGroups(tab, row)} />
              ) : (
                <Alert severity="info">Select a journal.</Alert>
              )
      }
      dataGridProps={{
        masterForm:
          tab === 'List' &&
          !actions.busy &&
          (hasPermission(PERMISSIONS.LEDGER_JOURNAL_EDIT) ||
            hasPermission(PERMISSIONS.LEDGER_JOURNAL_CREATE)),
        onEditingChange: setEditing,
        onNewRow: () => newPaymentJournal(company),
        onRowSave: async (partial, isNew) => {
          const row = {
            ...(isNew
              ? newPaymentJournal(company)
              : query.data?.find((item) => item.id === partial.id)),
            ...partial,
          } as PaymentJournal;
          if (!row.journalName) throw new Error('Select a customer payment journal name.');
          if (isNew) {
            if (!hasPermission(PERMISSIONS.LEDGER_JOURNAL_CREATE))
              throw new Error('You do not have permission to create journals.');
            const name = names.data?.find((item) => item.journalName === row.journalName);
            if (!name) throw new Error('Select a configured customer payment journal name.');
            await api.headers.create({
              ...row,
              name: row.name || name.name,
              currencyCode: name.currencyCode,
              defaultDimension: name.defaultDimension,
              offsetLedgerDimension: name.offsetLedgerDimension,
              offsetAccountType: name.offsetAccountType,
            });
          } else {
            if (!hasPermission(PERMISSIONS.LEDGER_JOURNAL_EDIT) || !editable(row))
              throw new Error('This journal cannot be edited.');
            await api.headers.update(row);
          }
          await actions.refresh();
        },
        storageKey: `LedgerJournalListCustPaym:${company}`,
        hideToolbar: true,
      }}
      dialogs={actions.dialogs}
    />
  );
}
