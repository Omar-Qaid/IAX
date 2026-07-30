import React from 'react';
import { AppTextField, type AppTextFieldProps } from './AppTextField';
import type { FieldValues } from 'react-hook-form';

export function AppDateField<TFieldValues extends FieldValues = FieldValues>(
  props: AppTextFieldProps<TFieldValues>
): React.ReactElement | null {
  return (
    <AppTextField
      {...props}
      type="date"
      slotProps={{
        inputLabel: { shrink: true },
      }}
    />
  );
}
