import React from 'react';
import { Button, Typography, Box } from '@mui/material';
import WarningAmberOutlinedIcon from '@mui/icons-material/WarningAmberOutlined';
import { AppDialog } from './AppDialog';
import { useAppTranslation } from '@core/localization/useAppTranslation';

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
  title,
  message,
  confirmLabel,
  cancelLabel,
  severity = 'warning',
  loading = false,
}) => {
  const { t } = useAppTranslation();
  const resolvedTitle = title ?? t('dialogs.confirmAction');
  const resolvedConfirmLabel = confirmLabel ?? t('actions.confirm');
  const resolvedCancelLabel = cancelLabel ?? t('actions.cancel');
  return (
    <AppDialog
      open={open}
      onClose={onClose}
      title={resolvedTitle}
      maxWidth="xs"
      actions={
        <>
          <Button onClick={onClose} disabled={loading} size="small">
            {resolvedCancelLabel}
          </Button>
          <Button
            onClick={onConfirm}
            color={severity === 'error' ? 'error' : 'primary'}
            variant="contained"
            disabled={loading}
            size="small"
          >
            {resolvedConfirmLabel}
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
  const { t } = useAppTranslation();
  return (
    <ConfirmationDialog
      title={t('dialogs.confirmDeletion')}
      confirmLabel={t('actions.delete')}
      severity="error"
      {...props}
    />
  );
};
