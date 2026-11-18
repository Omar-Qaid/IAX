import React from 'react';
import { Typography } from '@mui/material';

/**
 * One value block of a list page's related information (FactBox): multi-line
 * text, or a muted hint when no record is selected.
 */
export function ListInfoValue({ value, empty }: { value?: string | null; empty: string }): React.ReactElement {
  return (
    <Typography
      sx={{ fontSize: '0.75rem', color: value ? 'text.primary' : 'text.secondary', whiteSpace: 'pre-line' }}
    >
      {value || empty}
    </Typography>
  );
}
