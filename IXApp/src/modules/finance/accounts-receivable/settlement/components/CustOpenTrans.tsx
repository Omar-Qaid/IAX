import React, { useEffect, useMemo, useRef, useState } from 'react';
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Alert,
  Box,
  Button,
  Checkbox,
  MenuItem,
  Stack,
  Tab,
  Tabs,
  TextField,
  Typography,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useAuth } from '@core/auth/useAuth';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { UnavailableCommandButton } from '@shared/components/action-pane/UnavailableCommandButton';
import { AppActionDrawer } from '@shared/components/dialogs/AppActionDrawer';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { custOpenTransApi, type CustOpenTransRow } from '../api/custOpenTransApi';

type Row = CustOpenTransRow & { id: string };
const tabs = [
  'overview',
  'general',
  'payment',
  'settlement',
  'cashDiscount',
  'financialDimensions',
] as const;
type TabId = (typeof tabs)[number];
const detailFields: Record<Exclude<TabId, 'overview'>, (keyof CustOpenTransRow)[]> = {
  general: ['invoice', 'transDate', 'voucher', 'description', 'dueDate'],
  payment: ['currencyCode', 'paymentMode', 'paymentReference', 'paymentTerms', 'prepayment'],
  settlement: ['originalAmountCur', 'settledAmountCur', 'amountCur', 'amountMst', 'lastSettleDate'],
  cashDiscount: ['cashDiscDate', 'cashDiscCode', 'possibleCashDisc', 'cashDiscTaken'],
  financialDimensions: ['worker', 'businessUnit', 'department', 'costCenter', 'project'],
};
const dateValue = (value: string) =>
  value && !value.startsWith('0001-') ? value.slice(0, 10) : '';
const today = () => {
  const now = new Date();
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
};
export interface CustOpenTransProps {
  open: boolean;
  accountNum: string;
  customerName: string;
  onClose: () => void;
}
export function CustOpenTrans(props: CustOpenTransProps): React.ReactElement | null {
  const company = useCompanyStore((state) => state.currentCompany);
  if (!props.open) return null;
  return <SettlementDialog key={`${company}:${props.accountNum}`} {...props} company={company} />;
}
function SettlementDialog({
  accountNum,
  customerName,
  onClose,
  company,
}: CustOpenTransProps & { company: string }): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const { hasPermission } = useAuth();
  const canView = hasPermission(PERMISSIONS.CUSTOMER_PAYMENT_VIEW);
  const canPost = canView && hasPermission(PERMISSIONS.CUSTOMER_PAYMENT_POST);
  const queryClient = useQueryClient();
  const [tab, setTab] = useState<TabId>('overview');
  const [activeId, setActiveId] = useState('');
  const [marked, setMarked] = useState<number[]>([]);
  const [show, setShow] = useState<'all' | 'marked' | 'unmarked'>('all');
  const [dateMode, setDateMode] = useState<'latest' | 'today' | 'selected'>('latest');
  const [userDate, setUserDate] = useState(today);
  const [posting, setPosting] = useState(false);
  const postingLock = useRef(false);
  const [postError, setPostError] = useState('');
  const [success, setSuccess] = useState('');
  const query = useQuery({
    queryKey: ['cust-open-trans', company, accountNum],
    queryFn: ({ signal }) => custOpenTransApi.load(accountNum, signal),
    enabled: canView && Boolean(company && accountNum),
    refetchOnWindowFocus: false,
  });
  useEffect(() => {
    setMarked([]);
  }, [query.dataUpdatedAt]);
  const rows = useMemo<Row[]>(
    () => (query.data?.transactions ?? []).map((row) => ({ ...row, id: String(row.recId) })),
    [query.data]
  );
  const chosen = rows.filter((row) => marked.includes(row.transRecId));
  const visible = rows.filter(
    (row) => show === 'all' || (show === 'marked') === marked.includes(row.transRecId)
  );
  const active = visible.find((row) => row.id === activeId) ?? visible[0] ?? null;
  const latest = chosen.reduce(
    (value, row) =>
      [value, dateValue(row.transDate), dateValue(row.lastSettleDate)].sort().at(-1) ?? '',
    ''
  );
  const settleDate = dateMode === 'latest' ? latest : dateMode === 'today' ? today() : userDate;
  const currencySet = new Set(chosen.map((row) => row.currencyCode));
  const rates = new Set(
    chosen
      .filter((row) => row.originalAmountCur !== 0)
      .map((row) => (row.originalAmountMst / row.originalAmountCur).toFixed(6))
  );
  const hasBothSides =
    chosen.some((row) => row.amountCur > 0) && chosen.some((row) => row.amountCur < 0);
  const validDate = /^\d{4}-\d{2}-\d{2}$/.test(settleDate) && settleDate >= latest;
  const postEnabled =
    canPost &&
    !posting &&
    !query.isFetching &&
    !query.isError &&
    query.data?.reconciled === true &&
    chosen.length >= 2 &&
    hasBothSides &&
    currencySet.size === 1 &&
    rates.size <= 1 &&
    validDate;
  const disabled = posting || query.isFetching || query.isError;
  const tr = (key: string) => t(`custOpenTrans.${key}`);
  const number = new Intl.NumberFormat(isRtl ? 'ar-SA' : 'en-US', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
  const money = (value: number) => number.format(value);
  const value = (row: Row, field: keyof CustOpenTransRow): string => {
    const result = row[field];
    if (result == null || result === '') return '—';
    if (typeof result === 'boolean') return tr(result ? 'yes' : 'no');
    if (typeof result === 'number') return money(result);
    return field.toLowerCase().endsWith('date') ? dateValue(result) || '—' : result;
  };
  const field = (label: string, val: string) => (
    <TextField
      key={label}
      label={label}
      value={val}
      size="small"
      slotProps={{ input: { readOnly: true }, inputLabel: { shrink: true } }}
      sx={{ minWidth: 140, flex: '0 1 180px' }}
    />
  );
  const toggle = (row: Row) => {
    if (!canPost || disabled) return;
    setActiveId(row.id);
    setMarked((current) =>
      current.includes(row.transRecId)
        ? current.filter((id) => id !== row.transRecId)
        : [...current, row.transRecId]
    );
  };
  const columns: ColumnDef<Row>[] = [
    {
      field: 'mark',
      headerName: 'custOpenTrans.mark',
      width: 70,
      sortable: false,
      filterable: false,
      renderCell: ({ row }) => (
        <Checkbox
          size="small"
          checked={marked.includes(row.transRecId)}
          disabled={!canPost || disabled || row.amountCur === 0}
          onClick={(event) => event.stopPropagation()}
          onChange={() => toggle(row)}
          slotProps={{ input: { 'aria-label': `${tr('mark')} ${row.invoice || row.voucher}` } }}
        />
      ),
    },
    ...(['invoice', 'dueDate', 'cashDiscDate'] as const).map((name) => ({
      field: name,
      headerName: `custOpenTrans.fields.${name}`,
      width: name === 'invoice' ? 180 : 140,
      valueGetter: ({ row }: { row: Row }) =>
        name.endsWith('Date') ? dateValue(row[name]) : row[name],
    })),
    ...(['amountCur', 'amountMst'] as const).map((name) => ({
      field: name,
      headerName: `custOpenTrans.fields.${name}`,
      width: 160,
      align: 'right' as const,
      renderCell: ({ row }: { row: Row }) => money(row[name]),
    })),
    {
      preview: { entity: 'currency' },
      field: 'currencyCode',
      headerName: 'custOpenTrans.fields.currencyCode',
      width: 100,
    },
    { field: 'voucher', headerName: 'custOpenTrans.fields.voucher', width: 180 },
  ];
  const refresh = async () => {
    setMarked([]);
    await query.refetch();
  };
  const post = async () => {
    if (!postEnabled || postingLock.current) return;
    postingLock.current = true;
    setPosting(true);
    setPostError('');
    setSuccess('');
    try {
      const result = await custOpenTransApi.post(
        accountNum,
        settleDate,
        chosen.map((row) => row.transRecId)
      );
      setSuccess(t('custOpenTrans.posted', { voucher: result.settlementVoucher }));
      setMarked([]);
      setShow('all');
      await queryClient.invalidateQueries({
        predicate: (entry) =>
          [
            'cust-open-trans',
            'CustTrans',
            'customer-open-transactions',
            'party-balance-currency',
          ].includes(String(entry.queryKey[0])) && entry.queryKey.includes(company),
      });
    } catch (error) {
      setPostError(error instanceof Error ? error.message : tr('postFailed'));
      // Re-read balances before another attempt, including after a lost response.
      await refresh();
    } finally {
      postingLock.current = false;
      setPosting(false);
    }
  };
  const markedCurrency =
    currencySet.size === 1
      ? chosen[0]?.currencyCode
      : chosen.length
        ? '—'
        : (query.data?.customerCurrency ?? '');
  return (
    <AppActionDrawer
      open
      onClose={onClose}
      busy={posting}
      width={1793}
      appearance="posting"
      viewLabel={tr('standardView')}
      title={t('custOpenTrans.title', {
        name: query.data?.customerName || customerName || accountNum,
      })}
      actions={
        <>
          <Button variant="contained" disabled={!postEnabled} onClick={() => void post()}>
            {tr('post')}
          </Button>
          <UnavailableCommandButton label={tr('settleRemainder')} />
          <Button variant="outlined" disabled={posting} onClick={onClose}>
            {t('actions.cancel', 'Cancel')}
          </Button>
        </>
      }
    >
      {!canView ? (
        <Alert severity="warning">{t('partyBalance.accessDenied')}</Alert>
      ) : !accountNum || !company ? (
        <Alert severity="error">{tr('accountRequired')}</Alert>
      ) : (
        <>
          <Stack direction="row" sx={{ gap: 2, flexWrap: 'wrap', my: 2 }}>
            <TextField
              select
              size="small"
              label={tr('postingDate')}
              value={dateMode}
              disabled={disabled}
              onChange={(event) => setDateMode(event.target.value as typeof dateMode)}
              sx={{ minWidth: 200 }}
            >
              <MenuItem value="latest">{tr('latestDate')}</MenuItem>
              <MenuItem value="today">{tr('today')}</MenuItem>
              <MenuItem value="selected">{tr('selectedDate')}</MenuItem>
            </TextField>
            <TextField
              type="date"
              size="small"
              label={tr('date')}
              value={settleDate}
              disabled={disabled || dateMode !== 'selected'}
              onChange={(event) => setUserDate(event.target.value)}
              slotProps={{ inputLabel: { shrink: true } }}
            />
            <TextField
              select
              size="small"
              label={tr('discountDate')}
              value="transaction"
              disabled
              sx={{ minWidth: 260 }}
            >
              <MenuItem value="transaction">{tr('transactionDate')}</MenuItem>
            </TextField>
          </Stack>
          <Alert severity="info" sx={{ mb: 1 }}>
            {tr('basicSettlement')}
          </Alert>
          {postError && <Alert severity="error">{postError}</Alert>}
          {success && <Alert severity="success">{success}</Alert>}
          {query.data?.reconciled === false && (
            <Alert severity="warning">{tr('notReconciled')}</Alert>
          )}
          {chosen.length > 0 && (currencySet.size > 1 || rates.size > 1) && (
            <Alert severity="warning">{tr('sameCurrencyRate')}</Alert>
          )}
          {chosen.length > 0 && !validDate && <Alert severity="warning">{tr('invalidDate')}</Alert>}
          <Tabs
            value={tab}
            onChange={(_, next: TabId) => setTab(next)}
            variant="scrollable"
            scrollButtons="auto"
            aria-label={tr('views')}
            sx={{ mb: 1 }}
          >
            {tabs.map((id) => (
              <Tab key={id} value={id} label={tr(`tabs.${id}`)} disabled={posting} />
            ))}
          </Tabs>
          {query.isPending ? (
            <LoadingState />
          ) : query.isError ? (
            <ErrorState message={query.error.message} onRetry={() => void refresh()} />
          ) : (
            <>
              {tab === 'overview' ? (
                <>
                  <Stack
                    direction="row"
                    sx={{ gap: 0.5, flexWrap: 'wrap', alignItems: 'center', mb: 1 }}
                  >
                    <Button
                      disabled={!canPost || disabled || !active || active.amountCur === 0}
                      onClick={() =>
                        active &&
                        setMarked((current) => [...new Set([...current, active.transRecId])])
                      }
                    >
                      {tr('markSelected')}
                    </Button>
                    <Button
                      disabled={disabled || marked.length === 0}
                      onClick={() => setMarked([])}
                    >
                      {tr('unmarkAll')}
                    </Button>
                    <TextField
                      select
                      size="small"
                      label={tr('showMarked')}
                      value={show}
                      disabled={disabled}
                      onChange={(event) => setShow(event.target.value as typeof show)}
                      sx={{ minWidth: 140 }}
                    >
                      {(['all', 'marked', 'unmarked'] as const).map((mode) => (
                        <MenuItem key={mode} value={mode}>
                          {tr(mode)}
                        </MenuItem>
                      ))}
                    </TextField>
                    <UnavailableCommandButton label={tr('paymentSchedule')} />
                    <Button disabled={!active || posting} onClick={() => setTab('general')}>
                      {tr('inquiry')}
                    </Button>
                    {[
                      'primaryPayment',
                      'settleInvoiceLines',
                      'markPriority',
                      'functions',
                      'deductions',
                    ].map((key) => (
                      <UnavailableCommandButton key={key} label={tr(key)} />
                    ))}
                    <Button disabled={disabled} onClick={() => void refresh()}>
                      {tr('refresh')}
                    </Button>
                  </Stack>
                  <DataGrid<Row>
                    rows={visible}
                    columns={columns}
                    height={290}
                    getRowId={(row) => row.id}
                    selectedIds={active ? [active.id] : []}
                    onRowClick={(row) => setActiveId(row.id)}
                    onSelectionChange={(ids) => setActiveId(String(ids.at(-1) ?? ''))}
                    selectionMode="single"
                    rowHeight={31}
                    hideAddRowButton
                    hideInlineEditActions
                    showCellBorders
                    showColumnBorders
                    storageKey="accounts-receivable.cust-open-trans"
                  />
                </>
              ) : (
                <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', py: 2, minHeight: 180 }}>
                  {active ? (
                    detailFields[tab].map((name) =>
                      field(tr(`fields.${name}`), value(active, name))
                    )
                  ) : (
                    <Typography>{tr('noSelection')}</Typography>
                  )}
                </Box>
              )}
              <Accordion defaultExpanded elevation={0} sx={{ mt: 1 }}>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Typography variant="subtitle2">{tr('lineDetails')}</Typography>
                </AccordionSummary>
                <AccordionDetails>
                  {!active ? (
                    <Typography>{tr('noSelection')}</Typography>
                  ) : (
                    <Stack direction="row" sx={{ gap: 3, flexWrap: 'wrap' }}>
                      <Box>
                        <Typography variant="overline">{tr('cashDiscount')}</Typography>
                        <Typography variant="caption" sx={{ display: 'block' }}>
                          {tr('transactionCurrency')}: {active.currencyCode}
                        </Typography>
                        <Stack direction="row" sx={{ gap: 1, flexWrap: 'wrap', mt: 1 }}>
                          {field(tr('amount'), money(active.possibleCashDisc))}
                          {field(tr('taken'), money(active.cashDiscTaken))}
                          {field(tr('toTake'), money(0))}
                          {field(tr('useCashDiscount'), tr('never'))}
                        </Stack>
                      </Box>
                      <Box>
                        <Typography variant="overline">{tr('invoice')}</Typography>
                        <Stack direction="row" sx={{ gap: 1, flexWrap: 'wrap', mt: 1 }}>
                          {field(tr('fields.transDate'), dateValue(active.transDate))}
                          {field(tr('company'), query.data?.dataAreaId ?? '')}
                          {field(tr('fields.voucher'), active.voucher)}
                          {field(tr('fields.description'), active.description)}
                        </Stack>
                      </Box>
                    </Stack>
                  )}
                </AccordionDetails>
              </Accordion>
              <Accordion defaultExpanded elevation={0}>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Typography variant="subtitle2">{tr('totals')}</Typography>
                </AccordionSummary>
                <AccordionDetails>
                  <Stack sx={{ gap: 2 }}>
                    <Stack direction="row" sx={{ gap: 1, flexWrap: 'wrap', alignItems: 'center' }}>
                      <Typography sx={{ width: 230 }}>
                        {tr('accountingCurrency')}: {query.data?.accountingCurrency}
                      </Typography>
                      {field(
                        tr('settlementBalance'),
                        money(chosen.reduce((sum, row) => sum + row.amountMst, 0))
                      )}
                      {field(tr('estimatedDiscount'), money(0))}
                      {field(tr('customerBalance'), money(query.data?.customerBalanceMst ?? 0))}
                    </Stack>
                    <Stack direction="row" sx={{ gap: 1, flexWrap: 'wrap', alignItems: 'center' }}>
                      <Typography sx={{ width: 230 }}>
                        {tr('transactionCurrency')}: {markedCurrency}
                      </Typography>
                      {field(
                        tr('settlementBalance'),
                        currencySet.size > 1
                          ? '—'
                          : money(chosen.reduce((sum, row) => sum + row.amountCur, 0))
                      )}
                      {field(tr('estimatedDiscount'), money(0))}
                    </Stack>
                  </Stack>
                </AccordionDetails>
              </Accordion>
            </>
          )}
        </>
      )}
    </AppActionDrawer>
  );
}
