import { useNotificationStore } from '@shared/services/notificationStore';
import { useCallback } from 'react';

export function useNotifications() {
  const addNotification = useNotificationStore((s) => s.addNotification);
  const notifySuccess = useCallback((message: string) => addNotification({ message, type: 'success' }), [addNotification]);
  const notifyError = useCallback((message: string) => addNotification({ message, type: 'error' }), [addNotification]);
  const notifyWarning = useCallback((message: string) => addNotification({ message, type: 'warning' }), [addNotification]);
  const notifyInfo = useCallback((message: string) => addNotification({ message, type: 'info' }), [addNotification]);

  return {
    notifySuccess,
    notifyError,
    notifyWarning,
    notifyInfo,
  };
}
