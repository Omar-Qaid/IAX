import React from 'react';
import { Drawer } from '@mui/material';
import { useTheme } from '@mui/material/styles';
import {
  FastTabsDialog,
  type FastTabsDialogProps,
  type FastTabValue,
} from '@patterns/dialog-fast-tabs/FastTabsDialog';

export type FastTabsDrawerProps<TValues extends Record<string, FastTabValue>> =
  Omit<FastTabsDialogProps<TValues>, 'placement'>;

/**
 * Direction-aware quick-create surface. The existing fast-tab form remains the
 * source of field, validation, and action behavior while the drawer owns its
 * placement relative to the application direction.
 */
export function FastTabsDrawer<TValues extends Record<string, FastTabValue>>(
  props: FastTabsDrawerProps<TValues>
): React.ReactElement {
  const theme = useTheme();
  return (
    <Drawer
      open={props.open}
      anchor={theme.direction === 'ltr' ? 'right' : 'left'}
      onClose={props.onCancel}
      sx={{ zIndex: (currentTheme) => currentTheme.zIndex.drawer + 2 }}
      ModalProps={{ keepMounted: false }}
      slotProps={{
        paper: {
          sx: {
            width: { xs: '100vw', sm: 600, md: 826 },
            maxWidth: '100vw',
            bgcolor: 'transparent',
            boxShadow: 'none',
            overflow: 'hidden',
            direction: theme.direction,
          },
        },
      }}
    >
      <FastTabsDialog {...props} open placement="top-start" embedded />
    </Drawer>
  );
}
