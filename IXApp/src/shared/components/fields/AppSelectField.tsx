import React from 'react';
import { MenuItem, TextField } from '@mui/material';
import { Controller, type FieldValues } from 'react-hook-form';
import type { BaseFieldProps } from './types';
import type { SelectOption } from '@core/types/common';

export interface AppSelectFieldProps<
  TFieldValues extends FieldValues = FieldValues,
> extends BaseFieldProps<TFieldValues, string | number> {
  options: SelectOption[];
}

export function AppSelectField<TFieldValues extends FieldValues = FieldValues>({
  name,
  label,
  control,
  options,
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
        label={label}
        required={required}
        disabled={disabled || readOnly}
        helperText={helperText}
        fullWidth={fullWidth}
        size="small"
        variant={variant}
        value={value ?? ''}
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
          label={label}
          required={required}
          disabled={disabled || readOnly}
          error={!!error}
          helperText={error ? error.message : helperText}
          fullWidth={fullWidth}
          size="small"
          value={field.value ?? ''}
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
