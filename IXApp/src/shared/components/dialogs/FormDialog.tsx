import React from 'react';
import { Button, CircularProgress } from '@mui/material';
import { AppDialog, type AppDialogProps } from './AppDialog';
export interface FormDialogProps extends Omit<AppDialogProps, 'actions'> { onSubmit: () => void; submitLabel: string; cancelLabel: string; submitting?: boolean; submitDisabled?: boolean }
export const FormDialog: React.FC<FormDialogProps> = ({ onSubmit, submitLabel, cancelLabel, submitting = false, submitDisabled = false, ...props }) => <AppDialog {...props} actions={<><Button onClick={props.onClose} disabled={submitting}>{cancelLabel}</Button><Button variant="contained" onClick={onSubmit} disabled={submitting || submitDisabled} startIcon={submitting ? <CircularProgress size={14} /> : undefined}>{submitLabel}</Button></>}>{props.children}</AppDialog>;

