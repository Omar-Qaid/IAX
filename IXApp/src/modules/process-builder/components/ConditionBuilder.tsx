import React from 'react';
import { Button, MenuItem, Stack, TextField } from '@mui/material';
import type { BuilderCondition, BuilderVariable } from '../types/processBuilderTypes';

export function ConditionBuilder({ value, variables, onChange }: { value: BuilderCondition | null; variables: BuilderVariable[]; onChange: (value: BuilderCondition | null) => void }) {
  if (!value) return <TextField select size="small" label="Condition" value="" onChange={() => onChange({ variableId: variables[0]?.id ?? '', operator: '=', value: '' })}><MenuItem value="">No condition</MenuItem><MenuItem value="add">Add condition</MenuItem></TextField>;
  return <Stack spacing={1}><TextField select size="small" label="Variable" value={value.variableId} onChange={(e) => onChange({ ...value, variableId: e.target.value })}>{variables.map((x) => <MenuItem key={x.id} value={x.id}>{x.name}</MenuItem>)}</TextField><TextField select size="small" label="Operator" value={value.operator} onChange={(e) => onChange({ ...value, operator: e.target.value as BuilderCondition['operator'] })}>{['=','!=','>','<','>=','<=','contains','isEmpty'].map((x) => <MenuItem key={x} value={x}>{x}</MenuItem>)}</TextField>{value.operator !== 'isEmpty' && <TextField size="small" label="Value" value={value.value} onChange={(e) => onChange({ ...value, value: e.target.value })} />}<Button size="small" color="error" onClick={() => onChange(null)}>Clear condition</Button></Stack>;
}
