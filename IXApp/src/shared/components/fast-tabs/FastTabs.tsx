import React from 'react';
import { Box } from '@mui/material';

export interface FastTabsProps {
  children: React.ReactNode;
}

export const FastTabs: React.FC<FastTabsProps> = ({ children }) => {
  return <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>{children}</Box>;
};
