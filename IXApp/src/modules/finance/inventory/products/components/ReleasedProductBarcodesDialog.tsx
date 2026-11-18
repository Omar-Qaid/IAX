import React, { useEffect, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Alert, Box, Button, Checkbox, Dialog, DialogActions, DialogContent, DialogTitle, FormControlLabel, MenuItem, Stack, TextField, Typography } from '@mui/material';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { releasedProductApi, type ItemBarcodeInput, type ItemBarcodeRecord, type ReleasedProductLookupOption } from '../api/releasedProductApi';

const emptyBarcode = (): ItemBarcodeInput => ({ itemBarcode: '', unitId: '', qty: 1, description: '', inventDimId: '', useForInput: true, useForPrinting: false, blocked: false });

export function ReleasedProductBarcodesDialog({ product, units, onClose }: {
  product: { id: string; itemNumber: string } | null;
  units: ReleasedProductLookupOption[];
  onClose: () => void;
}): React.ReactElement {
  const { t } = useAppTranslation();
  const [editing, setEditing] = useState<number | null>(null);
  const [form, setForm] = useState<ItemBarcodeInput>(emptyBarcode);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  useEffect(() => {
    setEditing(null);
    setForm(emptyBarcode());
    setError(null);
  }, [product?.id]);
  const rows = useQuery({
    queryKey: ['inventory', 'released-product-barcodes', product?.id],
    queryFn: ({ signal }) => releasedProductApi.barcodes(product!.id, signal),
    enabled: !!product,
  });
  const edit = (row: ItemBarcodeRecord) => {
    setEditing(row.recId);
    setForm({ itemBarcode: row.itemBarcode, unitId: row.unitId, qty: row.qty, description: row.description, inventDimId: row.inventDimId, useForInput: row.useForInput, useForPrinting: row.useForPrinting, blocked: row.blocked });
    setError(null);
  };
  const save = async () => {
    if (!product) return;
    if (!form.itemBarcode.trim() || !form.unitId || !Number.isFinite(form.qty) || form.qty <= 0) {
      setError(t('inventory.barcode.validInput', 'Enter a barcode, unit, and positive quantity.'));
      return;
    }
    setBusy(true); setError(null);
    try {
      if (editing === null) await releasedProductApi.createBarcode(product.id, form);
      else await releasedProductApi.updateBarcode(product.id, editing, form);
      setEditing(null); setForm(emptyBarcode());
      await rows.refetch();
    } catch (reason) { setError(reason instanceof Error ? reason.message : String(reason)); }
    finally { setBusy(false); }
  };
  const remove = async (row: ItemBarcodeRecord) => {
    if (!product || !window.confirm(t('inventory.barcode.confirmDelete', 'Delete this barcode?'))) return;
    setBusy(true); setError(null);
    try { await releasedProductApi.deleteBarcode(product.id, row.recId); await rows.refetch(); }
    catch (reason) { setError(reason instanceof Error ? reason.message : String(reason)); }
    finally { setBusy(false); }
  };
  return <Dialog open={!!product} onClose={busy ? undefined : onClose} maxWidth="md" fullWidth>
    <DialogTitle>{t('inventory.barcode.title', 'Barcodes')} {product?.itemNumber}</DialogTitle>
    <DialogContent>
      {rows.isError && <Alert severity="error">{rows.error instanceof Error ? rows.error.message : t('errors.generic')}</Alert>}
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      <Stack spacing={1} sx={{ mb: 2 }}>
        {(rows.data ?? []).map((row) => <Box key={row.recId} sx={{ display: 'flex', alignItems: 'center', gap: 1, borderBottom: 1, borderColor: 'divider', py: 0.5 }}>
          <Typography sx={{ flex: 1 }}>{row.itemBarcode} · {row.unitId} · {row.qty}{row.blocked ? ` · ${t('inventory.barcode.blocked', 'Blocked')}` : ''}</Typography>
          <Button disabled={busy} onClick={() => edit(row)}>{t('actions.edit', 'Edit')}</Button>
          <Button color="error" disabled={busy} onClick={() => void remove(row)}>{t('actions.delete', 'Delete')}</Button>
        </Box>)}
      </Stack>
      <Typography variant="subtitle2" sx={{ mb: 1 }}>{editing === null ? t('inventory.barcode.add', 'Add barcode') : t('inventory.barcode.edit', 'Edit barcode')}</Typography>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} sx={{ mb: 1 }}>
        <TextField label={t('inventory.barcode.code', 'Barcode')} value={form.itemBarcode} onChange={(event) => setForm({ ...form, itemBarcode: event.target.value })} fullWidth required />
        <TextField select label={t('inventory.barcode.unit', 'Unit')} value={form.unitId} onChange={(event) => setForm({ ...form, unitId: event.target.value })} sx={{ minWidth: 150 }} required>
          {units.map((unit) => <MenuItem key={unit.value} value={unit.value}>{unit.label}</MenuItem>)}
        </TextField>
        <TextField label={t('inventory.barcode.quantity', 'Quantity')} type="number" value={form.qty} onChange={(event) => setForm({ ...form, qty: Number(event.target.value) })} slotProps={{ htmlInput: { min: 0.000001, step: 'any' } }} sx={{ width: 140 }} required />
      </Stack>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} sx={{ mb: 1 }}>
        <TextField label={t('inventory.barcode.description', 'Description')} value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} fullWidth />
        <TextField label={t('inventory.barcode.dimension', 'Inventory dimension ID')} value={form.inventDimId} onChange={(event) => setForm({ ...form, inventDimId: event.target.value })} fullWidth />
      </Stack>
      <FormControlLabel control={<Checkbox checked={form.useForInput} onChange={(event) => setForm({ ...form, useForInput: event.target.checked })} />} label={t('inventory.barcode.input', 'Use for input')} />
      <FormControlLabel control={<Checkbox checked={form.useForPrinting} onChange={(event) => setForm({ ...form, useForPrinting: event.target.checked })} />} label={t('inventory.barcode.print', 'Use for printing')} />
      <FormControlLabel control={<Checkbox checked={form.blocked} onChange={(event) => setForm({ ...form, blocked: event.target.checked })} />} label={t('inventory.barcode.blocked', 'Blocked')} />
    </DialogContent>
    <DialogActions>
      {editing !== null && <Button disabled={busy} onClick={() => { setEditing(null); setForm(emptyBarcode()); }}>{t('actions.cancel', 'Cancel edit')}</Button>}
      <Button disabled={busy} onClick={onClose}>{t('actions.close', 'Close')}</Button>
      <Button variant="contained" disabled={busy || !form.itemBarcode.trim() || !form.unitId || !Number.isFinite(form.qty) || form.qty <= 0} onClick={() => void save()}>{t('actions.save', 'Save')}</Button>
    </DialogActions>
  </Dialog>;
}
