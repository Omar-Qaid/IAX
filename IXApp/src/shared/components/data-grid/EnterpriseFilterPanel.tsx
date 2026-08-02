import React from 'react';
import { Box, Button, IconButton, Link, Stack, TextField, Typography } from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';

export interface EnterpriseFilterPanelProps {
  title: string;
  addLabel: string;
  fieldLabel: string;
  operatorLabel: string;
  value: string;
  applyLabel: string;
  resetLabel: string;
  onValueChange: (value: string) => void;
  onApply: () => void;
  onReset: () => void;
  onRemove: () => void;
}

export const EnterpriseFilterPanel: React.FC<EnterpriseFilterPanelProps> = ({ title, addLabel, fieldLabel, operatorLabel, value, applyLabel, resetLabel, onValueChange, onApply, onReset, onRemove }) => (
  <Box sx={{ width: 238, height: '100%', minHeight: 0, boxSizing: 'border-box', overflowY: 'auto', flexShrink: 0, bgcolor: 'background.paper', border: (theme) => `1px solid ${theme.palette.divider}`, borderRadius: 1, boxShadow: 2, p: 1.25 }}>
    <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between', mb: 1.25 }}>
      <Typography component="h2" sx={{ fontSize: '1rem', fontWeight: 600 }}>{title}</Typography>
      <Button size="small" sx={{ minWidth: 0, fontWeight: 400 }}>＋ {addLabel}</Button>
    </Stack>
    <Stack direction="row" sx={{ alignItems: 'flex-start', justifyContent: 'space-between' }}>
      <Box>
        <Typography sx={{ fontSize: '0.75rem', fontWeight: 500 }}>{fieldLabel}</Typography>
        <Link component="button" underline="none" sx={{ fontSize: '0.75rem' }}>{operatorLabel}⌄</Link>
      </Box>
      <IconButton size="small" aria-label={resetLabel} onClick={onRemove}><CloseIcon sx={{ fontSize: 16 }} /></IconButton>
    </Stack>
    <TextField value={value} onChange={(event) => onValueChange(event.target.value)} fullWidth size="small" sx={{ mt: 1, '& .MuiOutlinedInput-root': { height: 29, borderRadius: 0.5 } }} />
    <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end', mt: 3 }}>
      <Button variant="outlined" size="small" onClick={onApply}>{applyLabel}</Button>
      <Button variant="outlined" size="small" onClick={onReset}>{resetLabel}</Button>
    </Stack>
  </Box>
);
