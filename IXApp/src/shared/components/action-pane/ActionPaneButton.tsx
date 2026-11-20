import React from 'react';
import { Button, CircularProgress, Tooltip } from '@mui/material';
import { usePermission } from '@core/permissions/usePermission';
import type { ActionDefinition } from './types';
import { actionPaneControlSx } from './actionPaneControlStyles';

export interface ActionPaneButtonProps extends Omit<ActionDefinition, 'id'> {
  actionId?: string;
  selected?: boolean;
  controlsId?: string;
  editCommand?: boolean;
}

export const ActionPaneButton: React.FC<ActionPaneButtonProps> = ({
  label,
  icon,
  onClick,
  disabled = false,
  loading = false,
  permission,
  tooltip,
  hidden = false,
  selected = false,
  controlsId,
  editCommand,
}) => {
  const { hasPermission } = usePermission(permission);

  // `hidden` is part of the ActionDefinition contract these props extend, and ActionPaneMenu
  // already filters on it; without this the same action object renders when passed to a button.
  if (hidden || !hasPermission) return null;

  const buttonElement = (
    <Button
      data-grid-edit-command={editCommand || undefined}
      aria-label={label}
      variant="text"
      size="small"
      startIcon={loading ? <CircularProgress size={14} color="inherit" /> : icon}
      onClick={onClick}
      disabled={disabled || loading || !onClick}
      aria-expanded={controlsId ? selected : undefined}
      aria-controls={selected ? controlsId : undefined}
      sx={[
        actionPaneControlSx,
        selected
          ? {
              borderBottomColor: 'primary.main',
              borderBottomWidth: 3,
              borderRadius: 0,
              fontWeight: 600,
            }
          : {},
      ]}
    >
      {label}
    </Button>
  );

  if (tooltip) {
    // A disabled button fires no pointer events, so the tooltip needs an enabled wrapper;
    // this is the case that explains why a command is unavailable.
    const isDisabled = disabled || loading || !onClick;
    return (
      <Tooltip title={tooltip}>
        {isDisabled ? (
          <span style={{ display: 'inline-flex' }}>{buttonElement}</span>
        ) : (
          buttonElement
        )}
      </Tooltip>
    );
  }

  return buttonElement;
};
