import React from 'react';
import { Box, Typography } from '@mui/material';

/** Common sales-order typography and spacing for enterprise document headers. */
export function DocumentHeader({ title, subtitle, viewLabel, status, children }: {
  title: string;
  subtitle?: string;
  viewLabel?: string;
  status?: React.ReactNode;
  children?: React.ReactNode;
}): React.ReactElement {
  return <Box>
    {(subtitle || viewLabel) && <Typography color="primary" variant="body2" sx={{ mb: 1 }}>
      {[subtitle, viewLabel].filter(Boolean).join(' | ')}
    </Typography>}
    <Box sx={{ mb: 1, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
      <Typography component="h1" variant="h5">{title}</Typography>
      {status != null && <Typography variant="body2" sx={{ marginInlineEnd: 3, flexShrink: 0 }}>{status}</Typography>}
    </Box>
    {children}
  </Box>;
}
