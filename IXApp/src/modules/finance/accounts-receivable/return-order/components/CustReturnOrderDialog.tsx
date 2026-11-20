import React, { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  Step,
  StepLabel,
  Stepper,
  TextField,
  Typography,
} from '@mui/material';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { AppActionDrawer } from '@shared/components/dialogs/AppActionDrawer';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { custReturnApi, type CustReturnCreateResult } from '../api/custReturnApi';
import { custSalesLinesApi, type CustSalesLine } from '../../invoice/api/custSalesLinesApi';
import { salesOrderLinesApi } from '../../sales-order/api/salesOrderLinesApi';
import { returnOrderQueryKeys } from '../../queries/accountsReceivableQueryKeys';

interface Props {
  open: boolean;
  custAccount: string;
  customerName: string;
  canPost: boolean;
  onClose: () => void;
}

/** The three postings of a return: the order, the negative packing slip, the credit note. */
type Stage = 'create' | 'receive' | 'credit' | 'done';

const STAGES: Stage[] = ['create', 'receive', 'credit'];

const dateToday = () => {
  const date = new Date();
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
};

export function CustReturnOrderDialog({
  open,
  custAccount,
  customerName,
  canPost,
  onClose,
}: Props): React.ReactElement {
  const { t } = useAppTranslation();
  const company = useCompanyStore((state) => state.currentCompany);
  const queryClient = useQueryClient();
  const [stage, setStage] = useState<Stage>('create');
  const [invoiceId, setInvoiceId] = useState('');
  const [reasonCodeId, setReasonCodeId] = useState('');
  const [dispositionCodeId, setDispositionCodeId] = useState('');
  const [quantities, setQuantities] = useState<Record<string, string>>({});
  const [created, setCreated] = useState<CustReturnCreateResult>();
  const [receiptDate, setReceiptDate] = useState(dateToday);
  const [invoiceDate, setInvoiceDate] = useState(dateToday);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');
  const [requestIds, setRequestIds] = useState<{ receipt?: string; credit?: string }>({});

  const linesQuery = useQuery({
    queryKey: returnOrderQueryKeys.invoicedLines(company, custAccount),
    queryFn: ({ signal }) => custSalesLinesApi.load(custAccount, signal),
    enabled: open && Boolean(custAccount),
  });
  const dispositionQuery = useQuery({
    queryKey: returnOrderQueryKeys.dispositionCodes(company),
    queryFn: ({ signal }) => custReturnApi.dispositionCodes(signal),
    enabled: open,
  });
  const reasonQuery = useQuery({
    queryKey: returnOrderQueryKeys.reasonCodes(company),
    queryFn: ({ signal }) => custReturnApi.reasonCodes(signal),
    enabled: open,
  });
  // Only loaded once a return order exists: its own lines carry the ids the receipt posts against.
  const returnLinesQuery = useQuery({
    queryKey: returnOrderQueryKeys.returnLines(company, created?.recId),
    queryFn: ({ signal }) => salesOrderLinesApi.list(String(created!.recId), signal),
    enabled: open && Boolean(created),
  });

  useEffect(() => {
    if (!open) return;
    setStage('create');
    setInvoiceId('');
    setReasonCodeId('');
    setDispositionCodeId('');
    setQuantities({});
    setCreated(undefined);
    setReceiptDate(dateToday());
    setInvoiceDate(dateToday());
    setError('');
    setMessage('');
    setRequestIds({});
  }, [open, custAccount]);

  // Memoized because the ?? [] fallback is a new array each render, which would make the
  // derived memos below recompute every time.
  const allLines = useMemo(() => linesQuery.data?.lines ?? [], [linesQuery.data]);
  const invoiceIds = useMemo(
    () => Array.from(new Set(allLines.map((line) => line.invoiceId).filter(Boolean))).sort(),
    [allLines]
  );
  const invoiceLines = useMemo(
    () => allLines.filter((line) => line.invoiceId === invoiceId),
    [allLines, invoiceId]
  );

  const columns = useMemo<ColumnDef<CustSalesLine>[]>(
    () => [
      { field: 'itemId', headerName: 'custSalesLines.fields.itemId', width: 160 },
      { field: 'name', headerName: 'salesOrder.ui.text', minWidth: 200, flex: 1 },
      { field: 'salesUnit', headerName: 'fields.unit', width: 90 },
      {
        field: 'qty',
        headerName: 'custSalesLines.fields.qty',
        width: 120,
        align: 'right',
        type: 'number',
      },
      {
        field: 'lineNum',
        headerName: 'returnOrder.returnQuantity',
        width: 140,
        align: 'right',
        renderCell: ({ row }) => (
          <TextField
            size="small"
            type="number"
            value={quantities[row.recId] ?? ''}
            disabled={busy || stage !== 'create'}
            onChange={(event) =>
              setQuantities((current) => ({ ...current, [row.recId]: event.target.value }))
            }
            slotProps={{
              htmlInput: {
                min: 0,
                max: row.qty,
                step: 'any',
                'aria-label': t('returnOrder.returnQuantityFor', { itemId: row.itemId }),
              },
            }}
            sx={{
              width: '100%',
              '& .MuiInputBase-input': { py: 0.25, px: 0.5, textAlign: 'right' },
            }}
          />
        ),
      },
    ],
    [busy, quantities, stage, t]
  );

  const selectedLines = invoiceLines
    .map((line) => ({
      custInvoiceTransRecId: line.recId,
      qty: Number(quantities[line.recId] ?? 0),
      dispositionCodeId,
    }))
    .filter((line) => Number.isFinite(line.qty) && line.qty > 0);
  // A return cannot exceed what was invoiced on the line.
  const hasInvalidQuantity = invoiceLines.some((line) => {
    const raw = quantities[line.recId];
    if (raw === undefined || raw === '') return false;
    const value = Number(raw);
    return !Number.isFinite(value) || value < 0 || value > line.qty;
  });

  const run = async (what: 'create' | 'receive' | 'credit') => {
    setBusy(true);
    setError('');
    try {
      if (what === 'create') {
        const result = await custReturnApi.create({
          invoiceId,
          returnReasonCodeId: reasonCodeId || undefined,
          lines: selectedLines,
        });
        setCreated(result);
        setStage('receive');
        setMessage(t('returnOrder.created', { salesId: result.salesId }));
      } else if (what === 'receive' && created) {
        const id = requestIds.receipt ?? crypto.randomUUID();
        setRequestIds((current) => ({ ...current, receipt: id }));
        const lines = (returnLinesQuery.data ?? []).map((line) => ({
          salesLineRecId: Number(line.id),
          qty: Math.abs(line.quantity),
        }));
        const result = await custReturnApi.receive(created.recId, {
          requestId: id,
          receiptDate,
          lines,
        });
        setStage('credit');
        setMessage(t('returnOrder.received', { packingSlipId: result.packingSlipId }));
      } else if (what === 'credit' && created) {
        const id = requestIds.credit ?? crypto.randomUUID();
        setRequestIds((current) => ({ ...current, credit: id }));
        const result = await custReturnApi.credit(created.recId, { invoiceDate, requestId: id });
        setStage('done');
        setMessage(
          t('returnOrder.credited', {
            invoiceId: result.invoiceId,
            amount: result.creditAmount.toLocaleString(undefined, {
              minimumFractionDigits: 2,
              maximumFractionDigits: 2,
            }),
          })
        );
        await queryClient.invalidateQueries({
          queryKey: returnOrderQueryKeys.invoicedLines(company, custAccount),
        });
      }
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : t('returnOrder.failed'));
    } finally {
      setBusy(false);
    }
  };

  const canCreate =
    canPost &&
    !busy &&
    stage === 'create' &&
    Boolean(invoiceId) &&
    Boolean(dispositionCodeId) &&
    selectedLines.length > 0 &&
    !hasInvalidQuantity;
  const canReceive =
    canPost && !busy && stage === 'receive' && Boolean(receiptDate) && !returnLinesQuery.isLoading;
  const canCredit = canPost && !busy && stage === 'credit' && Boolean(invoiceDate);

  const primary = (): { label: string; onClick: () => void; disabled: boolean } => {
    if (stage === 'receive')
      return {
        label: t('returnOrder.postReceipt'),
        onClick: () => void run('receive'),
        disabled: !canReceive,
      };
    if (stage === 'credit')
      return {
        label: t('returnOrder.postCreditNote'),
        onClick: () => void run('credit'),
        disabled: !canCredit,
      };
    return {
      label: t('returnOrder.createOrder'),
      onClick: () => void run('create'),
      disabled: !canCreate,
    };
  };
  const action = primary();

  return (
    <AppActionDrawer
      open={open}
      onClose={onClose}
      title={t('returnOrder.title')}
      width={1200}
      busy={busy}
      actions={
        <>
          {error && (
            <Alert severity="error" sx={{ flex: 1, minWidth: 0 }}>
              {error}
            </Alert>
          )}
          {!error && message && (
            <Alert severity="success" sx={{ flex: 1, minWidth: 0 }}>
              {message}
            </Alert>
          )}
          <Button onClick={onClose} disabled={busy}>
            {stage === 'done' ? t('actions.close') : t('actions.cancel')}
          </Button>
          {stage !== 'done' && (
            <Button variant="contained" onClick={action.onClick} disabled={action.disabled}>
              {busy ? t('returnOrder.working') : action.label}
            </Button>
          )}
        </>
      }
    >
      <Stack spacing={1.5} sx={{ minHeight: '100%' }}>
        <Typography variant="body2" color="text.secondary">
          {t('returnOrder.hint', { custAccount, customerName })}
        </Typography>
        <Stepper activeStep={stage === 'done' ? STAGES.length : STAGES.indexOf(stage)}>
          {STAGES.map((step) => (
            <Step key={step}>
              <StepLabel>{t(`returnOrder.steps.${step}`)}</StepLabel>
            </Step>
          ))}
        </Stepper>
        {stage === 'create' && (
          <>
            <Stack direction="row" spacing={1.5} useFlexGap sx={{ flexWrap: 'wrap' }}>
              <FormControl size="small" sx={{ minWidth: 220 }}>
                <InputLabel id="return-invoice">{t('returnOrder.invoice')}</InputLabel>
                <Select
                  labelId="return-invoice"
                  label={t('returnOrder.invoice')}
                  value={invoiceId}
                  disabled={busy || linesQuery.isLoading}
                  onChange={(event) => {
                    setInvoiceId(event.target.value);
                    setQuantities({});
                  }}
                >
                  {invoiceIds.map((id) => (
                    <MenuItem key={id} value={id}>
                      {id}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              <FormControl size="small" sx={{ minWidth: 240 }}>
                <InputLabel id="return-disposition">{t('returnOrder.dispositionCode')}</InputLabel>
                <Select
                  labelId="return-disposition"
                  label={t('returnOrder.dispositionCode')}
                  value={dispositionCodeId}
                  disabled={busy || dispositionQuery.isLoading}
                  onChange={(event) => setDispositionCodeId(event.target.value)}
                >
                  {(dispositionQuery.data ?? []).map((code) => (
                    <MenuItem key={code.dispositionCodeId} value={code.dispositionCodeId}>
                      {code.dispositionCodeId} — {code.description}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              <FormControl size="small" sx={{ minWidth: 240 }}>
                <InputLabel id="return-reason">{t('returnOrder.reasonCode')}</InputLabel>
                <Select
                  labelId="return-reason"
                  label={t('returnOrder.reasonCode')}
                  value={reasonCodeId}
                  disabled={busy || reasonQuery.isLoading}
                  onChange={(event) => setReasonCodeId(event.target.value)}
                >
                  <MenuItem value="">{t('returnOrder.noReason')}</MenuItem>
                  {(reasonQuery.data ?? []).map((code) => (
                    <MenuItem key={code.reasonCodeId} value={code.reasonCodeId}>
                      {code.reasonCodeId} — {code.description}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Stack>
            {!invoiceId && <Alert severity="info">{t('returnOrder.selectInvoice')}</Alert>}
            {hasInvalidQuantity && (
              <Alert severity="warning">{t('returnOrder.quantityTooHigh')}</Alert>
            )}
          </>
        )}
        {stage === 'receive' && (
          <TextField
            label={t('returnOrder.receiptDate')}
            type="date"
            size="small"
            value={receiptDate}
            disabled={busy}
            onChange={(event) => setReceiptDate(event.target.value)}
            sx={{ maxWidth: 220 }}
          />
        )}
        {stage === 'credit' && (
          <TextField
            label={t('returnOrder.creditNoteDate')}
            type="date"
            size="small"
            value={invoiceDate}
            disabled={busy}
            onChange={(event) => setInvoiceDate(event.target.value)}
            sx={{ maxWidth: 220 }}
          />
        )}
        {stage === 'create' && (
          <DataGrid
            rows={invoiceLines}
            columns={columns}
            getRowId={(row) => String(row.recId)}
            loading={linesQuery.isLoading}
            height={360}
            rowHeight={34}
            headerHeight={32}
          />
        )}
      </Stack>
    </AppActionDrawer>
  );
}
