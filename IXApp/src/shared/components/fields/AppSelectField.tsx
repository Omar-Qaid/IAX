import React from 'react';
import { MenuItem, TextField } from '@mui/material';
import { Controller, type FieldValues } from 'react-hook-form';
import type { BaseFieldProps } from './types';
import type { SelectOption } from '@core/types/common';
import { lookupValueSx } from '../lookups/lookupValueStyle';
import { LookupNavigationLabel } from '../lookups/LookupNavigationLabel';

const selectPlaceholder = <span className="app-select-placeholder">Select...</span>;
const selectValue = (selected: unknown, options: SelectOption[]) =>
  selected === '' ? selectPlaceholder
    : options.find((option) => String(option.value) === String(selected))?.label ?? String(selected);

export interface AppSelectFieldProps<
  TFieldValues extends FieldValues = FieldValues,
> extends BaseFieldProps<TFieldValues, string | number> {
  options: SelectOption[];
  masterRoute?: string;
}

export function AppSelectField<TFieldValues extends FieldValues = FieldValues>({
  name,
  label,
  control,
  options,
  masterRoute,
  required = false,
  disabled = false,
  readOnly = false,
  hidden = false,
  helperText,
  fullWidth = true,
  variant = 'outlined',
  value,
  onChange,
}: AppSelectFieldProps<TFieldValues>): React.ReactElement | null {
  if (hidden) return null;

  if (!control || !name) {
    return (
      <TextField
        select
        name={name}
        label={label ? <LookupNavigationLabel label={label} masterRoute={masterRoute} /> : undefined}
        required={required}
        disabled={disabled || readOnly}
        helperText={helperText}
        fullWidth={fullWidth}
        size="small"
        variant={variant}
        value={value ?? ''}
        sx={{ ...lookupValueSx(value), '& .app-select-placeholder': { color: 'text.disabled' } }}
        slotProps={{ select: { displayEmpty: true, renderValue: (selected) => selectValue(selected, options) } }}
        onChange={(e) => onChange?.(e.target.value)}
      >
        {options.map((opt) => (
          <MenuItem key={opt.value} value={opt.value} disabled={opt.disabled}>
            {opt.label}
          </MenuItem>
        ))}
      </TextField>
    );
  }

  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState: { error } }) => (
        <TextField
          {...field}
          select
          label={label ? <LookupNavigationLabel label={label} masterRoute={masterRoute} /> : undefined}
          required={required}
          disabled={disabled || readOnly}
          error={!!error}
          helperText={error ? error.message : helperText}
          fullWidth={fullWidth}
          size="small"
          value={field.value ?? ''}
          sx={{ ...lookupValueSx(field.value), '& .app-select-placeholder': { color: 'text.disabled' } }}
          slotProps={{ select: { displayEmpty: true, renderValue: (selected) => selectValue(selected, options) } }}
        >
          {options.map((opt) => (
            <MenuItem key={opt.value} value={opt.value} disabled={opt.disabled}>
              {opt.label}
            </MenuItem>
          ))}
        </TextField>
      )}
    />
  );
}
