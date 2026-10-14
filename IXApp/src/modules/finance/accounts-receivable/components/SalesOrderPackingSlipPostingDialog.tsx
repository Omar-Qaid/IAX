import React, { useEffect, useMemo, useState } from 'react';
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Alert,
  Box,
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
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { AppActionDrawer } from '@shared/components/dialogs/AppActionDrawer';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';
import {
  salesOrderPackingSlipApi,
  type PackingSlipPostingLine,
  type PackingSlipQuantityMode,
} from '../api/salesOrderPackingSlipApi';

interface Props {
  open: boolean;
  orderId: string;
  salesId: string;
  canPost: boolean;
  onClose: () => void;
  onPosted: () => Promise<void>;
}

interface OverviewRow {
  id: string;
  update: string;
  customerPackingSlip: string;
  shipmentNumber: string;
  salesId: string;
  siteId: string;
  customerName: string;
  currencyCode: string;
}

const dateToday = () => {
  const date = new Date();
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
};

const overviewColumns: ColumnDef<OverviewRow>[] = [
  { field: 'update', headerName: 'Update', width: 145 },
  { field: 'customerPackingSlip', headerName: 'Customer packing slip', width: 160 },
  { field: 'shipmentNumber', headerName: 'Shipment number', width: 150 },
  { field: 'salesId', headerName: 'Sales order', width: 155 },
  { field: 'siteId', headerName: 'Site', width: 130 },
  { field: 'customerName', headerName: 'Name', minWidth: 220, flex: 1 },
  { field: 'currencyCode', headerName: 'Currency', width: 110 },
];

export function SalesOrderPackingSlipPostingDialog({
  open,
  orderId,
  salesId,
  canPost,
  onClose,
  onPosted,
}: Props): React.ReactElement {
  const queryClient = useQueryClient();
  const [deliveryDate, setDeliveryDate] = useState(dateToday);
  const [quantityMode, setQuantityMode] = useState<PackingSlipQuantityMode>('all');
  const [postingEnabled, setPostingEnabled] = useState(true);
  const [quantities, setQuantities] = useState<Record<string, string>>({});
  const [posting, setPosting] = useState(false);
  const [requestId, setRequestId] = useState<string>();
  const [error, setError] = useState('');

  const previewQuery = useQuery({
    queryKey: ['sales-order-packing-slip-posting-preview', orderId, quantityMode],
    queryFn: ({ signal }) => salesOrderPackingSlipApi.postingPreview(orderId, quantityMode, signal),
    enabled: open && Boolean(orderId),
    staleTime: 0,
  });

  useEffect(() => {
    if (!open) return;
    setDeliveryDate(dateToday());
    setPostingEnabled(true);
    setError('');
    setRequestId(undefined);
  }, [open, orderId]);

  useEffect(() => {
    const next: Record<string, string> = {};
    for (const line of previewQuery.data?.lines ?? []) next[line.id] = String(line.quantity);
    setQuantities(next);
  }, [previewQuery.data]);

  const preview = previewQuery.data;
  const overviewRows = useMemo<OverviewRow[]>(() => preview ? [{
    id: preview.header.id,
    update: 'Packing slip',
    customerPackingSlip: '',
    shipmentNumber: '',
    salesId,
    siteId: preview.header.siteId,
    customerName: preview.header.customerName,
    currencyCode: preview.header.currencyCode,
  }] : [], [preview, salesId]);

  const lineColumns = useMemo<ColumnDef<PackingSlipPostingLine>[]>(() => [
    { field: 'salesCategory', headerName: 'Sales category', width: 170 },
    { field: 'name', headerName: 'Text', width: 220 },
    { field: 'salesId', headerName: 'Sales order', width: 150, valueGetter: () => salesId },
    { field: 'inventLocationId', headerName: 'Warehouse', width: 155 },
    { field: 'wmsLocationId', headerName: 'Location', width: 145 },
    { field: 'catchWeightQuantity', headerName: 'CW update', width: 120, type: 'number', align: 'right' },
    { field: 'productNumber', headerName: 'Product number', width: 190 },
    { field: 'itemId', headerName: 'Item number', width: 160 },
    {
      field: 'quantity', headerName: 'Update', width: 125, align: 'right',
      renderCell: ({ row }) => (
        <TextField
          size="small"
          type="number"
          value={quantities[row.id] ?? '0'}
          disabled={!postingEnabled || posting}
          onChange={(event) => setQuantities((current) => ({ ...current, [row.id]: event.target.value }))}
          slotProps={{ htmlInput: { min: 0, max: row.quantity, step: 'any', 'aria-label': `Update quantity for ${row.itemId}` } }}
          sx={{ width: '100%', '& .MuiInputBase-input': { py: 0.25, px: 0.5, textAlign: 'right' } }}
        />
      ),
    },
    {
      field: 'netAmount', headerName: 'Net amount', width: 135, type: 'number', align: 'right',
      renderCell: ({ row }) => {
        const entered = Number(quantities[row.id] ?? row.quantity);
        const amount = row.quantity > 0 ? row.netAmount * entered / row.quantity : 0;
        return amount.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
      },
    },
  ], [posting, postingEnabled, quantities, salesId]);

  const selectedLines = (preview?.lines ?? []).map((line) => ({
    salesLineId: line.id,
    quantity: Number(quantities[line.id] ?? line.quantity),
  })).filter((line) => Number.isFinite(line.quantity) && line.quantity > 0);
  const hasInvalidQuantity = (preview?.lines ?? []).some((line) => {
    const value = Number(quantities[line.id] ?? line.quantity);
    return !Number.isFinite(value) || value < 0 || value > line.quantity;
  });

  const submit = async () => {
    if (!postingEnabled) { onClose(); return; }
    if (!canPost || posting || !preview || !deliveryDate || selectedLines.length === 0 || hasInvalidQuantity) return;
    setPosting(true);
    setError('');
    try {
      const id = requestId ?? crypto.randomUUID();
      setRequestId(id);
      await salesOrderPackingSlipApi.post(orderId, {
        deliveryDate,
        quantityMode,
        posting: true,
        requestId: id,
        lines: selectedLines,
      });
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['accounts-receivable', 'sales-orders'] }),
        queryClient.invalidateQueries({ queryKey: ['sales-order-lines', orderId] }),
        queryClient.invalidateQueries({ queryKey: ['sales-order-packing-slips', orderId] }),
        queryClient.invalidateQueries({ queryKey: ['sales-order-packing-slip-posting-preview', orderId] }),
        onPosted(),
      ]);
      setRequestId(undefined);
      onClose();
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'The packing slip could not be posted.');
    } finally {
      setPosting(false);
    }
  };

  return (
    <AppActionDrawer
      open={open}
      onClose={onClose}
      title="Packing slip posting"
      width={1770}
      busy={posting}
      actions={
        <>
          <Button onClick={onClose} disabled={posting}>Cancel</Button>
          <Button variant="outlined" disabled>Batch</Button>
          <Button
            variant="contained"
            onClick={() => void submit()}
            disabled={posting || previewQuery.isLoading || !preview || (postingEnabled && (!canPost
              || !deliveryDate || selectedLines.length === 0 || hasInvalidQuantity))}
          >
            {posting ? 'Posting…' : postingEnabled ? 'OK' : 'Close preview'}
          </Button>
        </>
      }
    >
      <Stack spacing={1} sx={{ minHeight: '100%' }}>
        <Typography variant="body2" color="text.secondary">Standard view</Typography>
        <Stack direction="row" spacing={2} sx={{ alignItems: 'center' }}>
          <Button size="small" disabled>Arrange</Button>
          <Button size="small" disabled>Select</Button>
          <Button size="small" disabled endIcon={<ExpandMoreIcon />}>Printer setup</Button>
        </Stack>
        {error && <Alert severity="error">{error}</Alert>}
        {previewQuery.error instanceof Error && <Alert severity="error">{previewQuery.error.message}</Alert>}
        <Alert severity="info">
          Posting records physical delivery and updates inventory. This flow does not create a customer invoice or calculate/post a general ledger voucher.
        </Alert>

        <Accordion disableGutters>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}><Typography sx={{ fontWeight: 600 }}>Parameters</Typography></AccordionSummary>
          <AccordionDetails sx={{ pt: 0 }}>
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, alignItems: 'center' }}>
              <FormControl size="small" sx={{ minWidth: 180 }}>
                <InputLabel id="packing-slip-quantity-mode-label">Quantity</InputLabel>
                <Select
                  labelId="packing-slip-quantity-mode-label"
                  label="Quantity"
                  value={quantityMode}
                  disabled={posting}
                  onChange={(event) => setQuantityMode(event.target.value as PackingSlipQuantityMode)}
                >
                  <MenuItem value="all">All</MenuItem>
                  <MenuItem value="deliverNow">Deliver now</MenuItem>
                  <MenuItem value="picked">Picked</MenuItem>
                </Select>
              </FormControl>
              <TextField
                label="Date"
                type="date"
                size="small"
                value={deliveryDate}
                disabled={posting}
                onChange={(event) => setDeliveryDate(event.target.value)}
                slotProps={{ inputLabel: { shrink: true } }}
              />
              <FormControlLabel
                label="Posting"
                control={<Switch checked={postingEnabled} disabled={posting} onChange={(event) => setPostingEnabled(event.target.checked)} />}
              />
            </Box>
          </AccordionDetails>
        </Accordion>

        <Accordion disableGutters defaultExpanded>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}><Typography sx={{ fontWeight: 600 }}>Overview</Typography></AccordionSummary>
          <AccordionDetails sx={{ pt: 0 }}>
            <Stack direction="row" spacing={1} sx={{ mb: 0.5 }}>
              <Button size="small" disabled>Add</Button>
              <Button size="small" disabled>Remove</Button>
              <Button size="small" disabled>Tracking information</Button>
              <Button size="small" disabled>Sales tax</Button>
              <Button size="small" disabled>Shipments</Button>
            </Stack>
            <DataGrid
              rows={overviewRows}
              columns={overviewColumns}
              getRowId={(row) => row.id}
              loading={previewQuery.isLoading}
              selectionMode="single"
              checkboxSelection
              selectedIds={overviewRows.length ? [overviewRows[0].id] : []}
              height={164}
              rowHeight={32}
              headerHeight={32}
              showCellBorders
              hideToolbar
              hideFooter
              hideColumnMenu
              hideSidebar
            />
          </AccordionDetails>
        </Accordion>

        <Accordion disableGutters>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}><Typography sx={{ fontWeight: 600 }}>Setup</Typography></AccordionSummary>
          <AccordionDetails sx={{ pt: 0 }}>
            <Typography variant="body2">Physical inventory update only. Financial inventory and invoice balances remain unchanged.</Typography>
          </AccordionDetails>
        </Accordion>

        <Accordion disableGutters defaultExpanded>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}><Typography sx={{ fontWeight: 600 }}>Lines</Typography></AccordionSummary>
          <AccordionDetails sx={{ pt: 0 }}>
            <Stack direction="row" spacing={1} sx={{ mb: 0.5 }}>
              <Button size="small" disabled>Delete</Button>
              <Button size="small" disabled>Sales order line</Button>
              <Button size="small" disabled>Inventory</Button>
              <Button size="small" disabled>Reset batch disposition code</Button>
              <Button size="small" disabled>Update line</Button>
            </Stack>
            <DataGrid
              rows={preview?.lines ?? []}
              columns={lineColumns}
              getRowId={(line) => line.id}
              loading={previewQuery.isLoading}
              selectionMode="single"
              checkboxSelection
              selectedIds={preview?.lines[0] ? [preview.lines[0].id] : []}
              height={260}
              rowHeight={34}
              headerHeight={32}
              showCellBorders
              hideToolbar
              hideFooter
              hideColumnMenu
              hideSidebar
            />
          </AccordionDetails>
        </Accordion>
      </Stack>
    </AppActionDrawer>
  );
}
