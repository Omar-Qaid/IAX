import React from 'react';
import { Button, Typography, Box } from '@mui/material';
import WarningAmberOutlinedIcon from '@mui/icons-material/WarningAmberOutlined';
import { AppDialog } from './AppDialog';

export interface ConfirmationDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title?: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  severity?: 'warning' | 'error' | 'info';
  loading?: boolean;
}

export const ConfirmationDialog: React.FC<ConfirmationDialogProps> = ({
  open,
  onClose,
  onConfirm,
  title = 'Confirm Action',
  message,
  confirmLabel = 'Confirm',
  cancelLabel = 'Cancel',
  severity = 'warning',
  loading = false,
}) => {
  return (
    <AppDialog
      open={open}
      onClose={onClose}
      title={title}
      maxWidth="xs"
      actions={
        <>
          <Button onClick={onClose} disabled={loading} size="small">
            {cancelLabel}
          </Button>
          <Button
            onClick={onConfirm}
            color={severity === 'error' ? 'error' : 'primary'}
            variant="contained"
            disabled={loading}
            size="small"
          >
            {confirmLabel}
          </Button>
        </>
      }
    >
      <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1.5, py: 1 }}>
        <WarningAmberOutlinedIcon color={severity === 'error' ? 'error' : 'warning'} sx={{ fontSize: 32 }} />
        <Typography variant="body2">{message}</Typography>
      </Box>
    </AppDialog>
  );
};

export const DeleteConfirmationDialog: React.FC<Omit<ConfirmationDialogProps, 'severity'>> = (props) => {
  return (
    <ConfirmationDialog
      title="Confirm Deletion"
      confirmLabel="Delete"
      severity="error"
      {...props}
    />
  );
};
