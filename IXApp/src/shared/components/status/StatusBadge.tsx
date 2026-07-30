import React from 'react';
import { Chip } from '@mui/material';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export interface StatusBadgeProps {
  status: string;
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ status }) => {
  const { t } = useAppTranslation();

  let color: 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' = 'default';

  const s = (status || '').toLowerCase();
  if (s === 'active' || s === 'confirmed' || s === 'invoiced') {
    color = 'success';
  } else if (s === 'onhold' || s === 'warning' || s === 'pending') {
    color = 'warning';
  } else if (s === 'blocked' || s === 'cancelled' || s === 'error') {
    color = 'error';
  } else if (s === 'open' || s === 'draft') {
    color = 'info';
  }

  const label = t(`common.${s}`) || status;

  return (
    <Chip
      label={label}
      size="small"
      color={color}
      sx={{
        height: 22,
        fontSize: '0.6875rem',
        fontWeight: 700,
        borderRadius: 0.5,
      }}
    />
  );
};
