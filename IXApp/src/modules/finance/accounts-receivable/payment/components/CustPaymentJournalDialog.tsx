import { RecordPreview } from '@shared/components/record-preview';
import React, { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Button,
  FormControl,
  FormControlLabel,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  Switch,
  TextField,
  Typography,
} from '@mui/material';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { AppActionDrawer } from '@shared/components/dialogs/AppActionDrawer';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { custPaymentApi } from '../api/custPaymentApi';
import { custOpenTransApi, type CustOpenTransRow } from '../../settlement/api/custOpenTransApi';
import { customerPaymentQueryKeys } from '../../queries/accountsReceivableQueryKeys';

interface Props {
  open: boolean;
  custAccount: string;
  customerName: string;
  canPost: boolean;
  onClose: () => void;
  onPosted?: () => Promise<void> | void;
}

const dateToday = () => {
  const date = new Date();
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
};

export function CustPaymentJournalDialog({
  open,
  custAccount,
  customerName,
  canPost,
  onClose,
  onPosted,
}: Props): React.ReactElement {
  const { t } = useAppTranslation();
  const company = useCompanyStore((state) => state.currentCompany);
  const queryClient = useQueryClient();
  const [bankAccountId, setBankAccountId] = useState('');
  const [amount, setAmount] = useState('');
  const [paymentDate, setPaymentDate] = useState(dateToday);
  const [settleAutomatically, setSettleAutomatically] = useState(true);
  const [prepayment, setPrepayment] = useState(false);
  const [settleInvoiceId, setSettleInvoiceId] = useState('');
  const [posting, setPosting] = useState(false);
  const [requestId, setRequestId] = useState<string>();
  const [error, setError] = useState('');
  const [result, setResult] = useState<string>('');

  const bankQuery = useQuery({
    queryKey: customerPaymentQueryKeys.bankAccounts(company),
    queryFn: ({ signal }) => custPaymentApi.bankAccounts(signal),
    enabled: open,
  });
  const openTransQuery = useQuery({
    queryKey: customerPaymentQueryKeys.openTransactions(company, custAccount),
    queryFn: ({ signal }) => custOpenTransApi.load(custAccount, signal),
    enabled: open && Boolean(custAccount),
  });

  useEffect(() => {
    if (!open) return;
    setBankAccountId('');
    setAmount('');
    setPaymentDate(dateToday());
    setSettleAutomatically(true);
    setPrepayment(false);
    setSettleInvoiceId('');
    setError('');
    setResult('');
    setRequestId(undefined);
  }, [open, custAccount]);

  // A prepayment is money received before an invoice exists, so there is nothing to settle.
  useEffect(() => {
    if (prepayment) {
      setSettleAutomatically(false);
      setSettleInvoiceId('');
    }
  }, [prepayment]);

  const openTransactions = openTransQuery.data?.transactions ?? [];
  const currency = openTransQuery.data?.customerCurrency ?? '';

  const columns = useMemo<ColumnDef<CustOpenTransRow>[]>(
    () => [
      { field: 'invoice', headerName: 'custOpenTrans.fields.invoice', width: 150 },
      { field: 'transDate', headerName: 'custOpenTrans.fields.transDate', width: 120 },
      { field: 'dueDate', headerName: 'custOpenTrans.fields.dueDate', width: 120 },
      {
        preview: { entity: 'currency' },
        field: 'currencyCode',
        headerName: 'custOpenTrans.fields.currencyCode',
        width: 100,
      },
      {
        field: 'amountCur',
        headerName: 'custOpenTrans.fields.amountCur',
        width: 150,
        align: 'right',
        type: 'number',
      },
    ],
    []
  );

  const amountValue = Number(amount);
  const amountValid = amount.trim().length > 0 && Number.isFinite(amountValue) && amountValue > 0;
  const canSubmit =
    canPost && !posting && Boolean(bankAccountId) && Boolean(paymentDate) && amountValid;

  const submit = async () => {
    if (!canSubmit) return;
    setPosting(true);
    setError('');
    try {
      // The server treats the request id as an idempotency key, so retrying a payment
      // whose response was lost returns the one already posted.
      const id = requestId ?? crypto.randomUUID();
      setRequestId(id);
      const posted = await custPaymentApi.post({
        custAccount,
        bankAccountId,
        amount: amountValue,
        paymentDate,
        settleInvoiceId: settleInvoiceId || undefined,
        settleAutomatically,
        prepayment,
        requestId: id,
      });
      setResult(
        t('customerPayment.posted', {
          journalNum: posted.journalNum,
          voucher: posted.voucher,
        })
      );
      await Promise.all([
        queryClient.invalidateQueries({
          queryKey: customerPaymentQueryKeys.openTransactions(company, custAccount),
        }),
        onPosted?.(),
      ]);
      setRequestId(undefined);
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : t('customerPayment.postFailed'));
    } finally {
      setPosting(false);
    }
  };

  return (
    <AppActionDrawer
      open={open}
      onClose={onClose}
      title={t('customerPayment.title')}
      width={1100}
      busy={posting}
      actions={
        <>
          {error && (
            <Alert severity="error" sx={{ flex: 1, minWidth: 0 }}>
              {error}
            </Alert>
          )}
          {!error && result && (
            <Alert severity="success" sx={{ flex: 1, minWidth: 0 }}>
              {result}
            </Alert>
          )}
          <Button onClick={onClose} disabled={posting}>
            {result ? t('actions.close') : t('actions.cancel')}
          </Button>
          <Button variant="contained" onClick={() => void submit()} disabled={!canSubmit}>
            {posting ? t('customerPayment.postingInProgress') : t('customerPayment.post')}
          </Button>
        </>
      }
    >
      <Stack spacing={1.5} sx={{ minHeight: '100%' }}>
        <Typography variant="body2" color="text.secondary">
          {t('customerPayment.hint', { custAccount, customerName })}
        </Typography>
        <Stack direction="row" spacing={1.5} useFlexGap sx={{ flexWrap: 'wrap' }}>
          <RecordPreview
            entity="referenceOption"
            recordKey={bankAccountId}
            option={{
              code: bankAccountId,
              name: bankQuery.data?.find((bank) => bank.accountId === bankAccountId)?.name,
            }}
          >
            <FormControl size="small" sx={{ minWidth: 260 }}>
              <InputLabel id="cust-payment-bank">{t('customerPayment.bankAccount')}</InputLabel>
              <Select
                labelId="cust-payment-bank"
                label={t('customerPayment.bankAccount')}
                value={bankAccountId}
                disabled={posting || bankQuery.isLoading}
                onChange={(event) => setBankAccountId(event.target.value)}
              >
                {(bankQuery.data ?? []).map((bank) => (
                  <MenuItem key={bank.accountId} value={bank.accountId}>
                    {bank.accountId} — {bank.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </RecordPreview>
          <TextField
            label={t('customerPayment.amount')}
            type="number"
            size="small"
            value={amount}
            disabled={posting}
            onChange={(event) => setAmount(event.target.value)}
            slotProps={{ htmlInput: { min: 0, step: 'any' } }}
            sx={{ width: 200 }}
            helperText={currency || undefined}
          />
          <TextField
            label={t('customerPayment.paymentDate')}
            type="date"
            size="small"
            value={paymentDate}
            disabled={posting}
            onChange={(event) => setPaymentDate(event.target.value)}
            sx={{ width: 200 }}
          />
          <FormControl size="small" sx={{ minWidth: 240 }}>
            <InputLabel id="cust-payment-invoice">{t('customerPayment.settleInvoice')}</InputLabel>
            <Select
              labelId="cust-payment-invoice"
              label={t('customerPayment.settleInvoice')}
              value={settleInvoiceId}
              disabled={posting || prepayment || openTransQuery.isLoading}
              onChange={(event) => setSettleInvoiceId(event.target.value)}
            >
              <MenuItem value="">{t('customerPayment.oldestFirst')}</MenuItem>
              {openTransactions
                .filter((row) => row.invoice)
                .map((row) => (
                  <MenuItem key={row.recId} value={row.invoice}>
                    {row.invoice} — {row.amountCur.toLocaleString()}
                  </MenuItem>
                ))}
            </Select>
          </FormControl>
        </Stack>
        <Stack direction="row" spacing={2} useFlexGap sx={{ flexWrap: 'wrap' }}>
          <FormControlLabel
            control={
              <Switch
                checked={settleAutomatically}
                disabled={posting || prepayment}
                onChange={(event) => setSettleAutomatically(event.target.checked)}
              />
            }
            label={t('customerPayment.settleAutomatically')}
          />
          <FormControlLabel
            control={
              <Switch
                checked={prepayment}
                disabled={posting}
                onChange={(event) => setPrepayment(event.target.checked)}
              />
            }
            label={t('customerPayment.prepayment')}
          />
        </Stack>
        {openTransQuery.isError && (
          <Alert severity="warning">{t('customerPayment.openTransactionsFailed')}</Alert>
        )}
        <Typography variant="subtitle2">{t('customerPayment.openTransactions')}</Typography>
        <DataGrid
          rows={openTransactions}
          columns={columns}
          getRowId={(row) => String(row.recId)}
          loading={openTransQuery.isLoading}
          height={320}
          rowHeight={34}
          headerHeight={32}
        />
      </Stack>
    </AppActionDrawer>
  );
}
