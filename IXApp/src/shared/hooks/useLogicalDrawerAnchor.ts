import type { DrawerProps } from '@mui/material/Drawer';

export type LogicalDrawerPlacement = 'start' | 'end';

export function getLogicalDrawerAnchor(
  placement: LogicalDrawerPlacement,
): DrawerProps['anchor'] {
  // MUI mirrors these semantic anchors through the active RTL Emotion cache.
  return placement === 'start' ? 'left' : 'right';
}

export function useLogicalDrawerAnchor(
  placement: LogicalDrawerPlacement,
): DrawerProps['anchor'] {
  return getLogicalDrawerAnchor(placement);
}
