import React from 'react';
import { Alert, Button, CircularProgress } from '@mui/material';
import { AppDialog, type AppDialogProps } from './AppDialog';
export interface ProcessDialogProps extends Omit<AppDialogProps, 'actions'> { processing?: boolean; error?: string; executeLabel: string; cancelLabel: string; onExecute: () => void }
export const ProcessDialog: React.FC<ProcessDialogProps> = ({ processing = false, error, executeLabel, cancelLabel, onExecute, children, ...props }) => <AppDialog {...props} actions={<><Button onClick={props.onClose} disabled={processing}>{cancelLabel}</Button><Button variant="contained" onClick={onExecute} disabled={processing} startIcon={processing ? <CircularProgress size={14} /> : undefined}>{executeLabel}</Button></>}>{error && <Alert severity="error" sx={{ mb: 1 }}>{error}</Alert>}{children}</AppDialog>;

