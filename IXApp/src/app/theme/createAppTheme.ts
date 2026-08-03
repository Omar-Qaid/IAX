import { createTheme, type Theme } from '@mui/material/styles';
import { lightPalette, darkPalette } from './palette';
import { typography } from './typography';
import { spacing } from './spacing';
import { getComponentOverrides } from './componentOverrides';
import type { ThemeMode } from '@app/store/usePreferenceStore';

export interface AppThemePreferences {
  contrast?: boolean;
  colorPreset?: string;
  fontFamily?: string;
  fontSize?: number;
  density?: 'compact' | 'comfortable';
}

const presetColors: Record<string, string> = {
  default: '#005a9e', emerald: '#107c41', rose: '#c4314b', amber: '#d83b01', cyan: '#0078d4', violet: '#5c2d91',
};

export function createAppTheme(mode: ThemeMode, direction: 'ltr' | 'rtl' = 'ltr', preferences: AppThemePreferences = {}): Theme {
  const sourcePalette = mode === 'light' ? lightPalette : darkPalette;
  const paletteOptions = {
    ...sourcePalette,
    primary: { ...sourcePalette.primary, main: presetColors[preferences.colorPreset ?? 'default'] ?? (mode === 'light' ? '#005a9e' : '#2899f5') },
    ...(preferences.contrast ? {
      divider: mode === 'light' ? '#8a8886' : '#c8c6c4',
      background: mode === 'light' ? { default: '#ffffff', paper: '#ffffff' } : sourcePalette.background,
    } : {}),
  };

  const baseTheme = createTheme({
    palette: paletteOptions,
    typography: {
      ...typography,
      fontFamily: preferences.fontFamily || typography.fontFamily,
      fontSize: preferences.fontSize ?? typography.fontSize,
    },
    spacing: preferences.density === 'comfortable' ? 5 : spacing,
    direction,
    shape: {
      borderRadius: 2,
    },
  });

  const themeWithOverrides = createTheme(baseTheme, {
    components: getComponentOverrides(baseTheme),
  });

  return themeWithOverrides;
}
