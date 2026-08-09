import React from 'react';
import { FormControlLabel, Checkbox, FormHelperText, Box } from '@mui/material';
import { Controller, type FieldValues } from 'react-hook-form';
import type { BaseFieldProps } from './types';

export function AppBooleanField<TFieldValues extends FieldValues = FieldValues>({
  name,
  label,
  control,
  disabled = false,
  readOnly = false,
  hidden = false,
  helperText,
  value,
  onChange,
}: BaseFieldProps<TFieldValues, boolean>): React.ReactElement | null {
  if (hidden) return null;

  if (!control || !name) {
    return (
      <FormControlLabel
        control={
          <Checkbox
            checked={!!value}
            onChange={(e) => onChange?.(e.target.checked)}
            disabled={disabled || readOnly}
            size="small"
          />
        }
        label={label || ''}
      />
    );
  }

  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState: { error } }) => (
        <Box>
          <FormControlLabel
            control={
              <Checkbox
                {...field}
                checked={!!field.value}
                disabled={disabled || readOnly}
                size="small"
              />
            }
            label={label}
          />
          {(error || helperText) && (
            <FormHelperText error={!!error}>{error ? error.message : helperText}</FormHelperText>
          )}
        </Box>
      )}
    />
  );
}
