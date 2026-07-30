import React from 'react';
import { AppTextField, type AppTextFieldProps } from './AppTextField';
import { InputAdornment } from '@mui/material';
import type { FieldValues } from 'react-hook-form';

export interface AppNumberFieldProps<TFieldValues extends FieldValues = FieldValues>
  extends Omit<AppTextFieldProps<TFieldValues>, 'type'> {
  min?: number;
  max?: number;
  step?: number;
}

export function AppNumberField<TFieldValues extends FieldValues = FieldValues>(
  props: AppNumberFieldProps<TFieldValues>
): React.ReactElement | null {
  return <AppTextField {...props} type="number" />;
}

export interface AppCurrencyFieldProps<TFieldValues extends FieldValues = FieldValues>
  extends AppNumberFieldProps<TFieldValues> {
  currencySymbol?: string;
}

export function AppCurrencyField<TFieldValues extends FieldValues = FieldValues>({
  currencySymbol = '$',
  ...props
}: AppCurrencyFieldProps<TFieldValues>): React.ReactElement | null {
  return (
    <AppTextField
      {...props}
      type="number"
      placeholder="0.00"
      slotProps={{
        input: {
          startAdornment: <InputAdornment position="start">{currencySymbol}</InputAdornment>,
        },
      }}
    />
  );
}
