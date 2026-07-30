import React from 'react';
import { Button, CircularProgress, Tooltip } from '@mui/material';
import { usePermission } from '@core/permissions/usePermission';
import type { ActionDefinition } from './types';

export interface ActionPaneButtonProps extends Omit<ActionDefinition, 'id'> {
  actionId?: string;
}

export const ActionPaneButton: React.FC<ActionPaneButtonProps> = ({
  label,
  icon,
  onClick,
  disabled = false,
  loading = false,
  permission,
  tooltip,
}) => {
  const { hasPermission } = usePermission(permission);

  if (!hasPermission) return null;

  const buttonElement = (
    <Button
      variant="text"
      size="small"
      startIcon={loading ? <CircularProgress size={14} color="inherit" /> : icon}
      onClick={onClick}
      disabled={disabled || loading}
      sx={{
        color: 'text.primary',
        fontWeight: 600,
        fontSize: '0.78125rem',
        px: 1.25,
        py: 0.5,
        borderRadius: 1,
        border: '1px solid transparent',
        '&:hover': {
          bgcolor: (t) => (t.palette.mode === 'light' ? '#e0e0e0' : '#333333'),
          borderColor: 'divider',
        },
        '&.Mui-disabled': {
          color: 'text.disabled',
        },
      }}
    >
      {label}
    </Button>
  );

  if (tooltip) {
    return <Tooltip title={tooltip}>{buttonElement}</Tooltip>;
  }

  return buttonElement;
};
