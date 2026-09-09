import React from 'react';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { ActionPaneButton } from './ActionPaneButton';
import { ActionPaneGroup } from './ActionPaneGroup';

export interface ActionPaneBackButtonProps {
  label: string;
  onClick: () => void;
  disabled?: boolean;
}

export function ActionPaneBackButton({
  label,
  onClick,
  disabled = false,
}: ActionPaneBackButtonProps): React.ReactElement {
  return (
    <ActionPaneGroup>
      <ActionPaneButton
        label={label}
        icon={
          <ArrowBackIcon
            sx={{ transform: (theme) => (theme.direction === 'rtl' ? 'scaleX(-1)' : 'none') }}
          />
        }
        onClick={onClick}
        disabled={disabled}
      />
    </ActionPaneGroup>
  );
}
