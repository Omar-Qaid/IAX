import React from 'react';
import { Alert, Snackbar, type AlertColor } from '@mui/material';
export interface AppNotificationProps { open: boolean; message: React.ReactNode; severity?: AlertColor; onClose: () => void; autoHideDuration?: number }
export const AppNotification: React.FC<AppNotificationProps> = ({ open, message, severity = 'info', onClose, autoHideDuration = 4000 }) => <Snackbar open={open} autoHideDuration={autoHideDuration} onClose={onClose}><Alert severity={severity} variant="filled" onClose={onClose}>{message}</Alert></Snackbar>;

