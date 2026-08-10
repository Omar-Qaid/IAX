import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { STORAGE_KEYS } from '@core/constants/storageKeys';

export interface FavoritePage {
  path: string;
  label: string;
}

export interface RecentPage {
  path: string;
  label: string;
  timestamp: number;
}

interface NavigationState {
  sidebarOpen: boolean;
  sidebarPinned: boolean;
  commandPaletteOpen: boolean;
  settingsPanelOpen: boolean;
  notificationDrawerOpen: boolean;
  favorites: FavoritePage[];
  recentPages: RecentPage[];
  activeModuleId: string | null;

  setSidebarOpen: (open: boolean) => void;
  toggleSidebarPinned: () => void;
  setCommandPaletteOpen: (open: boolean) => void;
  setSettingsPanelOpen: (open: boolean) => void;
  setNotificationDrawerOpen: (open: boolean) => void;
  toggleFavorite: (path: string, label: string) => void;
  addRecentPage: (path: string, label: string) => void;
  setActiveModule: (moduleId: string | null) => void;
}

export const useNavigationStore = create<NavigationState>()(
  persist(
    (set) => ({
      sidebarOpen: false,
      sidebarPinned: false,
      commandPaletteOpen: false,
      settingsPanelOpen: false,
      notificationDrawerOpen: false,
      favorites: [],
      recentPages: [],
      activeModuleId: null,

      setSidebarOpen: (open) => set({ sidebarOpen: open }),
      toggleSidebarPinned: () => set((state) => ({ sidebarPinned: !state.sidebarPinned })),
      setCommandPaletteOpen: (open) => set({ commandPaletteOpen: open }),
      setSettingsPanelOpen: (open) => set({ settingsPanelOpen: open }),
      setNotificationDrawerOpen: (open) => set({ notificationDrawerOpen: open }),

      toggleFavorite: (path, label) =>
        set((state) => {
          const exists = state.favorites.some((f) => f.path === path);
          if (exists) {
            return { favorites: state.favorites.filter((f) => f.path !== path) };
          }
          return { favorites: [...state.favorites, { path, label }] };
        }),

      addRecentPage: (path, label) =>
        set((state) => {
          const filtered = state.recentPages.filter((p) => p.path !== path);
          const newPage = { path, label, timestamp: Date.now() };
          return { recentPages: [newPage, ...filtered].slice(0, 10) };
        }),
      setActiveModule: (moduleId) => set({ activeModuleId: moduleId }),
    }),
    {
      name: STORAGE_KEYS.NAVIGATION,
      partialize: (state) => ({
        sidebarPinned: state.sidebarPinned,
        favorites: state.favorites,
        recentPages: state.recentPages,
      }),
    }
  )
);
