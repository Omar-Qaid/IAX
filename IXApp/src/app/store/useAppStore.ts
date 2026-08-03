import { create } from 'zustand';
import { DEFAULT_COMPANY } from '@core/constants/appConstants';
import { STORAGE_KEYS } from '@core/constants/storageKeys';

interface NotificationItem {
  id: string;
  message: string;
  type: 'success' | 'info' | 'warning' | 'error';
  autoHideDuration?: number;
}

interface AppStoreState {
  currentCompany: string;
  setCompany: (companyCode: string) => void;
  notifications: NotificationItem[];
  addNotification: (notification: Omit<NotificationItem, 'id'>) => void;
  removeNotification: (id: string) => void;
}

const getInitialCompany = (): string => {
  return localStorage.getItem(STORAGE_KEYS.COMPANY) || DEFAULT_COMPANY;
};

export const useAppStore = create<AppStoreState>((set) => ({
  currentCompany: getInitialCompany(),
  setCompany: (companyCode: string) => {
    localStorage.setItem(STORAGE_KEYS.COMPANY, companyCode);
    set({ currentCompany: companyCode });
  },
  notifications: [],
  addNotification: (notification) => {
    const id = `notif-${Date.now()}-${Math.random().toString(36).substring(2, 6)}`;
    set((state) => ({
      notifications: [...state.notifications, { ...notification, id }],
    }));
  },
  removeNotification: (id) => {
    set((state) => ({
      notifications: state.notifications.filter((n) => n.id !== id),
    }));
  },
}));
