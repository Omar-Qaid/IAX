import { create } from 'zustand';

export interface NotificationItem {
  id: string;
  message: string;
  type: 'success' | 'info' | 'warning' | 'error';
  autoHideDuration?: number;
}

interface NotificationState {
  notifications: NotificationItem[];
  addNotification: (notification: Omit<NotificationItem, 'id'>) => void;
  removeNotification: (id: string) => void;
}

export const useNotificationStore = create<NotificationState>((set) => ({
  notifications: [],
  addNotification: (notification) => {
    const id = `notif-${Date.now()}-${Math.random().toString(36).substring(2, 6)}`;
    set((state) => ({
      notifications: [...state.notifications, { ...notification, id }],
    }));
  },
  removeNotification: (id) => {
    set((state) => ({
      notifications: state.notifications.filter((notification) => notification.id !== id),
    }));
  },
}));
