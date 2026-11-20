import React, { useEffect, useMemo, useState } from 'react';
import { Alert, Button, Stack, TextField, Typography } from '@mui/material';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { AppActionDrawer } from '@shared/components/dialogs/AppActionDrawer';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { UnavailableCommandButton } from '@shared/components/action-pane/UnavailableCommandButton';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { salesOrderInvoiceApi } from '../api/salesOrderInvoiceApi';
import { salesOrderLinesApi, type SalesOrderLineRecord } from '../api/salesOrderLinesApi';
import { salesOrderQueryKeys } from '../../queries/accountsReceivableQueryKeys';

interface Props {
  open: boolean;
  orderId: string;
  salesId: string;
  canPost: boolean;
  onClose: () => void;
  onPosted: () => Promise<void>;
}

/**
 * A line is invoiceable for its remaining financial quantity: what has been ordered
 * but not yet invoiced. The server derives the same figure, so a line it would skip
 * is not offered here.
 */
const invoiceableQuantity = (line: SalesOrderLineRecord) =>
  line.remainSalesFinancial ?? line.quantity;

const dateToday = () => {
  const date = new Date();
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
};

export function SalesOrderInvoicePostingDialog({
  open,
  orderId,
  salesId,
  canPost,
  onClose,
  onPosted,
}: Props): React.ReactElement {
  const { t } = useAppTranslation();
  const company = useCompanyStore((state) => state.currentCompany);
  const queryClient = useQueryClient();
  const [invoiceDate, setInvoiceDate] = useState(dateToday);
  const [quantities, setQuantities] = useState<Record<string, string>>({});
  const [posting, setPosting] = useState(false);
  const [requestId, setRequestId] = useState<string>();
  const [error, setError] = useState('');

  const linesQuery = useQuery({
    queryKey: salesOrderQueryKeys.lines(company, orderId),
    queryFn: ({ signal }) => salesOrderLinesApi.list(orderId, signal),
    enabled: open && Boolean(orderId),
  });

  useEffect(() => {
    if (!open) return;
    setInvoiceDate(dateToday());
    setQuantities({});
    setError('');
    setRequestId(undefined);
  }, [open, orderId]);

  const invoiceableLines = useMemo(
    () => (linesQuery.data ?? []).filter((line) => invoiceableQuantity(line) > 0),
    [linesQuery.data]
  );

  const columns = useMemo<ColumnDef<SalesOrderLineRecord>[]>(
    () => [
      { field: 'lineNumber', headerName: 'salesOrder.lineNumber', width: 90, align: 'right' },
      { field: 'itemNumber', headerName: 'salesOrder.ui.itemNumber', width: 160 },
      { field: 'description', headerName: 'salesOrder.ui.text', minWidth: 220, flex: 1 },
      { field: 'unit', headerName: 'fields.unit', width: 90 },
      {
        field: 'remainSalesFinancial',
        headerName: 'salesOrder.remainSalesFinancial',
        width: 150,
        align: 'right',
        valueGetter: ({ row }) => invoiceableQuantity(row),
      },
      {
        field: 'quantity',
        headerName: 'salesOrder.ui.update',
        width: 125,
        align: 'right',
        renderCell: ({ row }) => (
          <TextField
            size="small"
            type="number"
            value={quantities[row.id] ?? String(invoiceableQuantity(row))}
            disabled={posting}
            onChange={(event) =>
              setQuantities((current) => ({ ...current, [row.id]: event.target.value }))
            }
            slotProps={{
              htmlInput: {
                min: 0,
                max: invoiceableQuantity(row),
                step: 'any',
                'aria-label': t('salesOrder.ui.updateQuantityFor', { itemId: row.itemNumber }),
              },
            }}
            sx={{
              width: '100%',
              '& .MuiInputBase-input': { py: 0.25, px: 0.5, textAlign: 'right' },
            }}
          />
        ),
      },
      {
        field: 'lineTotal',
        headerName: 'salesOrder.ui.netAmount',
        width: 140,
        align: 'right',
        renderCell: ({ row }) => {
          const available = invoiceableQuantity(row);
          const entered = Number(quantities[row.id] ?? available);
          const amount = available > 0 ? (row.lineTotal * entered) / available : 0;
          return amount.toLocaleString(undefined, {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          });
        },
      },
    ],
    [posting, quantities, t]
  );

  const selectedLines = invoiceableLines
    .map((line) => ({
      salesLineRecId: Number(line.id),
      qty: Number(quantities[line.id] ?? invoiceableQuantity(line)),
    }))
    .filter((line) => Number.isFinite(line.qty) && line.qty > 0);
  const hasInvalidQuantity = invoiceableLines.some((line) => {
    const value = Number(quantities[line.id] ?? invoiceableQuantity(line));
    return !Number.isFinite(value) || value < 0 || value > invoiceableQuantity(line);
  });

  const submit = async () => {
    if (!canPost || posting || !invoiceDate || selectedLines.length === 0 || hasInvalidQuantity)
      return;
    setPosting(true);
    setError('');
    try {
      // The server treats the request id as an idempotency key, so a retry after a
      // network failure returns the invoice already posted instead of posting a second one.
      const id = requestId ?? crypto.randomUUID();
      setRequestId(id);
      await salesOrderInvoiceApi.post(orderId, {
        invoiceDate,
        requestId: id,
        lines: selectedLines,
      });
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: salesOrderQueryKeys.list(company) }),
        queryClient.invalidateQueries({ queryKey: salesOrderQueryKeys.lines(company, orderId) }),
        queryClient.invalidateQueries({ queryKey: salesOrderQueryKeys.invoices(company, orderId) }),
        onPosted(),
      ]);
      setRequestId(undefined);
      onClose();
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : t('salesOrder.ui.invoicePostFailed'));
    } finally {
      setPosting(false);
    }
  };

  return (
    <AppActionDrawer
      open={open}
      onClose={onClose}
      title={t('salesOrder.ui.invoicePosting')}
      width={1200}
      busy={posting}
      actions={
        <>
          {error && (
            <Alert severity="error" sx={{ flex: 1, minWidth: 0 }}>
              {error}
            </Alert>
          )}
          <Button onClick={onClose} disabled={posting}>
            {t('actions.cancel')}
          </Button>
          <UnavailableCommandButton label={t('salesOrder.ui.batch')} />
          <Button
            variant="contained"
            onClick={() => void submit()}
            disabled={
              posting ||
              linesQuery.isLoading ||
              !canPost ||
              !invoiceDate ||
              selectedLines.length === 0 ||
              hasInvalidQuantity
            }
          >
            {posting ? t('salesOrder.ui.postingInProgress') : t('salesOrder.ui.ok')}
          </Button>
        </>
      }
    >
      <Stack spacing={1} sx={{ minHeight: '100%' }}>
        <Typography variant="body2" color="text.secondary">
          {t('salesOrder.ui.invoicePostingHint', { salesId })}
        </Typography>
        <TextField
          label={t('salesOrder.ui.invoiceDate')}
          type="date"
          size="small"
          value={invoiceDate}
          disabled={posting}
          onChange={(event) => setInvoiceDate(event.target.value)}
          sx={{ maxWidth: 220 }}
        />
        {linesQuery.isError && <Alert severity="error">{t('salesOrder.ui.linesLoadFailed')}</Alert>}
        {!linesQuery.isLoading && invoiceableLines.length === 0 && (
          <Alert severity="info">{t('salesOrder.ui.nothingToInvoice')}</Alert>
        )}
        <DataGrid
          rows={invoiceableLines}
          columns={columns}
          loading={linesQuery.isLoading}
          getRowId={(row) => row.id}
          height={420}
          rowHeight={34}
          headerHeight={32}
        />
      </Stack>
    </AppActionDrawer>
  );
}
