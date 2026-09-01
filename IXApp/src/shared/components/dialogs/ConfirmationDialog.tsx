import React from 'react';
import {
  alpha,
  Box,
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  Typography,
} from '@mui/material';
import CloseOutlinedIcon from '@mui/icons-material/CloseOutlined';
import ErrorOutlineRoundedIcon from '@mui/icons-material/ErrorOutlineRounded';
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined';
import WarningAmberRoundedIcon from '@mui/icons-material/WarningAmberRounded';
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

const severityPresentation = {
  warning: { color: 'warning.main', Icon: WarningAmberRoundedIcon },
  error: { color: 'error.main', Icon: ErrorOutlineRoundedIcon },
  info: { color: 'info.main', Icon: InfoOutlinedIcon },
} as const;

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
  const titleId = React.useId();
  const messageId = React.useId();
  const resolvedTitle = title ?? t('dialogs.confirmAction');
  const resolvedConfirmLabel = confirmLabel ?? t('actions.confirm');
  const resolvedCancelLabel = cancelLabel ?? t('actions.cancel');
  const { color, Icon } = severityPresentation[severity];

  return (
    <Dialog
      open={open}
      onClose={loading ? undefined : onClose}
      aria-labelledby={titleId}
      aria-describedby={messageId}
      maxWidth="xs"
      fullWidth
      slotProps={{
        paper: {
          sx: {
            width: 'calc(100% - 32px)',
            maxWidth: '480px !important',
            m: 2,
            borderRadius: 3,
            overflow: 'hidden',
          },
        },
      }}
    >
      <DialogTitle
        id={titleId}
        sx={{
          display: 'flex',
          alignItems: 'center',
          gap: 1.5,
          px: 3,
          py: 2.25,
          borderBottom: '1px solid',
          borderColor: 'divider',
        }}
      >
        <Box
          aria-hidden="true"
          sx={(theme) => ({
            width: 42,
            height: 42,
            flex: '0 0 auto',
            display: 'grid',
            placeItems: 'center',
            borderRadius: '50%',
            color,
            bgcolor: alpha(
              severity === 'error'
                ? theme.palette.error.main
                : severity === 'info'
                  ? theme.palette.info.main
                  : theme.palette.warning.main,
              0.12
            ),
          })}
        >
          <Icon sx={{ fontSize: 25 }} />
        </Box>

        <Typography
          component="span"
          variant="h6"
          sx={{ flex: 1, fontWeight: 700, lineHeight: 1.35 }}
        >
          {resolvedTitle}
        </Typography>

        <IconButton
          aria-label={t('actions.close')}
          onClick={onClose}
          disabled={loading}
          size="small"
          edge="end"
          sx={{ color: 'text.secondary' }}
        >
          <CloseOutlinedIcon fontSize="small" />
        </IconButton>
      </DialogTitle>

      <DialogContent sx={{ px: 3, py: '24px !important' }}>
        <Typography
          id={messageId}
          variant="body1"
          color="text.secondary"
          sx={{ lineHeight: 1.8, overflowWrap: 'anywhere' }}
        >
          {message}
        </Typography>
      </DialogContent>

      <DialogActions
        sx={{
          gap: 1,
          px: 3,
          py: 2,
          bgcolor: 'action.hover',
          borderTop: '1px solid',
          borderColor: 'divider',
        }}
      >
        <Button
          onClick={onClose}
          disabled={loading}
          variant="outlined"
          color="inherit"
          sx={{ minWidth: 96 }}
        >
          {resolvedCancelLabel}
        </Button>
        <Button
          onClick={onConfirm}
          color={severity === 'error' ? 'error' : 'primary'}
          variant="contained"
          disabled={loading}
          autoFocus
          sx={{ minWidth: 104 }}
          startIcon={loading ? <CircularProgress size={16} color="inherit" /> : undefined}
        >
          {resolvedConfirmLabel}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export const DeleteConfirmationDialog: React.FC<Omit<ConfirmationDialogProps, 'severity'>> = (
  props
) => {
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
