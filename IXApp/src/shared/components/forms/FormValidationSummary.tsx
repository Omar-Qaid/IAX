import React from 'react';
import { Alert, AlertTitle, Box, Typography } from '@mui/material';
import type { FieldErrors, FieldValues } from 'react-hook-form';

export interface FormValidationSummaryProps<T extends FieldValues = FieldValues> {
  errors: FieldErrors<T>;
  title?: string;
}

export function FormValidationSummary<T extends FieldValues = FieldValues>({
  errors,
  title = 'Please correct the following validation errors:',
}: FormValidationSummaryProps<T>): React.ReactElement | null {
  const errorKeys = Object.keys(errors);
  if (errorKeys.length === 0) return null;

  return (
    <Alert severity="error" sx={{ mb: 2, borderRadius: 1 }}>
      <AlertTitle sx={{ fontWeight: 700 }}>{title}</AlertTitle>
      <Box component="ul" sx={{ paddingInlineStart: 2, m: 0 }}>
        {errorKeys.map((key) => {
          const err = errors[key as keyof T];
          return (
            <li key={key}>
              <Typography variant="caption" sx={{ fontWeight: 600 }}>
                {key}: {err?.message ? String(err.message) : 'Invalid value'}
              </Typography>
            </li>
          );
        })}
      </Box>
    </Alert>
  );
}
