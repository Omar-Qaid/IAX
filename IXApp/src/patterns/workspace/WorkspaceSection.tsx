import React from 'react';
import { Box, Grid, Typography } from '@mui/material';

export interface WorkspaceSectionProps {
  title: string;
  subtitle?: string;
  children: React.ReactNode;
}

export const WorkspaceSection: React.FC<WorkspaceSectionProps> = ({ title, subtitle, children }) => (
  <Box component="section">
    <Typography variant="h6" sx={{ fontWeight: 600 }}>{title}</Typography>
    {subtitle && <Typography variant="body2" color="text.secondary" sx={{ mb: 1.5 }}>{subtitle}</Typography>}
    <Grid container spacing={2}>{children}</Grid>
  </Box>
);

