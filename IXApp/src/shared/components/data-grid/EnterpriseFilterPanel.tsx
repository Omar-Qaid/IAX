import React from 'react';
import { Box, Button, IconButton, MenuItem, Stack, TextField, Typography } from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';

export type EnterpriseFilterOperator = 'contains' | 'equals' | 'startsWith' | 'endsWith' | 'notEquals' | 'doesNotContain';
export interface EnterpriseFilterCondition { id: string; field: string; operator: EnterpriseFilterOperator; value: string }
export interface EnterpriseFilterOperatorOption { value: EnterpriseFilterOperator; label: string }
export interface EnterpriseFilterFieldOption { value: string; label: string }

export interface EnterpriseFilterPanelProps {
  title: string;
  addLabel: string;
  fieldOptions: EnterpriseFilterFieldOption[];
  conditions: EnterpriseFilterCondition[];
  operatorOptions: EnterpriseFilterOperatorOption[];
  applyLabel: string;
  resetLabel: string;
  onConditionsChange: (conditions: EnterpriseFilterCondition[]) => void;
  onApply: () => void;
  onReset: () => void;
}

export const createEnterpriseFilterCondition = (field = ''): EnterpriseFilterCondition => ({ id: `filter-${Date.now()}-${Math.random().toString(36).slice(2, 7)}`, field, operator: 'contains', value: '' });

export function matchesEnterpriseFilter(value: unknown, condition: EnterpriseFilterCondition, locale?: string): boolean {
  const actual = String(value ?? '').toLocaleLowerCase(locale);
  const expected = condition.value.trim().toLocaleLowerCase(locale);
  if (!expected) return true;
  switch (condition.operator) {
    case 'equals': return actual === expected;
    case 'startsWith': return actual.startsWith(expected);
    case 'endsWith': return actual.endsWith(expected);
    case 'notEquals': return actual !== expected;
    case 'doesNotContain': return !actual.includes(expected);
    default: return actual.includes(expected);
  }
}

export const EnterpriseFilterPanel: React.FC<EnterpriseFilterPanelProps> = ({ title, addLabel, fieldOptions, conditions, operatorOptions, applyLabel, resetLabel, onConditionsChange, onApply, onReset }) => {
  const update = (id: string, patch: Partial<EnterpriseFilterCondition>) => onConditionsChange(conditions.map((condition) => condition.id === id ? { ...condition, ...patch } : condition));
  const remove = (id: string) => onConditionsChange(conditions.filter((condition) => condition.id !== id));
  return <Box sx={{ width: 238, height: '100%', minHeight: 0, boxSizing: 'border-box', overflowY: 'auto', flexShrink: 0, bgcolor: 'background.paper', border: (theme) => `1px solid ${theme.palette.divider}`, borderRadius: 1, boxShadow: 2, p: 1.25 }}>
    <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between', mb: 1.25 }}>
      <Typography component="h2" sx={{ fontSize: '1rem', fontWeight: 600 }}>{title}</Typography>
      <Button size="small" onClick={() => onConditionsChange([...conditions, createEnterpriseFilterCondition(fieldOptions[0]?.value)])} sx={{ minWidth: 0, fontWeight: 400 }}>＋ {addLabel}</Button>
    </Stack>
    <Stack spacing={1.5}>
      {conditions.map((condition) => <Box key={condition.id}>
        <Stack direction="row" sx={{ alignItems: 'flex-start', justifyContent: 'space-between' }}>
          <Box sx={{ minWidth: 0, flex: 1 }}>
            <TextField select variant="standard" value={condition.field || fieldOptions[0]?.value || ''} onChange={(event) => update(condition.id, { field: event.target.value })} sx={{ width: '100%', '& .MuiInputBase-root': { fontSize: '0.75rem', fontWeight: 500 }, '& .MuiInputBase-root::before, & .MuiInputBase-root::after': { borderBottom: 0 } }}>
              {fieldOptions.map((option) => <MenuItem key={option.value} value={option.value} sx={{ fontSize: '0.78rem' }}>{option.label}</MenuItem>)}
            </TextField>
            <TextField select variant="standard" value={condition.operator} onChange={(event) => update(condition.id, { operator: event.target.value as EnterpriseFilterOperator })} sx={{ minWidth: 126, '& .MuiInputBase-root': { color: 'primary.main', fontSize: '0.75rem' }, '& .MuiInputBase-root::before, & .MuiInputBase-root::after': { borderBottom: 0 } }}>
              {operatorOptions.map((option) => <MenuItem key={option.value} value={option.value} sx={{ fontSize: '0.78rem' }}>{option.label}</MenuItem>)}
            </TextField>
          </Box>
          <IconButton size="small" aria-label={resetLabel} onClick={() => remove(condition.id)}><CloseIcon sx={{ fontSize: 16 }} /></IconButton>
        </Stack>
        <TextField value={condition.value} onChange={(event) => update(condition.id, { value: event.target.value })} fullWidth size="small" sx={{ mt: 0.5, '& .MuiOutlinedInput-root': { height: 36, borderRadius: 0 } }} />
      </Box>)}
    </Stack>
    <Stack direction="row" spacing={0.75} sx={{ justifyContent: 'flex-end', mt: 2 }}>
      <Button variant="outlined" size="small" onClick={onApply}>{applyLabel}</Button>
      <Button variant="outlined" size="small" onClick={onReset}>{resetLabel}</Button>
    </Stack>
  </Box>;
};
