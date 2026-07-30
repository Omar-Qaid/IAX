import { create } from 'zustand';
import { STORAGE_KEYS } from '@core/constants/storageKeys';

export type ThemeMode = 'light' | 'dark';
export type AppDensity = 'compact' | 'comfortable';

interface PreferenceState {
  themeMode: ThemeMode;
  density: AppDensity;
  setThemeMode: (mode: ThemeMode) => void;
  toggleThemeMode: () => void;
  setDensity: (density: AppDensity) => void;
}

const getInitialThemeMode = (): ThemeMode => {
  const saved = localStorage.getItem(STORAGE_KEYS.THEME_MODE);
  return saved === 'dark' ? 'dark' : 'light';
};

export const usePreferenceStore = create<PreferenceState>((set) => ({
  themeMode: getInitialThemeMode(),
  density: 'compact',
  setThemeMode: (mode: ThemeMode) => {
    localStorage.setItem(STORAGE_KEYS.THEME_MODE, mode);
    set({ themeMode: mode });
  },
  toggleThemeMode: () => {
    set((state) => {
      const nextMode: ThemeMode = state.themeMode === 'light' ? 'dark' : 'light';
      localStorage.setItem(STORAGE_KEYS.THEME_MODE, nextMode);
      return { themeMode: nextMode };
    });
  },
  setDensity: (density: AppDensity) => set({ density }),
}));
