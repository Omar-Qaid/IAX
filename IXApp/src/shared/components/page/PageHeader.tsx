import React from 'react';
import { Box, Stack } from '@mui/material';
import { PageTitle, type PageTitleProps } from './PageTitle';

export interface PageHeaderProps extends PageTitleProps {
  actions?: React.ReactNode;
}

export const PageHeader: React.FC<PageHeaderProps> = ({ actions, ...titleProps }) => {
  return (
    <Box sx={{ mb: 1, minWidth: 0 }}>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} sx={{ justifyContent: 'space-between', alignItems: { xs: 'stretch', sm: 'center' }, minWidth: 0 }}>
        <PageTitle {...titleProps} />
        {actions && <Box sx={{ minWidth: 0, maxWidth: '100%', overflowX: 'auto' }}>{actions}</Box>}
      </Stack>
    </Box>
  );
};
