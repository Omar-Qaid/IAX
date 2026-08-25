import React from 'react';
import { Button, CircularProgress, Tooltip } from '@mui/material';
import { usePermission } from '@core/permissions/usePermission';
import type { ActionDefinition } from './types';
import { actionPaneControlSx } from './actionPaneControlStyles';

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
      sx={actionPaneControlSx}
    >
      {label}
    </Button>
  );

  if (tooltip) {
    return <Tooltip title={tooltip}>{buttonElement}</Tooltip>;
  }

  return buttonElement;
};
