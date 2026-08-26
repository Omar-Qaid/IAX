import React from 'react';
import { MenuItem, TextField } from '@mui/material';
import type { BuilderDataType } from '../types/processBuilderTypes';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export const normalizeTransitionValue = (value: string, dataType?: BuilderDataType): string => {
  if (!value || !dataType || dataType === 'text' || dataType === 'object') return value;
  if (dataType === 'number') return Number.isFinite(Number(value)) ? value : '';
  if (dataType === 'boolean') return value === 'true' || value === 'false' ? value : '';
  if (dataType === 'date') return /^\d{4}-\d{2}-\d{2}$/.test(value) ? value : '';
  return value;
};

export function TransitionValueField({
  dataType,
  value,
  disabled = false,
  label,
  onChange,
}: {
  dataType?: BuilderDataType;
  value: string;
  disabled?: boolean;
  label?: string;
  onChange: (value: string) => void;
}) {
  const { t } = useAppTranslation();
  const resolvedLabel = label ?? t('wfProcessBuilder.settings.comparisonValue');
  if (dataType === 'boolean')
    return (
      <TextField select size="small" label={resolvedLabel} value={value} disabled={disabled} onChange={(event) => onChange(event.target.value)}>
        <MenuItem value="">{t('wfProcessBuilder.settings.selectValue')}</MenuItem>
        <MenuItem value="true">{t('wfProcessBuilder.settings.yes')}</MenuItem>
        <MenuItem value="false">{t('wfProcessBuilder.settings.no')}</MenuItem>
      </TextField>
    );

  return (
    <TextField
      size="small"
      label={resolvedLabel}
      value={value}
      disabled={disabled}
      type={dataType === 'number' ? 'number' : dataType === 'date' ? 'date' : 'text'}
      multiline={dataType === 'object'}
      minRows={dataType === 'object' ? 2 : undefined}
      slotProps={dataType === 'date' ? { inputLabel: { shrink: true } } : undefined}
      onChange={(event) => onChange(event.target.value)}
    />
  );
}
