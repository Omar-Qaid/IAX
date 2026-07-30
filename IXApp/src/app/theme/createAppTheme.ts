import { createTheme, type Theme } from '@mui/material/styles';
import { lightPalette, darkPalette } from './palette';
import { typography } from './typography';
import { spacing } from './spacing';
import { getComponentOverrides } from './componentOverrides';
import type { ThemeMode } from '@app/store/usePreferenceStore';

export function createAppTheme(mode: ThemeMode, direction: 'ltr' | 'rtl' = 'ltr'): Theme {
  const paletteOptions = mode === 'light' ? lightPalette : darkPalette;

  const baseTheme = createTheme({
    palette: paletteOptions,
    typography,
    spacing,
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
