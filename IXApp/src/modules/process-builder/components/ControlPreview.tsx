import React from 'react';
import { Box, Button, Checkbox, FormControlLabel, MenuItem, Radio, TextField, Typography } from '@mui/material';
import type { BuilderControl } from '../types/processBuilderTypes';
export function ControlPreview({ control }: { control: BuilderControl }) {
  if (control.type === 'checkbox') return <FormControlLabel control={<Checkbox disabled={control.readOnly} />} label={control.label} />;
  if (control.type === 'label') return <Typography sx={{ fontWeight: 600 }}>{control.label}</Typography>;
  if (control.type === 'file' || control.type === 'signature') return <Button variant="outlined" disabled={control.readOnly}>{control.type === 'file' ? 'Choose file' : 'Capture signature'}</Button>;
  if (control.type === 'table') return <Box sx={{ height: 64, border: '1px dashed #cbd5e1', display: 'grid', placeItems: 'center', color: 'text.secondary' }}>Table preview</Box>;
  if (control.type === 'checkboxlist' || control.type === 'radiobuttonlist') return <Box>{(control.options.length ? control.options : ['Option']).map((option) => <FormControlLabel key={option} control={control.type === 'checkboxlist' ? <Checkbox size="small" /> : <Radio size="small" />} label={option} />)}</Box>;
  const select = control.type === 'dropdown-db' || control.type === 'dropdown-manual';
  const inputType = control.type === 'digits' ? 'number' : control.type === 'date' ? 'date' : control.type === 'time' ? 'time' : control.type === 'url' ? 'url' : 'text';
  return <TextField fullWidth select={select} type={inputType} multiline={control.type === 'longtext'} minRows={control.type === 'longtext' ? 3 : undefined} size="small" label={control.label} required={control.required} disabled={control.readOnly} defaultValue={control.defaultValue} slotProps={control.type === 'date' || control.type === 'time' ? { inputLabel: { shrink: true } } : undefined}>{select ? control.options.map((x) => <MenuItem key={x} value={x}>{x}</MenuItem>) : null}</TextField>;
}
