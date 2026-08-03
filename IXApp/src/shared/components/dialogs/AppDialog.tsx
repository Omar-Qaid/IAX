import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  IconButton,
  Typography,
  Box,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';

export interface AppDialogProps {
  open: boolean;
  onClose: () => void;
  title: string;
  children: React.ReactNode;
  actions?: React.ReactNode;
  maxWidth?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
  fullWidth?: boolean;
}

export const AppDialog: React.FC<AppDialogProps> = ({
  open,
  onClose,
  title,
  children,
  actions,
  maxWidth = 'sm',
  fullWidth = true,
}) => {
  return (
    <Dialog open={open} onClose={onClose} maxWidth={maxWidth} fullWidth={fullWidth}>
      <DialogTitle sx={{ m: 0, p: 1.5, px: 2, display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <Typography component="span" variant="h6" sx={{ fontWeight: 700 }}>
          {title}
        </Typography>
        <IconButton size="small" aria-label="Close" onClick={onClose}>
          <CloseIcon fontSize="small" />
        </IconButton>
      </DialogTitle>
      <DialogContent dividers sx={{ p: 2 }}>
        <Box sx={{ pt: 0.5 }}>{children}</Box>
      </DialogContent>
      {actions && <DialogActions sx={{ p: 1.5, px: 2 }}>{actions}</DialogActions>}
    </Dialog>
  );
};
