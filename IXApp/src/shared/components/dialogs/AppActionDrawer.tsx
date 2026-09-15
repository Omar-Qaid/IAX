import React, { useId } from 'react';
import { Box, Drawer, IconButton, Typography } from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import { useLogicalDrawerAnchor } from '@shared/hooks/useLogicalDrawerAnchor';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export interface AppActionDrawerProps {
  open: boolean;
  onClose: () => void;
  title: string;
  children: React.ReactNode;
  actions?: React.ReactNode;
  width?: number;
  busy?: boolean;
}

/** Shared modal surface for commands with custom forms, lists, or grids. */
export function AppActionDrawer({
  open,
  onClose,
  title,
  children,
  actions,
  width = 480,
  busy = false,
}: AppActionDrawerProps) {
  const anchor = useLogicalDrawerAnchor('end');
  const { t } = useAppTranslation();
  const titleId = useId();
  const close = () => {
    if (!busy) onClose();
  };
  return (
    <Drawer
      open={open}
      anchor={anchor}
      onClose={close}
      sx={{ zIndex: (theme) => theme.zIndex.drawer + 2 }}
      slotProps={{
        paper: {
          role: 'dialog',
          'aria-modal': true,
          'aria-labelledby': titleId,
          'aria-busy': busy,
          sx: {
            width: { xs: '100vw', sm: width },
            maxWidth: '100vw',
            overflow: 'hidden',
            borderInlineStart: 1,
            borderColor: 'divider',
          },
        },
      }}
    >
      <Box sx={{ height: '100%', minHeight: 0, display: 'flex', flexDirection: 'column' }}>
        <Box
          sx={{
            minHeight: 57,
            px: 2,
            flexShrink: 0,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            gap: 1,
            borderBottom: 1,
            borderColor: 'divider',
            bgcolor: 'action.hover',
          }}
        >
          <Typography id={titleId} component="h2" variant="subtitle1" sx={{ fontWeight: 700 }}>
            {title}
          </Typography>
          <IconButton
            size="small"
            disabled={busy}
            onClick={close}
            aria-label={t('actions.close', 'Close')}
          >
            <CloseIcon fontSize="small" />
          </IconButton>
        </Box>
        <Box sx={{ flex: 1, minHeight: 0, overflow: 'auto', p: 2 }}>{children}</Box>
        {actions && (
          <Box
            sx={{
              flexShrink: 0,
              display: 'flex',
              justifyContent: 'flex-end',
              gap: 1,
              px: 2,
              py: 1.5,
              borderTop: 1,
              borderColor: 'divider',
            }}
          >
            {actions}
          </Box>
        )}
      </Box>
    </Drawer>
  );
}
