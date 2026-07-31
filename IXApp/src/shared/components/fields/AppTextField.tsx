import React from 'react';
import { TextField, type TextFieldProps } from '@mui/material';
import { Controller, type FieldValues } from 'react-hook-form';
import type { BaseFieldProps } from './types';

export interface AppTextFieldProps<TFieldValues extends FieldValues = FieldValues>
  extends BaseFieldProps<TFieldValues> {
  multiline?: boolean;
  rows?: number;
  type?: string;
  slotProps?: TextFieldProps['slotProps'];
}

export function AppTextField<TFieldValues extends FieldValues = FieldValues>({
  name,
  label,
  control,
  required = false,
  disabled = false,
  readOnly = false,
  hidden = false,
  helperText,
  fullWidth = true,
  placeholder,
  multiline = false,
  rows = 3,
  type = 'text',
  variant = 'outlined',
  value,
  onChange,
  slotProps,
  ...rest
}: AppTextFieldProps<TFieldValues>): React.ReactElement | null {
  if (hidden) return null;

  const combinedSlotProps = {
    ...slotProps,
    input: {
      readOnly,
      ...slotProps?.input,
    },
  };

  if (!control) {
    return (
      <TextField
        name={name}
        label={label}
        required={required}
        disabled={disabled}
        slotProps={combinedSlotProps}
        helperText={helperText}
        fullWidth={fullWidth}
        placeholder={placeholder}
        multiline={multiline}
        rows={multiline ? rows : undefined}
        type={type}
        size="small"
        variant={variant}
        value={value ?? ''}
        onChange={(e) => onChange?.(e.target.value)}
        {...rest}
      />
    );
  }

  return (
    <Controller
      name={name as any}
      control={control}
      render={({ field, fieldState: { error } }) => (
        <TextField
          {...field}
          label={label}
          required={required}
          disabled={disabled}
          slotProps={combinedSlotProps}
          error={!!error}
          helperText={error ? error.message : helperText}
          fullWidth={fullWidth}
          placeholder={placeholder}
          multiline={multiline}
          rows={multiline ? rows : undefined}
          type={type}
          size="small"
          value={field.value ?? ''}
        />
      )}
    />
  );
}
