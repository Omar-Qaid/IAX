import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { STORAGE_KEYS } from '@core/constants/storageKeys';
import {
  DEFAULT_ARABIC_UI_FONT_FAMILY,
  DEFAULT_UI_FONT_FAMILY,
} from '@shared/constants/fontFamilies';

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
  arabicFontFamily: string;
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
  setArabicFontFamily: (family: string) => void;
  setFontSize: (size: number) => void;
  setZoom: (zoom: number) => void;
  resetSettings: () => void;
}

const getInitialThemeMode = (): ThemeMode => {
  const saved = localStorage.getItem(STORAGE_KEYS.THEME_MODE);
  return saved === 'dark' ? 'dark' : 'light';
};

const defaultPreferences = {
  themeMode: 'light' as ThemeMode,
  density: 'compact' as AppDensity,
  contrast: false,
  rtl: false,
  navLayout: 'vertical' as const,
  navColor: 'integrate' as const,
  colorPreset: 'default',
  fontFamily: DEFAULT_UI_FONT_FAMILY,
  arabicFontFamily: DEFAULT_ARABIC_UI_FONT_FAMILY,
  fontSize: 13,
  zoom: 100,
};

const initialState = {
  ...defaultPreferences,
  themeMode: getInitialThemeMode(),
};

export const usePreferenceStore = create<PreferenceState>()(persist((set) => ({
  ...initialState,
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
  setArabicFontFamily: (arabicFontFamily) => set({ arabicFontFamily }),
  setFontSize: (fontSize) => set({ fontSize }),
  setZoom: (zoom) => set({ zoom }),
  resetSettings: () => {
    localStorage.setItem(STORAGE_KEYS.THEME_MODE, defaultPreferences.themeMode);
    set(defaultPreferences);
  },
}), {
  name: STORAGE_KEYS.PREFERENCES,
  partialize: ({ themeMode, density, contrast, rtl, navLayout, navColor, colorPreset, fontFamily, arabicFontFamily, fontSize, zoom }) => ({
    themeMode, density, contrast, rtl, navLayout, navColor, colorPreset, fontFamily, arabicFontFamily, fontSize, zoom,
  }),
}));
