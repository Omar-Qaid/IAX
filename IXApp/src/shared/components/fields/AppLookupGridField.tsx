import React from 'react';
import { LookupGridField } from '../lookups/LookupGridField';
import type { LookupGridFieldProps } from '../lookups/types';
import type { FieldValues } from 'react-hook-form';

export function AppLookupGridField<
  T extends object,
  TFieldValues extends FieldValues = FieldValues,
>(props: LookupGridFieldProps<T, TFieldValues>) {
  return <LookupGridField<T, TFieldValues> {...props} />;
}
