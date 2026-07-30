import { useCallback } from 'react';

interface UsePageRefreshOptions {
  onRefresh: () => void | Promise<void>;
  isDirty?: boolean;
  confirmMessage?: string;
}

export function usePageRefresh({
  onRefresh,
  isDirty = false,
  confirmMessage = 'You have unsaved changes. Are you sure you want to refresh and discard changes?',
}: UsePageRefreshOptions) {
  const handleRefresh = useCallback(async () => {
    if (isDirty) {
      const confirmed = window.confirm(confirmMessage);
      if (!confirmed) {
        return;
      }
    }
    await onRefresh();
  }, [onRefresh, isDirty, confirmMessage]);

  return {
    handleRefresh,
  };
}
