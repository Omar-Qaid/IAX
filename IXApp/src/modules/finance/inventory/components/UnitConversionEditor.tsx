import React, { useMemo, useState } from 'react';
import { Alert, Box, Button, Dialog, DialogActions, DialogContent, DialogTitle, IconButton, MenuItem, Stack, TextField, Tooltip, Typography } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlined';
import EditOutlinedIcon from '@mui/icons-material/EditOutlined';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { ecoResProductApi } from '../api/ecoResProductApi';
import { unitOfMeasureApi, type UnitConversionInput, type UnitConversionRecord, type UnitOfMeasureRecord } from '../api/unitOfMeasureApi';

const empty = (fromUnitOfMeasure: number): UnitConversionInput => ({ fromUnitOfMeasure, toUnitOfMeasure: 0, product: 0, factor: 1, numerator: 0, denominator: 0, innerOffset: 0, outerOffset: 0, rounding: 0 });

export function UnitConversionEditor({ unit }: { unit: UnitOfMeasureRecord }): React.ReactElement {
  const { t } = useAppTranslation();
  const queryClient = useQueryClient();
  const units = useQuery({ queryKey: ['unit-conversion-lookups', 'units'], queryFn: ({ signal }) => unitOfMeasureApi.list(signal) });
  const products = useQuery({ queryKey: ['unit-conversion-lookups', 'products'], queryFn: ({ signal }) => ecoResProductApi.list(signal) });
  const [editing, setEditing] = useState<UnitConversionRecord | null | undefined>(undefined);
  const [values, setValues] = useState<UnitConversionInput>(empty(unit.recId));
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const options = useMemo(() => units.data?.filter((x) => x.recId !== unit.recId && x.unitOfMeasureClass === unit.unitOfMeasureClass) ?? [], [unit.recId, unit.unitOfMeasureClass, units.data]);
  const open = (row?: UnitConversionRecord) => {
    setEditing(row ?? null);
    setValues(row ? { fromUnitOfMeasure: row.fromUnitOfMeasure, toUnitOfMeasure: row.toUnitOfMeasure, product: row.product, factor: row.factor, numerator: row.numerator, denominator: row.denominator, innerOffset: 0, outerOffset: 0, rounding: row.rounding } : empty(unit.recId));
    setError(null);
  };
  const refresh = async () => Promise.all([
    queryClient.invalidateQueries({ queryKey: ['list-details', 'units-of-measure'] }),
    queryClient.invalidateQueries({ queryKey: ['unit-conversion-lookups', 'units'] }),
  ]);
  const save = async () => {
    const hasFactor = values.factor > 0;
    const hasFraction = values.numerator > 0 && values.denominator > 0;
    if (!values.toUnitOfMeasure || hasFactor === hasFraction || values.factor < 0 ||
        values.numerator < 0 || values.denominator < 0 ||
        (values.numerator > 0) !== (values.denominator > 0)) {
      setError(t('unitOfMeasure.conversionRequired', 'Select a to unit and enter either a positive factor or a complete positive fraction.'));
      return;
    }
    setSaving(true); setError(null);
    try {
      if (editing) await unitOfMeasureApi.updateConversion(editing.recId, values); else await unitOfMeasureApi.createConversion(values);
      setEditing(undefined); await refresh();
    } catch (e) { setError(e instanceof Error ? e.message : t('errors.generic', 'Unable to save.')); }
    finally { setSaving(false); }
  };
  const remove = async (row: UnitConversionRecord) => {
    if (!globalThis.confirm(t('unitOfMeasure.deleteConversionConfirm', 'Delete this unit conversion?'))) return;
    setError(null);
    try { await unitOfMeasureApi.deleteConversion(row.recId); await refresh(); }
    catch (e) { setError(e instanceof Error ? e.message : t('errors.generic', 'Unable to delete.')); }
  };
  return <Box>
    <Button size="small" startIcon={<AddIcon />} onClick={() => open()} disabled={!unit.recId}>{t('actions.new', 'New')}</Button>
    {error && <Alert severity="error" sx={{ my: 0.5 }}>{error}</Alert>}
    <Stack spacing={0.25} sx={{ mt: 0.5 }}>
      {unit.conversions.length === 0 && <Typography color="text.secondary" sx={{ fontSize: '0.75rem' }}>{t('unitOfMeasure.noConversions', 'No conversions')}</Typography>}
      {unit.conversions.map((row) => <Box key={row.id} sx={{ display: 'grid', gridTemplateColumns: 'minmax(180px, 1fr) auto', alignItems: 'center', borderBottom: 1, borderColor: 'divider', py: 0.25 }}>
        <Typography sx={{ fontSize: '0.75rem' }}>{row.fromSymbol} × {row.factor > 0 ? row.factor : `${row.numerator}/${row.denominator}`} = {row.toSymbol}{row.productNumber ? ` (${row.productNumber})` : ` (${t('unitOfMeasure.standardConversion', 'Standard')})`}</Typography>
        <Box><Tooltip title={t('actions.edit', 'Edit')}><IconButton size="small" onClick={() => open(row)}><EditOutlinedIcon fontSize="inherit" /></IconButton></Tooltip><Tooltip title={t('actions.delete', 'Delete')}><IconButton size="small" color="error" onClick={() => remove(row)}><DeleteOutlineIcon fontSize="inherit" /></IconButton></Tooltip></Box>
      </Box>)}
    </Stack>
    <Dialog open={editing !== undefined} onClose={() => !saving && setEditing(undefined)} fullWidth maxWidth="sm">
      <DialogTitle>{editing ? t('unitOfMeasure.editConversion', 'Edit unit conversion') : t('unitOfMeasure.newConversion', 'New unit conversion')}</DialogTitle>
      <DialogContent sx={{ display: 'grid', gridTemplateColumns: 'repeat(2, minmax(0, 1fr))', gap: 1.5, pt: '12px !important' }}>
        <TextField label={t('unitOfMeasure.fromUnit', 'From unit')} value={unit.symbol} disabled size="small" />
        <TextField select label={t('unitOfMeasure.toUnit', 'To unit')} value={values.toUnitOfMeasure || ''} onChange={(e) => setValues((v) => ({ ...v, toUnitOfMeasure: Number(e.target.value) }))} size="small">{options.map((x) => <MenuItem key={x.recId} value={x.recId}>{x.symbol}</MenuItem>)}</TextField>
        <TextField select label={t('unitOfMeasure.productScope', 'Product scope')} value={values.product} onChange={(e) => setValues((v) => ({ ...v, product: Number(e.target.value) }))} size="small"><MenuItem value={0}>{t('unitOfMeasure.standardConversion', 'Standard')}</MenuItem>{products.data?.map((x) => <MenuItem key={x.id} value={Number(x.id)}>{x.productNumber} - {x.name}</MenuItem>)}</TextField>
        <TextField label={t('unitOfMeasure.factor', 'Factor')} type="number" value={values.factor} onChange={(e) => setValues((v) => ({ ...v, factor: Number(e.target.value), numerator: 0, denominator: 0 }))} size="small" />
        <TextField label={t('unitOfMeasure.numerator', 'Numerator')} type="number" value={values.numerator} onChange={(e) => setValues((v) => ({ ...v, factor: 0, numerator: Number(e.target.value) }))} size="small" />
        <TextField label={t('unitOfMeasure.denominator', 'Denominator')} type="number" value={values.denominator} onChange={(e) => setValues((v) => ({ ...v, factor: 0, denominator: Number(e.target.value) }))} size="small" />
        <TextField select label={t('unitOfMeasure.rounding', 'Rounding')} value={values.rounding} onChange={(e) => setValues((v) => ({ ...v, rounding: Number(e.target.value) }))} size="small"><MenuItem value={0}>{t('unitOfMeasure.roundingNone', 'None')}</MenuItem><MenuItem value={1}>{t('unitOfMeasure.roundingUp', 'Up')}</MenuItem><MenuItem value={2}>{t('unitOfMeasure.roundingDown', 'Down')}</MenuItem><MenuItem value={3}>{t('unitOfMeasure.roundingNearest', 'Nearest')}</MenuItem></TextField>
        {error && <Alert severity="error" sx={{ gridColumn: '1 / -1' }}>{error}</Alert>}
      </DialogContent>
      <DialogActions><Button onClick={() => setEditing(undefined)} disabled={saving}>{t('actions.cancel', 'Cancel')}</Button><Button variant="contained" onClick={save} disabled={saving}>{t('actions.save', 'Save')}</Button></DialogActions>
    </Dialog>
  </Box>;
}
