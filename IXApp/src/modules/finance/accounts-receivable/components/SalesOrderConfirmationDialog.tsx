import React, { useEffect, useState } from 'react';
import {
  Alert, Box, Button, CircularProgress, Dialog, DialogActions, DialogContent,
  DialogTitle, Divider, Stack, TextField, Typography,
} from '@mui/material';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { salesOrderConfirmationApi } from '../api/salesOrderConfirmationApi';

interface Props {
  open: boolean;
  orderId: string;
  salesId: string;
  canPost: boolean;
  onClose: () => void;
  onPosted: () => Promise<void>;
}

const money = (value: number, currency: string) => `${value.toFixed(2)} ${currency}`;

export function SalesOrderConfirmationDialog({ open, orderId, salesId, canPost, onClose, onPosted }: Props): React.ReactElement {
  const queryClient = useQueryClient();
  const [confirmationDate, setConfirmationDate] = useState(() => new Date().toISOString().slice(0, 10));
  const [selectedId, setSelectedId] = useState<string>();
  const [posting, setPosting] = useState(false);
  const [pendingRequestId, setPendingRequestId] = useState<string>();
  const [error, setError] = useState('');
  const listQuery = useQuery({
    queryKey: ['sales-order-confirmations', orderId],
    queryFn: ({ signal }) => salesOrderConfirmationApi.list(orderId, signal),
    enabled: open && Boolean(orderId),
  });
  useEffect(() => {
    if (!open) return;
    setSelectedId(undefined);
    setError('');
    setPendingRequestId(undefined);
  }, [open, orderId]);
  const detailQuery = useQuery({
    queryKey: ['sales-order-confirmation', orderId, selectedId],
    queryFn: ({ signal }) => salesOrderConfirmationApi.get(orderId, selectedId!, signal),
    enabled: open && Boolean(selectedId),
  });
  const post = async () => {
    if (!canPost || posting || !confirmationDate) return;
    setPosting(true);
    setError('');
    try {
      const requestId = pendingRequestId ?? crypto.randomUUID();
      setPendingRequestId(requestId);
      const journal = await salesOrderConfirmationApi.post(orderId, confirmationDate, requestId);
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['sales-order-confirmations', orderId] }),
        onPosted(),
      ]);
      setSelectedId(journal.id);
      setPendingRequestId(undefined);
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'The sales order could not be confirmed.');
    } finally {
      setPosting(false);
    }
  };
  const detail = detailQuery.data;
  return (
    <Dialog open={open} onClose={posting ? undefined : onClose} maxWidth="md" fullWidth>
      <DialogTitle>Sales order confirmations · {salesId}</DialogTitle>
      <DialogContent dividers>
        <Stack spacing={2}>
          {error && <Alert severity="error">{error}</Alert>}
          {listQuery.error instanceof Error && <Alert severity="error">{listQuery.error.message}</Alert>}
          {canPost && (
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems="center">
              <TextField label="Confirmation date" type="date" size="small" value={confirmationDate}
                onChange={(event) => setConfirmationDate(event.target.value)}
                slotProps={{ inputLabel: { shrink: true } }} />
              <Button variant="contained" disabled={posting || !confirmationDate} onClick={() => void post()}>
                {posting ? 'Posting…' : 'Post confirmation'}
              </Button>
            </Stack>
          )}
          <Typography variant="subtitle2">Posted confirmations</Typography>
          {listQuery.isLoading && <CircularProgress size={24} />}
          {!listQuery.isLoading && !listQuery.data?.length && <Typography variant="body2">No confirmations posted yet.</Typography>}
          <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
            {listQuery.data?.map((item) => (
              <Button key={item.id} size="small" variant={selectedId === item.id ? 'contained' : 'outlined'}
                onClick={() => setSelectedId(item.id)}>
                {item.confirmId} · {item.confirmDate.slice(0, 10)}
              </Button>
            ))}
          </Stack>
          {detailQuery.isLoading && <CircularProgress size={24} />}
          {detailQuery.error instanceof Error && <Alert severity="error">{detailQuery.error.message}</Alert>}
          {detail && (
            <Box>
              <Divider sx={{ mb: 2 }} />
              <Typography variant="h6">{detail.header.confirmId}</Typography>
              <Typography variant="body2">Customer: {detail.header.orderAccount} · Invoice account: {detail.header.invoiceAccount}</Typography>
              <Typography variant="body2">Quantity: {detail.header.qty} · Merchandise: {money(detail.header.salesBalance, detail.header.currencyCode)}</Typography>
              <Typography variant="body2">Charges: {money(detail.header.sumMarkup, detail.header.currencyCode)} · Tax: {money(detail.header.sumTax, detail.header.currencyCode)}</Typography>
              <Typography variant="subtitle1" sx={{ mt: 1 }}>Confirmed total: {money(detail.header.confirmAmount, detail.header.currencyCode)}</Typography>
              <Divider sx={{ my: 2 }} />
              {detail.lines.map((line) => (
                <Stack key={line.id} direction="row" justifyContent="space-between" spacing={2} sx={{ py: 0.5 }}>
                  <Typography variant="body2">{line.lineNum}. {line.itemId} · {line.name} · {line.qty} {line.salesUnit}</Typography>
                  <Typography variant="body2">{money(line.lineAmount, detail.header.currencyCode)}</Typography>
                </Stack>
              ))}
            </Box>
          )}
        </Stack>
      </DialogContent>
      <DialogActions><Button onClick={onClose} disabled={posting}>Close</Button></DialogActions>
    </Dialog>
  );
}
