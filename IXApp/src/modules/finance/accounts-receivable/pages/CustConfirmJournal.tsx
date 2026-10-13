import React, { useMemo, useState } from 'react';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import SearchIcon from '@mui/icons-material/Search';
import {
  Box, Button, InputAdornment, Paper, Radio, Tab, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow, Tabs, TextField, Typography,
} from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { salesOrderListApi } from '../api/salesOrderListApi';
import { salesOrderConfirmationApi } from '../api/salesOrderConfirmationApi';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../routes/accountsReceivableRoutePaths';

const amount = (value: number | undefined) => (value ?? 0).toLocaleString(undefined, {
  minimumFractionDigits: 2, maximumFractionDigits: 2,
});
const date = (value: string) => value ? new Date(value).toLocaleDateString() : '';

/** Read-only view of posted CustConfirmJour and CustConfirmTrans snapshots. */
export function CustConfirmJournal(): React.ReactElement {
  const { salesOrderId } = useParams<{ salesOrderId: string }>();
  const navigate = useNavigate();
  const [tab, setTab] = useState<'overview' | 'lines'>('overview');
  const [filter, setFilter] = useState('');
  const [selectedId, setSelectedId] = useState<string>();
  const [selectedLineId, setSelectedLineId] = useState<string>();
  const ordersQuery = useQuery({
    queryKey: ['accounts-receivable', 'sales-orders'],
    queryFn: ({ signal }) => salesOrderListApi.list(signal),
  });
  const journalsQuery = useQuery({
    queryKey: ['sales-order-confirmations', salesOrderId],
    queryFn: ({ signal }) => salesOrderConfirmationApi.list(salesOrderId!, signal),
    enabled: Boolean(salesOrderId),
  });
  const order = ordersQuery.data?.find((row) => row.id === salesOrderId);
  const journals = journalsQuery.data ?? [];
  const activeId = journals.some((row) => row.id === selectedId) ? selectedId : journals[0]?.id;
  const detailQuery = useQuery({
    queryKey: ['sales-order-confirmation', salesOrderId, activeId],
    queryFn: ({ signal }) => salesOrderConfirmationApi.get(salesOrderId!, activeId!, signal),
    enabled: Boolean(salesOrderId && activeId && tab === 'lines'),
  });
  const needle = filter.trim().toLocaleLowerCase();
  const visibleJournals = useMemo(() => journals.filter((row) =>
    !needle || [row.salesId, row.confirmId, row.currencyCode, date(row.confirmDate), String(row.confirmAmount)]
      .some((value) => value.toLocaleLowerCase().includes(needle))), [journals, needle]);
  const lines = detailQuery.data?.lines ?? [];
  const visibleLines = useMemo(() => lines.filter((row) =>
    !needle || [row.itemId, row.name, row.inventLocationId, row.wmsLocationId, String(row.lineNum)]
      .some((value) => value.toLocaleLowerCase().includes(needle))), [lines, needle]);

  if (!salesOrderId) return <ErrorState message="Sales order is missing." />;
  if (ordersQuery.isLoading || journalsQuery.isLoading) return <LoadingState />;
  if (ordersQuery.error || journalsQuery.error)
    return <ErrorState message={(ordersQuery.error || journalsQuery.error)?.message ?? 'Unable to load confirmations.'} />;
  if (!order) return <ErrorState message="Sales order not found." />;

  return (
    <Box sx={{ height: '100%', minHeight: 520, display: 'flex', flexDirection: 'column', bgcolor: 'background.paper' }}>
      <Paper elevation={1} sx={{ px: 1, py: 0.5, borderRadius: 1, display: 'flex', alignItems: 'center', gap: 1 }}>
        <Button size="small" startIcon={<ArrowBackIcon />} onClick={() => navigate(ACCOUNTS_RECEIVABLE_ROUTE_PATHS.salesOrder(order.id))}>Back</Button>
        <Typography variant="body2">Sales order confirmations</Typography>
      </Paper>
      <Box sx={{ px: 3, pt: 1.5 }}>
        <Typography variant="body2" color="text.secondary">Sales order confirmations&nbsp; | &nbsp;{order.salesId} : {order.customerName}</Typography>
        <Typography variant="h5" sx={{ mt: 0.5, fontWeight: 600 }}>Standard view</Typography>
        <Tabs value={tab} onChange={(_, value: 'overview' | 'lines') => { setTab(value); setFilter(''); }} sx={{ mt: 1 }}>
          <Tab value="overview" label="Overview" />
          <Tab value="lines" label="Lines" disabled={!activeId} />
        </Tabs>
        {tab === 'lines' && (
          <Typography variant="body2" sx={{ mt: 1.5, color: 'primary.main' }}>
            Confirmation {journals.find((row) => row.id === activeId)?.confirmId}
          </Typography>
        )}
        <TextField size="small" placeholder="Filter" aria-label="Filter confirmations" value={filter}
          onChange={(event) => setFilter(event.target.value)}
          slotProps={{ input: { startAdornment: <InputAdornment position="start"><SearchIcon fontSize="small" /></InputAdornment> } }}
          sx={{ width: 240, mt: 2, mb: 1 }} />
      </Box>
      <TableContainer sx={{ flex: 1, mx: 3, mb: 1, width: 'auto', border: 1, borderColor: 'divider' }}>
        {tab === 'overview' ? (
          <Table size="small" stickyHeader aria-label="Sales order confirmation overview">
            <TableHead><TableRow>
              <TableCell padding="checkbox" />
              <TableCell>Sales order</TableCell><TableCell>Date</TableCell><TableCell>Customer requisition</TableCell>
              <TableCell>Confirmation</TableCell><TableCell>Currency</TableCell><TableCell align="right">Amount</TableCell>
            </TableRow></TableHead>
            <TableBody>
              {visibleJournals.map((row) => <TableRow key={row.id} hover selected={row.id === activeId}
                onClick={() => setSelectedId(row.id)} onDoubleClick={() => { setSelectedId(row.id); setFilter(''); setTab('lines'); }}
                sx={{ cursor: 'pointer' }}>
                <TableCell padding="checkbox"><Radio size="small" checked={row.id === activeId} slotProps={{ input: { 'aria-label': `Select confirmation ${row.confirmId}` } }} /></TableCell>
                <TableCell>{row.salesId || order.salesId}</TableCell><TableCell>{date(row.confirmDate)}</TableCell>
                <TableCell />
                <TableCell>{row.confirmId}</TableCell><TableCell>{row.currencyCode}</TableCell>
                <TableCell align="right">{amount(row.confirmAmount)}</TableCell>
              </TableRow>)}
              {!visibleJournals.length && <TableRow><TableCell colSpan={7}>No confirmations found.</TableCell></TableRow>}
            </TableBody>
          </Table>
        ) : detailQuery.isLoading ? <LoadingState /> : detailQuery.error ?
          <ErrorState message={detailQuery.error.message} /> : (
          <Table size="small" stickyHeader aria-label="Sales order confirmation lines" sx={{ minWidth: 1400 }}>
            <TableHead><TableRow>
              <TableCell padding="checkbox" />
              <TableCell>Sales order</TableCell><TableCell>Item</TableCell><TableCell>Sales category</TableCell>
              <TableCell>Description</TableCell><TableCell>Warehouse</TableCell><TableCell>Location</TableCell>
              <TableCell align="right">CW quantity</TableCell><TableCell align="right">Quantity</TableCell>
              <TableCell align="right">Unit price</TableCell><TableCell align="right">Discount percent</TableCell>
              <TableCell align="right">Discount</TableCell><TableCell align="right">Amount</TableCell>
            </TableRow></TableHead>
            <TableBody>
              {visibleLines.map((row) => <TableRow key={row.id} hover selected={row.id === selectedLineId}
                onClick={() => setSelectedLineId(row.id)} sx={{ cursor: 'pointer' }}>
                <TableCell padding="checkbox"><Radio size="small" checked={row.id === selectedLineId} slotProps={{ input: { 'aria-label': `Select line ${row.lineNum}` } }} /></TableCell>
                <TableCell>{detailQuery.data?.header.salesId}</TableCell><TableCell>{row.itemId}</TableCell>
                <TableCell>{row.salesCategory || ''}</TableCell><TableCell>{row.name}</TableCell>
                <TableCell>{row.inventLocationId}</TableCell><TableCell>{row.wmsLocationId}</TableCell>
                <TableCell align="right">{amount(row.pdsCwQty)}</TableCell><TableCell align="right">{amount(row.qty)}</TableCell>
                <TableCell align="right">{amount(row.salesPrice)}</TableCell>
                <TableCell align="right">{amount(row.discPercent || row.linePercent)}</TableCell>
                <TableCell align="right">{amount(row.lineDisc)}</TableCell><TableCell align="right">{amount(row.lineAmount)}</TableCell>
              </TableRow>)}
              {!visibleLines.length && <TableRow><TableCell colSpan={13}>No confirmation lines found.</TableCell></TableRow>}
            </TableBody>
          </Table>
        )}
      </TableContainer>
    </Box>
  );
}
