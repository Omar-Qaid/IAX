import React from 'react';
import { Typography, Stack, Chip } from '@mui/material';

export interface PageTitleProps {
  title: string;
  subtitle?: string;
  badge?: string;
  badgeColor?: 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning';
}

export const PageTitle: React.FC<PageTitleProps> = ({ title, subtitle, badge, badgeColor = 'primary' }) => {
  return (
    <Stack spacing={0.25}>
      <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
        <Typography variant="h5" color="text.primary" sx={{ fontWeight: 700 }}>
          {title}
        </Typography>
        {badge && <Chip label={badge} size="small" color={badgeColor} sx={{ height: 20, fontSize: '0.65rem', fontWeight: 700 }} />}
      </Stack>
      {subtitle && (
        <Typography variant="caption" color="text.secondary">
          {subtitle}
        </Typography>
      )}
    </Stack>
  );
};
