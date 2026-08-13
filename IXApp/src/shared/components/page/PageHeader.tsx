import React from 'react';
import { Box, Stack } from '@mui/material';
import { PageTitle, type PageTitleProps } from './PageTitle';

export interface PageHeaderProps extends PageTitleProps {
  actions?: React.ReactNode;
}

export const PageHeader: React.FC<PageHeaderProps> = ({ actions, ...titleProps }) => {
  return (
    <Box sx={{ mb: 1 }}>
      <Stack direction="row" spacing={1} sx={{ justifyContent: 'space-between', alignItems: 'center' }}>
        <PageTitle {...titleProps} />
        {actions && <Box>{actions}</Box>}
      </Stack>
    </Box>
  );
};
