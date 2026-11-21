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
  appearance?: 'standard' | 'posting';
  viewLabel?: string;
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
  appearance = 'standard',
  viewLabel,
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
      sx={{
        zIndex: (theme) => theme.zIndex.drawer + 2,
        ...(appearance === 'posting'
          ? { '& .MuiBackdrop-root': { backgroundColor: 'transparent' } }
          : {}),
      }}
      slotProps={{
        paper: {
          role: 'dialog',
          'aria-modal': true,
          'aria-labelledby': titleId,
          'aria-busy': busy,
          sx: {
            width: { xs: '100vw', sm: width },
            maxWidth:
              appearance === 'posting' ? { xs: '100vw', md: 'calc(100vw - 120px)' } : '100vw',
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
            minHeight: appearance === 'posting' ? 84 : 57,
            px: appearance === 'posting' ? '24px' : 2,
            pt: appearance === 'posting' ? '20px' : 0,
            flexShrink: 0,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            gap: 1,
            borderBottom: appearance === 'posting' ? 0 : 1,
            borderColor: 'divider',
            bgcolor: appearance === 'posting' ? 'background.paper' : 'action.hover',
          }}
        >
          <Box>
            {viewLabel && <Typography sx={{ fontSize: 11, mb: '3px' }}>{viewLabel}⌄</Typography>}
            <Typography
              id={titleId}
              component="h2"
              variant="subtitle1"
              sx={{
                fontWeight: appearance === 'posting' ? 600 : 700,
                fontSize: appearance === 'posting' ? 14 : undefined,
              }}
            >
              {title}
            </Typography>
          </Box>
          <IconButton
            size="small"
            disabled={busy}
            onClick={close}
            aria-label={t('actions.close', 'Close')}
          >
            <CloseIcon fontSize="small" />
          </IconButton>
        </Box>
        <Box
          sx={{
            flex: 1,
            minHeight: 0,
            overflow: 'auto',
            p: appearance === 'posting' ? '0 24px' : 2,
          }}
        >
          {children}
        </Box>
        {actions && (
          <Box
            sx={{
              flexShrink: 0,
              display: 'flex',
              justifyContent: 'flex-end',
              gap: 1,
              px: appearance === 'posting' ? '14px' : 2,
              py: appearance === 'posting' ? '22px' : 1.5,
              borderTop: appearance === 'posting' ? 0 : 1,
              borderColor: 'divider',
              ...(appearance === 'posting'
                ? {
                    '& .MuiButton-root': {
                      minWidth: 50,
                      height: 26,
                      px: 1,
                      py: 0,
                      fontSize: 12,
                      borderRadius: '2px',
                    },
                  }
                : {}),
            }}
          >
            {actions}
          </Box>
        )}
      </Box>
    </Drawer>
  );
}
