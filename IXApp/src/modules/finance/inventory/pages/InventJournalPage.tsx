import React, { useMemo, useState } from 'react';
import {
  Alert, Autocomplete, Box, Button, Dialog, DialogActions, DialogContent, DialogTitle, FormControl, InputLabel,
  MenuItem, Paper, Select, Stack, Table, TableBody, TableCell, TableHead, TableRow, TextField, Typography,
} from '@mui/material';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { inventJournalApi, type InventJournal, type InventJournalLine, type InventJournalLineInput } from '../api/inventJournalApi';
import { inventLocationApi } from '../api/inventLocationApi';
import { wmsLocationApi } from '../api/wmsLocationApi';

const journalTypes = ['Movement', 'Inventory adjustment', 'Counting', 'Transfer'];
const blankLine = (): InventJournalLineInput => ({
  transDate: new Date().toISOString().slice(0, 10), itemId: '', inventSiteId: '', inventLocationId: '', inventBatchId: '', inventSerialId: '',
  wmsLocationId: '', toInventSiteId: '', toInventLocationId: '', toWmsLocationId: '', qty: 1, counted: 0, unit: '', costPrice: 0, ledgerDimension: 0,
});

export function InventJournalPage(): React.ReactElement {
  const qc = useQueryClient();
  const [selectedId, setSelectedId] = useState('');
  const [headerOpen, setHeaderOpen] = useState(false);
  const [lineOpen, setLineOpen] = useState(false);
  const [editing, setEditing] = useState<InventJournalLine | null>(null);
  const [error, setError] = useState('');
  const [ledgerSearch, setLedgerSearch] = useState('');
  const [header, setHeader] = useState({ journalNameId: '', description: '', inventSiteId: '', inventLocationId: '' });
  const [line, setLine] = useState<InventJournalLineInput>(blankLine());
  const journals = useQuery({ queryKey: ['inventory-journals'], queryFn: ({ signal }) => inventJournalApi.list(signal) });
  const selected = useQuery({ queryKey: ['inventory-journal', selectedId], queryFn: ({ signal }) => inventJournalApi.get(selectedId, signal), enabled: !!selectedId });
  const lookups = useQuery({ queryKey: ['inventory-journal-lookups'], queryFn: ({ signal }) => inventJournalApi.lookups(signal) });
  const ledgerDimensions = useQuery({
    queryKey: ['inventory-journal-ledger-dimensions', ledgerSearch],
    queryFn: ({ signal }) => inventJournalApi.ledgerDimensions(ledgerSearch, signal),
    enabled: lineOpen && selected.data?.journalType === 0 && ledgerSearch.trim().length > 0,
  });
  const warehouseRecords = useQuery({ queryKey: ['inventory-warehouses'], queryFn: ({ signal }) => inventLocationApi.list(signal) });
  const locationRecords = useQuery({ queryKey: ['inventory-basic-locations'], queryFn: ({ signal }) => wmsLocationApi.list(signal) });
  const refresh = async (journal?: InventJournal) => {
    await qc.invalidateQueries({ queryKey: ['inventory-journals'] });
    if (journal) { setSelectedId(journal.journalId); qc.setQueryData(['inventory-journal', journal.journalId], journal); }
    else if (selectedId) await qc.invalidateQueries({ queryKey: ['inventory-journal', selectedId] });
  };
  const createHeader = useMutation({ mutationFn: () => inventJournalApi.create(header), onSuccess: async (row) => { setHeaderOpen(false); setHeader({ journalNameId: '', description: '', inventSiteId: '', inventLocationId: '' }); setError(''); await refresh(row); }, onError: (e: Error) => setError(e.message) });
  const saveLine = useMutation({ mutationFn: () => editing ? inventJournalApi.updateLine(selectedId, editing.recId, line) : inventJournalApi.addLine(selectedId, line), onSuccess: async (row) => { setLineOpen(false); setEditing(null); setError(''); await refresh(row); }, onError: (e: Error) => setError(e.message) });
  const removeLine = useMutation({ mutationFn: (id: number) => inventJournalApi.deleteLine(selectedId, id), onSuccess: () => refresh(), onError: (e: Error) => setError(e.message) });
  const removeJournal = useMutation({ mutationFn: (id: string) => inventJournalApi.delete(id), onSuccess: async () => { setSelectedId(''); setError(''); await qc.invalidateQueries({ queryKey: ['inventory-journals'] }); }, onError: (e: Error) => setError(e.message) });
  const postJournal = useMutation({ mutationFn: (id: string) => inventJournalApi.post(id), onSuccess: async () => { setError(''); await refresh(); }, onError: (e: Error) => setError(e.message) });
  const current = selected.data;
  const currentType = current?.journalType ?? -1;
  const warehouseOptions = useMemo(() => (siteId: string) => (warehouseRecords.data ?? [])
    .filter((x) => !siteId || x.inventSiteId === siteId)
    .map((x) => ({ code: x.inventLocationId, description: x.name })), [warehouseRecords.data]);
  const locationOptions = useMemo(() => (warehouseId: string) => (locationRecords.data ?? [])
    .filter((x) => x.inventLocationId === warehouseId)
    .map((x) => ({ code: x.wmsLocationId, description: x.checkText || x.wmsLocationId })), [locationRecords.data]);
  const openLine = (row?: InventJournalLine) => {
    setEditing(row ?? null);
    setLedgerSearch(row?.ledgerDimension ? String(row.ledgerDimension) : '');
    setLine(row ? {
      transDate: row.transDate.slice(0, 10), itemId: row.itemId, inventSiteId: row.inventSiteId, inventLocationId: row.inventLocationId,
      inventBatchId: row.inventBatchId, inventSerialId: row.inventSerialId, wmsLocationId: row.wmsLocationId,
      toInventSiteId: row.toInventSiteId, toInventLocationId: row.toInventLocationId, toWmsLocationId: row.toWmsLocationId,
      qty: row.qty, counted: row.counted, unit: row.unit, costPrice: row.costPrice, ledgerDimension: row.ledgerDimension,
    } : blankLine());
    setError(''); setLineOpen(true);
  };
  const field = (label: string, value: string | number, onChange: (v: string) => void, type = 'text', disabled = false) =>
    <TextField size="small" label={label} type={type} value={value} disabled={disabled} onChange={(e) => onChange(e.target.value)} />;
  const select = (label: string, value: string, options: Array<{ code: string; description: string }>, onChange: (v: string) => void) =>
    <FormControl size="small" fullWidth><InputLabel>{label}</InputLabel><Select label={label} value={value} onChange={(e) => onChange(String(e.target.value))}>
      {options.map((x) => <MenuItem key={x.code} value={x.code}>{x.code} — {x.description}</MenuItem>)}
    </Select></FormControl>;

  return <Box sx={{ p: 2, display: 'grid', gridTemplateColumns: 'minmax(280px, 340px) 1fr', gap: 2, minHeight: 'calc(100vh - 130px)' }}>
    <Paper variant="outlined" sx={{ p: 1.5 }}>
      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
        <Typography variant="h6">Inventory journals</Typography><Button size="small" variant="contained" onClick={() => { setError(''); setHeaderOpen(true); }}>New</Button>
      </Stack>
      {journals.isLoading ? <Typography>Loading…</Typography> : <Table size="small"><TableHead><TableRow><TableCell>Journal</TableCell><TableCell>Status</TableCell></TableRow></TableHead><TableBody>
        {(journals.data ?? []).map((x) => <TableRow key={x.journalId} hover selected={x.journalId === selectedId} onClick={() => setSelectedId(x.journalId)} sx={{ cursor: 'pointer' }}>
          <TableCell><b>{x.journalId}</b><br />{x.description || x.journalNameId}</TableCell><TableCell>{x.posted ? 'Posted' : 'Open'}</TableCell>
        </TableRow>)}
      </TableBody></Table>}
    </Paper>
    <Paper variant="outlined" sx={{ p: 2, minWidth: 0 }}>
      {!current ? <Typography color="text.secondary">Select or create an inventory journal.</Typography> : <>
        <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', gap: 1, flexWrap: 'wrap', mb: 2 }}>
          <Box><Typography variant="h5">{current.journalId}</Typography><Typography color="text.secondary">{current.description} · {current.journalNameId} · {journalTypes[current.journalType] ?? 'Unknown'}</Typography></Box>
          <Stack direction="row" sx={{ gap: 1 }}>
            {!current.posted && <><Button onClick={() => openLine()} disabled={lookups.isLoading}>New line</Button><Button color="error" onClick={() => removeJournal.mutate(current.journalId)}>Delete</Button><Button variant="contained" onClick={() => postJournal.mutate(current.journalId)}>Post</Button></>}
          </Stack>
        </Stack>
        {error && <Alert severity="error" sx={{ mb: 1 }} onClose={() => setError('')}>{error}</Alert>}
        <Table size="small"><TableHead><TableRow><TableCell>Line</TableCell><TableCell>Item</TableCell><TableCell>Site / warehouse</TableCell><TableCell>Quantity</TableCell><TableCell>Counted</TableCell><TableCell>Unit</TableCell><TableCell>Cost / amount</TableCell><TableCell /></TableRow></TableHead><TableBody>
          {(current.lines ?? []).map((x) => <TableRow key={x.recId}><TableCell>{x.lineNum}</TableCell><TableCell>{x.itemId}</TableCell><TableCell>{x.inventSiteId} / {x.inventLocationId}{currentType === 3 ? ` → ${x.toInventSiteId} / ${x.toInventLocationId}` : ''}</TableCell><TableCell>{x.qty}</TableCell><TableCell>{currentType === 2 ? x.counted : ''}</TableCell><TableCell>{x.unit}</TableCell><TableCell>{x.costPrice} / {x.costAmount}</TableCell><TableCell>{!current.posted && <Stack direction="row"><Button size="small" onClick={() => openLine(x)}>Edit</Button><Button size="small" color="error" onClick={() => removeLine.mutate(x.recId)}>Delete</Button></Stack>}</TableCell></TableRow>)}
        </TableBody></Table>
      </>}
      {error && !current && <Alert severity="error" sx={{ mt: 1 }} onClose={() => setError('')}>{error}</Alert>}
    </Paper>

    <Dialog open={headerOpen} onClose={() => setHeaderOpen(false)} fullWidth maxWidth="sm"><DialogTitle>New inventory journal</DialogTitle><DialogContent><Stack sx={{ gap: 2, pt: 1 }}>
      {select('Journal name', header.journalNameId, lookups.data?.journalNames ?? [], (v) => setHeader({ ...header, journalNameId: v }))}
      {field('Description', header.description, (v) => setHeader({ ...header, description: v }))}
      {select('Site', header.inventSiteId, lookups.data?.sites ?? [], (v) => setHeader({ ...header, inventSiteId: v, inventLocationId: '' }))}
      {select('Warehouse', header.inventLocationId, warehouseOptions(header.inventSiteId), (v) => setHeader({ ...header, inventLocationId: v }))}
      {error && <Alert severity="error">{error}</Alert>}
    </Stack></DialogContent><DialogActions><Button onClick={() => setHeaderOpen(false)}>Cancel</Button><Button variant="contained" disabled={createHeader.isPending} onClick={() => createHeader.mutate()}>Create</Button></DialogActions></Dialog>

    <Dialog open={lineOpen} onClose={() => setLineOpen(false)} fullWidth maxWidth="md"><DialogTitle>{editing ? 'Edit journal line' : 'New journal line'} · {journalTypes[currentType]}</DialogTitle><DialogContent><Box sx={{ pt: 1, display: 'grid', gridTemplateColumns: 'repeat(2, minmax(0, 1fr))', gap: 1.5 }}>
      {select('Item', line.itemId, lookups.data?.items ?? [], (v) => setLine({ ...line, itemId: v }))}
      {select('Unit', line.unit, lookups.data?.units ?? [], (v) => setLine({ ...line, unit: v }))}
      {field('Transaction date', line.transDate, (v) => setLine({ ...line, transDate: v }), 'date')}
      {select('Site', line.inventSiteId, lookups.data?.sites ?? [], (v) => setLine({ ...line, inventSiteId: v, inventLocationId: '' }))}
      {select('Warehouse', line.inventLocationId, warehouseOptions(line.inventSiteId), (v) => setLine({ ...line, inventLocationId: v, wmsLocationId: '' }))}
      {currentType !== 2 && field('Quantity', line.qty, (v) => setLine({ ...line, qty: Number(v) }), 'number')}
      {currentType === 2 && field('Counted quantity', line.counted, (v) => setLine({ ...line, counted: Number(v) }), 'number')}
      {currentType !== 3 && field('Unit cost', line.costPrice, (v) => setLine({ ...line, costPrice: Number(v) }), 'number')}
      {currentType === 0 && <Autocomplete
        options={ledgerDimensions.data ?? []}
        value={(ledgerDimensions.data ?? []).find((x) => x.code === String(line.ledgerDimension)) ?? null}
        inputValue={ledgerSearch}
        loading={ledgerDimensions.isFetching}
        filterOptions={(options) => options}
        getOptionLabel={(option) => `${option.code} — ${option.description}`}
        isOptionEqualToValue={(option, value) => option.code === value.code}
        onInputChange={(_, value, reason) => { if (reason === 'input' || reason === 'clear') setLedgerSearch(value); }}
        onChange={(_, value) => setLine({ ...line, ledgerDimension: value ? Number(value.code) : 0 })}
        renderInput={(params) => <TextField {...params} size="small" label="Offset ledger account" placeholder="Search account or dimension" />}
      />}
      {select('Location', line.wmsLocationId, locationOptions(line.inventLocationId), (v) => setLine({ ...line, wmsLocationId: v }))}
      {field('Batch number (when required)', line.inventBatchId, (v) => setLine({ ...line, inventBatchId: v }))}
      {field('Serial number (when required)', line.inventSerialId, (v) => setLine({ ...line, inventSerialId: v }))}
      {currentType === 3 && <>{select('To site', line.toInventSiteId, lookups.data?.sites ?? [], (v) => setLine({ ...line, toInventSiteId: v, toInventLocationId: '', toWmsLocationId: '' }))}{select('To warehouse', line.toInventLocationId, warehouseOptions(line.toInventSiteId), (v) => setLine({ ...line, toInventLocationId: v, toWmsLocationId: '' }))}{select('To location', line.toWmsLocationId, locationOptions(line.toInventLocationId), (v) => setLine({ ...line, toWmsLocationId: v }))}</>}
      {error && <Alert severity="error" sx={{ gridColumn: '1 / -1' }}>{error}</Alert>}
    </Box></DialogContent><DialogActions><Button onClick={() => setLineOpen(false)}>Cancel</Button><Button variant="contained" disabled={saveLine.isPending} onClick={() => saveLine.mutate()}>Save line</Button></DialogActions></Dialog>
  </Box>;
}
