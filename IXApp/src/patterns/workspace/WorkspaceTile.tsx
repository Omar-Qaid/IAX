import React from 'react';
import { Paper, Box, Typography, Stack } from '@mui/material';

export interface WorkspaceTileProps {
  title: string;
  value: string | number;
  subtitle?: string;
  icon?: React.ReactNode;
  color?: 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'info';
  onClick?: () => void;
}

export const WorkspaceTile: React.FC<WorkspaceTileProps> = ({
  title,
  value,
  subtitle,
  icon,
  color = 'primary',
  onClick,
}) => {
  return (
    <Paper
      elevation={0}
      onClick={onClick}
      sx={{
        p: 1.25,
        borderRadius: 1,
        border: (t) => `1px solid ${t.palette.divider}`,
        cursor: onClick ? 'pointer' : 'default',
        transition: 'all 0.2s ease-in-out',
        '&:hover': onClick
          ? {
              transform: 'translateY(-2px)',
              boxShadow: (t) => t.shadows[2],
            }
          : undefined,
      }}
    >
      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <Box>
          <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600, textTransform: 'uppercase' }}>
            {title}
          </Typography>
          <Typography variant="h4" color={`${color}.main`} sx={{ fontWeight: 700, my: 0.5 }}>
            {value}
          </Typography>
          {subtitle && (
            <Typography variant="caption" color="text.secondary">
              {subtitle}
            </Typography>
          )}
        </Box>
        {icon && <Box sx={{ color: `${color}.main`, p: 0.5 }}>{icon}</Box>}
      </Stack>
    </Paper>
  );
};
