import { useCallback } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { useAppStore } from '@app/store/useAppStore';

export function usePageRefresh(queryKey?: readonly unknown[]) {
  const queryClient = useQueryClient();
  const addNotification = useAppStore((s) => s.addNotification);

  const refresh = useCallback(async () => {
    if (queryKey) {
      await queryClient.invalidateQueries({ queryKey });
    } else {
      await queryClient.invalidateQueries();
    }
    addNotification({ message: 'Page data refreshed successfully', type: 'info' });
  }, [queryClient, queryKey, addNotification]);

  return { refresh };
}
