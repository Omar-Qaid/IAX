import type { ReactNode } from 'react';
import { Box, Typography, type SxProps, type Theme } from '@mui/material';

export interface DocumentHeaderProps { title?: string; children: ReactNode; actions?: ReactNode; sx?: SxProps<Theme> }
export function DocumentHeader({ title, children, actions, sx }: DocumentHeaderProps) {
  return <Box component="section" aria-label={title} sx={sx}>
    {(title || actions) && <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
      {title && <Typography variant="subtitle1" component="h2">{title}</Typography>}{actions}
    </Box>}
    {children}
  </Box>;
}
