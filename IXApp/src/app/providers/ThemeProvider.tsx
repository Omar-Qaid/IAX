import React, { useMemo, useEffect } from 'react';
import { ThemeProvider as MuiThemeProvider, CssBaseline } from '@mui/material';
import { usePreferenceStore } from '@app/store/usePreferenceStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { createAppTheme } from '@app/theme/createAppTheme';

export const ThemeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const themeMode = usePreferenceStore((s) => s.themeMode);
  const density = usePreferenceStore((s) => s.density);
  const contrast = usePreferenceStore((s) => s.contrast);
  const rtl = usePreferenceStore((s) => s.rtl);
  const colorPreset = usePreferenceStore((s) => s.colorPreset);
  const fontFamily = usePreferenceStore((s) => s.fontFamily);
  const fontSize = usePreferenceStore((s) => s.fontSize);
  const zoom = usePreferenceStore((s) => s.zoom);
  const { currentLanguage } = useAppTranslation();
  const direction = rtl ? 'rtl' : currentLanguage.dir;

  const theme = useMemo(() => {
    return createAppTheme(themeMode, direction, { density, contrast, colorPreset, fontFamily, fontSize });
  }, [themeMode, direction, density, contrast, colorPreset, fontFamily, fontSize]);

  useEffect(() => {
    document.dir = direction;
    document.documentElement.dir = direction;
    document.documentElement.lang = currentLanguage.code;
    document.documentElement.dataset.density = density;
    document.documentElement.dataset.contrast = contrast ? 'high' : 'standard';
    document.documentElement.style.zoom = `${zoom}%`;
    return () => { document.documentElement.style.zoom = ''; };
  }, [direction, currentLanguage.code, density, contrast, zoom]);

  return (
    <MuiThemeProvider theme={theme}>
      <CssBaseline />
      {children}
    </MuiThemeProvider>
  );
};
