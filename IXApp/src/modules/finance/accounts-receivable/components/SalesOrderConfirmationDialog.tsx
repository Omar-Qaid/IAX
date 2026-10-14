import React, { useEffect, useState } from 'react';
import {
  Alert, Box, Button, CircularProgress, Dialog, DialogActions, DialogContent,
  DialogTitle, FormControlLabel, Stack, Switch, Table, TableBody,
  TableCell, TableContainer, TableHead, TableRow, TextField, Typography,
} from '@mui/material';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { salesOrderConfirmationApi } from '../api/salesOrderConfirmationApi';

interface Props {
  open: boolean;
  mode: 'confirmation' | 'proForma';
  orderId: string;
  salesId: string;
  customerName: string;
  canPost: boolean;
  onClose: () => void;
  onPosted: () => Promise<void>;
}

const money = (value: number, currency: string) => `${value.toFixed(2)} ${currency}`;

export function SalesOrderConfirmationDialog({ open, mode, orderId, salesId, customerName, canPost, onClose, onPosted }: Props): React.ReactElement {
  const queryClient = useQueryClient();
  const [confirmationDate, setConfirmationDate] = useState(() => new Date().toISOString().slice(0, 10));
  const [posting, setPosting] = useState(false);
  const [pendingRequestId, setPendingRequestId] = useState<string>();
  const [error, setError] = useState('');
  const [postingEnabled, setPostingEnabled] = useState(true);
  const [view, setView] = useState<'overview' | 'lines'>('overview');
  const previewQuery = useQuery({
    queryKey: ['sales-order-confirmation-preview', orderId],
    queryFn: ({ signal }) => salesOrderConfirmationApi.preview(orderId, signal),
    enabled: open && Boolean(orderId),
    staleTime: 0,
  });
  useEffect(() => {
    if (!open) return;
    setError('');
    setPendingRequestId(undefined);
    setPostingEnabled(mode === 'confirmation');
    setView('overview');
  }, [open, mode, orderId]);
  const post = async () => {
    if (mode === 'proForma' || !postingEnabled) { onClose(); return; }
    if (!canPost || posting || !confirmationDate) return;
    setPosting(true);
    setError('');
    try {
      const requestId = pendingRequestId ?? crypto.randomUUID();
      setPendingRequestId(requestId);
      await salesOrderConfirmationApi.post(orderId, confirmationDate, requestId);
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['sales-order-confirmations', orderId] }),
        onPosted(),
      ]);
      setPendingRequestId(undefined);
      onClose();
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'The sales order could not be confirmed.');
    } finally {
      setPosting(false);
    }
  };
  return (
    <Dialog open={open} onClose={posting ? undefined : onClose} maxWidth="md" fullWidth>
      <DialogTitle>{mode === 'proForma' ? 'Pro forma confirmation' : 'Confirm sales order'} · {salesId}</DialogTitle>
      <DialogContent dividers>
        <Stack spacing={2}>
          {error && <Alert severity="error">{error}</Alert>}
          {previewQuery.error instanceof Error && <Alert severity="error">{previewQuery.error.message}</Alert>}
          {mode === 'proForma' && <Alert severity="info">Preview only. No confirmation journal or order status change will be posted.</Alert>}
          <Typography variant="subtitle2">Parameters</Typography>
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, alignItems: 'center' }}>
            <TextField label="Confirmation date" type="date" size="small" value={confirmationDate}
              disabled={mode === 'proForma'} onChange={(event) => setConfirmationDate(event.target.value)}
              slotProps={{ inputLabel: { shrink: true } }} />
            {mode === 'confirmation' && <FormControlLabel label="Posting" control={
              <Switch checked={postingEnabled} onChange={(event) => setPostingEnabled(event.target.checked)} />
            } />}
            <Typography variant="body2">Quantity: All</Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button variant={view === 'overview' ? 'contained' : 'text'} onClick={() => setView('overview')}>Overview</Button>
            <Button variant={view === 'lines' ? 'contained' : 'text'} onClick={() => setView('lines')}>Lines</Button>
          </Box>
          {previewQuery.isLoading && <CircularProgress size={24} />}
          {previewQuery.data && view === 'overview' && <TableContainer><Table size="small" aria-label="Confirmation overview">
            <TableHead><TableRow><TableCell>Update</TableCell><TableCell>Sales order</TableCell><TableCell>Name</TableCell><TableCell>Currency</TableCell><TableCell align="right">Quantity</TableCell><TableCell align="right">Net amount</TableCell><TableCell align="right">Tax</TableCell><TableCell align="right">Total</TableCell></TableRow></TableHead>
            <TableBody><TableRow><TableCell>Confirmation</TableCell><TableCell>{salesId}</TableCell><TableCell>{customerName}</TableCell><TableCell>{previewQuery.data.header.currencyCode}</TableCell><TableCell align="right">{previewQuery.data.header.qty}</TableCell><TableCell align="right">{money(previewQuery.data.header.salesBalance, previewQuery.data.header.currencyCode)}</TableCell><TableCell align="right">{money(previewQuery.data.header.sumTax, previewQuery.data.header.currencyCode)}</TableCell><TableCell align="right">{money(previewQuery.data.header.confirmAmount, previewQuery.data.header.currencyCode)}</TableCell></TableRow></TableBody>
          </Table></TableContainer>}
          {previewQuery.data && view === 'lines' && <TableContainer><Table size="small" aria-label="Confirmation lines">
            <TableHead><TableRow><TableCell>Sales order</TableCell><TableCell>Item number</TableCell><TableCell>Description</TableCell><TableCell>Warehouse</TableCell><TableCell>Location</TableCell><TableCell align="right">Update</TableCell><TableCell align="right">Unit price</TableCell><TableCell align="right">Net amount</TableCell></TableRow></TableHead>
            <TableBody>{previewQuery.data.lines.map((line) => <TableRow key={`${line.lineNum}-${line.itemId}`}><TableCell>{salesId}</TableCell><TableCell>{line.itemId}</TableCell><TableCell>{line.name}</TableCell><TableCell>{line.inventLocationId}</TableCell><TableCell>{line.wmsLocationId}</TableCell><TableCell align="right">{line.qty}</TableCell><TableCell align="right">{money(line.salesPrice, previewQuery.data.header.currencyCode)}</TableCell><TableCell align="right">{money(line.lineAmount, previewQuery.data.header.currencyCode)}</TableCell></TableRow>)}</TableBody>
          </Table></TableContainer>}
        </Stack>
      </DialogContent>
      <DialogActions><Button onClick={onClose} disabled={posting}>Cancel</Button><Button variant="contained" onClick={() => void post()}
        disabled={posting || previewQuery.isLoading || !previewQuery.data || (mode === 'confirmation' && postingEnabled && (!canPost || !confirmationDate))}>
        {posting ? 'Posting…' : mode === 'proForma' || !postingEnabled ? 'Close preview' : 'OK'}
      </Button></DialogActions>
    </Dialog>
  );
}
