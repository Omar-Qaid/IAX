import { useAppStore } from '@app/store/useAppStore';

export function useNotifications() {
  const addNotification = useAppStore((s) => s.addNotification);

  return {
    notifySuccess: (message: string) => addNotification({ message, type: 'success' }),
    notifyError: (message: string) => addNotification({ message, type: 'error' }),
    notifyWarning: (message: string) => addNotification({ message, type: 'warning' }),
    notifyInfo: (message: string) => addNotification({ message, type: 'info' }),
  };
}
