import type { ReactNode } from 'react';
import { Box, Typography } from '@mui/material';

export interface DocumentLinesProps { title?: string; children: ReactNode; actions?: ReactNode; minHeight?: number }
export function DocumentLines({ title, children, actions, minHeight = 240 }: DocumentLinesProps) {
  return <Box component="section" aria-label={title}>
    {(title || actions) && <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
      {title && <Typography variant="subtitle1" component="h2">{title}</Typography>}{actions}
    </Box>}
    <Box sx={{ minHeight }}>{children}</Box>
  </Box>;
}
