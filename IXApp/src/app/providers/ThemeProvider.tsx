import React, { useMemo, useEffect } from 'react';
import { CacheProvider } from '@emotion/react';
import { ThemeProvider as MuiThemeProvider, CssBaseline } from '@mui/material';
import { usePreferenceStore } from '@app/store/usePreferenceStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { createAppTheme } from '@app/theme/createAppTheme';
import { ltrEmotionCache, rtlEmotionCache } from '@app/theme/emotionCache';

export const ThemeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const themeMode = usePreferenceStore((s) => s.themeMode);
  const density = usePreferenceStore((s) => s.density);
  const contrast = usePreferenceStore((s) => s.contrast);
  const colorPreset = usePreferenceStore((s) => s.colorPreset);
  const fontFamily = usePreferenceStore((s) => s.fontFamily);
  const arabicFontFamily = usePreferenceStore((s) => s.arabicFontFamily);
  const fontSize = usePreferenceStore((s) => s.fontSize);
  const zoom = usePreferenceStore((s) => s.zoom);
  const { currentLanguage } = useAppTranslation();
  const direction = currentLanguage.dir;
  const activeFontFamily = direction === 'rtl' ? arabicFontFamily : fontFamily;
  const emotionCache = direction === 'rtl' ? rtlEmotionCache : ltrEmotionCache;

  const theme = useMemo(() => {
    return createAppTheme(themeMode, direction, { density, contrast, colorPreset, fontFamily: activeFontFamily, fontSize });
  }, [themeMode, direction, density, contrast, colorPreset, activeFontFamily, fontSize]);

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
    <CacheProvider value={emotionCache}>
      <MuiThemeProvider theme={theme}>
        <CssBaseline />
        {children}
      </MuiThemeProvider>
    </CacheProvider>
  );
};
