import React from 'react';
import { Box, type SxProps, type Theme } from '@mui/material';

export interface FormContainerProps { children: React.ReactNode; columns?: number | Partial<Record<'xs' | 'sm' | 'md' | 'lg' | 'xl', number>>; gap?: number; sx?: SxProps<Theme> }
export const FormContainer: React.FC<FormContainerProps> = ({ children, columns = { xs: 1, sm: 2, lg: 4 }, gap = 1.5, sx }) => {
  const template = typeof columns === 'number' ? `repeat(${columns}, minmax(0, 1fr))` : Object.fromEntries(Object.entries(columns).map(([key, value]) => [key, `repeat(${value}, minmax(0, 1fr))`]));
  return <Box sx={[{ display: 'grid', gridTemplateColumns: template, gap, minWidth: 0 }, ...(Array.isArray(sx) ? sx : [sx])]}>{children}</Box>;
};

