import { localizedName } from '@shared/utilities/localizedName';
import React from 'react';
import { Button, MenuItem, Stack, TextField } from '@mui/material';
import type { BuilderCondition } from '../types/processBuilderTypes';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export function ConditionBuilder({ value, variables, onChange, sourceLabel }: { value: BuilderCondition | null; variables: Array<{ id: string; name: string; nameAlias?: string | null }>; onChange: (value: BuilderCondition | null) => void; sourceLabel?: string }) {
  const { t, isRtl } = useAppTranslation();
  const resolvedSourceLabel = sourceLabel ?? t('wfProcessBuilder.settings.fields.variable');
  if (!value) return <TextField select size="small" label={t('wfProcessBuilder.settings.condition')} value="" disabled={variables.length === 0} onChange={() => onChange({ variableId: variables[0]?.id ?? '', operator: '=', value: '' })}><MenuItem value="">{t('wfProcessBuilder.settings.noCondition')}</MenuItem><MenuItem value="add">{t('wfProcessBuilder.settings.addCondition')}</MenuItem></TextField>;
  return <Stack spacing={1}><TextField select size="small" label={resolvedSourceLabel} value={value.variableId} onChange={(e) => onChange({ ...value, variableId: e.target.value })}>{variables.map((x) => <MenuItem key={x.id} value={x.id}>{localizedName(x, isRtl)}</MenuItem>)}</TextField><TextField select size="small" label={t('wfProcessBuilder.settings.fields.operator')} value={value.operator} onChange={(e) => onChange({ ...value, operator: e.target.value as BuilderCondition['operator'] })}>{['=','!=','>','<','>=','<=','contains','isEmpty'].map((x) => <MenuItem key={x} value={x}>{x}</MenuItem>)}</TextField>{value.operator !== 'isEmpty' && <TextField size="small" label={t('wfProcessBuilder.settings.value')} value={value.value} onChange={(e) => onChange({ ...value, value: e.target.value })} />}<Button size="small" color="error" onClick={() => onChange(null)}>{t('wfProcessBuilder.settings.clearCondition')}</Button></Stack>;
}
