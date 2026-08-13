import React from 'react';
import { Paper, type PaperProps } from '@mui/material';

export type PageContentProps = PaperProps;

export const PageContent: React.FC<PageContentProps> = ({ children, sx, ...props }) => (
  <Paper
    component="section"
    elevation={0}
    sx={{ p: 1.25, borderRadius: 1, border: (theme) => `1px solid ${theme.palette.divider}`, ...sx }}
    {...props}
  >
    {children}
  </Paper>
);

