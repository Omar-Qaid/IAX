import type { FormEvent, ReactNode } from 'react';
import { Alert, Box, Button } from '@mui/material';
import { FormActions } from './FormActions';
import { FormContainer, type FormContainerProps } from './FormContainer';

export interface EntityFormProps extends Omit<FormContainerProps, 'children'> {
  children: ReactNode;
  errors?: Record<string, string | undefined>;
  submitLabel?: string;
  cancelLabel?: string;
  submitting?: boolean;
  disabled?: boolean;
  onSubmit?: () => void | Promise<void>;
  onCancel?: () => void;
  actions?: ReactNode;
}

export function EntityForm({ children, errors = {}, submitLabel = 'Save', cancelLabel = 'Cancel', submitting, disabled, onSubmit, onCancel, actions, ...layout }: EntityFormProps) {
  const messages = Object.values(errors).filter((message): message is string => Boolean(message));
  const handleSubmit = (event: FormEvent) => { event.preventDefault(); void onSubmit?.(); };
  return <Box component="form" noValidate onSubmit={handleSubmit}>
    {messages.length > 0 && <Alert severity="error" sx={{ mb: 2 }}>{messages.join(' ')}</Alert>}
    <FormContainer {...layout}>{children}</FormContainer>
    <FormActions sx={{ mt: 2 }}>
      {actions}
      {onCancel && <Button onClick={onCancel} disabled={submitting}>{cancelLabel}</Button>}
      {onSubmit && <Button type="submit" variant="contained" disabled={disabled || submitting}>{submitLabel}</Button>}
    </FormActions>
  </Box>;
}
