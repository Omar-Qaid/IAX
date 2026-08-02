import React from 'react';
import { EmptyState } from './EmptyState';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export interface NoResultsStateProps {
  title?: string;
  message?: string;
  onClear?: () => void;
}

export const NoResultsState: React.FC<NoResultsStateProps> = ({ title, message, onClear }) => {
  const { t } = useAppTranslation();
  return (
    <EmptyState
      title={title ?? t('common.noResults')}
      message={message ?? t('grid.no_results_msg')}
      actionLabel={onClear ? t('actions.clear') : undefined}
      onAction={onClear}
    />
  );
};
