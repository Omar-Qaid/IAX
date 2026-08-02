import React from 'react';
import { Box, Typography, type BoxProps } from '@mui/material';

export interface PageSectionProps extends Omit<BoxProps, 'title'> {
  title?: React.ReactNode;
  description?: React.ReactNode;
}

export const PageSection: React.FC<PageSectionProps> = ({ title, description, children, sx, ...props }) => (
  <Box component="section" sx={{ mb: 2, ...sx }} {...props}>
    {(title || description) && (
      <Box sx={{ pb: 1, mb: 1.5, borderBottom: (theme) => `1px solid ${theme.palette.divider}` }}>
        {title && <Typography variant="subtitle2" color="primary.main">{title}</Typography>}
        {description && <Typography variant="body2" color="text.secondary">{description}</Typography>}
      </Box>
    )}
    {children}
  </Box>
);

