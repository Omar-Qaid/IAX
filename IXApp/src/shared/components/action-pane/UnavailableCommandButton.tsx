import React from 'react';
import { Button, Tooltip } from '@mui/material';
import { useTranslation } from 'react-i18next';

export interface UnavailableCommandButtonProps {
  /** Translated command label. */
  label: string;
  startIcon?: React.ReactNode;
}

/**
 * A reference command that has no implementation behind it yet. It stays disabled and says why
 * on hover, so it reads as unbuilt rather than broken. A disabled button fires no pointer
 * events, so the tooltip hangs on an enabled wrapper.
 */
export const UnavailableCommandButton: React.FC<UnavailableCommandButtonProps> = ({ label, startIcon }) => {
  const { t } = useTranslation();
  return (
    <Tooltip title={t('common.notAvailableYet')}>
      <span style={{ display: 'inline-flex' }}>
        <Button disabled startIcon={startIcon}>{label}</Button>
      </span>
    </Tooltip>
  );
};
