import { create } from 'zustand';
import { STORAGE_KEYS } from '@core/constants/storageKeys';

export type ThemeMode = 'light' | 'dark';
export type AppDensity = 'compact' | 'comfortable';

interface PreferenceState {
  themeMode: ThemeMode;
  density: AppDensity;
  contrast: boolean;
  rtl: boolean;
  navLayout: 'vertical' | 'horizontal' | 'mini';
  navColor: 'integrate' | 'apparent';
  colorPreset: string;
  fontFamily: string;
  fontSize: number;
  zoom: number;
  
  setThemeMode: (mode: ThemeMode) => void;
  toggleThemeMode: () => void;
  setDensity: (density: AppDensity) => void;
  setContrast: (contrast: boolean) => void;
  setRtl: (rtl: boolean) => void;
  setNavLayout: (layout: 'vertical' | 'horizontal' | 'mini') => void;
  setNavColor: (color: 'integrate' | 'apparent') => void;
  setColorPreset: (preset: string) => void;
  setFontFamily: (family: string) => void;
  setFontSize: (size: number) => void;
  setZoom: (zoom: number) => void;
  resetSettings: () => void;
}

const getInitialThemeMode = (): ThemeMode => {
  const saved = localStorage.getItem(STORAGE_KEYS.THEME_MODE);
  return saved === 'dark' ? 'dark' : 'light';
};

const initialState = {
  density: 'compact' as AppDensity,
  contrast: false,
  rtl: false,
  navLayout: 'vertical' as const,
  navColor: 'integrate' as const,
  colorPreset: 'default',
  fontFamily: 'Inter',
  fontSize: 14,
  zoom: 100,
};

export const usePreferenceStore = create<PreferenceState>((set) => ({
  ...initialState,
  themeMode: getInitialThemeMode(),
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
  setDensity: (density) => set({ density }),
  setContrast: (contrast) => set({ contrast }),
  setRtl: (rtl) => set({ rtl }),
  setNavLayout: (navLayout) => set({ navLayout }),
  setNavColor: (navColor) => set({ navColor }),
  setColorPreset: (colorPreset) => set({ colorPreset }),
  setFontFamily: (fontFamily) => set({ fontFamily }),
  setFontSize: (fontSize) => set({ fontSize }),
  setZoom: (zoom) => set({ zoom }),
  resetSettings: () => set(initialState),
}));
